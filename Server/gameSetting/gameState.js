const Player = require("../packet/player.js");
const { Keyword } = require("../packet/keywords.js");
const Exit = require("../packet/exit.js");
const { generateId } = require("./utils.js");
const { setupLevel } = require("./levelManager.js");
const Physics = require("../common/Physics");
const { MonsterType } = require("../packet/monster.js");

//공격 관련 상수
const ATTACK_RANGE = Physics.colliderRadius * 10; //플레이어 충돌 범위의 3배
const ATTACK_RANGE_SQ = ATTACK_RANGE * ATTACK_RANGE;
const ATTACK_DAMAGE = 1; //플레이어 데미지_만약을 위해 상수로 따로 지정해두기

class GameState {
  constructor() {
    this.players = {};
    this.keywords = {};
    this.monsters = {};
    this.exit = null; //출구는 단일객체로 관리

    this.gameLevel = 1;
    this.mapName = "";
    this.timeLimit = 61;
    this.isGameOver = false;

    this.score = 0;
    this.gold = 0;
    this.completedSentences = []; //text, meaning, difficulty, id 등을 담을 것.

    this.status = "MATCHINGROOM"; //"MATCHINGROOM", "PLAY"
    this.countdown = 60; //준비완료 상태

    this.colliders = new Set(); //맵의 충돌체들. (x,y)좌표로 관리
  } //새 플레이어 추가

  // ================= ▼▼▼ 플레이어 생성 & 삭제 ▼▼▼ =================
  addPlayer(playerId) {
    const newPlayerId = playerId;

    const spawnArea = { minX: 1.0, maxX: 4.0, minY: 8.0, maxY: 11.0 };

    let spawnX = 2.0; //기본 스폰 위치
    let spawnY = 9.0;
    let isSpawnPointSafe = false;
    let attempts = 0;
    const maxAttempts = 50;

    //스폰 가능한 위치를 찾을 때까지 루프
    while (attempts < maxAttempts && !isSpawnPointSafe) {
      const randomX =
        Math.random() * (spawnArea.maxX - spawnArea.minX) + spawnArea.minX;
      const randomY =
        Math.random() * (spawnArea.maxY - spawnArea.minY) + spawnArea.minY;

      const tileX = Math.floor(randomX);
      const tileY = Math.floor(randomY);

      if (!this.colliders.has(`${tileX},${tileY}`)) {
        spawnX = randomX;
        spawnY = randomY;
        isSpawnPointSafe = true; // 안전한 위치를 찾았으므로 루프를 종료합니다.
      }
      attempts++;
    }

    if (!isSpawnPointSafe) {
      console.warn(
        `Could not find a safe spawn point in ${maxAttempts} attempts. Using default spawn point.`
      );
    }

    //Player 객체 생성 시 ID만 전달
    const player = new Player(newPlayerId, spawnX, spawnY);

    //우선은 inventory에 테스트용으로 최대 세팅
    player.inventory = {
      potion: 3,
      buff: 1,
      shield: 1,
    };

    this.players[newPlayerId] = player;

    console.log(`Player ${newPlayerId} connected`);
    return player;
  }

  removePlayer(playerId) {
    const player = this.players[playerId];

    if (!player) return; //플레이어가 키워드를 들고 있었다면 해당 키워드 드랍 처리

    if (player.holdingKeywordId) {
      const heldKeyword = this.keywords[player.holdingKeywordId];

      if (heldKeyword) {
        heldKeyword.carrierId = -1;
        heldKeyword.x = player.x;
        heldKeyword.y = player.y;
      }

      console.log(
        `Player ${playerId} disconnected, dropped keyword ${player.holdingKeywordId} at (${heldKeyword.x}, ${heldKeyword.y})`
      );
    }

    delete this.players[playerId];
    console.log(`Player ${playerId} disconnected`);

    if (Object.keys(this.players).length === 0) {
      this.monsters = {};
      console.log("All players have disconnected. Deleting all monsters.");
    }
  }

  playerInteracts(playerId) {
    const player = this.players[playerId];
    if (!player || player.isDown || this.isGameOver) return;

    //[1순위] 다른 플레이어 부활시키기
    if (player.revivablePlayerId >= 0) {
      //부활은 상호작용키를 누르는 것으로 처리. 다른 동작 블락.
      return;
    }

    //[2순위] 출구와 상호작용 가능상 상태인 경우 (키워드 홀딩 & 출구 근처)
    if (player.holdingKeywordId && player.canInteractWithExit && this.exit) {
      this.playerSubmitsKeywordToExit(playerId);
      return;
    }

    //[3순위] 키워드를 들 수 있는 상태인 경우 (키워드 홀딩X & 키워드 근처)
    else if (!player.holdingKeywordId && player.interactableKeywordId) {
      this.playerHoldingKeywords(playerId);
      return;
    }

    //[빈손으로 열린 출구와 상호작용]
    if (
      !player.holdingKeywordId &&
      player.canInteractWithExit &&
      this.exit &&
      this.exit.isOpen
    ) {
      player.isEscaped = true;
      console.log(`[Level] Player ${playerId} has escaped.`);
      // 탈출 시 들고 있던 키워드가 있다면 제거 (이 경우는 없지만 안전장치)
      if (player.holdingKeywordId) {
        delete this.keywords[player.holdingKeywordId];
        player.holdingKeywordId = null;
      }
      this.checkForLevelUp();
      return; // 행동 완료
    }
  }
  // ================= ▲▲▲ 플레이어 생성 & 삭제 ▲▲▲ =================
  // ================= ▼▼▼ 플레이어 준비완료 ▼▼▼ =================
  setPlayerReady(playerId, isReady) {
    const player = this.players[playerId];
    if (!player) return;

    player.isReady = isReady;

    const playersArray = Object.values(this.players);
    //모든 플레이어가 준비 완료 상태일 시
    const allReady = playersArray.every((p) => p.isReady);

    if (allReady) {
      if (this.countdown > 5) {
        this.countdown = 5;
        console.log(`All players are ready. Countdown started.`);
      }
    }
  }
  // ================= ▲▲▲ 플레이어 준비완료 ▲▲▲ =================
  // ================= ▼▼▼ 플레이어 이동 ▼▼▼ =================
  setPlayerInput(playerId, input) {
    const player = this.players[playerId];
    if (!player || player.isDown || this.isGameOver) return;

    this.players[playerId].inputH = input.x;
    this.players[playerId].inputV = input.y;

    player.isHoldingInteract = input.interactHold;

    //디버깅용 콘솔 로그
    // if (player.isHoldingInteract) {
    //   console.log(`[Server] ${playerId} is pressing Space Key...`);
    // }
  }
  // ================= ▲▲▲ 플레이어 이동 ▲▲▲ =================

  // ================= ▼▼▼ 플레이어 아이템 사용 ▼▼▼ =================
  playerUseItem(playerId, itemType) {
    const player = this.players[playerId];
    if (!player) return;

    console.log(`[Item] player ${playerId} tries to use ${itemType}`);

    switch (itemType) {
      case "potion":
        this.usePotion(player);
        break;
      case "buff":
        this.useBuff(player);
        break;
      case "shield":
        this.useShield(player);
        break;
    }
  }

  usePotion(player) {
    if (player.inventory.potion > 0 && player.hp < player.maxHp) {
      if (this.status === "PLAY") player.inventory.potion--;
      player.hp++;
      console.log(
        `[Item Success] Player ${player.id} used HP Potion. HP: ${player.hp}`
      );
    } else {
      console.log(`[Item Fail] Player ${player.id} failed to use HP Potion.`);
    }
  }

  useBuff(player) {
    if (player.inventory.buff > 0 && !player.isBuffed) {
      if (this.status === "PLAY") player.inventory.buff--;
      player.isBuffed = true;
      player.speed *= 1.25; //버프 속도
      //player.attackSpeed *= 1.25; //공격 속도는 나중에 추가
      console.log(
        `[Item Success] Player ${player.id} used Buff. Speed is now ${player.speed}`
      );
    } else {
      console.log(`[Item Fail] Player ${player.id} failed to use Buff.`);
    }
  }

  useShield(player) {
    if (player.inventory.shield > 0 && !player.hasShield) {
      if (this.status === "PLAY") player.inventory.shield--;
      player.hasShield = true;
      console.log(`[Item Success] Player ${player.id} used Shield.`);
    } else {
      console.log(`[Item Failed] Player ${player.id} failed to use Shield.`);
    }
  }
  // ================= ▲▲▲ 플레이어 아이템 사용 ▲▲▲ =================

  // ================= ▼▼▼ 플레이어 공격 ▼▼▼ =================
  playerAttacks(playerId) {
    const player = this.players[playerId];
    if (!player) return; //플레이어가 없으면 아무것도 안함

    //공격 쿨타임 중에는 아무것도 안함
    if (player.attackCooldown > 0) {
      return;
    }

    console.log(
      `[Debug] Checking player ${playerId}. Holding keyword ID:`,
      player.holdingKeywordId,
      "Type:",
      typeof player.holdingKeywordId
    );

    //플레이어가 키워드 홀딩 상태일 때도 아무것도 안함
    if (player.holdingKeywordId) {
      console.log(
        `[Attack Fail] Player ${playerId} cannot attack while holding a keyword.`
      );
      return;
    }

    //공격 로직 넣기
    console.log(`[ATTACK] Player ${playerId} initiated an attack.`);

    let hitMonster = null;

    //사거리 내에 몬스터가 있는지 검사
    for (const monsterId in this.monsters) {
      const monster = this.monsters[monsterId];

      if (!monster) continue;

      //공격 범위 주변에
      if (monster.type === "chaser" && !monster.isActive) {
        continue; //다음 몬스터로 넘어감
      }

      const distSq = Physics.squareDistance(player, monster);

      //사거리 내에 있다면,
      if (distSq < ATTACK_RANGE_SQ) {
        hitMonster = monster; //공격을 받은 몬스터를 저장
        break;
      }
    }

    //플레이어가 몬스터를 타격한 경우
    if (hitMonster) {
      hitMonster.hp -= ATTACK_DAMAGE;

      if (hitMonster.type === MonsterType.RUNNER) {
        hitMonster.hitFleeBoostTimer = 0.6; //돔황챠
      } else {
        hitMonster.hitStunTimer = 0.3; //체이서 피격 경직 타임
      }

      console.log(
        `[Hit Success] Player ${player.id} hit monster ${hitMonster}`
      );

      //타격한 몬스터의 hp가 0 이하가 될 경우
      if (hitMonster.hp <= 0) {
        console.log(`[Monster Down] Monster ${hitMonster.id} is dead.`);

        //그 몬스터가 키워드를 들고 있는 경우
        if (hitMonster.keywordData) {
          const keywordId = generateId(new Set(Object.keys(this.keywords)));
          //몬스터가 죽은 위치에 몬스터가 가진 정보로 새 키워드 생성
          const newKeyword = new Keyword(
            keywordId,
            hitMonster.keywordData.text,
            hitMonster.x,
            hitMonster.y,
            hitMonster.keywordData.isAnswer
          );

          this.keywords[keywordId] = newKeyword;
        }

        delete this.monsters[hitMonster.id];
      }
    } else {
      console.log(`[Hit Fail] Player ${player.id}'s attack hit nothing.`);
    }

    player.attackCooldown = player.isBuffed ? 0.1 : 0.2;
  }
  // ================= ▲▲▲ 플레이어 공격 ▲▲▲ =================

  // ================= ▼▼▼ 플레이어&키워드 상호작용 ▼▼▼ =================
  playerHoldingKeywords(playerId) {
    const player = this.players[playerId];

    if (!player) return; //플레이어 주변에 상호작용 가능한 키워드가 있는 경우 //상호작용 처리 -> 플레이어를 키워드 홀딩 상태로 전환

    if (player.interactableKeywordId) {
      const keywordId = player.interactableKeywordId;

      const keyword = this.keywords[keywordId]; //다른 사람이 키워드를 들고 있지 않은지 혹시 모르니 재차 확인 //(동시에 상호작용 버튼 눌렀을 때의 버그 방지)

      if (keyword && keyword.carrierId < 0) {
        player.holdingKeywordId = keywordId;
        keyword.carrierId = playerId;

        console.log(
          `Player ${playerId} is interacting with keyword ${keywordId}`
        );
      }
    }
  }

  playerSubmitsKeywordToExit(playerId) {
    const player = this.players[playerId];

    if (!player || !player.holdingKeywordId || !player.canInteractWithExit) {
      return; //키워드를 안들고 있거나, 출구 근처가 아니라면 실행 안함
    }

    const exit = this.exit;
    const holdingKeywordId = player.holdingKeywordId;
    const keyword = this.keywords[holdingKeywordId]; //플레이어가 출구 근처에 없거나 들고 있는 키워드가 없는 경우

    if (!exit || !keyword) return; //출구에 판정 요청 //키워드 판정이나 잠금해제 여부는 matchKeyword에서 해줌

    const result = exit.matchKeyword(keyword.text);

    if (result.matched) {
      //정답 키워드일 경우
      console.log(
        `[Correct] Player ${player.id} submitted correct keyword: ${keyword.text}`
      );
      this.score += 500; //[정답] 500점 추가
      delete this.keywords[holdingKeywordId];
    } else {
      console.log(
        `[Incorrect] Player ${player.id} submitted wrong keyword: ${keyword.text}. Deleting it.`
      );

      this.score -= 200; //[오답] -200점 감점
      // if (score <= 0) score = 0; //점수 0점 이하로 떨어지지 않게
      delete this.keywords[holdingKeywordId];
    } //플레이어의 키워드 홀딩 상태 해제

    player.holdingKeywordId = null;
  }
  // ================= ▲▲▲ 플레이어&키워드 상호작용 ▲▲▲ =================

  // ================= ▼▼▼ 몬스터 피격 ▼▼▼ =================
  monsterHitsPlayer(monsterId, playerId) {
    const player = this.players[playerId];
    const monster = this.monsters[monsterId];

    if (!player || !monster) return;

    if (player.invincibility <= 40) {
      return;
    }

    //피격이 확정일 땐 짧은 시간동안 무적 활성화
    player.invincibility = 0;

    if (player.hasShield) {
      player.hasShield = false;
      console.log(
        `[Player HIt] Player ${player.id}'s shield blocked an attack from monster ${monster.id}`
      );
      return;
    }

    console.log(
      `[Player Hit] Monster ${monster.type} ${monster.id} hit player ${player.id}`
    );

    //플레이어 hp 감소
    player.hp -= 1;
    console.log(`[HP Loss] Player ${player.id}'s HP is noe ${player.hp}`);

    if (player.holdingKeywordId) {
      //들고 있던 키워드 드랍
      const droppedKeyword = this.keywords[player.holdingKeywordId];

      if (droppedKeyword) {
        console.log(`Player dropped keyword ${droppedKeyword.text}`);
        droppedKeyword.carrierId = -1;

        //키워드 드랍 위치
        const maxDropDistance = 2.0;

        const offsetX = (Math.random() - 0.5) * 2 * maxDropDistance;
        const offsetY = (Math.random() - 0.5) * 2 * maxDropDistance;

        droppedKeyword.x = player.x + offsetX;
        droppedKeyword.y = player.y + offsetY;

        player.holdingKeywordId = null;
      }
    }

    //HP가 0이 되면 전투 불능.
    if (player.hp <= 0) {
      player.hp = 0;
      player.isDown = true;
      console.log(`[Player Down] Player ${player.id} is down.`);

      //전투불능 시 입력 0으로 만들기
      player.inputH = 0;
      player.inputV = 0;

      let allPlayersDown = true;

      //만약 모든 플레이어가 전투불능(isDown)상태라면:
      for (const id in this.players) {
        if (!this.players[id].isDown) {
          allPlayersDown = false;
          break;
        }
      }

      if (allPlayersDown) {
        console.log("[GAME OVER] All players are down.");
        this.isGameOver = true;
      }
    }
  }

  // ================= ▲▲▲ 몬스터 피격 ▲▲▲ =================

  checkForLevelUp() {
    //모든 플레이어 탈출 확인
    for (const id in this.players) {
      if (!this.players[id].isEscaped) {
        return;
      }
    }

    console.log("[Level] All players escaped! Proceeding to the next level.");

    const lastExitData = this.exit; //방금 클리어한 레벨의 출구 데이터

    //스테이지의 영문장 레벨에 따라 골드 획득량
    if (lastExitData && lastExitData.difficulty) {
      switch (lastExitData.difficulty) {
        case 1:
          this.gold += 30;
          break;
        case 2:
          this.gold += 50;
          break;
        case 3:
          this.gold += 70;
          break;
        case 4:
          this.gold += 90;
          break;
        case 5:
          this.gold += 110;
          break;
        default:
          this.gold += 0;
          break;
      }

      this.completedSentences.push({
        id: lastExitData.sentenceId,
        text: lastExitData.originalSentence,
        meaning: lastExitData.translation,
      });
    }

    this.gameLevel++;
    this.timeLimit += 15; //보너스 시간 30초 추가

    //다음 레벨 설정(levelManager에게 레벨 다시 세팅하게 시킴)
    setupLevel(this, this.gameLevel);

    //새 레벨을 위해 플레이어 상태 초기화
    const spawnArea = { minX: 1, maxX: 5, minY: 6, maxY: 12 };

    for (const id in this.players) {
      const player = this.players[id];
      player.isEscaped = false;
      player.holdingKeywordId = null;

      if (player.isBuffed) {
        player.isBuffed = false;
        player.speed = player.baseSpeed;
      }

      //플레이어 위치 리셋
      player.x =
        Math.random() * (spawnArea.maxX - spawnArea.minX) + spawnArea.minX;
      player.y =
        Math.random() * (spawnArea.maxY - spawnArea.minY) + spawnArea.minY;

      console.log(
        `[Level] Player ${id} spawned at (${player.x.toFixed(
          2
        )}, ${player.y.toFixed(2)})`
      );
    }
  }

  // ================= ▼▼▼ 게임 상태 데이터 패킹 ▼▼▼ =================
  getFullStatePacket() {
    //플레이어 패킷

    const playersPacket = {};

    for (const id in this.players) {
      playersPacket[id] = this.players[id].toPacket();
    } //키워드 패킷

    const keywordsPacket = {};

    for (const id in this.keywords) {
      keywordsPacket[id] = this.keywords[id].toPacket();
    }

    //몬스터 패킷 추가
    const monstersPacket = {};
    for (const id in this.monsters) {
      monstersPacket[id] = this.monsters[id].toPacket();
    }

    //출구 패킷
    const exitPacket = this.exit ? this.exit.toPacket() : null;

    //전체 게임 상태 패킷!
    return {
      type: "gameStateUpdate",
      payload: JSON.stringify({
        players: playersPacket,
        keywords: keywordsPacket,
        monsters: monstersPacket,
        exit: exitPacket,

        gameLevel: this.gameLevel,
        mapName: this.mapName,
        score: this.score,
        gold: this.gold,
        completedSentences: this.completedSentences,
        timeLimit: this.timeLimit,
        countdown: this.countdown,

        status: this.status,
        isGameOver: this.isGameOver,
      }),
    };
  }

  resetPlayerInputs() {
    for (const id in this.players) {
      this.players[id].inputH = 0;
      this.players[id].inputV = 0;
    }
  }
}
// ================= ▲▲▲ 게임 상태 데이터 패킹 ▲▲▲ =================

module.exports = { GameState };
