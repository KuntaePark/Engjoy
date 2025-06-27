package com.engjoy.entity;

import com.engjoy.constant.EXPRTYPE;
import jakarta.persistence.*;
import lombok.Getter;
import lombok.Setter;

@Entity
@Getter @Setter
public class Expression {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "expr_id")
    private Long id;

    @Column(nullable = false)
    @Enumerated(EnumType.STRING)
    private EXPRTYPE exprType;

    private String wordText;

    @Column(columnDefinition = "TEXT")
    private String meaning;

    private int difficulty;

    private String pronAudio;

    @OneToOne(cascade = CascadeType.ALL, mappedBy = "expression")
    @PrimaryKeyJoinColumn
    private WordInfo wordInfo;
}
