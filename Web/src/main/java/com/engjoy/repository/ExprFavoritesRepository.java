package com.engjoy.repository;

import com.engjoy.entity.Account;
import com.engjoy.entity.ExprFavorites;
import com.engjoy.entity.Expression;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;
import java.util.Set;

@Repository
public interface ExprFavoritesRepository extends JpaRepository<ExprFavorites,Long> {
    Optional<ExprFavorites> findByAccountAndExpression(Account account, Expression expression);
    Page<ExprFavorites> findByAccount(Account account, Pageable pageable);

    @Query("SELECT ef.expression.id FROM ExprFavorites ef WHERE ef.account = :account " +
            "AND ef.expression.id IN :expressionIds")
    Set<Long> findFavoriteExpressionIdsByAccountAndExpressionIds(@Param("account") Account account,
                                                                 @Param("expressionIds")List<Long> expressionIds);
}
