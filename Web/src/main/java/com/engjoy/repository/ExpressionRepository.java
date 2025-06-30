package com.engjoy.repository;

import com.engjoy.constant.EXPRTYPE;
import com.engjoy.entity.ExprUsed;
import com.engjoy.entity.Expression;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface ExpressionRepository extends JpaRepository<Expression, Long> {
    Page<Expression> findByExprType(EXPRTYPE exprType, Pageable pageable);
    Page<Expression> findByWordTextContainingIgnoreCase(String WordText, Pageable pageable);
    Page<Expression> findByDifficulty(int difficulty, Pageable pageable);

    @Query("SELECT e.meaning FROM Expression e")
    List<String> findAllMeanings();
}
