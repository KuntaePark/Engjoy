package com.engjoy.service;

import com.engjoy.entity.AchievementDesc;
import com.engjoy.repository.AchievementDescRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
@RequiredArgsConstructor
public class AchievementService {

    private final AchievementDescRepository achievementDescRepository;

    public AchievementService(AchievementDescRepository achievementDescRepository) {
        this.achievementDescRepository = achievementDescRepository;
    }

    public List<AchievementDesc> getAllAchievements() {
        return achievementDescRepository.findAll();

}



}
