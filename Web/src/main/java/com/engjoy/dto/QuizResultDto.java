package com.engjoy.dto;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.Setter;

@Getter @Setter
@AllArgsConstructor
public class QuizResultDto {
    private int totalQuestions;
    private int correctCount;
    private int rewardGold;

    public static QuizResultDto from(int totalQuestions, int correctCount, int rewardGold){
        return new QuizResultDto(totalQuestions,correctCount,rewardGold);
    }
}
