package com.engjoy.Dto;

import jakarta.validation.constraints.NotEmpty;
import lombok.Getter;
import lombok.Setter;
import org.hibernate.validator.constraints.Length;

@Getter
@Setter
public class PasswordChangeDto {
    @NotEmpty
    @Length(min=8,max=32,
    message="영문 대문자/영문 소문자/숫자/기호 중 2가지 이상 조합하여 8~32자로 입력.")
    private String password;

}
