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
    private List<String> multipleChoices;
    private List<String> sentenceWords;
    private boolean isFavorite;
    private String pronAudio;
    private String finalPunctuation; // 문장 부호 담는 필드

    public static QuizQuestionDto from(Long exprId,
                                       EXPRTYPE exprType,
                                       String questionText,
                                       List<String> multipleChoices,
                                       List<String> sentenceWords,
                                       boolean isFavorite,
                                       String pronAudio,
                                       String finalPunctuation) {
        QuizQuestionDto dto = new QuizQuestionDto();
        dto.setExprId(exprId);
        dto.setExprType(exprType);
        dto.setQuestionText(questionText);
        dto.setMultipleChoices(multipleChoices);
        dto.setSentenceWords(sentenceWords);
        dto.setFavorite(isFavorite);
        dto.setPronAudio(pronAudio);
        dto.setFinalPunctuation(finalPunctuation);
        return dto;
    }

}
