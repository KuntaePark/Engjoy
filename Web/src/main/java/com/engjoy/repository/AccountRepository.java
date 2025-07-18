package com.engjoy.repository;

import com.engjoy.entity.Account;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.util.Optional;

@Repository
public interface AccountRepository extends JpaRepository<Account, Long > {
    @Query("SELECT a.email FROM Account a WHERE a.id = :id")
    Optional<String> findEmailById(@Param("id") Long id);


    @Query("SELECT a.password FROM Account a WHERE a.id = :id")
    Optional<String> findPasswordById(@Param("id") Long id);

    @Query("SELECT a.nickname FROM Account a WHERE a.id = :id")
    Optional<String> findNicknameById(@Param("id") Long id);

    Optional<Account> findByEmail(String email);

    boolean existsByEmail(String email);
    boolean existsByNickname(String nickname);


}
