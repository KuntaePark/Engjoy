package com.engjoy.entity;

import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Getter @Setter
public class WordInfo {
    @Id
    @Column(name = "expr_id")
    private Long id;

    @OneToOne
    @MapsId
    @JoinColumn(name = "expr_id")
    private Expression expression;

    @Column(columnDefinition = "TEXT")
    private String wordInfoJson;
}
