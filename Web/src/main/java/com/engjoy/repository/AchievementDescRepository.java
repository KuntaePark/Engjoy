package com.engjoy.repository;

import com.engjoy.entity.AchievementDesc;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface AchievementDescRepository extends JpaRepository<AchievementDesc ,Long> {
    List<AchievementDesc> findAll();

}
