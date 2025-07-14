/*
 * 게임 세션 정보 저장 클래스.
 */
const {Player, userDataDB} = require('./Player');
const {deltaTime} = require('./GameLogic');
const {makePacket} = require('../common/Packet');
const {preciseSetInterval, clearPreciseInterval} = require('../common/PreciseInterval');


const CUTSCENE_LENGTH = 0; //in milliseconds, 게임 시작 전 컷씬 시간
const COUNTDOWN_LENGTH = 4000; //in milliseconds

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
        //게임 상태 업데이트 여부 확인용
        this.hasChange = false;


        //게임 상태: ready: 대기, countdown: 카운트다운, running: 게임중, end: 게임종료
        this.state = "ready";
        this.usedWords = [];   //해당 라운드에서 사용된 단어들
        
        //상대 판정 쉽게 하기 위해 배열로 저장
        try {
            this.players = [new Player(0,id1, this), new Player(1, id2, this)]; //2인
        } catch (e) {
            //db 로드 오류
            throw new Error(`Player creation failed: ${e.message}`);
        }

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
                player.socket['sessionId'] = this.id;
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
        //게임 시작 전 컷씬 및 카운트다운
        console.log('game start countdown');
        this.state = 'countdown';
        this.startTime = Date.now();
        this.broadcast(makePacket('gameState', this));
        //게임 컷씬 및 타이머 계산 시간 이후 게임 시작
        setTimeout(() => {
            console.log('game start');
            this.intervalId = preciseSetInterval(() => this.tick(), deltaTime * 1000);
            this.state = "start";
            this.broadcast(makePacket('gameState', this));
        }, CUTSCENE_LENGTH + COUNTDOWN_LENGTH);
    }

    //프레임 당 연산
    tick() {
        this.hasChange = false;
        for(const player of this.players) {
            //딜레이 스킬 발동
            player.activateDelayedSkill();
        }
        
        //딜레이 스킬 연산 후, 게임 종료 체크
        //게임 종료 체크
        this.checkGameEnd();


        //각 플레이어에 대해 input 체크 후 행동 수행
        if(this.state === 'start') {
            for(const player of this.players) {
                //먼저 초기화 필요한 애니메이션 플래그 초기황
                player.clearAnimFlag();
                
                if(player.hasInput()) {
                    //행동 수행
                    player.doAction();
                }
            }
            
            this.checkGameEnd();
        }


        //게임 상태 브로드캐스트
        if(this.hasChange)
            this.broadcast(makePacket('gameState',this));
        

    }
    
    //각 플레이어에게 브로드캐스트
    broadcast(message) {
        // console.log(`broadcasting message: ${message}`);
        for(const player of this.players) {
            player.socket.send(message);
        }
    }

    //게임 종료 체크
    checkGameEnd() {
        const now = Date.now();
        //시간 체크
        let winner = -1;
        if(now - this.startTime >= (timeLimit) * 1000 + COUNTDOWN_LENGTH) {
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
            this.broadcast(makePacket('gameState',this));
            this.state = 'end';
            clearPreciseInterval(this.intervalId);
            this.intervalId = null;

            this.announceScore(winner);
            
            const message = makePacket('gameEnd', winner);
            this.broadcast(message);
            this.close();
        }
    }

    announceScore(winnerIdx) {
        //승리 시 25점, 패배 시 - 25점
        for(let i = 0; i < 2; i++) {
            const player = this.players[i];
            let diff = 0;
            if(2 === winnerIdx) {
                //비겼을 시 둘 다 10점
                diff = 10;
            }
            else {
                if(i === winnerIdx) {
                    //+25점
                    diff = 25;
                } else {
                    diff = -25;
                }                
            }
            player.userScore += diff;
            userDataDB.updateUserPoint(player.userScore, player.id);
            player.socket.send(makePacket('gameEnd', {winner: winnerIdx, score: player.userScore, diff}))
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
        console.log(`closing session ${this.id}`);
        if(this.intervalId) clearPreciseInterval(this.intervalId);
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