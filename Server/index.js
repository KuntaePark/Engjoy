const WebSocket = require("ws");
const { RoomManager } = require("./room/roomManager.js");
const { startGameUpdate } = require("./gameSetting/gameUpdate.js");
const { makePacket } = require("./common/Packet.js");

const wss = new WebSocket.Server({ port: 7780 }, () => {
  console.log("✅ Server started on port 7780 ✅ ");
});

const matchServerId = 'MATCHSERVER';

let matchWs = null;

//허가된 유저 저장, id - sessionId
const authorizedMap = new Map();

const roomManager = new RoomManager();

const PacketHandler = {
  'auth': (ws, payload) => {        
    const data = JSON.parse(payload);
    const id = data.id;
    console.log(`${typeof id}`)
    if(matchServerId === id) {
        console.log('match server connected.');
        ws['id'] = matchServerId;
        ws.send(makePacket('auth_success', ''));
        matchWs = ws;
    }else if(authorizedMap.has(id)) {
      console.log(`received auth request from user. ${id}`)
      //세션에 배정
      ws.id = id;
      roomManager.handleConnection(ws);
    } else {
      console.log(`auth rejected of ${id}`);
      ws.send(makePacket('auth_reject','auth_unauthorized'));
      ws.close();
    }
  },

  'auth_allow': (ws, payload) => {
    if(ws.id !== matchServerId) return;
    console.log(`auth allowed for ${ws.id}`)
    const data = JSON.parse(payload);
    const id = data.id;
    console.log(`${typeof id}`)
    authorizedMap.set(Number(id), true);
  },

  'ready': (ws, payload) => {
    const room = roomManager.getRoom(ws.roomId);
    if (!room) return; //방이 없으면 돌아가세요..
    room.gameState.setPlayerReady(
      ws.playerId,
      JSON.parse(payload).isReady
    );
  },

  'input': (ws, payload) => {
    const room = roomManager.getRoom(ws.roomId);
    if (!room) return; //방이 없으면 돌아가세요..
    //MATCHINGROOM 상태일 때 입력 처리
    const inputs = JSON.parse(payload);
    room.gameState.setPlayerInput(ws.playerId, inputs);
  },

  'interact': (ws, payload) => {
    const room = roomManager.getRoom(ws.roomId);
    if (!room) return; //방이 없으면 돌아가세요..
    room.gameState.playerInteracts(ws.playerId);
  },

  'playerAttack': (ws, payload) => {
    const room = roomManager.getRoom(ws.roomId);
    if (!room) return; //방이 없으면 돌아가세요..
    room.gameState.playerAttacks(ws.playerId);
  }, 
  'useItem': (ws, payload) => {
    const room = roomManager.getRoom(ws.roomId);
    if (!room) return; //방이 없으면 돌아가세요..
    const itemData = JSON.parse(payload);
    room.gameState.playerUseItem(ws.playerId, itemData.itemType);
  }
};

wss.on("connection", (ws) => {
  console.log("[Connection] A new client has connected.");

  ws.on("message", (data) => {
    const message = JSON.parse(data);
    
    // console.log(message);
    // const room = roomManager.getRoom(ws.roomId);
    // if (!room) return; //방이 없으면 돌아가세요..
    const packetHandler = PacketHandler[message.type];
    if(!packetHandler) {
      ws.send(makePacket('auth_reject', 'auth_unauthorized'));
    }
    packetHandler(ws, message.payload);
  });

  //연결 종료 핸들러
  ws.on("close", () => {
    const room = roomManager.getRoom(ws.roomId);
    if (room) {
      room.removePlayer(ws.playerId);
    }
  });
});

