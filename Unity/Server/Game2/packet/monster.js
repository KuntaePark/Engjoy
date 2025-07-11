const MonsterType = {
  RUNNER: "runner",
  CHASER: "chaser",
};

class Monster {
  constructor(id, type, x, y, hp, keywordData = null) {
    this.id = id;
    this.type = type; //몬스터 유형(RUNNER, CHASER)
    this.x = x;
    this.y = y;
    this.hp = hp; //몬스터의 체력
    this.keywordData = keywordData; //키워드 정보 직접 저장
    this.isActive = false;

    this.hitStunTimer = 0; //피격 시 경직 타이머

    if (this.type === MonsterType.RUNNER) {
      this.roamingTargetX = null; //로밍 X좌표
      this.roamingTargetY = null; //로밍 Y좌표
      this.hitFleeBoostTimer = 0; // 피격 후 이속 부스트 타이머
    } else if (this.type === MonsterType.CHASER) {
      this.teleportTime = 2;
    }
  }

  toPacket() {
    return {
      id: this.id,
      type: this.type,
      x: this.x,
      y: this.y,
      hp: this.hp,
      isActive: this.isActive,
    };
  }
}

module.exports = { Monster, MonsterType };
