package com.engjoy.control;

import com.engjoy.Dto.MyInfoChangeDto;
import com.engjoy.entity.Account;
import com.engjoy.repository.AccountRepository;
import com.engjoy.service.AccountService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;

import java.security.Principal;
import java.util.Optional;


@Controller
public class MyPageController {

    private final AccountService accountService;
    private final PasswordEncoder passwordEncoder;
    private final AccountRepository accountRepository;

    public MyPageController(PasswordEncoder passwordEncoder,
                            AccountRepository accountRepository,
                            AccountService accountService) {
        this.passwordEncoder = passwordEncoder;
        this.accountRepository = accountRepository;
        this.accountService = accountService;
    }

    @GetMapping("/myPage")
    public String myPage(Model model, Principal principal) {
        if (principal != null) {
            String email = principal.getName();
            Account account = accountService.findByEmail(email)
                    .orElseThrow(() -> new RuntimeException("사용자 정보 없음"));
            model.addAttribute("nickname", account.getNickname());
            return "myPage";
        } else {
            // principal이 null이면 로그인 상태 아님 → login 페이지로
            return "redirect:/login";
        }
    }

    @GetMapping("/passwordSearch")
    public String passwordSearchPage(Model model){
        return "passwordSearch";
    }

    @PostMapping("/passwordSearch")
    public String passwordSearch(@RequestParam("email") String email, Model model){
        Optional<Account> optionalAccount = accountRepository.findByEmail(email);
        return "passwordSearch";
    }

    @GetMapping("/passwordChange")
    public String passwordChangePage(Model model){
        return "passwordChange";
    }


    @GetMapping("/myInfoChange")
    public String myInfoChangePage(Model model){
        return "myInfoChange";
    }
    @PostMapping("/myInfoChange")
    public String myInfoChange(@RequestParam("email") String email, @RequestParam("nickname") String nickname,
                               MyInfoChangeDto myInfoChangeDto, Model model){
        Optional<Account> optionalAccount = accountRepository.findByEmail(email);
        if (optionalAccount.isPresent()) {
            Account account = optionalAccount.get();

            // 정보 업데이트
            account.setNickname(nickname);  // 따로 받았으니 이건 따로 처리


            // 비밀번호 암호화 후 저장
            String rawPassword = myInfoChangeDto.getPassword();
            String encodedPassword = passwordEncoder.encode(rawPassword);
            account.setPassword(encodedPassword);

            // 저장
            accountRepository.save(account);

            model.addAttribute("message", "개인정보가 수정되었습니다.");
        } else {
            model.addAttribute("error", "해당 이메일을 가진 사용자를 찾을 수 없습니다.");
        }

        return "myInfoChange"; // 다시 개인정보 수정 페이지로 이동
    }
    }



