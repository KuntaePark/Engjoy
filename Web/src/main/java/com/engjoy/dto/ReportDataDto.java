package com.engjoy.dto;

import lombok.Getter;
import lombok.Setter;

import java.time.LocalDate;
import java.util.Map;

@Getter @Setter
public class ReportDataDto {
    private int weekReviews;
    private int weekIncorrects;
    private Map<String,Integer> dailyLearningCounts;
    private Map<String, Integer> dailyIncorrectCounts;
    private Map<String,Boolean> dailyDidQuizMap;

    public static ReportDataDto from(
            int weekReviews,
            int weekIncorrects,
            Map<String, Integer> dailyReviewCounts,
            Map<String,Integer> dailyIncorrectCounts,
            Map<String,Boolean> dailyDidQuizMap

    ) {
        ReportDataDto dto = new ReportDataDto();
        dto.setWeekReviews(weekReviews);
        dto.setWeekIncorrects(weekIncorrects);
        dto.setDailyLearningCounts(dailyReviewCounts);
        dto.setDailyIncorrectCounts(dailyIncorrectCounts);
        dto.setDailyDidQuizMap(dailyDidQuizMap);

        return dto;
    }
}
