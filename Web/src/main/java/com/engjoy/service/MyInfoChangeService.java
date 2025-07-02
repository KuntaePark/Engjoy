package com.engjoy.service;

import com.engjoy.repository.AccountRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

@Service
@RequiredArgsConstructor
public class MyInfoChangeService {
    private final AccountRepository accountRepository;

    public MyInfoChangeService(AccountRepository accountRepository){
        this.accountRepository = accountRepository;
    }
    public boolean emailCheck(String email){
        return accountRepository.existsByEmail(email);

    }
    public boolean nicknameCheck(String nickname){
        return accountRepository.existsByNickname(nickname);
    }
}
