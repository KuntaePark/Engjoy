package com.engjoy.dto;

import lombok.Getter;
import lombok.Setter;

@Getter @Setter
public class QuizResultDto {
    private int totalQuestions;
    private int correctCount;
    private int incorrectCount;
    private int rewardGold;
}
