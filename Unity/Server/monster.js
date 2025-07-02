const MonsterType = {
  RUNNER: "runner",
  CHASER: "chaser",
};

class Monster {
  constructor(id, type, x, y, hp, holdingKeywordId = null) {
    this.id = id;
    this.type = type; //몬스터 유형(RUNNER, CHASER)
    this.x = x;
    this.y = y;
    this.hp = hp; //몬스터의 체력
    this.holdingKeywordId = holdingKeywordId; //몬스터가 떨굴 키워드ID
    this.isActive = false;

    //로밍 상태를 위한 변수 추가
    this.roamingTargetX = null; //로밍 X좌표
    this.roamingTargetY = null; //로밍 Y좌표
    this.roamTimer = 0; //로밍 쿨타임
  }

  toPacket() {
    return {
      id: this.id,
      type: this.type,
      x: this.x,
      y: this.y,
      hp: this.hp,
      holdingKeywordId: this.holdingKeywordId,
      isActive: this.isActive,
    };
  }
}

module.exports = { Monster, MonsterType };
