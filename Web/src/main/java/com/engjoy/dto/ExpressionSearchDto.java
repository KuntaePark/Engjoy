package com.engjoy.dto;

import com.engjoy.constant.DATERANGE;
import com.engjoy.constant.EXPRTYPE;
import com.engjoy.constant.SORTTYPE;
import lombok.Getter;
import lombok.Setter;
import org.springframework.format.annotation.DateTimeFormat;

import java.time.LocalDate;

@Getter @Setter
public class ExpressionSearchDto {
    private String keyword;
    private String exprType;
    private int difficulty;
    private boolean isFavorite;
    private boolean isUsed;
    private DATERANGE dateRange;
    @DateTimeFormat(iso = DateTimeFormat.ISO.DATE)
    private LocalDate StartDate;
    @DateTimeFormat(iso = DateTimeFormat.ISO.DATE)
    private LocalDate EndDate;
    private SORTTYPE sortType;

}
