package com.engjoy.repository;

import com.engjoy.entity.AchievementDesc;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface AchievementDescRepository extends JpaRepository<AchievementDesc, Integer> {
}
