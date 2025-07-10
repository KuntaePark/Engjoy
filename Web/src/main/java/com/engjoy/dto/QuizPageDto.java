package com.engjoy.dto;

import com.engjoy.constant.CATEGORY;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

import java.util.List;

@NoArgsConstructor
@Getter @Setter
public class QuizPageDto {
    private CATEGORY category;
    private List<QuizQuestionDto> questions;
    private String notifyMsg;
    private int quizCount;

    public static QuizPageDto from(List<QuizQuestionDto> questions, String notifyMsg, int quizCount, CATEGORY category){
        QuizPageDto dto = new QuizPageDto();
        dto.setQuestions(questions);
        dto.setNotifyMsg(notifyMsg);
        dto.setQuizCount(quizCount);
        dto.setCategory(category);
        return dto;
    }
}
