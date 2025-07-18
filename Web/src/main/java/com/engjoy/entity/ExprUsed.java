package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;
import org.hibernate.annotations.ColumnDefault;
import org.springframework.data.annotation.CreatedDate;

import java.time.LocalDateTime;

@Entity
@Getter @Setter
@Table(
        uniqueConstraints = {
                @UniqueConstraint(columnNames = {"account_id", "expr_id"})
        }
)
public class ExprUsed {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "eu_id")
    private Long id;

    @ManyToOne
    @JoinColumn(name = "account_id")
    private Account account;

    @ManyToOne
    @JoinColumn(name = "expr_id")
    private Expression expression;

    private LocalDateTime usedTime;


}
