const { GameState } = require("../gameSetting/gameState.js");
const { startGameUpdate } = require("../gameSetting/gameUpdate.js");
const { setupLevel, loadMap } = require("../gameSetting/levelManager.js");

class Room {
  constructor(id) {
    this.id = id;
    this.players = {};
    this.status = "MATCHINGROOM"; //'MATCHINGROOM', 'PLAY'
    this.countdown = -1; //게임 시작 전 카운트다운 변수

    //매칭룸 상태에서의 플레이어 데이터
    this.matchingRoomPlayers = {};

    //게임 시작 시 사용
    this.gameState = new GameState();
    this.gameState.status = "MATCHINGROOM";

    loadMap(this.gameState, "matchingroom");

    this.gameUpdateInterval = null;
  }

  // ================= ▼▼▼ 플레이어 생성 & 삭제 ▼▼▼ =================
  addPlayer(ws) {
    const player = this.gameState.addPlayer(ws.id);
    this.players[player.id] = ws;
    return player;
  }

  //플레이어 제거
  removePlayer(playerId) {
    delete this.players[playerId];

    if (this.gameState && this.gameState.players[playerId]) {
      this.gameState.removePlayer(playerId);
    }
    console.log(`[Room-${this.id}] Player ${playerId} left.`);
  }
  // ================= ▲▲▲ 플레이어 생성 & 삭제 ▲▲▲ =================
  // ================= ▼▼▼ 게임 시작 ▼▼▼ =================
  startGame() {
    this.gameUpdateInterval = startGameUpdate(
      this.gameState,
      this.broadcast.bind(this)
    );

    console.log(`[Room-${this.id}] Game Update Started.`);
  }
  // ================= ▲▲▲ 게임 시작 ▲▲▲ =================

  broadcast(message) {
    const messageString =
      typeof message === "string" ? message : JSON.stringify(message);
    for (const playerId in this.players) {
      const ws = this.players[playerId];
      ws.send(messageString);
    }
  }
}

module.exports = { Room };
