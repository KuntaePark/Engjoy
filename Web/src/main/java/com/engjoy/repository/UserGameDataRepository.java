package com.engjoy.repository;

import com.engjoy.entity.UserGameData;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface UserGameDataRepository extends JpaRepository<UserGameData, Integer> {
}
