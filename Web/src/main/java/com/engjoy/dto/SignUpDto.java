package com.engjoy.dto;

import jakarta.validation.constraints.NotEmpty;
import lombok.Getter;
import lombok.Setter;
import org.hibernate.validator.constraints.Length;
import org.springframework.format.annotation.DateTimeFormat;

import java.time.LocalDate;

@Getter
@Setter
public class SignUpDto {
    @NotEmpty
    private String email;
    @NotEmpty
    @Length(min=8, max=32, message="영문 대문자, 소문자,숫자,기호 중 2가지 이상 조합하여 8~32자로 입력.")
    private String password;
    @NotEmpty
    private String nickname;
    @NotEmpty
    private String name;
    @DateTimeFormat(pattern = "yyyy-MM-dd")
    private LocalDate birth;

    public String getEmail() { return email; }
    public String getNickname() { return nickname; }
    public LocalDate getBirth() {
        return birth;}
    public String getName(){
        return name;
    }
    public String getPassword(){
        return password;
    }
}
