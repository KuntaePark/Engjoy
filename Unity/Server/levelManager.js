const { Keyword } = require("./keywords.js");
const Exit = require("./exit.js");
const { generateId } = require("./utils.js");
const { levelPresets, dummyWordPool } = require("./levelData.js");

//랜덤 정수 생성 헬퍼
function getRandomInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

function setupLevel(gameState) {
  console.log("LevelManager : Setting up the level...");

  gameState.keywords = {};
  gameState.exits = {};

  //영문장 랜덤으로 가져오기
  const chosenLevel =
    levelPresets[Math.floor(Math.random() * levelPresets.length)];
  console.log(`[Level Setup] Randomly selected level : "${chosenLevel.id}"`);

  //띄어쓰기로로 문장을 단어로 분리
  const allWordsInstance = Array.from(
    new Set(chosenLevel.sentence.toLowerCase().split(" "))
  );
  const totalWordCount = allWordsInstance.length;

  //문장에 뚫을 빈칸 개수(정답 개수) 랜덤 지정
  const wordsToLeaveBlank = getRandomInt(
    chosenLevel.leaveBlankRange[0],
    chosenLevel.leaveBlankRange[1]
  );

  //오답 수 랜덤 지정
  const answerCount = Math.max(1, totalWordCount - wordsToLeaveBlank);
  const dummyWordCount = getRandomInt(
    chosenLevel.dummyWordRange[0],
    chosenLevel.dummyWordRange[1]
  );

  //랜덤 지정된 수만큼 정답 키워드 생성
  allWordsInstance.sort(() => 0.5 - Math.random());
  const answerTexts = allWordsInstance.slice(0, answerCount);

  //랜덤 지정된 수만큼 오답 키워드 생성
  //DB에서 가져올 경우 -> 정답과 똑같은 단어는 자연스럽게 오답 키워드에서 걸러짐.
  const dummies = dummyWordPool.filter(
    (dummy) => !allWordsInstance.includes(dummy)
  );
  dummies.sort(() => 0.5 - Math.random());
  const dummyTexts = dummies.slice(0, dummyWordCount);

  //exit의 정답키워드 배열
  const answerKeywordIds = [];

  //정답 키워드 생성 및 gameState에 추가
  answerTexts.forEach((text) => {
    const keywordId = generateId(new Set(Object.keys(gameState.keywords)));
    const newKeyword = new Keyword(
      keywordId,
      text,
      Math.random() * 15,
      Math.random() * 15,
      true
    );
    gameState.keywords[keywordId] = newKeyword;
    answerKeywordIds.push(keywordId);
  });

  //오답 키워드 생성 및 gameState에 추가
  dummyTexts.forEach((text) => {
    const keywordId = generateId(new Set(Object.keys(gameState.keywords)));
    const newKeyword = newKeyword(
      keywordId,
      text,
      Math.random() * 15,
      Math.random() * 15,
      false
    );
    gameState.keywords[keywordId] = newKeyword;
  });

  //출구 생성 및 gameState에 추가
  const exitId = generateId(new Set(Object.keys(gameState.exits)));
  let exitSentence = chosenLevel.sentence;
  answerTexts.forEach((ans) => {
    const regex = new RegExp(`\\b${ans}\\b`, "gi");
    exitSentence = displaySentence.replace(regex, "___");
  });

  const newExit = new Exit(
    exitId,
    7.5, //x
    7.5, //y
    exitSentence,
    answerKeywordIds,
    chosenLevel.translation
  );
  gameState.exits[exitId] = newExit;

  console.log("LevelManager: Level setup complete.");
}

module.exports = { setupLevel };
