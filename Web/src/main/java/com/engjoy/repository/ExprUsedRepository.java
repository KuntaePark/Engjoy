package com.engjoy.repository;

import com.engjoy.constant.EXPRTYPE;
import com.engjoy.entity.Account;
import com.engjoy.entity.ExprUsed;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.time.LocalDateTime;

@Repository
public interface ExprUsedRepository extends JpaRepository<ExprUsed,Long> {
    @Query("SELECT eu FROM ExprUsed eu WHERE eu.account = :account " +
            "AND eu.expression.exprType = :exprType ORDER BY eu.useTime DESC")
    Page<ExprUsed> findRecentUsed(
            @Param("account")Account account,
            @Param("exprType")EXPRTYPE exprType,
            Pageable pageable);

    @Query("SELECT eu FROM ExprUsed eu WHERE eu.account = :account " +
            "AND eu.expression.exprType = :exprType " +
            "AND eu.useTime BETWEEN :startDate AND :endDate ORDER BY eu.useTime DESC")
    Page<ExprUsed> findUsedByDateRange(
            @Param("account") Account account,
            @Param("exprType") EXPRTYPE exprType,
            @Param("startDate")LocalDateTime startDate,
            @Param("endDate") LocalDateTime endDate,
            Pageable pageable);

    @Query("SELECT COUNT(eu) FROM ExprUsed eu WHERE eu.account = :account AND eu.useTime BETWEEN : startDate AND :endDate")
    Long countUsedByDateRange(
            @Param("account") Account account,
            @Param("startDate") LocalDateTime startDate,
            @Param("endDate") LocalDateTime endDate);

    @Query("SELECT COUNT(eu) FROM ExprUsed eu WHERE eu.account = :account " +
            "AND eu.expression.exprType = :exprType AND eu.useTime BETWEEN :startDate AND : endDate")
    Long countUsedByTypeAndDateRange(
            @Param("account") Account account,
            @Param("exprType") EXPRTYPE exprType,
            @Param("startDate") LocalDateTime startDate,
            @Param("endDate") LocalDateTime endDate);

}
