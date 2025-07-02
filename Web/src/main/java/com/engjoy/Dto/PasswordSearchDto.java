package com.engjoy.Dto;

import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotEmpty;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class PasswordSearchDto {
    @NotEmpty
    @Email
    private String email;
}
