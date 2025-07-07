// =============================== ▼▼▼ ID 생성 함수 ▼▼▼ ===============================

//서버 실행 시 이 함수 먼저 정의해준 뒤,
//이 함수를 사용하는 다른 js들에게 전달.

function generateId(existingIds, length = 16) {
  const chars =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
  while (true) {
    let result = "";
    for (let i = 0; i < length; i++) {
      result += chars[Math.floor(Math.random() * chars.length)];
    }
    if (!existingIds.has(result)) {
      return result;
    }
  }
}

// =============================== ▲▲▲ ID 생성 함수 ▲▲▲ ===============================

module.exports = { generateId };
