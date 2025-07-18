package com.engjoy.repository;

import com.engjoy.entity.UserGameData;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;


import java.util.Optional;

@Repository
public interface UserGameDataRepository extends JpaRepository<UserGameData, Long> {
        UserGameData findByAccount_Email(String email);

        @Query("SELECT a.charImgUrl FROM UserGameData a WHERE a.account.id = :accountId")
        Optional<String> findCharImgUrlByAccountId(@Param("accountId") Long accountId);

        @Query("SELECT a.game1Score FROM UserGameData a WHERE a.account.id = :accountId")
        Optional<String> findGame1ScoreByAccountId(@Param("accountId") Long accountId);

        @Query("SELECT a.game2HighScore FROM UserGameData a WHERE a.account.id = :accountId")
        Optional<String> findGame2HighScoreByAccountId(@Param("accountId") Long accountId);

        @Query("SELECT a.ranking FROM UserGameData a WHERE a.account.id = :accountId")
        Optional<String> findRankingByAccountId(@Param("accountId") Long accountId);

        //랭킹 산정용

    }
