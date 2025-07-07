class Exit {
  constructor(x, y, originalSentence, answerTexts, translation) {
    this.x = x;
    this.y = y;

    this.originalSentence = originalSentence; //원본 문장
    this.answerTexts = answerTexts; //정답 단어 목록
    this.translation = translation; //문장 해설
    this.isOpen = false;
    this.correctedTexts = [];
  }

  toPacket() {
    let sentenceToShow = this.originalSentence;

    for (const answer of this.answerTexts) {
      if (!this.correctedTexts.includes(answer)) {
        const regex = new RegExp(`\\b${answer}\\b`, "gi");
        sentenceToShow = sentenceToShow.replace(regex, "___");
      }
    }

    return {
      x: this.x,
      y: this.y,
      isOpen: this.isOpen,
      sentence: sentenceToShow, //실시간으로 생성된 문장 전송
      translation: this.translation,
      answerCount: this.answerTexts.length,
      correctedCount: this.correctedTexts.length,
    };
  }

  matchKeyword(holdingKeywordText) {
    //키워드 매칭

    const isCorrectAnswer = this.answerTexts.includes(holdingKeywordText);
    // const isAlreadySubmitted = this.correctedTexts.includes(holdingKeywordText);

    if (isCorrectAnswer) {
      this.correctedTexts.push(holdingKeywordText);
      console.log(`[Exit] Correct keyword submitted: ${holdingKeywordText}`);

      if (this.correctedTexts.length === this.answerTexts.length) {
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
