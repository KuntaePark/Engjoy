package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class UserAchievement {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name="userAchievement_id")
    private Long Id;
    @ManyToOne
    @JoinColumn(name="account_id")
    private Account accountId;
    @ManyToOne
    @JoinColumn(name="ad_id")
    private AchievementDesc achievementDescId;
}
