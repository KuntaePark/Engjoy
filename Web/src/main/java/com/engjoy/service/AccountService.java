package com.engjoy.service;

import com.engjoy.Dto.SignUpDto;
import com.engjoy.entity.Account;
import com.engjoy.repository.AccountRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;

import java.util.Optional;

@Service

public class AccountService {

    private final AccountRepository accountRepository;

    @Autowired
    private PasswordEncoder passwordEncoder;

    public AccountService(AccountRepository accountRepository) {
        this.accountRepository = accountRepository;
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
        Account account = new Account();
        account.setEmail(signUpDto.getEmail());
        account.setPassword(signUpDto.getPassword());
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



    }



