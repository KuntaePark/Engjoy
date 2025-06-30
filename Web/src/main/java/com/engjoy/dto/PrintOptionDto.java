package com.engjoy.dto;

import com.engjoy.constant.PRINTFORM;
import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter @Setter
public class PrintOptionDto {
    private PRINTFORM printForm;
    private boolean selectAll;
    private List<Long> exprIdsToPrint;
    private PrintOptionDto printOptionDto;
    private QuizSettingDto quizSettingDto;
}
