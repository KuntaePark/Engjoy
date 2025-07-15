package com.engjoy.dto;

import com.engjoy.constant.PRINTFORM;
import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter @Setter
public class PrintReviewDto {
    private PRINTFORM printForm;
    private List<PrintContentDto> contents;
}
