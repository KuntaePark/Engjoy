const { GameState } = require("../gameSetting/gameState.js");
const { update, deltaTime } = require("../gameSetting/gameUpdate.js");
const { setupLevel, loadMap } = require("../gameSetting/levelManager.js");
const UserDataDB = require('../common/UserDataDB');

const userDataDB = new UserDataDB();


class Room {
  constructor(id, owner) {
    this.id = id;
    this.players = {};
    this.status = "MATCHINGROOM"; //'MATCHINGROOM', 'PLAY'
    this.countdown = -1; //게임 시작 전 카운트다운 변수
    this.owner = owner;

    //게임 시작 시 사용
    this.gameState = new GameState();
    this.gameState.status = "MATCHINGROOM";

    loadMap(this.gameState, "matchingroom");

    this.gameUpdateInterval = null;
  }

  // ================= ▼▼▼ 플레이어 생성 & 삭제 ▼▼▼ =================
  addPlayer(ws) {
    const id = ws['id'];
    //db에서 정보 불러오기
    return userDataDB.getUserData(id).then((data) => {
        if(data) {
            console.log(`User data loaded for id ${id}`);
            console.log(data);
            const player = this.gameState.addPlayer(id, data);
            this.players[player.id] = ws;
            return player;
          } else {
            throw new Error(`no user data found for id ${id}`);
          }
        }).catch((err) => {
          throw new Error(`Failed to load user data for id ${id}: ${err.message}`);
        });
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
    this.gameUpdateInterval = this.startGameUpdate();

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

  startGameUpdate() {
    const gameState = this.gameState;
    const intervalId = setInterval(() => {
      //게임 로직 업데이트 실행!
  
      if (Object.keys(gameState.players).length === 0) {
        console.log(`game end. close room.`);
        this.close();
        return;
      }
      if(gameState.isGameOver) {
        console.log(`game end by Game Over.`);
        //정상 게임 종료 시 사용 데이터 저장 및 골드 갱신
        const ids = gameState.completedSentences.map(sentence => sentence.id);
        Object.values(gameState.players).forEach((player) => {
          const id = player.id;
          userDataDB.saveUsedExpressions(id, ids);
          const newGold = player.gold + gameState.gold;
          const gameScore = gameState.score > player.game2HighScore ? gameState.score : player.game2HighScore;
          userDataDB.saveGame2Result(gameScore, newGold, id);
        })
        
        this.close();
        return;
      }
  
      update(gameState); //업데이트된 전체 상태를 모든 클라이언트에게 브로드캐스트!
  
      const packet = gameState.getFullStatePacket();
      this.broadcast(JSON.stringify(packet));
  
      gameState.resetPlayerInputs();
    }, deltaTime * 1000);
    return intervalId;
  }

  close() {
    clearInterval(this.gameUpdateInterval);
    this.owner.removeRoom(this.id);
  }
}

module.exports = { Room };
