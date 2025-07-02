package com.engjoy.control;



import com.engjoy.Dto.SignUpDto;
import com.engjoy.service.AccountService;
import jakarta.validation.Valid;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.validation.BindingResult;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;


import java.util.Optional;


@Controller

public class HomeController {
    private final AccountService accountService;

    public HomeController(AccountService accountService) {
        this.accountService = accountService;
    }

    @GetMapping("/")
    public String index() {

        return "index";
    }

    @GetMapping("/login")
    public String loginPage() {

        return "login";
    }


    @GetMapping("/mainPage")
    public String mainPage() {
        return "mainPage";
    }

    @GetMapping("/signUp")
    public String signUpPage() {
        return "signUp";
    }

    @PostMapping("/signUp")
    public String signUp(@Valid SignUpDto signUpDto,
                         BindingResult bindingResult,
                         Model model) {

        // 유효성 검사 실패 시 다시 회원가입 페이지로
        if (bindingResult.hasErrors()) {
            return "signUp"; // signUp.html로 돌아감
        }

        // 중복 이메일, 닉네임 등의 비즈니스 로직 검사 예시
        if (accountService.existsByEmail(signUpDto.getEmail())) {
            model.addAttribute("error", "이미 사용 중인 이메일입니다.");
            return "signUp";
        }

        if (accountService.existsByNickname(signUpDto.getNickname())) {
            model.addAttribute("error", "이미 사용 중인 닉네임입니다.");
            return "signUp";
        }
        accountService.insert(signUpDto);

        // 5. 회원가입 완료 → 로그인 페이지로 이동
        return "redirect:/login";

    }
}



    







