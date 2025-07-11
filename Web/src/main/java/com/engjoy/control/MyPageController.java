package com.engjoy.control;

import com.engjoy.Dto.MyInfoChangeDto;
import com.engjoy.entity.Account;
import com.engjoy.repository.AccountRepository;
import com.engjoy.service.AccountService;
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
            String email = principal.getName(); // 현재 로그인된 사용자의 이메일
            Account account = accountService.findByEmail(email)
                    .orElseThrow(() -> new RuntimeException("사용자 정보를 찾을 수 없습니다."));

            model.addAttribute("nickname", account.getNickname());
            model.addAttribute("email", account.getEmail());
        }
        return "myPage";
    }


    @GetMapping("/passwordSearch")
    public String passwordSearchPage() {

        return "passwordSearch";
    }

    @PostMapping("/passwordSearch")
    public String passwordSearch(@RequestParam("email") String email, Model model) {
        Optional<Account> optionalAccount = accountRepository.findByEmail(email);
        return "passwordSearch";
    }

    @GetMapping("/passwordChange")
    public String passwordChangePage(Model model) {
        return "passwordChange";
    }


    @GetMapping("/myInfoChange")
    public String myInfoChangePage(Model model, Principal principal) {
        if (principal != null) {
            String email = principal.getName();
            Account account = accountService.findByEmail(email)
                    .orElseThrow(() -> new RuntimeException("사용자 정보를 찾을 수 없습니다."));

            model.addAttribute("nickname", account.getNickname());
        } else {
            model.addAttribute("nickname", "");
        }
        return "myInfoChange";
    }

    @PostMapping("/myInfoChange")
    public String myInfoChange(@RequestParam("email") String email, @RequestParam("nickname") String nickname,
                               MyInfoChangeDto myInfoChangeDto, Principal principal, Model model) {
        String currentEmail = principal.getName();
        Optional<Account> optionalAccount = accountRepository.findByEmail(currentEmail);
        if (optionalAccount.isPresent()) {
            Account account = optionalAccount.get();

            account.setEmail(email);
            account.setNickname(nickname);

            String rawPassword = myInfoChangeDto.getPassword();
            if (rawPassword != null && !rawPassword.trim().isEmpty()) {
                String encodedPassword = passwordEncoder.encode(rawPassword);
                account.setPassword(encodedPassword);
            }

            accountRepository.save(account);
            model.addAttribute("message", "개인정보가 수정되었습니다.");
        } else {
            model.addAttribute("error", "해당 이메일을 가진 사용자를 찾을 수 없습니다.");
        }

        return "redirect:/logout";
    }

    @PostMapping("/passwordChange")
    public String passwordChange(@RequestParam("password") String password,
                                 Principal principal) {
        String email = principal.getName();
        Optional<Account> optionalAccount = accountRepository.findByEmail(email);

        if (optionalAccount.isPresent()) {
            Account account = optionalAccount.get();
            String encodedPassword = passwordEncoder.encode(password);
            account.setPassword(encodedPassword);
            accountRepository.save(account);

            return "redirect:/logout";
        } else {
            return "error";
        }
    }





}



