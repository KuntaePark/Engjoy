class Keyword {
  constructor(id, text, x, y, isAnswer = false) {
    this.id = id;
    this.text = text;
    this.x = x;
    this.y = y;
    this.isAnswer = isAnswer; //Exit의 정답 플래그
    this.carrierId = null; //상호작용중인 플레이어 ID
  }

  toPacket() {
    return {
      id: this.id,
      text: this.text,
      x: this.x,
      y: this.y,
      isAnswer: this.isAnswer,
      carrierId: this.carrierId,
    };
  }
}

module.exports = { Keyword };
