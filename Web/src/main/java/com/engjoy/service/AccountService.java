package com.engjoy.service;

import com.engjoy.dto.SignUpDto;
import com.engjoy.entity.Account;
import com.engjoy.repository.AccountRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.security.crypto.password.PasswordEncoder;
import com.engjoy.entity.Account;
import com.engjoy.repository.AccountRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.security.core.userdetails.User;
import org.springframework.security.core.userdetails.UserDetails;
import org.springframework.security.core.userdetails.UserDetailsService;
import org.springframework.security.core.userdetails.UsernameNotFoundException;
import org.springframework.stereotype.Service;

import java.util.Optional;

@Service
public class AccountService implements UserDetailsService {

    private final AccountRepository accountRepository;
    private final PasswordEncoder passwordEncoder;

    @Override
    public UserDetails loadUserByUsername(String username) throws UsernameNotFoundException {
        //해당 유저의 이메일로
        Account account = accountRepository.findByEmail(username).get();

        if(account == null) {
            throw new UsernameNotFoundException(username);
        }

        return User.builder()
                .username(account.getEmail())
                .password(account.getPassword())
                .build();
    }


    public AccountService(AccountRepository accountRepository, PasswordEncoder passwordEncoder) {
        this.accountRepository = accountRepository;
        this.passwordEncoder = passwordEncoder;
    }


    public Optional<Account> findByEmail(String email) {
        return accountRepository.findByEmail(email);
    }


    public boolean existsByEmail(String email) {
        return accountRepository.existsByEmail(email);
    }

    public boolean existsByNickname(String nickname) {
        return accountRepository.existsByNickname(nickname);
    }





    public void insert(SignUpDto signUpDto) {
        System.out.println("=== 회원가입 정보 확인 ===");
        System.out.println("이메일: " + signUpDto.getEmail());
        System.out.println("비밀번호: " + signUpDto.getPassword());
        System.out.println("이름: " + signUpDto.getName());
        System.out.println("닉네임: " + signUpDto.getNickname());
        System.out.println("생일: " + signUpDto.getBirth());
        Account account = new Account();
        account.setEmail(signUpDto.getEmail());
        account.setPassword(passwordEncoder.encode(signUpDto.getPassword()));
        account.setNickname(signUpDto.getNickname());
        account.setName(signUpDto.getName());
        account.setBirth(signUpDto.getBirth());

        accountRepository.save(account);
    }

    public boolean passwordCheck(Long id, String password){
        Optional<String> optionalPassword = accountRepository.findPasswordById(id);

        if (optionalPassword.isEmpty()) {
            return false;
        }
        String storedPassword = optionalPassword.get();


        return passwordEncoder.matches(password, storedPassword);


    }
    public Account login(String email, String rawPassword) {
        Account account = accountRepository.findByEmail(email)
                .orElseThrow(() -> new IllegalArgumentException("존재하지 않는 이메일입니다."));

        if (!passwordEncoder.matches(rawPassword, account.getPassword())) {
            throw new IllegalArgumentException("비밀번호가 일치하지 않습니다.");
        }

        return account;
    }






}



