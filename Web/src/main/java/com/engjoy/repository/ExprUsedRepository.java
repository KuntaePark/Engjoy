package com.engjoy.repository;

import com.engjoy.constant.EXPRTYPE;
import com.engjoy.entity.Account;
import com.engjoy.entity.ExprUsed;
import com.engjoy.entity.Expression;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.time.LocalDateTime;
import java.util.List;
import java.util.Set;

@Repository
public interface ExprUsedRepository extends JpaRepository<ExprUsed,Long> {
//    @Query("SELECT eu FROM ExprUsed eu WHERE eu.account = :account " +
//            "AND eu.expression.exprType = :exprType ORDER BY eu.useTime DESC")
//    Page<ExprUsed> findRecentUsed(
//            @Param("account")Account account,
//            @Param("exprType")EXPRTYPE exprType,
//            Pageable pageable);

    boolean existsByAccountAndExpression(Account account, Expression expression);

    @Query("SELECT eu.expression.id FROM ExprUsed eu WHERE eu.account = :account AND eu.expression.id IN :expressionIds")
    Set<Long> findUsedExpressionIdsByAccountAndExpressionIds(@Param("account") Account account, @Param("expressionIds") List<Long> expressionIds);

    @Query("SELECT eu FROM ExprUsed eu JOIN FETCH eu.expression " +
            "WHERE eu.account = :account " +
            "AND eu.expression.exprType = :exprType " +
            "AND eu.usedTime BETWEEN :start AND :end")
    Page<ExprUsed> findUsedByDateRangeFetchExpr(
            @Param("account") Account account,
            @Param("exprType") EXPRTYPE exprType,
            @Param("start") LocalDateTime start,
            @Param("end") LocalDateTime end,
            Pageable pageable);


    @Query("SELECT COUNT(eu) FROM ExprUsed eu " +
            "WHERE eu.account = :account " +
            "AND eu.usedTime >= :startDate " +
            "AND eu.usedTime < :endDate")
    Long countUsedByDateRange(
            @Param("account") Account account,
            @Param("startDate") LocalDateTime startDate,
            @Param("endDate") LocalDateTime endDate);

    @Query("SELECT COUNT(eu) FROM ExprUsed eu " +
            " WHERE eu.account = :account " +
            "AND eu.expression.exprType = :exprType " +
            "AND (:startDate IS NULL OR eu.usedTime >= :startDate) " +
            "AND (:endDate IS NULL OR eu.usedTime < :endDate)")
    Long countUsedByTypeAndDateRange(
            @Param("account") Account account,
            @Param("exprType") EXPRTYPE exprType,
            @Param("startDate") LocalDateTime startDate,
            @Param("endDate") LocalDateTime endDate);


}
