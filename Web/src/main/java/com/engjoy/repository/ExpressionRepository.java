package com.engjoy.repository;

import com.engjoy.constant.CATEGORY;
import com.engjoy.constant.EXPRTYPE;
import com.engjoy.dto.ExpressionSearchDto;
import com.engjoy.entity.ExprUsed;
import com.engjoy.entity.Expression;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.time.LocalDateTime;
import java.util.Arrays;
import java.util.List;

@Repository
public interface ExpressionRepository extends JpaRepository<Expression, Long> {
//    Page<Expression> findByExprType(EXPRTYPE exprType, Pageable pageable);
//    Page<Expression> findByWordTextContainingIgnoreCase(String WordText, Pageable pageable);
//    Page<Expression> findByDifficulty(int difficulty, Pageable pageable);

    @Query(value = "SELECT e FROM Expression e " +
            "WHERE (:keyword IS NULL OR e.wordText LIKE :keyword) " +
            "AND (:exprType IS NULL OR e.exprType = :exprType) " +
            "AND (:difficulty = 0 OR e.difficulty = :difficulty)",
            countQuery = "SELECT count(e) FROM Expression e " +
                    "WHERE (:keyword IS NULL OR e.wordText LIKE :keyword) " +
                    "AND (:exprType IS NULL OR e.exprType = :exprType) " +
                    "AND (:difficulty = 0 OR e.difficulty = :difficulty)")
    Page<Expression> findPageBySearchDto(
            @Param("keyword") String keyword,
            @Param("exprType") EXPRTYPE exprType,
            @Param("difficulty") int difficulty,
            Pageable pageable
    );

    @Query("SELECT e.meaning FROM Expression e")
    List<String> findAllMeanings();


    @Query("SELECT e FROM Expression e LEFT JOIN FETCH e.wordInfo ORDER BY function('RAND')")
    List<Expression> findRandomExpressions(Pageable pageable);

    @Query("SELECT e.meaning FROM Expression e " +
            "WHERE e.exprType = 'WORD' " +
            "AND e.id NOT IN :excludeIds ORDER BY RAND()")
    List<String> findRandomMeanings(@Param("excludeIds") List<Long> excludeIds, Pageable pageable);

    @Query("select e.wordText from Expression e")
    List<String> findAllWordTexts();

    @Query("SELECT e.wordText FROM Expression e WHERE e.exprType = :exprType")
    List<String> findWordTextsByType(@Param("exprType") EXPRTYPE exprType);

}
