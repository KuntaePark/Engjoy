package com.engjoy.service;

import com.engjoy.entity.Account;
import com.engjoy.entity.PasswordResetToken;
import com.engjoy.repository.AccountRepository;
import com.engjoy.repository.PasswordResetTokenRepository;
import org.springframework.stereotype.Service;

import java.time.Instant;
import java.time.LocalDateTime;
import java.time.chrono.ChronoLocalDateTime;
import java.util.Optional;
import java.util.UUID;

@Service
public class PasswordResetService {

    private final AccountRepository accountRepository;
    private final PasswordResetTokenRepository passwordResetTokenRepository;

    public PasswordResetService(AccountRepository accountRepository, PasswordResetTokenRepository passwordResetTokenRepository) {
        this.accountRepository = accountRepository;
        this.passwordResetTokenRepository = passwordResetTokenRepository;
    }

    public boolean sendResetLink(String email) {
        Optional<Account> optionalAccount = accountRepository.findByEmail(email);

        if (optionalAccount.isPresent()) {
            Account account = optionalAccount.get();

            String token = UUID.randomUUID().toString();
            LocalDateTime expiry = LocalDateTime.now().plusHours(1); // 1시간 유효

            PasswordResetToken resetToken = new PasswordResetToken();
            resetToken.setToken(token);
            resetToken.setAccount(account);
            resetToken.setExpiryDate(expiry);
            passwordResetTokenRepository.save(resetToken);

            // 이메일 전송은 여기에서 생략하거나 구현
            System.out.println("비밀번호 재설정 링크: http://localhost:8080/reset-password?token=" + token);

            return true;
        }

        return false;
    }

    public boolean resetPassword(String token, String newPassword) {
        Optional<PasswordResetToken> optionalToken = passwordResetTokenRepository.findByToken(token);

        if (optionalToken.isPresent()) {
            PasswordResetToken resetToken = optionalToken.get();

            if (resetToken.getExpiryDate().isAfter(ChronoLocalDateTime.from(Instant.from(LocalDateTime.now())))) {
                Account account = resetToken.getAccount();
                account.setPassword(newPassword); // 실제로는 암호화 필요 (예: BCrypt)
                accountRepository.save(account);
                return true;
            }
        }

        return false;
    }
}
