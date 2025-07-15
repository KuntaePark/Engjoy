class Player {
  //많다..
  constructor(id, x, y) {
    this.id = id;
    this.x = x;
    this.y = y;
    this.inputH = 0.0;
    this.inputV = 0.0;
    this.isHoldingInteract = false; //상호작용 키를 계속 누르고 있는지의 여부

    this.holdingKeywordId = null; //들고 있는 키워드 ID
    this.interactableKeywordId = null; //상호작용 가능한 키워드 ID
    this.revivablePlayerId = -1; //부활시킬 수 있는 플레이어 ID

    this.isReady = false; //플레이어 준비 완료 플래그
    this.isEscaped = false; //플레이어 탈출 플래그
    this.isDown = false; //플레이어 전투불능 상태 플래그
    this.reviveProgress = 0; //부활 진행도

    this.invincibility = 41; //무적 시간(gameState.monsterHitsPlayer 함수에서 사용)

    this.maxHp = 3;
    this.hp = 3;
    this.baseSpeed = 7.0; //기본 이동 속도
    this.speed = 7.0; //현 이동 속도
    this.attackCooldown = 0; //공격 쿨다운

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
      inputH: this.inputH,
      inputV: this.inputV,
      holdingKeywordId: this.holdingKeywordId,

      isEscaped: this.isEscaped,
      isDown: this.isDown,

      revivablePlayerId: this.revivablePlayerId,
      reviveProgress: this.reviveProgress,

      attackCooldown: this.attackCooldown,

      hp: this.hp,
      maxHp: this.maxHp,
      isBuffed: this.isBuffed,
      hasShield: this.hasShield,

      //인벤토리 구조
      inventory: this.inventory,
    };
  }
}

module.exports = Player;
