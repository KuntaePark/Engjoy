class Player {
  constructor(id, x = 2.0, y = 9.0) {
    this.id = id;
    this.x = x;
    this.y = y;
    this.inputH = 0.0;
    this.inputV = 0.0;
    this.holdingKeywordId = null; //들고 있는 키워드 ID
    this.interactableKeywordId = null; //상호작용 가능한 키워드 ID
    this.isEscaped = false; //플레이어 탈출 여부
    this.invincibility = 41; //무적 시간(gameState.monsterHitsPlayer 함수에서 사용)

    this.maxHp = 3;
    this.hp = 3;
    this.speed = 7.0; //기본 이동 속도
    this.attackSpeed = 1.0; //기본 공격 속도

    this.isBuffed = false;
    this.hasShield = false;

    //인벤토리
    this.inventory = {
      Potion: 0,
      buff: 0,
      shield: 0,
    };
  }

  toPacket() {
    return {
      id: this.id,
      x: this.x,
      y: this.y,
      holdingKeywordId: this.holdingKeywordId,
      isEscaped: this.isEscaped,

      hp: this.hp,
      maxHp: this.maxHp,
      isBuffed: this.isBuffed,
      hasShield: this.hasShield,

      //임시 인벤토리 구조
      inventory: this.inventory,
    };
  }
}

module.exports = Player;
