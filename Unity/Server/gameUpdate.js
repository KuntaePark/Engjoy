const { MonsterType } = require("./monster.js");
const Physics = require("./physics.js");

const deltaTime = 1 / 40; //40 FPS _ 프레임

const PLAYER_SPEED = 7.0; //플레이어 속도
const RUNNER_SPEED = 5.0; //몬스터-러너 속도
const CHASER_SPEED = 6.0; //몬스터-체이서 속도

//러너의 움직임 트리거 범위
const RUNNER_FLEE_RANGE = 5;
const RUNNER_FLEE_RANGE_SQ = RUNNER_FLEE_RANGE * RUNNER_FLEE_RANGE; //거리 제곱값

//체이서의 어그로 범위
const CHASER_AGGRO_RANGE = 10;
const CHASER_AGGRO_RANGE_SQ = CHASER_AGGRO_RANGE * CHASER_AGGRO_RANGE; //거리 제곱값

//몬스터의 콜라이더 범위
const MONSTER_COLLIDER_DISTANCE = 1;
const MONSTER_COLLIDER_DISTANCE_SQ =
  MONSTER_COLLIDER_DISTANCE * MONSTER_COLLIDER_DISTANCE; //거리 제곱값

const ROAMING_SPEED = 2.0; //로밍 속도
const ROAM_AREA_SIZE = 15; //로밍 범위

function update(gameState) {
  // ================= ▼▼▼ 플레이어 관련 동기화 ▼▼▼ =================
  for (const id in gameState.players) {
    const p = gameState.players[id]; //키워드 들고 있으면 키워드 위치도 플레이어 위치로 고정!(동기화)

    if (p.holdingKeywordId && gameState.keywords[p.holdingKeywordId]) {
      const holdingKeyword = gameState.keywords[p.holdingKeywordId];

      holdingKeyword.x = p.x;
      holdingKeyword.y = p.y;
    } //플레이어 이동 계산

    const newX = p.x + p.inputH * deltaTime * PLAYER_SPEED;
    const newY = p.y + p.inputV * deltaTime * PLAYER_SPEED; //충돌 체크 로직

    let isColliding = false;

    for (const otherId in gameState.players) {
      if (id === otherId) continue;

      const otherPlayer = gameState.players[otherId];
      const myPos = new Physics.Vector2(newX, newY);

      const otherCurrentPos = new Physics.Vector2(otherPlayer.x, otherPlayer.y);

      if (Physics.checkCollision(myPos, otherCurrentPos, 30)) {
        console.log(`Player ${id} is colliding with ${otherId}`);

        isColliding = true;

        break;
        // ================= ▲▲▲ 플레이어 관련 동기화 ▲▲▲ =================
      }
    }

    if (!isColliding) {
      p.x = newX;
      p.y = newY;
    } //입력값 초기화

    p.inputH = 0;
    p.inputV = 0; //상호작용 가능 상태 업데이트(키워드, 출구)

    updateInteractionState(p, gameState.keywords);
    updateExitInteractionState(p, gameState.exit);
  }

  //몬스터가 들고 있는 키워드 위치 동기화
  for (const id in gameState.monsters) {
    const monster = gameState.monsters[id];
    if (
      monster.holdingKeywordId &&
      gameState.keywords[monster.holdingKeywordId]
    ) {
      const keyword = gameState.keywords[monster.holdingKeywordId];
      keyword.x = monster.x;
      keyword.y = monster.y;
    }
  }

  // ================= ▼▼▼ 몬스터 관련 함수 호출 ▼▼▼ =================
  for (const id in gameState.monsters) {
    const monster = gameState.monsters[id];
    if (!monster) continue;

    updateMonsterMovement(monster, gameState);
  }
  // ================= ▲▲▲ 몬스터 관련 함수 호출 ▲▲▲ =================
}

function updateExitInteractionState(player, exit) {
  if (!player.holdingKeywordId || !exit || exit.isOpen) {
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

//플레이어와 키워드 간의 상호작용 상태 업데이트 함수
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
    if (keyword.carrierId) continue;

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

// ================= 몬스터 이동 =================
function updateMonsterMovement(monster, gameState) {
  let target = null;
  let closestDistSq = Infinity;

  //러너, 체이서 타입에 따라 움직임 패턴 부여
  switch (monster.type) {
    case MonsterType.RUNNER: //러너
      //해당 몬스터와 가장 가까운 플레이어를 찾는다.
      for (const id in gameState.players) {
        const player = gameState.players[id];
        const distSq = Physics.squareDistance(monster, player);

        if (distSq < closestDistSq) {
          closestDistSq = distSq;
          target = player;
        }
      }

      //타겟이 있다면 도망치는 움직임 부여
      if (target && closestDistSq < RUNNER_FLEE_RANGE_SQ) {
        const dx = monster.x - target.x;
        const dy = monster.y - target.y;
        //몬스터의 일정한 로밍 속도를 위해 제곱근 계산(Math.sqrt)를 이용
        const dist = Math.sqrt(dx * dx + dy * dy);

        if (dist > 0) {
          //거리가 0 이상일 때에만 이동
          const moveX = (dx / dist) * RUNNER_SPEED * deltaTime;
          const moveY = (dy / dist) * RUNNER_SPEED * deltaTime;

          if (!isNaN(moveX) && !isNaN(moveY)) {
            monster.x += moveX;
            monster.y += moveY;
          }
        }
      }
      break;

    case MonsterType.CHASER: //체이서
      for (const id in gameState.players) {
        const player = gameState.players[id];
        if (player.holdingKeywordId) {
          //해당 플레이어가 키워드 홀딩 상태인지 확인
          const distSq = Physics.squareDistance(monster, player);
          if (distSq < closestDistSq) {
            closestDistSq = distSq;
            target = player;
          }
        }
      }

      //타겟이 있다면 그 방향으로 이동
      if (target && closestDistSq < CHASER_AGGRO_RANGE_SQ) {
        monster.isActive = true;

        const dx = target.x - monster.x;
        const dy = target.y - monster.y;
        const dist = Math.sqrt(dx * dx + dy * dy);

        if (dist > 0) {
          //거리가 0 이상일 때에만 이동
          const moveX = (dx / dist) * CHASER_SPEED * deltaTime;
          const moveY = (dy / dist) * CHASER_SPEED * deltaTime;

          if (!isNaN(moveX) && !isNaN(moveY)) {
            monster.x += moveX;
            monster.y += moveY;
          }
        }
        if (
          target &&
          Physics.squareDistance(monster, target) < MONSTER_COLLIDER_DISTANCE_SQ
        ) {
          gameState.monsterHitsPlayer(monster.id, target.id);
        }
      } else {
        //타겟이 없다면,
        //비활성 상태.
        monster.isActive = false;

        //로밍
        monster.roamTimer -= deltaTime;

        //타이머가 다 되거나 목표 지점이 없으면 새로운 목표 설정
        if (monster.roamTimer <= 0 || monster.roamTargetX === null) {
          //현재 위치 주변에 새로운 목표 좌표 랜덤 설정
          monster.roamTargetX =
            monster.x + (Math.random() - 0.5) * ROAMING_SPEED * deltaTime;
          monster.roamTargetY =
            monster.y + (Math.random() - 0.5) * ROAMING_SPEED * deltaTime;

          //2~6초 사이의 랜덤 시간 지정
          monster.roamTimer = 2 + Math.random() * 4;

          console.log(
            `Monster ${monster.id} is roaming to (${monster.roamTargetX.toFixed(
              0
            )}, ${monster.roamTargetY.toFixed(0)})`
          );
        }

        //목표 지점을 향해 이동
        if (monster.roamTargetX !== null) {
          const dx = monster.roamTargetX - monster.x;
          const dy = monster.roamTargetY - monster.y;
          const dist = Math.sqrt(dx * dx + dy * dy);

          //목표에 거의 도달하면 멈추기
          if (dist > 1) {
            const moveX = (dx / dist) * ROAMING_SPEED * deltaTime;
            const moveY = (dy / dist) * ROAMING_SPEED * deltaTime;

            if (!isNaN(moveX) && !isNaN(moveY)) {
              monster.x += moveX;
              monster.Y += moveY;
            }
          }
        }
      }
      break;
  }
}

// ================= SETINTERVAL =================
function startGameUpdate(gameState, broadcast) {
  setInterval(() => {
    //게임 로직 업데이트 실행!

    update(gameState); //업데이트된 전체 상태를 모든 클라이언트에게 브로드캐스트!

    const packet = gameState.getFullStatePacket();

    broadcast(JSON.stringify(packet));
  }, deltaTime * 1000);
}

module.exports = { startGameUpdate };
