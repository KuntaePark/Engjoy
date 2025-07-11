package com.engjoy.service;

import com.engjoy.entity.Account;
import com.engjoy.entity.PasswordResetToken;
import com.engjoy.repository.AccountRepository;
import com.engjoy.repository.PasswordResetTokenRepository;
import org.springframework.mail.SimpleMailMessage;
import org.springframework.mail.javamail.JavaMailSender;
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
    private final JavaMailSender mailSender;

    public PasswordResetService(AccountRepository accountRepository,
                                PasswordResetTokenRepository passwordResetTokenRepository,
                                JavaMailSender mailSender) {
        this.accountRepository = accountRepository;
        this.passwordResetTokenRepository = passwordResetTokenRepository;
        this.mailSender = mailSender;
    }

    public boolean sendResetLink(String email) {
        Optional<Account> optionalAccount = accountRepository.findByEmail(email);

        if (optionalAccount.isPresent()) {
            Account account = optionalAccount.get();

            // 1. 토큰 생성 및 저장
            String token = UUID.randomUUID().toString();
            LocalDateTime expiry = LocalDateTime.now().plusHours(1);

            PasswordResetToken resetToken = new PasswordResetToken();
            resetToken.setToken(token);
            resetToken.setAccount(account);
            resetToken.setExpiryDate(expiry);

            passwordResetTokenRepository.save(resetToken);


            String resetLink = "http://localhost/reset-password?token=" + token;

            SimpleMailMessage message = new SimpleMailMessage();
            message.setTo(email);
            message.setFrom("layum@naver.com");
            message.setSubject("비밀번호 재설정 안내");
            message.setText("다음 링크를 클릭하여 비밀번호를 재설정하세요:\n\n" + resetLink);

            mailSender.send(message);

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
                account.setPassword(newPassword);
                accountRepository.save(account);
                return true;
            }
        }

        return false;
    }
}
