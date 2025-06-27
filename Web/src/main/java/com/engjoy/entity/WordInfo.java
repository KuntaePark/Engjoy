package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Getter @Setter
public class WordInfo {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "wi_id")
    private Long id;

    @OneToOne
    @JoinColumn(name = "expr_id")
    private Expression expression;

    @Column(columnDefinition = "TEXT")
    private String wordInfoJson;
}
