package com.engjoy.dto;

import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter @Setter
public class QuizPageDto {
    private Listt<QuizQestionDto> questions;
    private String nofityMsg;
    private int quizCount;
}
