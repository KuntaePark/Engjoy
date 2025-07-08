/*
   server for game lobby.
*/
const WebSocket = require('ws')
const Physics = require('./physics.js')
const {makePacket} = require('../common/Packet.js')
const wss = new WebSocket.Server({ port: 7777 },()=>{
    console.log('LOBBY SERVER START')
})

const authorizedMap = new Map();
//웹 서버에서 인증받은 아이디들
const authByWebServer = new Set();

//lobbies
const perLobbyMax = 4;
const webServerId = 'WEBSERVER';

let lobbyIndexCount = 0;
const lobbies = {}; //lobby member infos

const idLength = 16; //temporal player id generation. will be substituted with id in database later.
const idSet = new Set();
const exitQueue = [];

const deltaTime = 0.016; //per frame time
const speed = 5.0; //character speed

class PlayerStateData {
    constructor(x,y) {
        this.x = x;
        this.y = y;

        this.isRunning = false;
    }
}


wss.on('connection', function connection(ws) {
   console.log('connection established.');

   ws.on('message', (data) => {
      //update location
      const {type, payload} = JSON.parse(data);
      
      const now = new Date(Date.now());
      console.log("[" + now.toUTCString()+"]" + " incoming message, type: ", type);
      
      (PacketHandler[type] || (()=> console.log("unknown packet type.")))(ws,payload);
   })
   
   ws.on('close', () =>{
      if(ws['id'] === webServerId || !ws['id']) return;

      const id = ws['id'];
      const lobbyId = ws['lobbyId'];
      console.log("connection closed on " + id +", in lobby "+ lobbyId);
      delete lobbies[lobbyId]["members"][id];
      lobbies[lobbyId].memberCount--;
      console.log("Current lobby member count of lobby "+ lobbyId + ": "+ lobbies[lobbyId].memberCount);
      if(lobbies[lobbyId].memberCount === 0) {
         console.log("closing lobby "+ lobbyId);
         delete lobbies[lobbyId];
      }
      else {
         exitQueue.push({playerId: id, lobbyId: lobbyId});
      }
   })
})

wss.on('listening',()=>{
   console.log('listening on 7777')
})

const PacketHandler = {
   'auth': (ws, payload) => {
      const id = payload;
      console.log(`auth key type ${typeof payload}`)
      if(webServerId === id) {
         //웹 서버 인증
         ws.send(makePacket('auth_success_server',''));
         console.log('web server connected');
         ws['id'] = id;
         authorizedMap.set(ws, true);
      }
      else if(authByWebServer.has(id)) {
         //client, allow match
         console.log(`user with id ${id} authenticated for lobby.`)
         authorizedMap.set(ws, true);
         //인증 완료됐으므로 웹 인증 목록에서 제거
         authByWebServer.delete(id);
         ws['id'] = id;
         const lobbyId = enterLobby(ws, id);
         ws['lobbyId'] = lobbyId;
         //Announce id to client
         ws.send(JSON.stringify({
            type: 'lobby_enter_success',
            payload: JSON.stringify({
               playerId: id,
               lobbyId: lobbyId
            })
         }));
      } else {
         //reject
         console.log('unauthorized');
         ws.send(makePacket('auth_reject', 'auth_unauthorized'));
         ws.close();
      }
   },
   'auth_allow': (ws, payload) => {
      if(ws['id'] !== webServerId) return; //웹 서버가 아닐 경우 차단
      else {
            console.log(`user with id ${payload} allowed for lobby.`)
            console.log(`auth_allow type ${typeof payload}`)
            authByWebServer.add(payload); //해당 아이디를 허가 명단에 추가
      }
   },
   'input_move': (ws, payload) => {
      //update position based on input
      id = ws['id'];
      const inputs = JSON.parse(payload);
      lobbies[lobbyId]["members"][id].inputH = inputs.x;
      lobbies[lobbyId]["members"][id].inputV = inputs.y;   
   }
};

//for each frame, 60fps
setInterval(() => {
   while(exitQueue.length !== 0) {
      //broadcast exit to lobby members
      const exitPlayer = exitQueue.pop();
      
      const lobbyId = exitPlayer.lobbyId;
      const id = exitPlayer.playerId;
      console.log("announcing exit player "+id+" in lobby "+lobbyId);

      if(!lobbies[lobbyId]) return;
      const lobbyMembers = lobbies[lobbyId]["members"];
      const packet = JSON.stringify({type: 'player_exit', payload: id })
      for(memberId in lobbyMembers) {
         const memberWs = lobbyMembers[memberId].socket;
         if(memberWs.readyState === WebSocket.OPEN) {
            memberWs.send(packet);
         }
      }
   }

   //broadcast member location to all members
   for(lobbyId in lobbies) {
      const members = lobbies[lobbyId]["members"];

      const updatedPositions = calculatePositions(members);

      // 전체 위치 브로드캐스트
      const packet = JSON.stringify({ type: 'player_update', payload: JSON.stringify(updatedPositions) });
      //console.log(packet);
      for(id in members) {
         memberWs = members[id].socket;
         if(memberWs.readyState === WebSocket.OPEN) {
            memberWs.send(packet);
         }
      }
   }
},deltaTime * 1000)

function enterLobby(ws, playerId) {
   //find empty lobby and add player.
   const newPlayer = {
      socket: ws,
      x: 0.0, 
      y: 0.0, 
      inputH: 0.0, 
      inputV: 0.0
   }

   for(lobbyId in lobbies) {
      const curLobby = lobbies[lobbyId];
      if(curLobby.memberCount < perLobbyMax) {
         curLobby["members"][playerId] = newPlayer;
         curLobby.memberCount++;
         return lobbyId;
      }
   }

   //if no empty lobby, create new
   const newLobbyId = lobbyIndexCount++;
   const newLobby = {
      id: newLobbyId,
      memberCount: 0,
      members: {}
   }

   newLobby.members[playerId] = newPlayer;
   newLobby.memberCount++;
   lobbies[newLobbyId] = newLobby;

   return newLobbyId;
}

function calculatePositions(players) {
   const updatedPositions = {};

   //position update
   for (let id in players) {
      const p = players[id];
      const dX = p.inputH * deltaTime * speed;
      const dY = p.inputV * deltaTime * speed;

      //마을 내 캐릭 충돌 비활성화
      //check collision
      // let colliding = false;
      // let isRunning = false;

      // for(let others in players) {
      //    if(others === id) continue;
         
      //    const o = players[others];
      //    const myLoc = new Physics.Vector2(p.x + dX, p.y + dY);
      //    const otherLoc = new Physics.Vector2(o.x, o.y);
         
      //    if(Physics.checkCollision(myLoc, otherLoc)) {
      //       console.log("colliding");
      //       colliding = true; break;
      //    }
      // }

      // if(!colliding)
      // {
      //    //update only when not colliding
      // }
      p.x += dX;
      p.y += dY;
      if(dX !== 0 || dY !== 0) {
         isRunning = true;
      }
      
      //out of boundary
      if(p.x >= 9 - Physics.colliderRadius) {
         p.x = 8.99 - Physics.colliderRadius;
      }
      if(p.x <= -9 + Physics.colliderRadius) {
         p.x = -8.99 + Physics.colliderRadius;
      }
      if(p.y >= 5 - Physics.colliderRadius) {
         p.y = 4.99 - Physics.colliderRadius;
      }
      if(p.y <= -5 + Physics.colliderRadius) {
         p.y = -4.99 + Physics.colliderRadius;
      }

      // console.log(p.x, p.y)
      updatedPositions[id] = new PlayerStateData(p.x, p.y);
      p.inputH = 0; p.inputV = 0;
   }
   return updatedPositions;
}