const { MonsterType, Monster } = require("../packet/monster.js");
const { setupLevel } = require("./levelManager.js");
const Physics = require("../common/Physics.js");

const deltaTime = 1 / 40; //40 FPS _ 프레임

const RUNNER_SPEED = 5.0; //몬스터-러너 속도
const CHASER_SPEED = 6.0; //몬스터-체이서 속도

//러너의 움직임 트리거 범위
const RUNNER_FLEE_RANGE = 5;
const RUNNER_FLEE_RANGE_SQ = RUNNER_FLEE_RANGE * RUNNER_FLEE_RANGE; //거리 제곱값

//체이서의 어그로 범위
const CHASER_AGGRO_RANGE = 10;
const CHASER_AGGRO_RANGE_SQ = CHASER_AGGRO_RANGE * CHASER_AGGRO_RANGE; //거리 제곱값
const CHASER_TELEPORT_RADIUS = 8.0; //체이서 순간이동 범위

//몬스터의 콜라이더 범위
const MONSTER_COLLIDER_DISTANCE = 1;
const MONSTER_COLLIDER_DISTANCE_SQ =
  MONSTER_COLLIDER_DISTANCE * MONSTER_COLLIDER_DISTANCE; //거리 제곱값

const ROAMING_SPEED = 1.0; //로밍 속도
const ROAM_AREA_SIZE = 15; //로밍 범위

// =================================================================
// ## 세부 기능별 업데이트 함수
// =================================================================
// ================= ▼▼▼ 플레이어 관련 동기화 ▼▼▼ =================
function updatePlayers(gameState) {
  for (const id in gameState.players) {
    const p = gameState.players[id];

    //공격 쿨다운
    if (p.attackCooldown > 0) {
      p.attackCooldown -= deltaTime;
    }

    //플레이어 이동 계산
    const newX = p.x + p.inputH * deltaTime * p.speed;
    const newY = p.y + p.inputV * deltaTime * p.speed;

    //충돌 체크 로직 - 맵 콜라이더
    let isCollidingWithMap = false;

    if (gameState.colliders && gameState.colliders.size > 0) {
      isCollidingWithMap = Physics.checkMapCollision(
        gameState.colliders,
        newX,
        newY
      );
    }

    //충돌 체크 로직 - 플레이어
    let isCollidingWithPlayer = false;

    for (const otherId in gameState.players) {
      if (id === otherId) continue;

      const otherPlayer = gameState.players[otherId];
      const myPos = new Physics.Vector2(newX, newY);

      const otherCurrentPos = new Physics.Vector2(otherPlayer.x, otherPlayer.y);

      if (Physics.checkCollision(myPos, otherCurrentPos, 30)) {
        console.log(`Player ${id} is colliding with ${otherId}`);

        isCollidingWithPlayer = true;

        break;
      }
    }

    if (!isCollidingWithMap && !isCollidingWithPlayer) {
      p.x = newX;
      p.y = newY;
    }

    // -------------- 게임 플레이 중일 때에만 실행되는 로직 --------------
    if (gameState.status === "PLAY") {
      //플레이어 무적 처리
      if (p.invincibility <= 40) p.invincibility++;

      updateInteractionState(p, gameState.keywords);
      updateExitInteractionState(p, gameState.exit);
      updateRevive(p, gameState);

      //키워드 위치 동기화
      if (p.holdingKeywordId && gameState.keywords[p.holdingKeywordId]) {
        const holdingKeyword = gameState.keywords[p.holdingKeywordId];

        holdingKeyword.x = p.x;
        holdingKeyword.y = p.y;
      }
    }
  }
}
// ================= ▲▲▲ 플레이어 관련 동기화 ▲▲▲ =================
// ================= ▼▼▼ 몬스터 관련 동기화 ▼▼▼ =================
function updateMonsters(gameState) {
  for (const id in gameState.monsters) {
    const monster = gameState.monsters[id];
    if (!monster) continue;

    updateMonsterMovement(monster, gameState);
  }
}
// ================= ▲▲▲ 몬스터 관련 동기화 ▲▲▲ =================
// ================= ▼▼▼ 제한시간 동기화 ▼▼▼ =================
function updateGameTimer(gameState) {
  if (gameState.timeLimit > 0) {
    gameState.timeLimit -= deltaTime;
  }

  if (gameState.timeLimit <= 0) {
    gameState.timeLimit = 0;
    gameState.isGameOver = true;
    console.log("[GAME OVER] Time is up.");
  }
}
// ================= ▲▲▲ 제한시간 동기화 ▲▲▲ =================
// ================= ▼▼▼ [대기방]카운트다운 동기화 ▼▼▼ =================
function updateCountdown(gameState) {
  if (gameState.countdown > -1) {
    gameState.countdown -= deltaTime;
    if (gameState.countdown <= 0) {
      gameState.status = "PLAY";

      for (const id in gameState.players) {
        const player = gameState.players[id];
        if (player.isBuffed) {
          player.isBuffed = false;
          player.speed = player.baseSpeed;
        }
        if (player.hasShield) player.hasShield = false;

        const spawnArea = { minX: 1, maxX: 5, minY: 6, maxY: 12 };

        //플레이어 위치 리셋
        player.x =
          Math.random() * (spawnArea.maxX - spawnArea.minX) + spawnArea.minX;
        player.y =
          Math.random() * (spawnArea.maxY - spawnArea.minY) + spawnArea.minY;
      }

      setupLevel(gameState, 1);
    }
  }
}

// =================================================================
// ## 메인 업데이트
// =================================================================
// ================= ▼▼▼ 게임2 동기화! ▼▼▼ =================
function update(gameState) {
  //항상 실행되는 동기화
  updatePlayers(gameState);

  //'PLAY' 상태(게임 플레이 중)일 때에만 실행되는 로직
  if (gameState.status === "PLAY") {
    updateMonsters(gameState);
    updateGameTimer(gameState);
  }

  //'MATCHING' 상태(대기방)일 때에만 실행되는 로직
  else if (gameState.status === "MATCHINGROOM") {
    updateCountdown(gameState);
  }
}

// =================================================================
// ## 헬퍼 함수들
// =================================================================
// ------ 플레이어 부활 함수 ------
function updateRevive(player, gameState) {
  if (player.isDown || gameState.isGameOver) {
    player.revivablePlayerId = -1;
    player.reviveProgress = 0;
    return;
  }

  //부활 대상 찾기
  let closestDownPlayerId = -1;
  let closestDistSq = Physics.interactionDistSq;

  for (const id in gameState.players) {
    if (player.id === id) continue;
    const otherPlayer = gameState.players[id];
    if (otherPlayer.isDown) {
      const distSq = Physics.squareDistance(player, otherPlayer);

      if (distSq < closestDistSq) {
        closestDistSq = distSq;
        closestDownPlayerId = id;
      }
    }
  }
  player.revivablePlayerId = closestDownPlayerId;

  // console.log(
  //   `[REVIVE CONFIRM] Player: ${player.id}, rivived: ${player.revivablePlayerId}`
  // );

  if (player.revivablePlayerId >= 0 && player.isHoldingInteract) {
    player.reviveProgress += deltaTime;

    console.log(
      `[REVIVE] ${player.id} is reviving ${player.reviveProgress.toFixed(2)}`
    );

    if (player.reviveProgress >= 2) {
      const targetPlayer = gameState.players[player.revivablePlayerId];
      if (targetPlayer) {
        targetPlayer.isDown = false;
        targetPlayer.hp = 2;
        targetPlayer.invincibility = 0;
      }

      player.reviveProgress = 0;
    }
  } else {
    player.reviveProgress = 0;
  }
}

// ------ 플레이어와 출구 간의 상호작용 상태 업데이트 함수 ------
function updateExitInteractionState(player, exit) {
  if (!exit) {
    player.canInteractWithExit = false;

    return;
  }

  const distSq = Physics.squareDistance(
    { x: player.x, y: player.y },
    { x: exit.x, y: exit.y }
  );

  if (distSq < Physics.interactionDistSq) {
    player.canInteractWithExit = true;
  } else {
    player.canInteractWithExit = false;
  }
}

// ------ 플레이어와 키워드 간의 상호작용 상태 업데이트 함수 ------
function updateInteractionState(player, keywords) {
  //키워드를 이미 들고 있는 상태라면 상호작용 불가

  if (player.holdingKeywordId) {
    player.interactableKeywordId = null;

    return;
  }

  let closestDistSq = Infinity;
  let closestKeywordId = null;

  for (const id in keywords) {
    const keyword = keywords[id];

    //다른 사람이 들고 있는 키워드는 무시하세요.
    //남의 포켓몬을 빼앗으면 도둑!
    if (keyword.carrierId >= 0) continue;

    const distSq = Physics.squareDistance(
      { x: player.x, y: player.y },
      { x: keyword.x, y: keyword.y }
    );

    if (distSq < Physics.interactionDistSq && distSq < closestDistSq) {
      closestDistSq = distSq;

      closestKeywordId = id;
    }
  }

  player.interactableKeywordId = closestKeywordId;
}

// ------ 몬스터 이동 함수 ------
function updateMonsterMovement(monster, gameState) {
  if (monster.hitStunTimer > 0) {
    monster.hitStunTimer -= deltaTime;

    return; //경직 중에는 모든 행동 중지!
  } //러너, 체이서 타입에 따라 움직임 패턴 부여

  switch (monster.type) {
    case MonsterType.RUNNER: {
      //러너

      let target = null;
      let closestDistSq = Infinity;

      for (const id in gameState.players) {
        const player = gameState.players[id];

        if (player.isDown) continue; // 쓰러진 플레이어 무시

        const distSq = Physics.squareDistance(monster, player);

        if (distSq < closestDistSq) {
          closestDistSq = distSq;

          target = player;
        }
      }

      const isFleeing = target && closestDistSq < RUNNER_FLEE_RANGE_SQ;
      const isBoosted = monster.hitFleeBoostTimer > 0;

      let moveX = 0;
      let moveY = 0;

      let currentSpeed = ROAMING_SPEED; //행동 결정: 도망, 부스트, 로밍

      if (isFleeing || isBoosted) {
        if (!target) {
          //부스트 상태지만 도망칠 대상이 없다면 로밍처럼 행동
        } else {
          const dx = monster.x - target.x;
          const dy = monster.y - target.y;
          const dist = Math.sqrt(dx * dx + dy * dy);
          currentSpeed = RUNNER_SPEED;

          if (isBoosted) {
            monster.hitFleeBoostTimer -= deltaTime;
            currentSpeed *= 1.5; //부스트 시 러너 이속증가
          }

          if (dist > 0) {
            moveX = (dx / dist) * currentSpeed * deltaTime;
            moveY = (dy / dist) * currentSpeed * deltaTime;
          }
        }
      } //일반 로밍 상태

      if (moveX === 0 && moveY === 0) {
        if (
          monster.roamingTargetX === null ||
          Physics.squareDistance(monster, {
            x: monster.roamingTargetX,

            y: monster.roamingTargetY,
          }) < 1
        ) {
          monster.roamingTargetX =
            monster.x + (Math.random() - 0.5) * ROAM_AREA_SIZE;

          monster.roamingTargetY =
            monster.y + (Math.random() - 0.5) * ROAM_AREA_SIZE;
        } //목표를 향해 이동

        const dx = monster.roamingTargetX - monster.x;
        const dy = monster.roamingTargetY - monster.y;
        const dist = Math.sqrt(dx * dx + dy * dy);

        if (dist > 1) {
          moveX = (dx / dist) * ROAMING_SPEED * deltaTime;
          moveY = (dy / dist) * ROAMING_SPEED * deltaTime;
        }
      } //이동 적용

      if (moveX !== 0 || moveY !== 0) {
        const y_offset = -0.7; //충돌 판정 y좌표 보정값
        const OVER_COLLISION = 0.3; // X축 이동 시도

        if (moveX !== 0) {
          const newX = monster.x + moveX;
          const checkX = newX + Math.sign(moveX) * OVER_COLLISION;

          if (
            !gameState.colliders.has(
              `${Math.floor(checkX)}, ${Math.floor(monster.y + y_offset)}`
            )
          ) {
            monster.x = newX;
          }
        } // Y축 이동 시도

        if (moveY !== 0) {
          const newY = monster.y + moveY;
          const checkY = newY + Math.sign(moveY) * OVER_COLLISION;

          if (
            !gameState.colliders.has(
              `${Math.floor(monster.x)}, ${Math.floor(checkY + y_offset)}`
            )
          ) {
            monster.y = newY;
          }
        }
      }

      break;
    }

    case MonsterType.CHASER:
      {
        //체이서
        let moveX = 0;
        let moveY = 0;
        monster.justTeleported = false;

        //키워드를 든 모든 플레이어를 리스트에 저장
        const keywordHolders = [];
        for (const id in gameState.players) {
          const player = gameState.players[id];
          if (player.holdingKeywordId && !player.isDown) {
            keywordHolders.push(player);
          }
        }

        //키워드를 든 플레이어가 한 명이라도 있을 경우
        if (keywordHolders.length > 0) {
          //각 체이서가 타겟 리스트에서 무작위로 한 명을 선택
          const targetPlayer =
            keywordHolders[Math.floor(Math.random() * keywordHolders.length)];

          //비활성화 상태라면 타겟 근처로 순간이동하며 활성화되기
          if (!monster.isActive) {
            const teleportAngle = Math.random() * 2 * Math.PI;

            monster.x =
              targetPlayer.x + Math.cos(teleportAngle) * CHASER_TELEPORT_RADIUS;

            monster.y =
              targetPlayer.y + Math.sin(teleportAngle) * CHASER_TELEPORT_RADIUS;

            monster.isActive = true;
            monster.justTeleported = true;
          }

          //활성화 상태 - 타겟 플레이어를 찢으러 달려감
          const dx = targetPlayer.x - monster.x;
          const dy = targetPlayer.y - monster.y;
          const dist = Math.sqrt(dx * dx + dy * dy);

          if (dist > 0) {
            moveX = (dx / dist) * CHASER_SPEED * deltaTime;
            moveY = (dy / dist) * CHASER_SPEED * deltaTime;
          }
        } else {
          //비활성화 상태로 전환
          monster.isActive = false;
          monster.teleportTime -= deltaTime;

          if (monster.teleportTime <= 0) {
            //타이머가 다된다면

            let newX, newY;
            let isSafe = false; //스폰되어도 괜찮은 위치인지 알려주는 플래그
            let attempts = 0;

            while (!isSafe && attempts < 50) {
              newX = Math.random() * 15;
              newY = Math.random() * 15;

              if (
                !gameState.colliders.has(
                  `${Math.floor(newX)}, ${Math.floor(newY)}`
                )
              ) {
                isSafe = true;
              }
              attempts++;
            }

            if (isSafe) {
              monster.x = newX;
              monster.y = newY;
            }
            monster.teleportTime = 2;
          }
        }
        //이동 적용
        if (!isNaN(monster.x + moveX)) monster.x += moveX;
        if (!isNaN(monster.y + moveY)) monster.y += moveY;

        //피격 판정 로직
        if (monster.isActive) {
          for (const id in gameState.players) {
            const player = gameState.players[id];
            if (player.isDown) continue; //쓰러진 플레이어 무시

            const distSq = Physics.squareDistance(monster, player);
            if (distSq < MONSTER_COLLIDER_DISTANCE_SQ) {
              gameState.monsterHitsPlayer(monster.id, player.id);
            }
          }
        }
      }
      break;
  }
}


module.exports = { update , deltaTime };
