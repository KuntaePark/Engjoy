package com.engjoy.control;



import com.engjoy.Dto.SignUpDto;
import com.engjoy.entity.Account;
import com.engjoy.repository.AccountRepository;
import com.engjoy.service.AccountService;
import jakarta.validation.Valid;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.validation.BindingResult;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.ResponseBody;


import java.security.Principal;
import java.util.Map;
import java.util.Optional;


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

        return "index";
    }




    @GetMapping("/mainPage")
    public String mainPage(Model model, Principal principal) {
        if (principal != null) {
            String email = principal.getName();
            System.out.println("▶ 로그인된 사용자 이메일: " + email); // 디버깅용 로그

            Account account = accountService.findByEmail(email)
                    .orElseThrow(() -> new RuntimeException("사용자 정보를 찾을 수 없습니다."));

            System.out.println("▶ 닉네임: " + account.getNickname()); // 닉네임 확인

            model.addAttribute("nickname", account.getNickname());
        } else {
            System.out.println("▶ principal이 null입니다 (로그인 안됨)");
        }

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

    @GetMapping("/agree")
    public String goToAgree(){
        return "agree";
    }

    @GetMapping("/gameInfo")
    public String goToGameInfo(Model model, Principal principal){
        if (principal != null) {
            String email = principal.getName();
            Account account = accountService.findByEmail(email)
                    .orElseThrow(() -> new RuntimeException("사용자 정보를 찾을 수 없습니다."));
            model.addAttribute("nickname", account.getNickname());
        }
        return "gameInfo";
    }


    @GetMapping("/pwChangeDone")
    public String goToPwChangeDone(){
        return "pwChangeDone";
    }

    @GetMapping("/serviceInfo")
    public String goToServiceInfo(Model model, Principal principal){
        if (principal != null) {
            String email = principal.getName();
            Account account = accountService.findByEmail(email)
                    .orElseThrow(() -> new RuntimeException("사용자 정보를 찾을 수 없습니다."));
            model.addAttribute("nickname", account.getNickname());
        }
        return "serviceInfo";
    }



    @GetMapping("/signUpFinished")
    public String goToSignUpFinished(){
        return "signUpFinished";
    }
    @PostMapping("/signUpCheck")
    public String signUpCheck(@Valid SignUpDto signUpDto,
                              BindingResult bindingResult,
                              Model model) {
        // 회원가입 로직 작성
        return "redirect:/login";
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

}



    







