/*
 * 게임 세션 정보 저장 클래스.
 */
const {Player} = require('./Player');
const {deltaTime} = require('./GameLogic');
const {makePacket} = require('../common/Packet');

/* [sessionId] : [Session] */
const sessions = new Map(); //게임 세션

const timeLimit = 99; //in seconds

class Session {
    static sessionCount = 0;

    constructor(id1, id2) {
        //매칭 서버로부터 두 플레이어의 아이디를 받아 세션 생성, 연결은 별도 지정
        this.id = (Session.sessionCount++).toString();
        this.intervalId = null;

        //게임 상태 관련
        this.startTime = null; //게임 시작 시간

        //게임 상태: ready: 대기, countdown: 카운트다운, running: 게임중, end: 게임종료
        this.state = "ready";
        this.usedWords = [];   //해당 라운드에서 사용된 단어들
        
        //상대 판정 쉽게 하기 위해 배열로 저장
        this.players = [new Player(0,id1, this), new Player(1, id2, this)]; //2인

        //10초 이내에 두 플레이어 모두 접속하지 않았을 경우, 세션 파기
        setTimeout(() => {
            if(!this.isBothConnected()) {
                //세션 삭제
                console.log(`session ${this.id} closed due to connection timeout.`);
                sessions.delete(this.id);
                this.close();
            }
        }, 10000);
    }

    isBothConnected() {
        return this.players[0].socket !== null && 
        this.players[1].socket !== null;
    }

    setPlayerConnection(id, ws) {
        for(const player of this.players) {
            if(player.id === id) {
                player.socket = ws;
                player['sessionId'] = this.id;
                console.log(`player connection set.`);
                ws.send(makePacket('session_connect', player.idx));
            }
        }
        //두 플레이어 모두 접속 완료 시, 게임 준비 상태? 또는 게임 시작?
        if(this.isBothConnected()) {
            console.log('both players ready.');
            this.gameStart();
        }
    }

    //게임 시작 시 호출
    gameStart() {
        console.log('game start');
        this.startTime = Date.now();
        this.intervalId = setInterval(() => this.tick(), deltaTime * 1000);
        this.state = "start";
        this.broadcast(makePacket('gameState', this));
    }

    //프레임 당 연산
    tick() {
        let hasUpdate = false;

        //각 플레이어에 대해 input 체크 후 행동 수행
        for(const player of this.players) {
            if(player.hasInput()) {
                //행동 수행
                player.doAction();
                hasUpdate = true;
            }
        }

        //업데이트 존재 시 브로드캐스트
        if(hasUpdate) this.broadcast(makePacket('gameState',this));

        //게임 종료 체크
        this.checkGameEnd();
    }
    
    //각 플레이어에게 브로드캐스트
    broadcast(message) {
        for(const player of this.players) {
            player.socket.send(message);
        }
    }

    //게임 종료 체크
    checkGameEnd() {
        const now = Date.now();
        //시간 체크
        let winner = -1;
        if(now - this.startTime >= timeLimit * 1000) {
            //게임 끝
            console.log("game end by timeover");
            if(this.players[0].hp > this.players[1].hp) {
                winner = 0;
            } else if(this.players[0].hp < this.players[1].hp) {
                winner = 1;
            } else {
                //draw
                winner = 2;
            }
        } else if(this.players[0].hp === 0) {
            //게임 끝
            winner = 1;
        } else if(this.players[1].hp === 0) {
            //게임 끝
            winner = 0;
        }
        if(winner >= 0) {
            console.log("game end");
            clearInterval(this.intervalId);
            this.intervalId = null;
            this.state = 'end';
            const message = makePacket('gameEnd', winner);
            this.broadcast(message);
            this.close();
        }
    }

    //플레이어 아이디로 플레이어 검색
    getPlayer(playerId) {
        for(const player of this.players) {
            if(player.id === playerId) return player;
        }
        return null;
    }

    close() {
        console.log(`closing session ${id}`);
        if(this.intervalId) clearInterval(this.intervalId);
        sessions.delete(this.id);
        
    }

    //게임 상태 전송용
    toJSON() {
        return {
            startTime: this.startTime,
            state: this.state,
            players: this.players
        }
    }
}

module.exports = {Session, sessions} ;