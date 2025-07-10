package com.engjoy.repository;

import com.engjoy.entity.Account;
import com.engjoy.entity.Expression;
import com.engjoy.entity.IncorrectExpr;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

@Repository
public interface IncorrectExprRepository extends JpaRepository<IncorrectExpr,Long> {
    Page<IncorrectExpr> findByAccount(Account account, Pageable pageable);
    Long countByAccountAndUsedTimeBetween(Account account, LocalDateTime startDate, LocalDateTime endDate);
    Optional<IncorrectExpr> findByAccountAndExpression(Account account, Expression expression);
    List<IncorrectExpr> findByAccount(Account account);

    @Query("SELECT ie FROM IncorrectExpr ie " +
            "WHERE ie.account = :account " +
            "AND ie.incorrectCount >= :minCount " +
            "AND (ie.lastRecommendedDate IS NULL OR ie.lastRecommendedDate < :today) " +
            "ORDER BY ie.incorrectCount DESC, ie.usedTime ASC")
    List<IncorrectExpr> findTopWordDaily(
            @Param("account") Account account,
            @Param("minCount") int minCount,
            @Param("today") LocalDate today,
            Pageable pageable);

    @Query(
            value = """
        SELECT DATE_FORMAT(i.used_time, '%Y-%m-%d') AS day,
               COUNT(*) AS cnt
          FROM incorrect_expr i
         WHERE i.account_id = :accountId
           AND i.used_time BETWEEN :start AND :end
         GROUP BY day
         ORDER BY day
      """,
            nativeQuery = true
    )
    List<Object[]> countIncorrectPerDayNative(
            @Param("accountId") Long accountId,
            @Param("start")     LocalDateTime start,
            @Param("end")       LocalDateTime end
    );

}
