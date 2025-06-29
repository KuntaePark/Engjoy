package com.engjoy.dto;

import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter @Setter
public class QuizPageDto {
    private List<QuizQuestionDto> questions;
    private String nofityMsg;
    private int quizCount;
}
