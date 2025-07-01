const { Keyword } = require("./keywords.js");
const Exit = require("./exit.js");
const { generateId } = require("./utils.js");
const { getRandomSentence, getRandomWords } = require("./dbUtils.js");

//랜덤 정수 생성 헬퍼
function getRandomInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

async function setupLevel(gameState, gameLevel = 10) {
  console.log(`LevelManager: Setting up level for gameLevel ${gameLevel}...`);
  gameState.keywords = {};
  gameState.exit = null;

  //db에서 영문장 가져오기
  const sentenceData = await getRandomSentence(gameLevel);

  //db조회 실패 시 함수 종료
  if (!sentenceData) {
    console.error("Failed to setup level: Could not fetch sentence from DB.");
    return;
  }

  //.,!?;: 등의 구두점 제거
  const cleanedText = sentenceData.word_text
    .toLowerCase()
    .replace(/[.,!?;:]/g, "");
  const allWords = cleanedText.split(" ").filter((word) => word.length > 0);

  const totalWordCount = allWords.length;

  //영문장 키워드 수에 따라 빈칸 개수 결정
  let answerCount;
  if (totalWordCount <= 4) {
    answerCount = 1;
  } else if (totalWordCount <= 9) {
    answerCount = 2;
  } else {
    answerCount = 3;
  }

  //오답 키워드 개수 랜덤 지정
  let dummyCount;
  if (answerCount === 1) {
    dummyCount = 1;
  } else {
    dummyCount = 2;
  }

  //정답 키워드 랜덤 선택
  //1.문장 내 단어 등장 횟수 계산
  const wordCounts = allWords.reduce((counts, word) => {
    counts[word] = (counts[word] || 0) + 1;
    return counts;
  }, {});

  //2.등장 횟수가 1인 단어들만 따로 추려내기
  const singleWords = Object.keys(wordCounts).filter(
    (word) => wordCounts[word] === 1
  );

  //3.한 번만 등장한 단어들 중 정답 선택
  singleWords.sort(() => 0.5 - Math.random()); //단어 무작위 섞기
  const answerTexts = singleWords.slice(0, answerCount); //필요한 개수만큼 자르기

  //만약 한 번만 등장하는 단어의 수가 정답 수보다 부족할 경우에 대한 경고
  if (answerTexts.length < answerCount) {
    console.warn(
      `[LevelManager] Warning: Not enough singhe-occurance words.Target: ${answerCount}, Actual: ${answerTexts.length}`
    );
  }

  //db에서 오답 키워드 가져오기
  const dummyTexts = await getRandomWords(dummyCount);

  const answerMap = {};
  //정답 키워드 생성 및 gameState에 추가
  answerTexts.forEach((text) => {
    const keywordId = generateId(new Set(Object.keys(gameState.keywords)));
    const newKeyword = new Keyword(
      keywordId,
      text,
      Math.random() * 15,
      Math.random() * 15,
      true //정답 처리
    );
    gameState.keywords[keywordId] = newKeyword;
    answerMap[keywordId] = text;
  });

  //오답 키워드 생성 및 gameState에 추가
  dummyTexts.forEach((text) => {
    const keywordId = generateId(new Set(Object.keys(gameState.keywords)));
    const newKeyword = new Keyword(
      keywordId,
      text,
      Math.random() * 15,
      Math.random() * 15,
      false //오답 처리
    );
    gameState.keywords[keywordId] = newKeyword;
  });

  //출구 생성 및 gameState에 추가
  const newExit = new Exit(
    7.5, //x
    7.5, //y
    sentenceData.word_text,
    answerMap,
    sentenceData.meaning
  );
  gameState.exit = newExit;

  console.log("LevelManager: Level setup complete.");
}

module.exports = { setupLevel };
