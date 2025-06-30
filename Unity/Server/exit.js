class Exit {
  constructor(id, x, y, sentence, answerKeywordIds) {
    this.id = id;
    this.x = x;
    this.y = y;
    this.sentence = sentence; //출구에 세팅될 영문장
    this.translation = this.translation; //출구에 세팅될 영문장의 해설
    this.answerKeywordIds = answerKeywordIds; //정답 리스트

    this.isOpen = false; //출구 개방상태
    this.correctedKeywordIds = []; //매칭된 정답 키워드 ID
  }

  toPacket() {
    return {
      id: this.id,
      x: this.x,
      y: this.y,
      isOpen: this.isOpen,
      sentence: this.sentence,
      translation: this.translation,
      answerCount: this.answerKeywordIds.length,
      correctedCount: this.correctedKeywordIds.length,
    };
  }

  matchKeyword(holdingKeywordId) {
    //키워드 매칭
    const isCorrectAnswer = this.answerKeywordIds.includes(holdingKeywordId);

    //예비용: 이미 제출된 답인지 아닌지 확인
    // const isAlreadySubmitted =
    //   this.correctedKeywordIds.includes(holdingKeywordId);

    //만약 정답이라면
    if (isCorrectAnswer) {
      this.correctedKeywordIds.push(holdingKeywordId);
      console.log`[Exit ${this.id}] Correct keyword submitted: ${holdingKeywordId} `;

      if (this.correctedKeywordIds.length === this.answerKeywordIds.length) {
        this.isOpen = true;
        console.log(
          `[Exit ${this.id}]All keywords matched. The exit is now open.`
        );

        //매칭 성공 여부와 출구 잠금해제 여부를 반환
        return { matched: true, opened: true };
      }

      //정답은 맞지만 아직 다 모으지 못한 경우
      return { matched: true, opened: false };
    }
    //정답이 아닌 경우
    return { matched: false, opened: false };
  }
}

module.exports = Exit;
