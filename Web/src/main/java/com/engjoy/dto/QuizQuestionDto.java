package com.engjoy.dto;

import com.engjoy.constant.EXPRTYPE;
import com.engjoy.entity.Expression;
import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter @Setter
public class QuizQuestionDto {
    private Long quizQuestionId;
    private Long exprId;
    private EXPRTYPE exprType;
    private String questionText;
    private List<String> choices;
    private boolean isFavorite;
    private String pronAudio;

    public static QuizQuestionDto from(Expression expression,List<String> choices,
                                       boolean isFavorite){
        QuizQuestionDto dto = new QuizQuestionDto();
        dto.setExprId(expression.getId());
        dto.setExprType(expression.getExprType());
        dto.setQuestionText(expression.getMeaning()); // 단어 퀴즈면 뜻이 문제가 될 수 잇음
        dto.setChoices(choices);
        dto.setFavorite(isFavorite);
        dto.setPronAudio(expression.getPronAudio());
        return dto;
    }

}
