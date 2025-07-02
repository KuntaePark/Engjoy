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

    public static QuizQuestionDto from(Long exprId,
                                       EXPRTYPE exprType,
                                       String questionText,
                                       List<String> choices,
                                       boolean isFavorite,
                                       String pronAudio) {
        QuizQuestionDto dto = new QuizQuestionDto();
        dto.setExprId(exprId);
        dto.setExprType(exprType);
        dto.setQuestionText(questionText);
        dto.setChoices(choices);
        dto.setFavorite(isFavorite);
        dto.setPronAudio(pronAudio);
        return dto;
    }

}
