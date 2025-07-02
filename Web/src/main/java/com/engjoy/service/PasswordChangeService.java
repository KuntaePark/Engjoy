package com.engjoy.service;

import com.engjoy.repository.AccountRepository;
import org.springframework.stereotype.Service;

@Service
public class PasswordChangeService {

    private final AccountRepository accountRepository;

    public PasswordChangeService(AccountRepository accountRepository){
        this.accountRepository=accountRepository;
    }

    public boolean passwordCheck(String password){
        return accountRepository.findPasswordById(password);
    }

}
