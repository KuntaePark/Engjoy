package com.engjoy.control;



import com.engjoy.dto.SignUpDto;
import com.engjoy.entity.Account;
import com.engjoy.repository.AccountRepository;
import com.engjoy.service.AccountService;
import jakarta.validation.Valid;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.validation.BindingResult;
import org.springframework.validation.Errors;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.ResponseBody;


import java.security.Principal;
import java.util.List;
import java.util.Map;



@Controller

public class HomeController {


    private final PasswordEncoder passwordEncoder;
    private final AccountService accountService;


    public HomeController(PasswordEncoder passwordEncoder,
                          AccountService accountService,
                          AccountRepository accountRepository) {
        this.passwordEncoder = passwordEncoder;
        this.accountService = accountService;

    }


    @GetMapping("/")
    public String index() {
        return "mainPage";
    }


    @GetMapping("/mainPage")
    public String mainPage(Model model, Principal principal) {
        if (principal != null) {
            String email = principal.getName();
            System.out.println("▶ 로그인된 사용자 이메일: " + email);

            Account account = accountService.findByEmail(email)
                    .orElseThrow(() -> new RuntimeException("사용자 정보를 찾을 수 없습니다."));

            System.out.println("▶ 닉네임: " + account.getNickname());

            model.addAttribute("nickname", account.getNickname());
        } else {
            System.out.println("▶ principal이 null입니다 (로그인 안됨)");
        }

        return "mainPage";
    }

    @PostMapping("/agree")
    public String agreeToPolicy(@RequestParam(required = false, value="agreed") List<String> agreed,
                                Model model) {
        if(agreed == null || agreed.size() < 2) {
            //reject
            model.addAttribute("error", "모든 약관에 동의하셔야 합니다.");
            model.addAttribute("agreed", agreed);
            return "agree";

        }

        //동의
        model.addAttribute("signUpDto", new SignUpDto());
        return "signUp";
    }

    @PostMapping("/signUp")
    public String signUp(@Valid SignUpDto signUpDto,
                         BindingResult bindingResult,
                         Model model) {

        if (accountService.existsByEmail(signUpDto.getEmail())) {
            bindingResult.rejectValue("email","email.using","이미 사용중인 이메일입니다.");
        }

        if (accountService.existsByNickname(signUpDto.getNickname())) {
            bindingResult.rejectValue("nickname","nickname.using", "이미 사용 중인 닉네임입니다.");
        }

        if(!signUpDto.getPassword().equals(signUpDto.getConfirmPassword())) {
            bindingResult.rejectValue("confirmPassword","confirmPassword.mismatch", "비밀번호가 일치하지 않습니다.");
        }

        if (bindingResult.hasErrors()) {
            return "signUp"; // signUp.html로 돌아감
        }

        accountService.insert(signUpDto);

        return "redirect:/login";
    }

    @GetMapping("/agree")
    public String goToAgree() {
        return "agree";
    }

    @GetMapping("/gameInfo")
    public String goToGameInfo(Model model, Principal principal) {
        if (principal != null) {
            String email = principal.getName();
            Account account = accountService.findByEmail(email)
                    .orElseThrow(() -> new RuntimeException("사용자 정보를 찾을 수 없습니다."));
            model.addAttribute("nickname", account.getNickname());
        }
        return "gameInfo";
    }


    @GetMapping("/serviceInfo")
    public String goToServiceInfo(Model model, Principal principal) {
        if (principal != null) {
            String email = principal.getName();
            Account account = accountService.findByEmail(email)
                    .orElseThrow(() -> new RuntimeException("사용자 정보를 찾을 수 없습니다."));
            model.addAttribute("nickname", account.getNickname());
        }
        return "serviceInfo";
    }


    @GetMapping("/api/check-email")
    @ResponseBody
    public Map<String, Boolean> checkEmail(@RequestParam String email) {
        boolean exists = accountService.existsByEmail(email);
        return Map.of("exists", exists);
    }

    @GetMapping("/login")
    public String loginPage(@RequestParam(value = "error", required = false) String error,
                            Model model) {
        if (error != null) {
            model.addAttribute("error", "이메일 또는 비밀번호가 잘못되었습니다.");
        }
        return "login";
    }

    @GetMapping("/api/check-nickname")
    @ResponseBody
    public Map<String, Boolean> checkNickname(@RequestParam String nickname) {
        boolean exists = accountService.existsByNickname(nickname);
        return Map.of("exists", exists);
    }

    @GetMapping("/intro")
    public String introPage(){
        return "intro";
    }
}