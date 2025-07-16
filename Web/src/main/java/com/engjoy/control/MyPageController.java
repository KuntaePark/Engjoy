package com.engjoy.control;

import com.engjoy.dto.MyInfoChangeDto;
import com.engjoy.dto.SignUpDto;
import com.engjoy.dto.UserGameDataDto;
import com.engjoy.entity.Account;
import com.engjoy.entity.UserGameData;
import com.engjoy.repository.AccountRepository;
import com.engjoy.security.CustomUserDetails;
import com.engjoy.service.AccountService;
import com.engjoy.service.GameService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.validation.BindingResult;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;

import java.lang.reflect.Member;
import java.security.Principal;
import java.util.Optional;


@Controller
@RequiredArgsConstructor
public class MyPageController {

    private final AccountService accountService;
    private final GameService gameService;
    private final PasswordEncoder passwordEncoder;
    private final AccountRepository accountRepository;


    @GetMapping("/myPage")
    public String myPage(Model model, Principal principal) {
        if (principal != null) {
            String email = principal.getName(); // 현재 로그인된 사용자의 이메일
            UserGameDataDto userGameData = gameService.getUserGameData(email);

            model.addAttribute("nickname", userGameData.getNickname());
            model.addAttribute("email", email);
            model.addAttribute("game1score", userGameData.getGame1Score());
            model.addAttribute("game2score", userGameData.getGame2Score());
            model.addAttribute("ranking", userGameData.getRanking());
        }
        return "myPage";
    }


    @GetMapping("/passwordSearch")
    public String passwordSearchPage() {

        return "passwordSearch";
    }


    @GetMapping("/passwordChange")
    public String passwordChangePage(Model model, Principal principal) {
        String email = principal.getName();
        Account account = accountRepository.findByEmail(email).orElseThrow();
        model.addAttribute("nickname", account.getNickname());


        return "passwordChange";
    }


    @GetMapping("/myInfoChange")
    public String myInfoChangePage(Model model, Principal principal) {
        String email = principal.getName();
        Account account = accountRepository.findByEmail(email).orElseThrow();
        model.addAttribute("signUpDto", SignUpDto.from(account));

        return "myInfoChange";
    }

    @PostMapping("/myInfoChange")
    public String myInfoChange(@Valid SignUpDto signUpDto,
                               @AuthenticationPrincipal CustomUserDetails userDetails,
                                Model model, BindingResult bindingResult) {
        String email = userDetails.getUsername();
        String nickname = userDetails.getNickname();
        if (accountService.existsByEmail(signUpDto.getEmail()) && !email.equals(signUpDto.getEmail())) {
            bindingResult.rejectValue("email","email.using","이미 사용중인 이메일입니다.");
        }



        if (accountService.existsByNickname(signUpDto.getNickname()) && !nickname.equals(signUpDto.getNickname())) {
            bindingResult.rejectValue("nickname","nickname.using", "이미 사용 중인 닉네임입니다.");
        }

        if(!signUpDto.getPassword().equals(signUpDto.getConfirmPassword())) {
            bindingResult.rejectValue("confirmPassword","confirmPassword.mismatch", "비밀번호가 일치하지 않습니다.");
        }

        if (bindingResult.hasErrors()) {
            return "signUp"; // signUp.html로 돌아감
        }

        accountService.insert(signUpDto);

        return "redirect:/myPage";
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



