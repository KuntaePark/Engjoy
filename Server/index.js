const WebSocket = require("ws");
const { RoomManager } = require("./room/roomManager.js");
const { startGameUpdate } = require("./gameSetting/gameUpdate.js");

const wss = new WebSocket.Server({ port: 7777 }, () => {
  console.log("✅ Server started on port 7777 ✅ ");
});

const roomManager = new RoomManager();

wss.on("connection", (ws) => {
  console.log("[Connection] A new client has connected.");
  roomManager.handleConnection(ws);

  ws.on("message", (data) => {
    const message = JSON.parse(data);
    const room = roomManager.getRoom(ws.roomId);
    if (!room) return; //방이 없으면 돌아가세요..

    switch (message.type) {
      case "ready":
        room.gameState.setPlayerReady(
          ws.playerId,
          JSON.parse(message.payload).isReady
        );
        break;
      case "input":
        //MATCHINGROOM 상태일 때 입력 처리
        const inputs = JSON.parse(message.payload);
        room.gameState.setPlayerInput(ws.playerId, inputs);
        break;
      //상호작용
      case "interact":
        room.gameState.playerInteracts(ws.playerId);
        break;
      //공격
      case "playerAttack":
        room.gameState.playerAttacks(ws.playerId);
        break;
      //아이템 사용
      case "useItem":
        const itemData = JSON.parse(message.payload);
        room.gameState.playerUseItem(ws.playerId, itemData.itemType);
        break;
    }
  });

  //연결 종료 핸들러
  ws.on("close", () => {
    const room = roomManager.getRoom(ws.roomId);
    if (room) {
      room.removePlayer(ws.playerId);
    }
  });
});
