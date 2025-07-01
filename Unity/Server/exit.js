class Exit {
  constructor(x, y, originalSentence, answerMap, translation) {
    this.x = x;
    this.y = y;

    this.originalSentence = originalSentence; //출구에 세팅될 영문장
    this.answerMap = answerMap; //정답 키워드 목록
    this.answerKeywordIds = Object.keys(this.answerMap); //정답 키워드 ID
    this.translation = translation; //출구에 세팅될 영문장의 해설

    this.isOpen = false; //출구 개방상태
    this.correctedKeywordIds = []; //매칭된 정답 키워드 ID
  }

  toPacket() {
    let currentExitSentence = this.originalSentence;

    for (const [id, text] of Object.entries(this.answerMap)) {
      //아직 정답이 맞춰지지 않았다면

      if (!this.correctedKeywordIds.includes(id)) {
        //해당 텍스트를 빈칸으로 교체
        const regex = new RegExp(`\\b${text}\\b`, "gi");
        currentExitSentence = currentExitSentence.replace(regex, "___");
      }
    }

    return {
      x: this.x,
      y: this.y,
      isOpen: this.isOpen,
      sentence: currentExitSentence,
      translation: this.translation,
      answerCount: this.answerKeywordIds.length,
      correctedCount: this.correctedKeywordIds.length,
    };
  }

  matchKeyword(holdingKeywordId) {
    //키워드 매칭

    const isCorrectAnswer = this.answerKeywordIds.includes(holdingKeywordId); //예비용: 이미 제출된 답인지 아닌지 확인 // const isAlreadySubmitted = //   this.correctedKeywordIds.includes(holdingKeywordId); //만약 정답이라면

    if (isCorrectAnswer) {
      this.correctedKeywordIds.push(holdingKeywordId);
      console.log(`[Exit] Correct keyword submitted: ${holdingKeywordId}`);

      if (this.correctedKeywordIds.length === this.answerKeywordIds.length) {
        this.isOpen = true;
        console.log("[Exit] All keywords matched. The exit is now open.");

        //매칭 성공 여부와 출구 잠금해제 여부를 반환
        return { matched: true, opened: true };
      } //정답은 맞지만 아직 다 모으지 못한 경우
      return { matched: true, opened: false };
    } //정답이 아닌 경우
    return { matched: false, opened: false };
  }
}

module.exports = Exit;
