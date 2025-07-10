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
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class ReportService {
    private final ExprUsedRepository exprUsedRepository;
    private final IncorrectExprRepository incorrectExprRepository;

    public ReportDataDto getReportData(Long accountId){

        LocalDate today = LocalDate.now();
        LocalDateTime start = today.minusDays(364).atStartOfDay();
        LocalDateTime end   = today.plusDays(1).atStartOfDay();

        // 한 번에 날짜별 사용 횟수, 오답 횟수 조회
        List<Object[]> reviews   = exprUsedRepository.countPerDayNative(accountId, start, end);
        List<Object[]> incorrect = incorrectExprRepository.countIncorrectPerDayNative(accountId, start, end);

        // 결과를 Map으로 변환
        Map<String, Integer> reviewMap = initializeDateMap(today);
        Map<String, Integer> incorrectMap = initializeDateMap(today);

        for (Object[] row : reviews) {
            String day = (String) row[0];
            reviewMap.put(day, ((Number) row[1]).intValue());
        }
        for (Object[] row : incorrect) {
            String day = (String) row[0];
            incorrectMap.put(day, ((Number) row[1]).intValue());
        }


        // 주간 합계
        long weekStartIndex = 364 - 6;
        int weekReviews = reviewMap.values().stream().skip(weekStartIndex).mapToInt(Integer::intValue).sum();
        int weekIncorrect = incorrectMap.values().stream().skip(weekStartIndex).mapToInt(Integer::intValue).sum();

        // 복습 여부 맵
        Map<String, Boolean> didQuizMap = reviewMap.entrySet().stream()
                .collect(Collectors.toMap(
                        Map.Entry::getKey,
                        e -> e.getValue() > 0,
                        (a,b)->a, LinkedHashMap::new
                ));

        return ReportDataDto.from(weekReviews, weekIncorrect, reviewMap, incorrectMap, didQuizMap);
    }

    private Map<String, Integer> initializeDateMap(LocalDate today) {
        Map<String, Integer> map = new LinkedHashMap<>();
        LocalDate cursor = today.minusDays(364);
        while (!cursor.isAfter(today)) {
            map.put(cursor.toString(), 0);
            cursor = cursor.plusDays(1);
        }
        return map;
    }
}
