package com.engjoy.service;

import com.engjoy.constant.ORDERTYPE;
import com.engjoy.constant.PRINTFORM;
import com.engjoy.dto.PrintContentDto;
import com.engjoy.dto.PrintOptionDto;
import com.engjoy.dto.PrintReviewDto;
import com.engjoy.dto.QuizSettingDto;
import com.engjoy.entity.Expression;
import com.engjoy.repository.ExpressionRepository;
import com.engjoy.repository.WordInfoRepository;
import lombok.RequiredArgsConstructor;
import org.aspectj.weaver.ast.Expr;
import org.springframework.stereotype.Service;

import java.io.IOException;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class PrintService {

    private final ExpressionRepository expressionRepository;
    private final WordInfoRepository wordInfoRepository;

    // 사용자의 인쇄 옵션에 따라 PDF생성 후 byte배열로 반환
//    public byte[] createPrintablePdf(PrintOptionDto printOptionDto, Long accountId) throws IOException {
//        // 인쇄할 Expression 목록 조회
//        List<Expression> expressionsToPrint = getExpressionsToPrint(printOptionDto, accountId);
//        // 사용자가 선택한 정렬 적용
//        List<Expression> sortedExpressions = applyOrderType(expressionsToPrint, printOptionDto.getPrintOptionDetailDto().getOrderType());
//        // 인쇄 형태에 맞게 데이터 목록 생성
//        List<PrintContentDto> contents = createContents(sortedExpressions, printOptionDto.getPrintForm());
//
//        return createPdf(contents, printOptionDto);
//    }

    // 인쇄 옵션에 따라 인쇄할 목록 가져오기
    private List<Expression> getExpressionsToPrint(PrintOptionDto printOptionDto, Long accountId) {
        if (printOptionDto.isSelectAll()) {
            // '전체 선택'
            QuizSettingDto filters = printOptionDto.getQuizSettingDto();
            // 날짜 범위 설정
            LocalDateTime startDate = null;
            LocalDateTime endDate = null;

            if (filters.getDateRange() != null) {
                switch (filters.getDateRange()) {
                    case TODAY:
                        startDate = LocalDateTime.now().toLocalDate().atStartOfDay();
                        endDate = startDate.plusDays(1);
                        break;
                    case LAST_WEEK:
                        startDate = LocalDateTime.now().minusWeeks(1);
                        endDate = LocalDateTime.now();
                        break;
                    case LAST_MONTH:
                        startDate = LocalDateTime.now().minusMonths(1);
                        endDate = LocalDateTime.now();
                        break;
                    case CUSTOM:
                        if (filters.getStartDate() != null && filters.getEndDate() != null) {
                            startDate = filters.getStartDate().atStartOfDay();
                            endDate = filters.getEndDate().plusDays(1).atStartOfDay();
                        }
                        break;

                }
            }

            // 레퍼지토리의 필터링 메서드 호출
            return expressionRepository.findWithFilters(
                    accountId,
                    filters.getCategory(),
                    startDate,
                    endDate
            );
        } else {
            return expressionRepository.findAllById(printOptionDto.getExprIdsToPrint());
        }
    }

    // Expression을 선택한 정렬 옵션대로 정렬
    private List<Expression> applyOrderType(List<Expression> expressions, ORDERTYPE orderType) {
        List<Expression> sortedExpressions = new ArrayList<>(expressions);
        switch (orderType) {
            case SUFF:
                Collections.shuffle(sortedExpressions);
                break;
            case ABC:
                sortedExpressions.sort((e1, e2) -> e1.getWordText().compareToIgnoreCase(e2.getWordText()));
                break;
            case STAY:
            default:
                break;
        }
        return sortedExpressions;
    }

    // 인쇄 형태에 따라 컨텐츠 생성메서드 호출
    private List<PrintContentDto> createContents(List<Expression> expressions, PRINTFORM printForm) {
        switch (printForm) {
            case EXAM:
                return makeTestSheet(expressions);
            case LIST:
                return makeList(expressions);
            case WORKSHEET:
                return makeWorkSheet(expressions);
            default:
                throw new IllegalArgumentException("지원하지 않는 인쇄 형태입니다.");
        }
    }

    // 시험지 형식에 맞는 데이터 목록 생성
    private List<PrintContentDto> makeTestSheet(List<Expression> expressions){
        List<String> allMeanings = expressionRepository.findAllMeanings(); // 오답 보기용 전체 뜻 목록

        return expressions.stream().map(expression -> {
            List<String> choices = selectRandomWrongChoices(allMeanings, expression.getMeaning(),3);
            choices.add(expression.getMeaning());
            Collections.shuffle(choices);

            return new PrintContentDto(expression.getWordText(), expression.getMeaning(), choices, null, null, null, null, null);
        }).collect(Collectors.toList());
    }

    // 단어-뜻 리스트 형식
    private List<PrintContentDto> makeList(List<Expression> expressions) {
        return expressions.stream()
                .map(expr -> new PrintContentDto(expr.getWordText(), expr.getMeaning(), null, null, null, null, null, null))
                .collect(Collectors.toList());
    }

    // 워크시트 형식
    private List<PrintContentDto> makeWorkSheet(List<Expression> expressions) {
        return expressions.stream()
                .map(expr -> new PrintContentDto(expr.getWordText(), "________________", null, null, null, null, null, null))
                .collect(Collectors.toList());
    }
    // 오답 보기 무작위 추출
    private List<String> selectRandomWrongChoices(List<String> allMeanings, String correctAnswer, int count) {
        List<String> wrongAnswers = new ArrayList<>(allMeanings);
        wrongAnswers.remove(correctAnswer);
        Collections.shuffle(wrongAnswers);
        return wrongAnswers.stream().limit(count).collect(Collectors.toList());
    }

    // 생성된 데이터 목록을 받아 최총 PDF 문서


}


