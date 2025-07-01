const pool = require("./db.js");

async function getRandomSentence(gameLevel = 10) {
  try {
    //게임 레벨을 expression difficulty 로 변환
    let dbDifficulty;

    if (gameLevel <= 2) {
      dbDifficulty = [1]; //level 1-2 : 1
    } else if (gameLevel <= 5) {
      dbDifficulty = [2]; //level 3-5 : 2
    } else if (gameLevel <= 9) {
      dbDifficulty = [3]; //level 6-9 : 3
    } else {
      dbDifficulty = [4, 5]; //else 4,5
    }

    //selecting sentence
    const sql =
      "SELECT expr_id, word_text, meaning, difficulty FROM expression WHERE expr_type = 'SENTENCE' AND difficulty IN (?) ORDER BY RAND() LIMIT 1";

    const [rows] = await pool.query(sql, [dbDifficulty]);

    return rows[0];
  } catch (error) {
    console.error(
      `DB에서 난이도 레벨 ${gameLevel}에 맞는 문장을 가져오는 중 오류가 발생했습니다.`
    );
    console.error("ㄴ[Detailed Error]:", error);
    return null;
  }
}

async function getRandomWords(count) {
  try {
    const sql =
      "SELECT expr_id, word_text FROM expression WHERE expr_type = 'WORD' ORDER BY RAND() LIMIT ?";
    const [rows] = await pool.query(sql, [count]);
    return rows.map((row) => row.word_text);
  } catch (error) {
    console.error(
      `DB에서 랜덤 단어 ${count}개를 가져오는 중 오류 발생:`,
      error
    );
    console.error("ㄴ[Detailed Error]:", error);
    return [];
  }
}

module.exports = {
  getRandomSentence,
  getRandomWords,
};
