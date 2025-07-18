package com.engjoy.service;
import com.engjoy.repository.AccountRepository;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;

import java.util.Optional;

@Service
public class PasswordChangeService {

    private final AccountRepository accountRepository;
    private final PasswordEncoder passwordEncoder;

    public PasswordChangeService(AccountRepository accountRepository, PasswordEncoder passwordEncoder){
        this.accountRepository = accountRepository;
        this.passwordEncoder = passwordEncoder;
    }

    public boolean passwordCheck(Long accountId, String inputPassword){
        Optional<String> encodedPasswordOpt = accountRepository.findPasswordById(accountId);

        if (encodedPasswordOpt.isEmpty()) {
            return false;
        }

        String encodedPassword = encodedPasswordOpt.get();


        return passwordEncoder.matches(inputPassword, encodedPassword);
    }
}
