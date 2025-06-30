package com.engjoy.dto;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter @Setter
@AllArgsConstructor
public class QuizGradedDto {
    private Long exprId;
    private boolean isCorrect;
    private String message;
    private String correctWordAnswer;
    private List<String> correctSentenceAnswer;

    public static QuizGradedDto from(Long exprId, boolean isCorrect, String correctWordAnswer, List<String> correctSentenceAnswer){
        return new QuizGradedDto(exprId, isCorrect,null, correctWordAnswer, correctSentenceAnswer);
    }
}
