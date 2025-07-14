package com.engjoy.dto;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class PasswordResetDto {
    private String token;
    private String newPassword;

    public String getToken() {
        return token;
    }

    public String getNewPassword() {
        return newPassword;
    }
}
