package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Getter
@Setter
public class AchievementDesc {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name="ad_id")
    private Long id;
    private String achvName;
    private String achvDesc;
}
