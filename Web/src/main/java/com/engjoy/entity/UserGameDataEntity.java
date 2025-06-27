package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Getter
@Setter
public class UserGameDataEntity {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long accountId;
    private String charImgUrl;
    private int game1Score;
    private int game2Score;
    private int gold;
    private Long ranking;
    private float rankingPercent;



}
