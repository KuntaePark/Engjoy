package com.engjoy.dto;

import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotEmpty;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class MyInfoChangeDto {
    @NotEmpty
    @Email
    private String email;
    @NotEmpty
    private String nickname;
    @NotEmpty
    private String password;


    public String getPassword(){return password;}
}
