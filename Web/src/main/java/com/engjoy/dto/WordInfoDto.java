package com.engjoy.dto;

import com.engjoy.entity.Expression;
import com.engjoy.entity.WordInfo;
import lombok.Getter;
import lombok.Setter;

@Getter @Setter
public class WordInfoDto {
    private Long exprId;
    private String wordInfoJson;

    public static WordInfoDto from(WordInfo wordInfo){
        WordInfoDto dto = new WordInfoDto();
        dto.setExprId(wordInfo.getId());
        dto.setWordInfoJson(wordInfo.getWordInfoJson());
        return dto;
    }
}
