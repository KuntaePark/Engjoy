//======== 행동 정의 ========
//스킬 종류 및 스킬 관련 수치(기본 공격 및 방어 포함)
const skills = {
    attack: {
        behavior: 'attack',
        minMana: 1,
        delay: 0.5 //in seconds, 공격 딜레이    
    },

    defense: {
        behavior: 'defense',
        unitShieldRate: 0.08, //단위 데미지 감소율
        minMana: 1,
        delay: 0 //in seconds, 방어 딜레이
    },

    heal : {
        behavior: 'heal',
        unitHeal : 3, //단위 힐량
        minMana : 3, //최소 요구 힐량
        delay: 0 //in seconds, 힐 딜레이
    },
    
    strengthen: { //공버프
        behavior : 'strengthen',
        unitAtkIncRate: 0.05, //단위 공격력 증가율
        minMana: 3,
        delay: 0
    }
}
Object.freeze(skills);

//스킬에 해당하는 행동
//공격자 관련 수치는 스냅샷 적용, 방어자 관련 수치는 현재 수치 적용
const skillBehaviors = {
    //(user, target, skill)
    attack: (user, target, skill) => {
        //강도 * 공격력 
        const attackDamage = user.snapshot.strengthLevel * user.snapshot.atk;
        const realDamage = attackDamage * (1 - target.shieldRate); //데미지 감소율 적용
        target.hp = Math.max(0, target.hp - realDamage);
        //체력 0되면 게임 종료 콜
        target.shieldRate = 0; //뎀감은 한턴 적용
    },

    defense: (user, _, skill) => {
        user.shieldRate = user.snapshot.strengthLevel * skill.unitShieldRate;
    },

    heal: (user, _, skill) => {
        const healAmount = skill.unitHeal * user.snapshot.strengthLevel;
        user.hp = Math.min(100, user.hp + healAmount);
    }
}
Object.freeze(skillBehaviors);
//======== 행동 정의 ========

function useSkill(user, target, skillName) {
    const skill = skills[skillName];
    
    //최소 마나와 강도와 비교해서 효과 발동 여부 결정
    if(skill.minMana > user.strengthLevel) {
        //발동 실패
        console.log('failed to use skill!');
        user.mp = Math.max(0, user.mp - skill.minMana);
        return;
    } else {
        //스킬 발동
        user.mp = Math.max(0, user.mp - user.strengthLevel);
        user.castEnd = true; //애니메이션 플래그
        console.log(`use skill!: ${skillName}`);
        const skillBehavior = skillBehaviors[skill.behavior];
        skillBehavior(user, target, skill);
    }
    
}

//프레임 당 시간
const deltaTime = 0.016;

module.exports = {
    deltaTime, skills, skillBehaviors, useSkill
}
