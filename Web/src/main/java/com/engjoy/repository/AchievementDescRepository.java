package com.engjoy.repository;

import com.engjoy.entity.AchievementDesc;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.List;


public interface AchievementDescRepository extends JpaRepository<AchievementDesc ,Long> {
    List<AchievementDesc> findAll();

}
