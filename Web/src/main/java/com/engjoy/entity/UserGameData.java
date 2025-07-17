package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;
import lombok.ToString;

@Entity
@Getter
@Setter
@ToString
public class UserGameData {
    @Id
    @Column(name="account_id")
    private Long id;
    private String charImgUrl = "";
    private int game1Score = 0;
    private int game2HighScore = 0;
    private int gold = 0;
    private Long ranking = 0L;
    private float rankingPercent = 0f;

    private int bodyTypeIndex = 0;
    private int weaponTypeIndex = 0;
    @OneToOne
    @MapsId
    @JoinColumn(name="account_id")
    private Account account;
}
