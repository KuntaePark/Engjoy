package com.engjoy.dto;

import com.engjoy.entity.UserGameData;
import lombok.Getter;
import lombok.Setter;

@Getter@Setter
public class LobbyAuthDataDto {
    private long id;
    private int bodyTypeIndex;
    private int weaponTypeIndex;

    public static LobbyAuthDataDto from(UserGameData userGameData) {
        LobbyAuthDataDto lobbyAuthDataDto = new LobbyAuthDataDto();
        lobbyAuthDataDto.setId(userGameData.getId());
        lobbyAuthDataDto.setBodyTypeIndex(userGameData.getBodyTypeIndex());
        lobbyAuthDataDto.setWeaponTypeIndex(userGameData.getWeaponTypeIndex());

        return lobbyAuthDataDto;
    }
}
