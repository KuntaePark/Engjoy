package com.engjoy.repository;

import com.engjoy.entity.Expression;
import com.engjoy.entity.WordInfo;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.Optional;

@Repository
public interface WordInfoRepository extends JpaRepository<WordInfo,Long> {
    Optional<WordInfo> findByExpression(Expression expression);
}
