package com.engjoy.dto;

import com.engjoy.entity.UserGameData;
import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Getter@Setter
@AllArgsConstructor
@NoArgsConstructor
public class UserGameDataDto {
    private String nickname;
    private int game1Score;
    private int game2Score;
    private int gold;
    private Long ranking;
    private float rankingPercent;
    private int bodyTypeIndex;
    private int weaponTypeIndex;

    public static UserGameDataDto from(UserGameData userGameData) {
        UserGameDataDto dto = new UserGameDataDto();
        dto.nickname = userGameData.getAccount().getNickname();
        dto.game1Score = userGameData.getGame1Score();
        dto.game2Score = userGameData.getGame2HighScore();
        dto.gold = userGameData.getGold();
        dto.ranking = userGameData.getRanking();
        dto.rankingPercent = userGameData.getRankingPercent();
        dto.bodyTypeIndex = userGameData.getBodyTypeIndex();
        dto.weaponTypeIndex = userGameData.getWeaponTypeIndex();

        return dto;

    }
}
