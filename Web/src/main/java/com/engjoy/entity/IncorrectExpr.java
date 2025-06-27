package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;
import org.hibernate.annotations.ColumnDefault;
import org.springframework.data.annotation.CreatedDate;

import java.time.LocalDate;
import java.time.LocalDateTime;

@Entity
@Getter @Setter
public class IncorrectExpr {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "ie_id")
    private Long id;

    @ManyToOne
    @JoinColumn(name = "account_id")
    private Account account;

    @ManyToOne
    @JoinColumn(name = "expr_id")
    private Expression expression;

    private LocalDateTime usedTime;

    @ColumnDefault("0")
    private int incorrectCount;

    private LocalDate lastRecommendedDate;

    // 복습 단어 추천 시 오늘 날짜로 업데이트
    public void updateLastRecommendedDate(){
        this.lastRecommendedDate = LocalDate.now();
    }

}
