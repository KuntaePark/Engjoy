package com.engjoy.dto;

import com.engjoy.constant.CATEGORY;
import com.engjoy.constant.DATERANGE;
import com.engjoy.constant.QUIZCOUNT;
import lombok.Getter;
import lombok.Setter;

import java.time.LocalDate;

@Getter @Setter
public class QuizSettingDto {
    private DATERANGE dateRange;
    private LocalDate startDate;
    private LocalDate endDate;
    private CATEGORY category;
    private QUIZCOUNT quizcount;
}
