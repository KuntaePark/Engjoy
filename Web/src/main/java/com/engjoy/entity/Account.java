package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

import java.time.LocalDate;

@Entity
@Getter
@Setter
public class Account {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name="account_id")
    private Long id;
    @Column(nullable=false)
    private String email;
    @Column(nullable=false)
    private String password;
    @Column(nullable=false)
    private String name;
    @Column(nullable=false)
    private String nickname;
    @Column(nullable=false)
    private LocalDate birth;

    @OneToOne(cascade = CascadeType.ALL,mappedBy = "account")
    @PrimaryKeyJoinColumn
    private UserGameData userGameData;
}

