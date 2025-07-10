/*
   server for game lobby.
*/
const WebSocket = require('ws')
const Physics = require('./physics.js')
const {makePacket} = require('../common/Packet.js')
const {Lobby, joinLobby, lobbies} = require('./Lobby.js')
const wss = new WebSocket.Server({ port: 7777 },()=>{
    console.log('LOBBY SERVER START')
})

const TEST = true;

const authorizedMap = new Map();
//웹 서버에서 인증받은 아이디들
const authByWebServer = new Map();

const webServerId = 'WEBSERVER';

wss.on('connection', function connection(ws) {
   console.log('connection established.');

   ws.on('message', (data) => {
      //update location
      const {type, payload} = JSON.parse(data);
      
      const now = new Date(Date.now());
      // console.log("[" + now.toUTCString()+"]" + " incoming message, type: ", type);
      
      (PacketHandler[type] || (()=> console.log("unknown packet type.")))(ws,payload);
   })
   
   ws.on('close', () =>{
      if(ws['id'] === webServerId || !ws['id']) return;

      const id = ws['id'];
      const lobby = lobbies.get(ws.lobbyId);
      console.log("connection closed on " + id +", in lobby "+ ws.lobbyId);
      lobby.exitMember(id);
   })
})

wss.on('listening',()=>{
   console.log('listening on 7777')
})

const PacketHandler = {
   'auth': (ws, payload) => {
      const authData = JSON.parse(payload);
      const id = authData.id;
      console.log(`auth key type ${typeof id}`)
      if(webServerId === id) {
         //웹 서버 인증
         ws.send(makePacket('auth_success_server',''));
         console.log('web server connected');
         ws['id'] = id;
         authorizedMap.set(ws, true);
      }
      else if(authByWebServer.has(id)) {
         //client, allow match
         console.log(`user with id ${id} authenticated for lobby.`);
         authorizedMap.set(ws, true);
         //인증 완료됐으므로 웹 인증 목록에서 제거
         ws['id'] = id;
         data = authByWebServer.get(id);
         if(!(TEST && id === '0')) {
            authByWebServer.delete(id);
         }
         const lobbyId = joinLobby(ws, data);
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
            const data = JSON.parse(payload);
            console.log(`user with id ${data.id} allowed for lobby.`)
            console.log(`auth_allow type ${typeof data.id}`)
            console.log(`body index: ${data.bodyTypeIndex}`);
            console.log(`weapon index: ${data.weaponTypeIndex}`);

            authByWebServer.set(data.id, data); //해당 아이디를 허가 명단에 추가
      }
   },
   'customization_update': (ws, payload) => {
      if(ws['id'] !== webServerId) return; //웹 서버가 아닐 경우 차단
      else {
         const data = JSON.parse(payload);
         wss.clients.forEach((ws) => {
            if(ws['id'] === data.id) {
               const lobby = lobbies.get(ws['lobbyId']);
               const player = lobby.members[data.id];
               player.bodyTypeIndex = data.bodyTypeIndex;
               player.weaponTypeIndex = data.weaponTypeIndex;
            }
         })
      }
   },
   'input_move': (ws, payload) => {
      //update position based on input
      const id = ws['id'];
      const lobbyId = ws['lobbyId'];
      const inputs = JSON.parse(payload);
      const lobby = lobbies.get(lobbyId);
      
      lobby.members[id].inputH = inputs.x;
      lobby.members[id].inputV = inputs.y;
   },
   'input_interact': (ws, _) => {
      const id = ws['id'];
      const lobbyId = ws['lobbyId'];
      const lobby = lobbies.get(lobbyId);
      lobby.members[id].isAttacking = true;
   }
};

