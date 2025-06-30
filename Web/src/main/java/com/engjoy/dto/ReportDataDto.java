package com.engjoy.dto;

import lombok.Getter;
import lombok.Setter;

import java.time.LocalDate;
import java.util.Map;

@Getter @Setter
public class ReportDataDto {
    private int weekReviews;
    private int weekIncorrects;
    private Map<LocalDate,Integer> dailyLearningCounts;
}
