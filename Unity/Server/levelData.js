//임시로 사용하는 영문장 정보들.
//나중에 DB에서 끌어와서 직접 사용하는 식으로 수정.

const levelPresets = [
  {
    id: "fox_puzzle",
    sentence: "The quick brown fox jumps over the lazy dog",
    translation: "날쌘 갈색 여우가 게으른 개를 뛰어넘는다",
    leaveBlankRange: [2, 4],
    dummyWordRange: [2, 3],
  },
  {
    id: "space_puzzle",
    sentence: "To infinity and beyond",
    translation: "무한한 공간, 저 너머로!",
    leaveBlankRange: [1, 2],
    dummyWordRange: [3, 4],
  },
  {
    id: "code_puzzle",
    sentence: "Talk is cheap Show me the code",
    translation: "말은 쉽지, 코드를 보여줘",
    leaveBlankRange: [2, 3],
    dummyWordRange: [2, 2],
  },

  {
    id: "dream_puzzle",
    sentence:
      "The future belongs to those who believe in the beauty of their dreams",
    translation: "미래는 자신의 꿈의 아름다움을 믿는 사람들의 것이다",
    leaveBlankRange: [3, 5],
    dummyWordRange: [3, 4],
  },
  {
    id: "time_puzzle",
    sentence: "Time is what we want most but what we use worst",
    translation: "시간은 우리가 가장 원하면서도 가장 헛되이 쓰는 것이다",
    leaveBlankRange: [2, 4],
    dummyWordRange: [2, 3],
  },
  {
    id: "knowledge_puzzle",
    sentence: "The only true wisdom is in knowing you know nothing",
    translation:
      "유일하게 진정한 지혜는 당신이 아무것도 모른다는 것을 아는 것이다",
    leaveBlankRange: [3, 4],
    dummyWordRange: [3, 3],
  },
  {
    id: "journey_puzzle",
    sentence: "A journey of a thousand miles begins with a single step",
    translation: "천 리 길도 한 걸음부터 시작된다",
    leaveBlankRange: [2, 4],
    dummyWordRange: [2, 3],
  },
  {
    id: "happiness_puzzle",
    sentence:
      "Happiness is not something ready made It comes from your own actions",
    translation: "행복은 기성품이 아니다. 그것은 당신 자신의 행동에서 비롯된다",
    leaveBlankRange: [3, 5],
    dummyWordRange: [3, 4],
  },
  {
    id: "art_puzzle",
    sentence:
      "Every child is an artist The problem is how to remain an artist once we grow up",
    translation:
      "모든 아이는 예술가이다. 문제는 우리가 자라서도 어떻게 예술가로 남아있느냐이다",
    leaveBlankRange: [4, 6],
    dummyWordRange: [2, 3],
  },
  {
    id: "shakespeare_puzzle",
    sentence: "To be or not to be that is the question",
    translation: "죽느냐 사느냐 그것이 문제로다",
    leaveBlankRange: [2, 3],
    dummyWordRange: [4, 5],
  },
  {
    id: "newton_puzzle",
    sentence:
      "If I have seen further it is by standing on the shoulders of Giants",
    translation:
      "내가 더 멀리 보았다면, 그것은 거인들의 어깨 위에 서 있었기 때문이다",
    leaveBlankRange: [3, 5],
    dummyWordRange: [2, 4],
  },
  {
    id: "imagination_puzzle",
    sentence: "Imagination is more important than knowledge",
    translation: "상상력은 지식보다 더 중요하다",
    leaveBlankRange: [1, 3],
    dummyWordRange: [3, 4],
  },
  {
    id: "jobs_puzzle",
    sentence: "Stay hungry Stay foolish",
    translation: "항상 갈망하고, 항상 우직하게 나아가라",
    leaveBlankRange: [1, 2],
    dummyWordRange: [4, 5],
  },
];

const dummyWordPool = [
  "apple",
  "banana",
  "car",
  "desk",
  "earth",
  "flower",
  "guitar",
  "house",
  "internet",
  "jungle",
  "key",
  "lemon",
  "mountain",
  "notebook",
  "ocean",
  "pencil",
  "queen",
  "river",
  "sun",
  "table",
  "umbrella",
  "violet",
  "water",
  "xylophone",
  "yacht",
  "zebra",
];

module.exports = { levelPresets, dummyWordPool };
