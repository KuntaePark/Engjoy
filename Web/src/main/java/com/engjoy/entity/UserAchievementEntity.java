package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Getter
@Setter
public class UserAchievementEntity {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long userAchievementId;
    @ManyToOne
    @JoinColumn(name="account_id")
    private AccountEntity accountId;
    @ManyToOne
    @JoinColumn(name="ad_id")
    private AchievementDescEntity achievementDescId;
}
