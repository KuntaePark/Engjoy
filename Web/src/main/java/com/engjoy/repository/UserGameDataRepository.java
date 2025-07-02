package com.engjoy.repository;

import com.engjoy.entity.UserGameData;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;


import java.util.Optional;

public interface UserGameDataRepository extends JpaRepository<UserGameData, Long> {

        @Query("SELECT a.charImgUrl FROM UserGameData a WHERE a.account.id = :accountId")
        Optional<String> findCharImgUrlByAccountId(@Param("accountId") Long accountId);

        @Query("SELECT a.game1Score FROM UserGameData a WHERE a.account.id = :accountId")
        Optional<String> findGame1ScoreByAccountId(@Param("accountId") Long accountId);

        @Query("SELECT a.game2HighScore FROM UserGameData a WHERE a.account.id = :accountId")
        Optional<String> findGame2HighScoreByAccountId(@Param("accountId") Long accountId);

        @Query("SELECT a.ranking FROM UserGameData a WHERE a.account.id = :accountId")
        Optional<String> findRankingByAccountId(@Param("accountId") Long accountId);
    }


