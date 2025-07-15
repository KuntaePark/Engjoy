const WebSocket = require('ws')
const {makePacket} = require('../common/Packet')
const Physics = require('./physics')

const perLobbyMax = 4;
const lobbies = new Map(); //lobbyId : Lobby

const deltaTime = 0.016; //per frame time
const speed = 5.0; //character speed

class Player {
    constructor(ws) {
        this.id = ws['id'];
        this.socket = ws;
        
        this.x = 0; //x 위치
        this.y = 0;  //y 위치

        this.inputH = 0; //horizontal
        this.inputV = 0; //vertical
        
        this.isAttacking = false; //애니메이션용 플래그
        
        this.bodyTypeIndex = 0; //몸통 커마 인덱스
        this.weaponTypeIndex = 0; //무기 커마 인덱스
    }

    toJSON() {
        return {
            x: this.x,
            y: this.y,
            isAttacking: this.isAttacking,
            bodyTypeIndex: this.bodyTypeIndex,
            weaponTypeIndex: this.weaponTypeIndex,
        }
    }
}

class Lobby {
    static lobbyIndexCount = 0;

    constructor() {
        this.id = Lobby.lobbyIndexCount++;
        this.members = {}; //멤버 저장 오브젝트 id: player
        this.memberCount = 0;

        this.exitQueue = []; //나간 사람 목록. 다음 프레임에 알림

        this.intervalId = null;
    }

    startLobby() {
        this.intervalId = setInterval(() => this.tick(), deltaTime * 1000);
    }

    tick() {
        //로비가 비어있을 경우 해산
        if(this.memberCount === 0) {
            //해산
            console.log('closing lobby...')
            this.close();
            return;
        }

        //나간 사람 있을 시 먼저 처리
        while(this.exitQueue.length !== 0) {
            const id = this.exitQueue.pop();
            const packet = makePacket('player_exit', id);
            for(const memberId in this.members) {
                const ws = this.members[memberId].socket;
                if(ws.readyState === WebSocket.OPEN) {
                    ws.send(packet);
                }
            }
        }

        //인풋에 따른 위치 계산
        this.calculatePositions();

        this.broadcast(makePacket('player_update', this.members));

        //트리거 초기화
        this.clearTrigger();
    }

    clearTrigger() {
        for(const id in this.members) {
            const player = this.members[id];
            player.isAttacking = false;
        }
    }

    broadcast(message) {
        for(const id in this.members) {
            const ws = this.members[id].socket;
            if(ws.readyState === WebSocket.OPEN) {
                ws.send(message);
            }
        }
    }

    calculatePositions() {
        //position update
        for (let id in this.members) {
            const p = this.members[id];
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
            
    
            // console.log(p.x, p.y)
            p.inputH = 0; p.inputV = 0;
        }
    }

    addMember(player) {
        this.members[player.id] = player;
        this.memberCount++;
        console.log(`member ${player.id} joined lobby ${this.id}`);
        console.log(`member Count: ${this.memberCount}`);
    }

    exitMember(id) {
        delete this.members[id];
        this.memberCount--;
        this.exitQueue.push(id);
        console.log("Current lobby member count of lobby "+ this.id + ": "+ this.memberCount);      
    }

    close() {
        clearInterval(this.intervalId);
        lobbies.delete(this.id);
    }
}

function joinLobby(ws, authData) {
    const player = new Player(ws);
    console.log(authData);
    player.bodyTypeIndex = authData.bodyTypeIndex;
    player.weaponTypeIndex = authData.weaponTypeIndex;

    for(const [lobbyId, lobby] of lobbies) {
        console.log(`lobby ${lobbyId}: ${lobby.memberCount}`);
        if(lobby.memberCount < perLobbyMax) {
            lobby.addMember(player);
            return lobbyId;
        }
    }

    //if no empty lobby, create new
    const newLobby = new Lobby();
    newLobby.addMember(player);
    lobbies.set(newLobby.id, newLobby);
    newLobby.startLobby();

    return newLobby.id;
}

module.exports = {Lobby, lobbies, joinLobby}