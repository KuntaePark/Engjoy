const { Keyword } = require("../packet/keywords.js");
const Exit = require("../packet/exit.js");
const { Monster, MonsterType } = require("../packet/monster.js");
const { generateId } = require("./utils.js");
const { getRandomSentence, getRandomWords } = require("../db/dbUtils.js");

const fs = require("fs");
const path = require("path");

//랜덤 정수 생성 헬퍼
function getRandomInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

// ================= ▼▼▼ 맵 세팅 ▼▼▼ =================
function loadMap(gameState, mapName) {
  try {
    const mapPath = path.join(__dirname, "..", "maps", `${mapName}.json`);
    const mapFileContent = fs.readFileSync(mapPath, "utf8");
    const mapData = JSON.parse(mapFileContent);

    gameState.colliders = new Set(
      mapData.colliders.map((c) => `${c.x}, ${c.y}`)
    );

    console.log(
      `[Map] Successfully loaded map: ${mapName}. ${gameState.colliders.size} colliders registered.`
    );
  } catch (error) {
    console.error(
      "[Map] Failed to load map data. Make sure 'maps/map1.json' exists."
    );
    gameState.colliders = new Set();
  }
}
// ================= ▲▲▲ 맵 세팅 ▲▲▲ =================

// ================= ▼▼▼ 레벨 세팅 ▼▼▼ =================
async function setupLevel(gameState, gameLevel = 1) {
  console.log(`LevelManager: Setting up level for gameLevel ${gameLevel}...`);
  gameState.keywords = {};
  gameState.exit = null;

  gameState.monsters = {};

  const mapName = "map1";
  loadMap(gameState, mapName);

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
  if (totalWordCount <= 4 && gameLevel <= 3) {
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

  //키워드 텍스트를 배열로 정리
  const allKeywordTexts = [
    ...answerTexts.map((text) => ({ text, isAnswer: true })),
    ...dummyTexts.map((text) => ({ text, isAnswer: false })),
  ];

  //키워드를 가진 Runner몬스터 스폰
  allKeywordTexts.forEach((keywordInfo) => {
    const monsterId = generateId(new Set(Object.keys(gameState.monsters)));

    const newMonster = new Monster(
      monsterId,
      MonsterType.RUNNER,
      getRandomInt(0, 15),
      getRandomInt(0, 15),
      3,
      keywordInfo
    );
    gameState.monsters[monsterId] = newMonster;
  });

  //키워드를 가지지 않은 더미 러너 몬스터 생성
  const dummyRunnerCount = allKeywordTexts.length / 2;
  for (let i = 0; i < dummyRunnerCount; i++) {
    const monsterId = generateId(new Set(Object.keys(gameState.monsters)));
    const newMonster = new Monster(
      monsterId,
      MonsterType.RUNNER,
      getRandomInt(0, 15),
      getRandomInt(0, 15),
      3
    );
    gameState.monsters[monsterId] = newMonster;
  }

  //체이서 몬스터 생성
  const chaserCount = 2;
  for (let i = 0; i < chaserCount; i++) {
    const monsterId = generateId(new Set(Object.keys(gameState.monsters)));
    const newMonster = new Monster(
      monsterId,
      MonsterType.CHASER,
      getRandomInt(0, 15),
      getRandomInt(0, 15),
      2
    );
    gameState.monsters[monsterId] = newMonster;
  }

  //출구 위치도 랜덤으로 생성해주기

  //출구 생성 및 gameState에 추가
  const newExit = new Exit(
    7.5, //x
    7.5, //y
    sentenceData.word_text, //원본 문장 전달
    answerTexts, //정답 단어들의 배열
    sentenceData.meaning, //해설문
    sentenceData.expr_id, //문장ID
    sentenceData.difficulty //문장 난이도
  );
  gameState.exit = newExit;

  console.log("LevelManager: Level setup complete.");
}
// ================= ▲▲▲ 레벨 세팅 ▲▲▲ =================

module.exports = { setupLevel, loadMap };
