package com.engjoy.dto;

import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter @Setter
public class QuizAnsweredDto {
    private Long exprId;
    private String submitWord;
    private List<String> submitSentence;
}
