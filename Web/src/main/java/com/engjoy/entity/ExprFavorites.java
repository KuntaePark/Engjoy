package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Getter @Setter
public class ExprFavorites {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "ef_id")
    private Long id;

    @ManyToOne
    @JoinColumn(name = "account_id")
    private Account account;

    @ManyToOne
    @JoinColumn(name = "expr_id")
    private Expression expression;

    public static ExprFavorites of(Account account, Expression expression){
        ExprFavorites favorites = new ExprFavorites();
        favorites.setAccount(account);
        favorites.setExpression(expression);
        return favorites;
    }
}
