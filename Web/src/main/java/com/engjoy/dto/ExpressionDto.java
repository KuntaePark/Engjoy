package com.engjoy.dto;

import com.engjoy.constant.EXPRTYPE;
import com.engjoy.entity.Expression;
import lombok.Getter;
import lombok.Setter;

import java.time.LocalDate;

@Getter
@Setter
public class ExpressionDto {
    private Long exprId;
    private String exprType;
    private String wordText;
    private String meaning;
    private int difficulty;
    private boolean isUsed;
    private boolean isFavorite;
    private String pronAudio;
    private LocalDate date;

    public static ExpressionDto from(Expression expression){
        ExpressionDto dto = new ExpressionDto();
        dto.setExprId(expression.getId());
        dto.setExprType(expression.getExprType().toString());
        dto.setWordText(expression.getWordText());
        dto.setMeaning(expression.getMeaning());
        dto.setDifficulty(expression.getDifficulty());
        dto.setPronAudio(expression.getPronAudio());
        return dto;
    }

    public static ExpressionDto from(Expression expression,boolean isFavorite, boolean isUsed, LocalDate date){
        ExpressionDto dto = from(expression);
        dto.setFavorite(isFavorite);
        dto.setUsed(isUsed);
        dto.setDate(date);
        return dto;
    }
}
