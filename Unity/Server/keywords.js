const { generateId } = require("./utils.js");

class Keyword {
  constructor(id, text, x, y, isAnswer = false) {
    this.id = id;
    this.text = text;
    this.x = x;
    this.y = y;
    this.isAnswer = this.isAnswer; //Exit의 정답 플래그
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

//테스트용 랜덤 키워드 텍스트 생성
//(임시. 나중에 db 받아서 할듯)
function generateKeyword(length = 8) {
  const chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
  let result = "";
  for (let i = 0; i < length; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return result;
}

//gameState에 추가
function spawnKeywords(gameState, count) {
  gameState.keywords = {};
  console.log("Spawning Keywords...");
  for (let i = 0; i < count; i++) {
    const keywordId = generateId(new Set(Object.keys(gameState.keywords)));
    const text = generateKeyword();
    const x = Math.random() * 15;
    const y = Math.random() * 15;

    const newKeyword = new Keyword(keywordId, text, x, y);
    gameState.keywords[keywordId] = newKeyword;
  }
  console.log(`${Object.keys(gameState.keywords).length} keywords spawned.`);
}

module.exports = { Keyword, spawnKeywords };
