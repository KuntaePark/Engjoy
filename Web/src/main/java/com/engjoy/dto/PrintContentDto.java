package com.engjoy.dto;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter @Setter
@AllArgsConstructor
public class PrintContentDto {
    // 시험지용 필드
    private String question;
    private String answer;
    private List<String> choices;
    // 워크시트/카드용 필드
    private String wordText;
    private String meaning;

    private String expSentence;
    private String partOfSpeech;
    private List<String> synonyms;
    private List<String> antonyms;
    private List<String> collocations;
}
