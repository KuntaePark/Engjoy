const Player = require("./player.js");
const { Keyword } = require("./keywords.js");
const Exit = require("./exit.js");
const { generateId } = require("./utils.js");
const { setupLevel } = require("./levelManager.js");
const Physics = require("./physics.js");

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
  } //새 플레이어 추가

  // ================= ▼▼▼ 플레이어 생성 & 삭제 ▼▼▼ =================
  addPlayer() {
    const idSet = new Set(Object.keys(this.players));
    const newPlayerId = generateId(idSet);
    const player = new Player(newPlayerId);
    this.players[newPlayerId] = player; //첫 플레이어 접속 시 키워드 스폰

    //첫 플레이어가 접속하면 게임 레벨 설정 (이후 매칭 및 대기방 구현 시 변경)
    if (Object.keys(this.players).length === 1) {
      setupLevel(this, 1);
    }

    console.log(`Player ${newPlayerId} connected`);
    return player;
  }

  playerInteracts(playerId) {
    const player = this.players[playerId];
    if (!player) return;

    //출구와 상호작용 가능상 상태인 경우 (키워드 홀딩 & 출구 근처)
    if (player.canInteractWithExit && player.holdingKeywordId) {
      this.playerSubmitsKeywordToExit(playerId);
    }
    //키워드를 들 수 있는 상태인 경우 (키워드 홀딩X & 키워드 근처)
    else if (!player.holdingKeywordId && player.interactableKeywordId) {
      this.playerHoldingKeywords(playerId);
    }
    //(키워드 들고 있지만 출구 근처가 아니라면 => do nothing)
  }

  removePlayer(playerId) {
    const player = this.players[playerId];

    if (!player) return; //플레이어가 키워드를 들고 있었다면 해당 키워드 드랍 처리

    if (player.holdingKeywordId) {
      const heldKeyword = this.keywords[player.holdingKeywordId];

      if (heldKeyword) {
        heldKeyword.carrierId = null;
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
  // ================= ▲▲▲ 플레이어 생성 & 삭제 ▲▲▲ =================

  // ================= ▼▼▼ 플레이어 이동 ▼▼▼ =================
  setPlayerInput(playerId, input) {
    if (this.players[playerId]) {
      this.players[playerId].inputH = input.x;

      this.players[playerId].inputV = input.y;
    }
  }
  // ================= ▲▲▲ 플레이어 이동 ▲▲▲ =================

  // ================= ▼▼▼ 플레이어 공격 ▼▼▼ =================
  playerAttacks(playerId) {
    const player = this.players[playerId];
    if (!player) return; //플레이어가 없으면 아무것도 안함

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
  }
  // ================= ▲▲▲ 플레이어 공격 ▲▲▲ =================

  // ================= ▼▼▼ 플레이어&키워드 상호작용 ▼▼▼ =================
  playerHoldingKeywords(playerId) {
    const player = this.players[playerId];

    if (!player) return; //플레이어 주변에 상호작용 가능한 키워드가 있는 경우 //상호작용 처리 -> 플레이어를 키워드 홀딩 상태로 전환

    if (player.interactableKeywordId) {
      const keywordId = player.interactableKeywordId;

      const keyword = this.keywords[keywordId]; //다른 사람이 키워드를 들고 있지 않은지 혹시 모르니 재차 확인 //(동시에 상호작용 버튼 눌렀을 때의 버그 방지)

      if (keyword && !keyword.carrierId) {
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

      delete this.keywords[holdingKeywordId];
    } else {
      console.log(
        `[Incorrect] Player ${player.id} submitted wrong keyword: ${keyword.text}. Deleting it.`
      );

      delete this.keywords[holdingKeywordId];
    } //플레이어의 키워드 홀딩 상태 해제

    player.holdingKeywordId = null;
  }
  // ================= ▲▲▲ 플레이어&키워드 상호작용 ▲▲▲ =================

  // ================= ▼▼▼ 몬스터 피격 ▼▼▼ =================
  monsterHitsPlayer(monsterId, playerId) {
    const player = this.players[playerId];

    if (!player || !player.holdingKeywordId) {
      return;
    }

    const monster = this.monsters[monsterId];
    console.log(
      `[Player Hit] MOnster ${monster.type} ${monster.id} hit player ${player.id}`
    );

    //들고 있던 키워드 드랍
    const droppedKeyword = this.keywords[player.holdingKeywordId];
    if (droppedKeyword) {
      console.log(`Player dropped keyword ${droppedKeyword.text}`);
      droppedKeyword.carrierId = null;
    }

    player.holdingKeywordId = null;
  }
  // ================= ▲▲▲ 몬스터 피격 ▲▲▲ =================

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
      }),
    };
  }
}
// ================= ▲▲▲ 게임 상태 데이터 패킹 ▲▲▲ =================

module.exports = { GameState };
