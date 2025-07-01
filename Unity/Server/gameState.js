const Player = require("./player.js");
const { Keyword } = require("./keywords.js");
const Exit = require("./exit.js");
const { generateId } = require("./utils.js");
const { setupLevel } = require("./levelManager.js");

class GameState {
  constructor() {
    this.players = {};
    this.keywords = {};
    this.exit = null; //출구는 단일객체로 관리
  } //새 플레이어 추가

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
  }

  setPlayerInput(playerId, input) {
    if (this.players[playerId]) {
      this.players[playerId].inputH = input.x;

      this.players[playerId].inputV = input.y;
    }
  }

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

    const result = exit.matchKeyword(holdingKeywordId);

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

  getFullStatePacket() {
    //플레이어 패킷

    const playersPacket = {};

    for (const id in this.players) {
      playersPacket[id] = this.players[id].toPacket();
    } //키워드 패킷

    const keywordsPacket = {};

    for (const id in this.keywords) {
      keywordsPacket[id] = this.keywords[id].toPacket();
    } //출구 패킷

    const exitPacket = this.exit ? this.exit.toPacket() : null; //전체 게임 상태 패킷!

    return {
      type: "gameStateUpdate",
      payload: JSON.stringify({
        players: playersPacket,
        keywords: keywordsPacket,
        exit: exitPacket,
      }),
    };
  }
}

module.exports = { GameState };
