package com.engjoy.service;

import com.engjoy.dto.ReportDataDto;
import com.engjoy.entity.Account;
import com.engjoy.repository.ExprUsedRepository;
import com.engjoy.repository.IncorrectExprRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.LinkedHashMap;
import java.util.Map;

@Service
@RequiredArgsConstructor
public class ReportService {
    private final ExprUsedRepository exprUsedRepository;
    private final IncorrectExprRepository incorrectExprRepository;

    public ReportDataDto getReportData(Long accountId){
        Account account = new Account();
        account.setId(accountId);

        LocalDate today = LocalDate.now();
        LocalDate weekStart = today.minusDays(6);

        // 주간 데이터
        Long weekReviews = exprUsedRepository.countUsedByDateRange(
                account, weekStart.atStartOfDay(), today.plusDays(1).atStartOfDay()
        );

        // 주간 오답 수
        Long weekIncorrectCount = incorrectExprRepository.countByAccountAndUsedTimeBetween(
                account,weekStart.atStartOfDay(), today.plusDays(1).atStartOfDay()
        );

        // 일별 학습 수 + 오답 수
        Map<String, Integer> dailyReviewMap = new LinkedHashMap<>();
        Map<String, Integer> dailyIncorrectMap = new LinkedHashMap<>();
        Map<String, Boolean> dailyDidQuizMap = new LinkedHashMap<>(); // 복습 여부

        for (int i = 29; i >= 0; i--) {
            LocalDate day = today.minusDays(i);
            LocalDateTime start = day.atStartOfDay();
            LocalDateTime end = day.plusDays(1).atStartOfDay();
            String key = day.toString();

            Long dailyReview = exprUsedRepository.countUsedByDateRange(account, start, end);
            Long dailyIncorrect = incorrectExprRepository.countByAccountAndUsedTimeBetween(account, start, end);

            dailyReviewMap.put(key, dailyReview != null ? dailyReview.intValue() : 0);
            dailyIncorrectMap.put(key, dailyIncorrect != null ? dailyIncorrect.intValue() : 0);
            dailyDidQuizMap.put(key, dailyIncorrect > 0);  // 복습한 날이면 true
        }

        return ReportDataDto.from(
                weekReviews != null ? weekReviews.intValue() : 0,
                weekIncorrectCount != null ? weekIncorrectCount.intValue() : 0,
                dailyReviewMap,
                dailyIncorrectMap,
                dailyDidQuizMap
        );
    }
}
