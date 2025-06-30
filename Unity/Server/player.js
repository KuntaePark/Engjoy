class Player {
  constructor(id, x = 2.0, y = 9.0) {
    this.id = id;
    this.x = x;
    this.y = y;
    this.inputH = 0.0;
    this.inputV = 0.0;
    this.holdingKeywordId = null; //들고 있는 키워드 ID
    this.interactableKeywordId = null; //상호작용 가능한 키워드 ID
  }

  toPacket() {
    return {
      id: this.id,
      x: this.x,
      y: this.y,
      holdingKeywordId: this.holdingKeywordId,
    };
  }
}

module.exports = Player;
