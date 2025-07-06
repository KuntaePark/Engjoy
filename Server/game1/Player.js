/*
 * 게임1에서의 각 플레이어에 대한 정보를 저장하는 클래스.
 */
const {WordDB} = require('./WordDB');
const {deltaTime, skills, useSkill} = require('./GameLogic');

const maxStrengthLevel = 5;

const wordDB = new WordDB();
wordDB.getWordData();

class Player {
    constructor(idx, id, session) {
        this.id = id;
        this.idx = idx; //1p인지 2p인지 인덱스
        this.socket = null; 
        this.session = session;
        this.inputData = null;
        
        //기본 수치
        this.hp = 100;                  //체력
        this.mp = 0;                    //마나
        this.unitMana = 4;              //초당 마나 회복량
        this.atk = 5;                   //기초공
        this.strengthLevel = 0;         //현재 행동 강도
        this.skillId = "heal";          //선택 스킬 종류, 기본값 회복
        
        //상태 관련
        this.isActionSelected = false;  //행동 결정 여부
        this.currentAction = 'ATTACK'   // 현재 행동

        //단어 관련
        this.words = [];                //단어 선택지, 4개 중 하나의 단어를 표시, 나머지는 뜻 옵션
        this.correctIdx = -1;           //정답 인덱스

        //특수 플래그
        this.shieldRate = 0.0;          //방어 데미지 감소율, 한턴 유효
    }

    doAction() {
        const type = this.inputData.type;
        inputActions[type](this, deltaTime);
        this.inputData = null;
    }

    setInput(inputData) {
        if(this.inputData) return; //처리 안된 입력 있을 시 스킵
        else this.inputData = inputData;        
    }

    hasInput() {return !!this.inputData;}

    //단어 관련
    loadWords() {
        this.words = wordDB.pick4();
        this.correctIdx = Math.floor(Math.random() * 4);
    }

    resetWords() {
        this.words = [];
        this.correctIdx = -1;
    }

    getSkillName(action) {
        if(action === 'SPECIAL') {
            return this.skillId;
        } else {
            return action.toLowerCase();
        }
    }

    //스킬 사용 연산 처리
    activateSkill() {
        const opponent = this.session.players[1-this.idx];

        const skillName = this.getSkillName(this.currentAction);
        useSkill(this, opponent, skillName);
        this.isActionSelected = false;
        this.resetWords();
        this.strengthLevel = 0;
    }

    toJSON() {
        return {
            id: this.id,
            idx: this.idx,
            hp: this.hp,
            mp: this.mp,
            strengthLevel: this.strengthLevel,
            isActionSelected: this.isActionSelected,
            currentAction: this.currentAction,
            words: this.words,
            correctIdx: this.correctIdx     
        }
    }
}

/*
 * 인풋에 해당하는 행동 함수
 */
const inputActions = {
    'chargeMana': (user) => {
        if(user.isActionSelected) return;
        user.mp = Math.min(user.mp + deltaTime * user.unitMana,10);
        console.log(`user ${user.id} charged mana : ${user.mp}`)
    },
    'actionSelect': (user) => {
        //행동 선택, 단어 맞추기 페이즈로
        const action = user.inputData.action;
        const skillName = user.getSkillName(action);
        //최소 요구 마나량 확인, 요구량 보다 마나량 작을 시 행동 선택 안됨
        const minMana = skills[skillName].minMana;
        if(minMana > user.mp) {
            console.log('not enough mp');
            return;
        }
        console.log(`user ${user.id} do actionSelect`);

        user.currentAction = action;
        user.isActionSelected = true;
        user.loadWords();

    },
    'actionConfirm': (user) => {
        //행동 조기 실행. 현재까지의 강도로 실행
        const action = user.currentAction;
        const skillName = user.getSkillName(action);
        //최소 소모 마나 확인, 현재 강도보다 클 시 시행 불가
        const minMana = skills[skillName].minMana;
        if(minMana > user.strengthLevel) {
            console.log('strengthlevel not enough');
            return;
        }
        console.log(`user ${user.id} do actionConfirm`);

        user.activateSkill();
    },
    'actionCancel': (user) => {
        //행동 취소, 아무 마나 소모 없음
        console.log(`user ${user.id} do actionCancel`);
        user.isActionSelected = false;
        user.resetWords();
    },
    'wordSelect': (user) => {
        //단어 맞추기 단계, 틀릴 시 즉시 효과 발동 체크
        console.log(`user ${user.id} do wordSelect`);
        const idx = user.inputData.idx;

        console.log("str lev: "+ user.strengthLevel);
        if(idx === user.correctIdx && 
            user.strengthLevel < maxStrengthLevel && 
            Math.floor(user.mp) > user.strengthLevel) {
            user.strengthLevel++;
            //단어 맞추면 즉시 다음 단어 로드
            if(user.strengthLevel == maxStrengthLevel || Math.floor(user.mp) == user.strengthLevel) {
                user.activateSkill();
            } else {
                user.loadWords(user.strengthLevel);
            }
        } else {
            //틀리거나, 강도가 10에 도달하거나, 강도가 사용 가능한 최대 마나일 경우 행동 계산
            user.activateSkill();
        }
    }
}

module.exports = {Player};