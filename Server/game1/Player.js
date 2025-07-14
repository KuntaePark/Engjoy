/*
 * 게임1에서의 각 플레이어에 대한 정보를 저장하는 클래스.
 */
const {WordDB} = require('./WordDB');
const {deltaTime, skills, skillBehaviors} = require('./GameLogic');

const UserDataDB = require('../common/UserDataDB');

const userDataDB = new UserDataDB();

const maxStrengthLevel = 5;

const wordDB = new WordDB();
wordDB.getWordData();

class Player {
    constructor(idx, id, session) {
        //플레이어 관련 정보
        this.id = id;
        this.idx = idx; //1p인지 2p인지 인덱스
        this.socket = null; 
        this.session = session;
        this.inputData = null;
        this.nickname = ""; //닉네임, 초기값은 빈 문자열

        //기본 수치
        this.hp = 100;                  //체력
        this.mp = 0;                    //마나
        this.unitMana = 7.5;              //초당 마나 회복량
        this.atk = 10;                   //기초공
        this.strengthLevel = 0;         //현재 행동 강도
        this.skillId = "heal";          //선택 스킬 종류, 기본값 회복
        
        //상태 관련
        this.isActionSelected = false;  //행동 결정 여부
        this.currentAction = 'ATTACK'   // 현재 행동

        //단어 관련
        this.words = [];                //단어 선택지, 4개 중 하나의 단어를 표시, 나머지는 뜻 옵션
        this.correctIdx = -1;           //정답 인덱스
        this.usedWords = [];

        //애니메이션 관련
        this.isCharging = false;
        this.isCasting = false;
        this.castEnd = false;

        //특수 플래그
        this.shieldRate = 0.0;          //방어 데미지 감소율, 한턴 유효

        //커스터마이징 관련
        this.bodyTypeIndex = 0;         //몸통 커마 인덱스
        this.weaponTypeIndex = 0;       //무기 커마 인덱스
        
        this.snapshot = null; //게임 상태 스냅샷, 게임 중에만 사용
        this.delayTimer = 0; //딜레이 연산용 타이머
        this.delaySkillId = null; //딜레이 연산용 스킬

        //db에서 정보 불러오기
        userDataDB.getUserData(id).then((data) => {
            if(data) {
                console.log(`User data loaded for id ${id}`);
                console.log(data);
                this.nickname = data.nickname;
                this.bodyTypeIndex = data['body_type_index'];
                this.weaponTypeIndex = data['weapon_type_index'];
            } else {
                console.log(`no user data found for id ${id}`);
            }
        }).catch((err) => {
            throw new Error(`Failed to load user data for id ${id}: ${err.message}`);
        });
    }

    doAction() {
        const type = this.inputData.type;
        inputActions[type](this);
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

    //스킬 사용 연산 처리, 발동 가능한지 여부 확인 후 발동 가능하면 예약
    activateSkill() {
        const skillName = this.getSkillName(this.currentAction);
        const skill = skills[skillName];
        if(skill.minMana > this.strengthLevel) {
                //틀림으로 인한 강제 발동 진입 시 상황
                console.log('failed to use skill!');
                //즉시 마나 회수
                this.mp = Math.max(0, this.mp - skill.minMana);
                return;
        } else {
            //스킬 발동
            this.mp = Math.max(0, this.mp - this.strengthLevel);
            console.log(`use skill!: ${skillName} +${Date.now()}`);
            
            //스킬 예약
            this.takeSnapshot(); //스냅샷 저장
            this.delaySkillId = skillName;
            this.delayTimer = skills[skillName].delay; //딜레이 설정

            this.strengthLevel = 0; //강도 초기화
            this.isActionSelected = false;
            this.resetWords();
            this.isCasting = false; //애니메이션 플래그
            this.castEnd = true; //애니메이션 플래그
        }
        
    }

    activateDelayedSkill() {
        if(!this.delaySkillId) return; //딜레이 스킬이 없으면 아무것도 안함
        if(this.delayTimer > 0) {
            this.delayTimer -= deltaTime;
            return; //딜레이 중이면 아무것도 안함
        }
        //딜레이가 끝나면 스킬 사용
        console.log(`activate delayed skill: ${this.delaySkillId} + ${Date.now()}`);
        const opponent = this.session.players[1-this.idx];
        const skill = skills[this.delaySkillId];
        const skillBehavior = skillBehaviors[skill.behavior];
        skillBehavior(this, opponent, skill);
        this.delaySkillId = null; //딜레이 스킬 초기화
    }
    

    clearAnimFlag() {
        //단발성 트리거의 경우 매 프레임 초기화
        this.isCharging = false;
        this.castEnd = false;
    }

    //딜레이 연산용 스냅샷
    takeSnapshot() {
        this.snapshot = {
            id: this.id,
            idx: this.idx,
            hp: this.hp,
            mp: this.mp,
            atk: this.atk,
            strengthLevel: this.strengthLevel,
            isActionSelected: this.isActionSelected,
            currentAction: this.currentAction,
            skillId: this.skillId,
            words: this.words,
            correctIdx: this.correctIdx,

            isCharging: this.isCharging,
            isCasting: this.isCasting,
            castEnd: this.castEnd,

            bodyTypeIndex: this.bodyTypeIndex,
            weaponTypeIndex: this.weaponTypeIndex
        }
    }

    toJSON() {
        return {
            id: this.id,
            nickname: this.nickname,
            idx: this.idx,
            hp: this.hp,
            mp: this.mp,
            strengthLevel: this.strengthLevel,
            isActionSelected: this.isActionSelected,
            currentAction: this.currentAction,
            skillId: this.skillId,
            words: this.words,
            correctIdx: this.correctIdx,
            
            isCharging: this.isCharging,
            isCasting: this.isCasting,
            castEnd: this.castEnd,

            bodyTypeIndex: this.bodyTypeIndex,
            weaponTypeIndex: this.weaponTypeIndex,
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
        
        user.isCharging = true; //애니메이션 플래그
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

        user.isCasting = true; //애니메이션 플래그
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
        user.isCasting = false; //애니메이션 플래그
    },

    'wordSelect': (user) => {
        //단어 맞추기 단계, 틀릴 시 즉시 효과 발동 체크

        //일단 맞추든 틀리든 사용 단어에 등록
        user.usedWords.push(user.words[user.correctIdx]);
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