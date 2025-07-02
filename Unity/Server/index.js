const WebSocket = require("ws");

const { GameState } = require("./gameState.js");

const { startGameUpdate } = require("./gameUpdate.js");

const { setupLevel } = require("./levelManager.js");

const wss = new WebSocket.Server({ port: 7777 }, () => {
  console.log("Server started on port 7777");
});

const gameState = new GameState();

//모든 클라이언트들에게 메시지 보내는 함수

function broadcast(message) {
  wss.clients.forEach((client) => {
    if (client.readyState === WebSocket.OPEN) {
      client.send(message);
    }
  });
}

wss.on("connection", (ws) => {
  const player = gameState.addPlayer();
  const playerId = player.id;

  ws.playerId = playerId; //ws객체에 playerId 저장

  console.log(`[Connection] Player ${playerId} has connected.`); //클라이언트(각 플레이어)에게 ID 알려주기

  ws.send(JSON.stringify({ type: "playerId", payload: playerId }));
  console.log(`[Message Sent] Sent 'yourId' to ${playerId}.`); //메시지 핸들러

  ws.on("message", (data) => {
    const message = JSON.parse(data);

    switch (message.type) {
      case "input":
        const inputs =
          typeof message.payload === "string"
            ? JSON.parse(message.payload)
            : message.payload;
        gameState.setPlayerInput(ws.playerId, inputs);
        break;

      case "interact":
        gameState.playerInteracts(ws.playerId);
        break;

      case "playerAttack":
        gameState.playerAttacks(ws.playerId);
        break;
    }
  });

  //연결 종료 핸들러
  ws.on("close", () => {
    gameState.removePlayer(ws.playerId);
  });
});

startGameUpdate(gameState, broadcast);

console.log("✅ GameUpdate started.");
