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
        // 1. DB에서 암호화된 비밀번호 조회
        Optional<String> encodedPasswordOpt = accountRepository.findPasswordById(accountId);

        if (encodedPasswordOpt.isEmpty()) {
            return false; // 계정이 없으면 실패
        }

        String encodedPassword = encodedPasswordOpt.get();

        // 2. 입력한 비밀번호와 암호화된 비밀번호 비교
        return passwordEncoder.matches(inputPassword, encodedPassword);
    }
}
