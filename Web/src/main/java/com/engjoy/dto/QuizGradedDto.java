package com.engjoy.dto;

import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter @Setter
public class QuizGradedDto {
    private Long exprId;
    private boolean isCorrect;
    private String message;
    private String correctWordAnswer;
    private List<String> correctSentenceAnswer;
}
