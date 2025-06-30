package com.engjoy.repository;

import com.engjoy.entity.Account;
import com.engjoy.entity.ExprFavorites;
import com.engjoy.entity.Expression;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.Optional;

@Repository
public interface ExprFavoritesRepository extends JpaRepository<ExprFavorites,Long> {
    Optional<ExprFavorites> findByAccountAndExpression(Account account, Expression expression);
    Page<ExprFavorites> findByAccount(Account account, Pageable pageable);
}
