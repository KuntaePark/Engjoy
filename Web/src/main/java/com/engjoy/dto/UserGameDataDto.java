package com.engjoy.dto;

import com.engjoy.entity.UserGameData;
import lombok.Getter;
import lombok.Setter;

@Getter@Setter
public class UserGameDataDto {
    private String nickname;
    private int game1Score;
    private int game2Score;
    private int gold;
    private Long ranking;
    private float rankingPercent;

    public static UserGameDataDto from(UserGameData userGameData) {
        UserGameDataDto dto = new UserGameDataDto();
        dto.nickname = userGameData.getAccount().getNickname();
        dto.game1Score = userGameData.getGame1Score();
        dto.game2Score = userGameData.getGame2Score();
        dto.gold = userGameData.getGold();
        dto.ranking = userGameData.getRanking();
        dto.rankingPercent = userGameData.getRankingPercent();

        return dto;

    }
}
