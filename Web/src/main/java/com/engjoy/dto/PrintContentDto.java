package com.engjoy.dto;

import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter @Setter
public class PrintContentDto {
    private String question;
    private String answer;
    private List<String> choices;
    private String expSentence;
    private String partOfSpeech;
    private List<String> synonyms;
    private List<String> antonyms;
    private List<String> collocations;
}
