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

    // ✅ 기본 생성자 - 다른 생성자가 추가되었으므로 명시적으로 추가해주는 것이 안전합니다.
    public ExpressionDto() {}

    // ✅ 테스트용 목(mock) 데이터 생성을 위해 새로 추가하는 생성자
    public ExpressionDto(String wordText, String meaning, String exprType, Integer difficulty, boolean isFavorite, boolean isUsed, String pronAudio) {
        this.wordText = wordText;
        this.meaning = meaning;
        this.exprType = exprType;
        this.difficulty = difficulty;
        this.isFavorite = isFavorite;
        this.isUsed = isUsed;
        this.pronAudio = pronAudio;
    }

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
