package com.engjoy.dto;

import com.engjoy.constant.FONTSIZE;
import com.engjoy.constant.ORDERTYPE;
import lombok.Getter;
import lombok.Setter;

@Getter @Setter
public class PrintOptionDetailDto {
    private String printTitle;
    private boolean userName;
    private FONTSIZE fontSize;
    private ORDERTYPE orderType;
    private boolean includeExpSentence;
    private boolean includePartOfSpeech;
    private boolean includeSynonyms;
    private boolean includeAntonyms;
    private boolean includeCollocations;
}
