package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class UserGameData {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name="userGameData_id")
    private Long Id;
    private String charImgUrl;
    private int game1Score;
    private int game2Score;
    private int gold;
    private Long ranking;
    private float rankingPercent;
    @OneToOne
    @MapsId
    @JoinColumn(name="account_id")
    private Account account;
}
