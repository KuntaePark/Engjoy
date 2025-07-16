const WebSocket = require('ws');
const {makePacket} = require('../common/Packet');
const {Matcher, ServerSocket} = require('./Matcher');

const gameServerAddr = ['ws://localhost:7778', 'ws://localhost:7780'];

const wss = new WebSocket.Server({port: 7779}, () => {
    console.log('MATCH SERVER STARTED AT 7779');
})

/* 웹 서버 고유 아이디, 임시 아이디 적용 */
const webServerId = 'WEBSERVER';

const matcher = new Matcher(gameServerAddr[0]);

const game2Socket = new ServerSocket(gameServerAddr[1]);

const authorizedMap = new Map();

//웹 서버에서 인증받은 아이디들
const authByWebServer = new Map();

wss.on('connection', function connection(ws) {

    authorizedMap.set(ws, false);

   const authTimeout = setTimeout(() => {
        if(!authorizedMap.get(ws)) {
            console.log('Unauthorized: timeout');
            ws.send(makePacket('auth_reject','auth_timeout'));  
            ws.close();
        }
   }, 5000);
    
    ws.on('message', (message) => {
        //message
        const {type, payload} = JSON.parse(message);
        console.log(`received packet ${type} from id ${ws['id'] ? ws['id'] : 'unknown'}`);
        (PacketHandler[type] || (() => {console.log("error: unknown type.");}))(ws, payload);
    }); 

    ws.on('close', () => {
        //close
        console.log('connection closed.');
        if(authorizedMap.has(ws)) {
            //접속 종료 시 권한 제거
            authorizedMap.delete(ws);
        }
        if(authTimeout) {
            clearTimeout(authTimeout);
        }
    });
});

const PacketHandler = {
    'auth': (ws, payload) => {
        /*
            {type: 'auth', payload: {id: [id], gameId: [gameId]}
         */
        const authData = JSON.parse(payload);
        console.log(authData);
        const id = authData.id;
        console.log(`id type ${typeof id}`)
        if(webServerId === id) {
            //웹 서버 인증
            ws.send(makePacket('auth_success_server',''));
            console.log('web server connected');
            ws['id'] = id;
            authorizedMap.set(ws, true);
        }
        else if(authByWebServer.has(id)) {
            const gameId = authByWebServer.get(id);

            //client, allow match
            console.log(`user with id ${id} authenticated for match.`)
            authorizedMap.set(ws, true);
            //인증 완료됐으므로 웹 인증 목록에서 제거
            authByWebServer.delete(id);
            ws['id'] = id;
            if(gameId === 0) {
                //game 1
                console.log("game 1 match.");
                matcher.findMatch(ws);
            }
            else {
                //game 2
                console.log("game 2 match.");
                game2Socket.send(makePacket('auth_allow', {id}));
                // ** 주의 ** 게임 서버 쪽 허용이 종료되기 전에 접속할 가능성 있음
                ws.send(makePacket('match_success', 1));
            }

        } else {
            //reject
            ws.send(makePacket('auth_reject', 'auth_unauthorized'));
            ws.close();
        }
    },
    'auth_allow': (ws, payload) => {
        if(ws['id'] !== webServerId) return; //웹 서버가 아닐 경우 차단
        else {
            const data = JSON.parse(payload);
            console.log(`user with id ${data.id} for game ${data.gameId} allowed to match.`)
            authByWebServer.set(Number(data.id), Number(data.gameId)); //해당 아이디를 허가 명단에 추가
        }
    },
    'match_cancel' : (ws, _) => {
        matcher.cancelMatch(ws);
    }
}

