package com.engjoy.dto;

import com.engjoy.constant.DATERANGE;
import com.engjoy.constant.EXPRTYPE;
import com.engjoy.constant.SORTTYPE;
import lombok.Getter;
import lombok.Setter;

import java.time.LocalDate;

@Getter @Setter
public class ExpressionSearchDto {
    private String keyword;
    private EXPRTYPE exprType;
    private int difficulty;
    private boolean isFavorite;
    private boolean isUsed;
    private DATERANGE dateRange;
    private LocalDate StartDate;
    private LocalDate EndDate;
    private SORTTYPE sortType;

}
