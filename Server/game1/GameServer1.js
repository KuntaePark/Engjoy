/*
 * 게임 1 전용 서버.
 */

const WebSocket = require('ws');
const {Session, sessions} = require('./Session');
const {makePacket} = require('../common/Packet');

const wss = new WebSocket.Server({ port: 7778 },()=>{
    console.log('GAME1 SERVER START ON 7778');
});

//매칭 서버 아이디
const matchServerId = 'MATCHSERVER';

//허가된 유저 저장, id - sessionId
const authorizedMap = new Map();



//임시 매칭 큐. 매칭 서버 구현 이후 제거.
const playerQueue = [];
const idSet = new Set();
const idLength = 16;

//매칭 서버에 의해 해당 유저들의 세션이 이미 생성되었다고 가정
//todo - 매칭 서버에 의한 세션 생성 요청 및 수락 구현
wss.on('connection', function connection(ws) {
    ws.on('message', (data) => {
        const {type, payload} = JSON.parse(data);
        // console.log(`[${UTCTimeStamp()}] incoming message, type: ${type}`);
        (PacketHandler[type] || (() => console.log('unknown packet type')))(ws, payload);
    })

    ws.on('close', () => {
        console.log("connection closed");
        //todo - 세션 퇴장 처리 필요
        if(ws.id !== matchServerId) {
            //세션에서 해당 플레이어 제거
            const sessionId = ws['sessionId'];
            session.close();
        }
        
    })
});


//세션 생성 함수. 해당 아이디들에 대해 세션 생성, 연결은 별도 지정
function createSession(id1, id2) {
    const session = new Session(id1, id2);
    sessions.set(session.id, session);
    console.log(`session ${session.id} created`);
    return session.id;
}

//수신 패킷 처리
const PacketHandler = {
    'auth': (ws, payload) => {
        const data = JSON.parse(payload);
        const id = data.id;
        if(matchServerId === id) {
            console.log('match server connected.');
            ws['id'] = matchServerId;
            ws.send(makePacket('auth_success', ''));
            matchWs = ws;
        }else if(authorizedMap.has(id)) {
            //세션에 배정
            console.log(`received auth request from user. ${id}`)
            const session = sessions.get(authorizedMap.get(id));
            ws['id'] = id;
            ws['sessionId'] = session.id;
            session.setPlayerConnection(id, ws);
        } else {
            console.log(`auth rejected of ${id}`);
            ws.send(makePacket('auth_reject','auth_unauthorized'));
            ws.close();
        }
    },

    'create_session': (ws,payload) => {
        if(ws['id'] !== matchServerId) return;
        //payload의 아이디로 세션 생성
        console.log('session creation request.');
        const {ids} = JSON.parse(payload);
        const id1 = Number(ids[0]);
        const id2 = Number(ids[1]);
        const sessionId = createSession(id1, id2);
        for(const id of ids) {
            authorizedMap.set(Number(id), sessionId);
            console.log(`${id} added to auth map`);
        }
    },

    'input' : (ws, payload) => {
        if(ws.sessionId) handleInput(ws, payload);
    }
}

//인풋 처리
function handleInput(ws, payload) {
    const playerId = ws.id;
    const sessionId = ws.sessionId;
    // console.log(`input by playerId ${playerId} in session ${sessionId}`)
    const player = sessions.get(sessionId).getPlayer(playerId);
    const inputData = JSON.parse(payload);
    player.setInput(inputData);

}

//임시 아이디 생성 함수.
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

//********기타 편의성 함수*********//
function UTCTimeStamp() {return new Date(Date.now()).toUTCString();}
