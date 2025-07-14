package com.engjoy.dto;

import com.engjoy.constant.EXPRTYPE;
import com.engjoy.entity.Expression;
import lombok.Getter;
import lombok.Setter;

import java.util.List;

@Getter @Setter
public class QuizQuestionDto {
    // 공통 필드
    private Long exprId;
    private EXPRTYPE exprType;
    private String questionText;
    private boolean isFavorite;
    private String pronAudio;

    // 퀴즈 타입별 필드
    private List<String> multipleChoices;    // 단어 퀴즈용
    private List<String> shuffledWords;      // 문장 퀴즈용 (이름을 sentenceWords -> shuffledWords로 변경)

    public static QuizQuestionDto from(Long exprId,
                                       EXPRTYPE exprType,
                                       String questionText,
                                       List<String> multipleChoices,
                                       List<String> shuffledWords,
                                       boolean isFavorite,
                                       String pronAudio) {
        QuizQuestionDto dto = new QuizQuestionDto();
        dto.setExprId(exprId);
        dto.setExprType(exprType);
        dto.setQuestionText(questionText);
        dto.setFavorite(isFavorite);
        dto.setPronAudio(pronAudio);
        dto.setMultipleChoices(multipleChoices);
        dto.setShuffledWords(shuffledWords);
        return dto;
    }

}
