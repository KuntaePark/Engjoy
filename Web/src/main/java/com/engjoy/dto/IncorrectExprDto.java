package com.engjoy.dto;

import com.engjoy.entity.IncorrectExpr;
import lombok.Getter;
import lombok.Setter;

import java.time.LocalDate;

@Getter @Setter
public class IncorrectExprDto {
    private Long incorrectExprId;
    private Long exprId;
    private String wordText;
    private String meaning;
    private int incorrectCount;
    private LocalDate lastReviewDate;

    public static IncorrectExprDto from(IncorrectExpr incorrectExpr) {
        IncorrectExprDto dto = new IncorrectExprDto();
        dto.setIncorrectExprId(incorrectExpr.getId());
        dto.setExprId(incorrectExpr.getExpression().getId());
        dto.setWordText(incorrectExpr.getExpression().getWordText());
        dto.setMeaning(incorrectExpr.getExpression().getMeaning());
        dto.setIncorrectCount(incorrectExpr.getIncorrectCount());
        dto.setLastReviewDate(incorrectExpr.getUsedTime().toLocalDate());
        return dto;
    }
}
