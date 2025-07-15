package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

import java.time.LocalDateTime;


@Entity
@Getter
@Setter
public class PasswordResetToken {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private String token;

    @OneToOne
    private Account account;

    private LocalDateTime expiryDate;

    public void setToken(String token) {
    }

    public void setAccount(Account account) {
    }

    public void setExpiryDate(LocalDateTime expiry) {
    }

    public LocalDateTime getExpiryDate() {
        return expiryDate;
    }

    public Account getAccount() {
        return account;
    }
}
