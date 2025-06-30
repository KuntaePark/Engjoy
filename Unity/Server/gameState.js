const Player = require("./player.js");
const { Keyword, spawnKeywords } = require("./keywords.js");
const Exit = require("./exit.js");
const { generateId } = require("./utils.js");

class GameState {
  constructor() {
    this.players = {};
    this.keywords = {};
    this.exits = {};
    this.config = {
      maxKeywords: 5,
    };
  }

  //새 플레이어 추가
  addPlayer() {
    const idSet = new Set(Object.keys(this.players));
    const newPlayerId = generateId(idSet);
    const player = new Player(newPlayerId);
    this.players[newPlayerId] = player;

    //첫 플레이어 접속 시 키워드 스폰
    if (Object.keys(this.players).length === 1) {
      spawnKeywords(this, this.config.maxKeywords);
    }
    console.log(`Player ${newPlayerId} connected`);
    return player;
  }

  removePlayer(playerId) {
    const player = this.players[playerId];
    if (!player) return;

    //플레이어가 키워드를 들고 있었다면 해당 키워드 드랍 처리
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
    if (!player) return;

    //플레이어 주변에 상호작용 가능한 키워드가 있는 경우
    //상호작용 처리 -> 플레이어를 키워드 홀딩 상태로 전환
    if (player.interactableKeywordId) {
      const keywordId = player.interactableKeywordId;
      const keyword = this.keywords[keywordId];

      //다른 사람이 키워드를 들고 있지 않은지 혹시 모르니 재차 확인
      //(동시에 상호작용 버튼 눌렀을 때의 버그 방지)
      if (keyword && !keyword.carrierId) {
        player.holdingKeywordId = keywordId;
        keyword.carrierId = playerId;
        console.log(
          `Player ${playerId} is interacting with keyword ${keywordId}`
        );
      }
    }
  }

  getFullStatePacket() {
    //플레이어 패킷
    const playersPacket = {};
    for (const id in this.players) {
      playersPacket[id] = this.players[id].toPacket();
    }

    //키워드 패킷
    const keywordsPacket = {};
    for (const id in this.keywords) {
      keywordsPacket[id] = this.keywords[id].toPacket();
    }

    //전체 게임 상태 패킷!
    return {
      type: "gameStateUpdate",
      payload: JSON.stringify({
        players: playersPacket,
        keywords: keywordsPacket,
      }),
    };
  }
}

module.exports = { GameState };
