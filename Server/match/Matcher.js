const WebSocket = require('ws');
const {makePacket} = require('../common/Packet');
const { json } = require('express');

const matchServerId = 'MATCHSERVER';
const gameServerAddr = 'ws://localhost:7778';

class Matcher {
    constructor() {
        this.matchQueue = [];
        this.game1ws = this.connectToGameServer();
    }

    connectToGameServer() {
        //게임 서버 연결
        const game1ws = new WebSocket(gameServerAddr);

        game1ws.on('open',() => {
            console.log('connection to game 1 server successful.');
            game1ws.send(makePacket('auth',{id: matchServerId}));
        });

        game1ws.on('message',(message) => {
            const {type, payload} = JSON.parse(message);
        });

        game1ws.on('close', (code, reason) => {
            console.log(`connection to game server has been closed.`);
            this.tryReconnect();
        });

        game1ws.on('error', (err) => {
            console.log(`error occurred while connecting to game server.`);
        });

        return game1ws;
    }


    //연결 종료 시 5초마다 연결 재시도
    tryReconnect() {
        console.log(`try reconnect in 5 seconds...`)
        setTimeout(() => {
            console.log(`trying reconnect...`);
            this.game1ws = this.connectToGameServer();
        }, 5000);
    }

    findMatch(ws) {
        //플레이어를 매칭 큐에 추가.
        //임시 매칭으로 두명 차면 바로 세션 생성.

        //중복 요청시 차단
        for(const other in this.matchQueue) {
            if(other.id === ws.id) return;
        }
        
        if(this.matchQueue.length >= 1) {
            console.log('matching complete. requesting session create.');
            this.requestSessionCreate(ws, this.matchQueue.pop());
        } else {
            this.matchQueue.push(ws);
        }
    }

    cancelMatch(ws) {
        const idx = this.matchQueue.findIndex((item) => item === ws);
        if(idx >= 0) this.matchQueue.splice(idx, 1);
    }

    requestSessionCreate(ws1, ws2) {
        this.game1ws.send(makePacket('create_session',{ids: [ws1.id, ws2.id]}));
        //해당 유저들에게 세션 생성이 요청되었음을 알림
        //해당 유저는 매칭 성공 알림을 받고 게임 서버 접속 시도
        ws1.send(makePacket('match_success', ''));
        ws2.send(makePacket('match_success', ''));
        
    }

};

module.exports = {Matcher};