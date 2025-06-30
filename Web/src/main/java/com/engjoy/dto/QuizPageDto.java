package com.engjoy.dto;

import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

import java.util.List;

@NoArgsConstructor
@Getter @Setter
public class QuizPageDto {
    private List<QuizQuestionDto> questions;
    private String notifyMsg;
    private int quizCount;

    public static QuizPageDto from(List<QuizQuestionDto> questions, String notifyMsg, int quizCount){
        QuizPageDto dto = new QuizPageDto();
        dto.setQuestions(questions);
        dto.setNotifyMsg(notifyMsg);
        dto.setQuizCount(quizCount);
        return dto;
    }
}
