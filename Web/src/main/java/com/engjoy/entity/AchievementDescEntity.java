package com.engjoy.entity;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import lombok.Getter;
import lombok.Setter;
import org.springframework.boot.autoconfigure.web.WebProperties;

@Entity
@Getter
@Setter
public class AchievementDescEntity {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long adId;
    private String achvName;
    private String achvDesc;
}
