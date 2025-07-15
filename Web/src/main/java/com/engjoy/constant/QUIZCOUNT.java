package com.engjoy.constant;

import lombok.Getter;

@Getter
public enum QUIZCOUNT {
    FIVE(5),TEN(10),FIFTEEN(15);

    private final int value;

    QUIZCOUNT(int value){
        this.value=value;
    }
}
