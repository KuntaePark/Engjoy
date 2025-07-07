/*
   server for game lobby.
*/
const WebSocket = require('ws')
const Physics = require('./physics.js')
const wss = new WebSocket.Server({ port: 7777 },()=>{
    console.log('LOBBY SERVER START')
})

//lobbies
const perLobbyMax = 4;
let lobbyIndexCount = 0;
const lobbies = {}; //lobby member infos

const idLength = 16; //temporal player id generation. will be substituted with id in database later.
const idSet = new Set();
const exitQueue = [];

const deltaTime = 0.016; //per frame time
const speed = 5.0; //character speed

wss.on('connection', function connection(ws) {
   console.log('connection established.');
   
   //generate id
   const id = generateId();
   
   //add to lobby
   const lobbyId = enterLobby(ws, id);
   ws["playerId"] = id;
   console.log("id: " + id + ",lobbyId: "+ lobbyId);
   console.log("Current lobby member count: "+ lobbies[lobbyId].memberCount);


   //Announce id to client
   ws.send(JSON.stringify({
      type: 'lobbyEnterSuccess',
      payload: JSON.stringify({
         playerId: id,
         lobbyId: lobbyId
      })
   }));

   ws.on('message', (data) => {
      //update location
      const dataobj = JSON.parse(data);
      
      const now = new Date(Date.now());
      //console.log("[" + now.toUTCString()+"]" + " incoming message, type: ", dataobj.type);

      switch(dataobj.type) {
         case "inputMove":
            //update position based on input
            const inputs = JSON.parse(dataobj.payload);
            lobbies[lobbyId]["members"][id].inputH = inputs.x;
            lobbies[lobbyId]["members"][id].inputV = inputs.y;   
            break;
         case "inputInteract":
            //interact input
            break;
         default:
            console.log('Unknown packet type');
            break;
      }
   })
   
   ws.on('close', () =>{
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
      const packet = JSON.stringify({type: 'playerExit', payload: id })
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
      const packet = JSON.stringify({ type: 'playerUpdate', payload: JSON.stringify(updatedPositions) });
      //console.log(packet);
      for(id in members) {
         memberWs = members[id].socket;
         if(memberWs.readyState === WebSocket.OPEN) {
            memberWs.send(packet);
         }
      }
   }
},deltaTime * 1000)

function generateId() {
   const chars = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
   
   while(true)
   {
      //simple id redundancy checking. should use UUID or player login id later.
      let result = '';
      for (let i = 0; i < idLength; i++) {
         result += chars[Math.floor(Math.random() * chars.length)];
      }

      if(!idSet.has(result))
      {
         idSet.add(result);
         return result;
      }
   }
}

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
      const newX = p.x + p.inputH * deltaTime * speed;
      const newY = p.y + p.inputV * deltaTime * speed;

      //check collision
      let colliding = false;

      for(let others in players) {
         if(others === id) continue;
         
         const o = players[others];
         const myLoc = new Physics.Vector2(newX, newY);
         const otherLoc = new Physics.Vector2(o.x, o.y);
         
         if(Physics.checkCollision(myLoc, otherLoc)) {
            console.log("colliding");
            colliding = true; break;
         }
      }

      if(!colliding)
      {
         //update only when not colliding
         p.x = newX;
         p.y = newY;
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
      updatedPositions[id] = { x: p.x, y: p.y };
      p.inputH = 0; p.inputV = 0;
   }

   return updatedPositions;
}