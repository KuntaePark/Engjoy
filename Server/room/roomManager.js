const { Room } = require("./room.js");
const MAX_PLAYERS_PER_ROOM = 4; //방의 최대 인원수

class RoomManager {
  constructor() {
    this.rooms = {};
    this.nextRoomId = 1;
  }

  //새 매칭룸 생성
  createRoom() {
    const roomId = this.nextRoomId++;
    const newRoom = new Room(roomId, this);
    this.rooms[roomId] = newRoom;

    newRoom.startGame();

    console.log(`[RoomManager] Room ${roomId} created.`);

    return newRoom;
  }

  //플레이어와 방 연결
  handleConnection(ws) {
    let roomToJoin = null;

    //꽉 차지 않은 방 찾기
    for (const roomId in this.rooms) {
      const room = this.rooms[roomId];
      if (Object.keys(room.players).length < MAX_PLAYERS_PER_ROOM) {
        roomToJoin = room;
        break;
      }
    }

    //방이 다 빵빵하면 새 방 만들기
    if (!roomToJoin) {
      roomToJoin = this.createRoom();
    }

    //방에 새 플레이어 추가
    const player = roomToJoin.addPlayer(ws);
    ws.playerId = player.id;
    ws.roomId = roomToJoin.id;

    ws.send(JSON.stringify({ type: "playerId", payload: player.id }));
  }

  //특정 방을 ID 로 찾기
  getRoom(roomId) {
    return this.rooms[roomId];
  }
  
  removeRoom(roomId) {
    console.log(`removing room ${roomId}.`);
    delete this.rooms[roomId];
  }
}

module.exports = { RoomManager };
