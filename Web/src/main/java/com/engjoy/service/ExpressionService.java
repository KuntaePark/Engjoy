package com.engjoy.service;

import com.engjoy.constant.EXPRTYPE;
import com.engjoy.constant.SORTTYPE;
import com.engjoy.dto.ExpressionDto;
import com.engjoy.dto.ExpressionSearchDto;
import com.engjoy.dto.IncorrectExprDto;
import com.engjoy.dto.WordInfoDto;
import com.engjoy.entity.*;
import com.engjoy.repository.*;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.data.domain.Sort;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.*;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class ExpressionService {
    private final ExpressionRepository expressionRepository;
    private final ExprUsedRepository exprUsedRepository;
    private final ExprFavoritesRepository exprFavoritesRepository;
    private final WordInfoRepository wordInfoRepository;
    private final IncorrectExprRepository incorrectExprRepository;
    private final AccountRepository accountRepository;

    // 사용자의 전체 단어장 페이지 조회 or 필터링된 카드 목록 페이징 반환
    public Page<ExpressionDto> getExpressions(Long accountId, ExpressionSearchDto expressionSearchDto, Pageable pageable){

//        Account account = accountRepository.findById(accountId)
//                .orElseThrow(() -> new IllegalArgumentException("계정이 존재하지 않습니다."));
        Account account = new Account();
        account.setId(accountId);

        LocalDateTime startDateTime = (expressionSearchDto.getStartDate() != null) ? expressionSearchDto.getStartDate().atStartOfDay() : null;
        LocalDateTime endDateTime = (expressionSearchDto.getEndDate() != null) ? expressionSearchDto.getEndDate().plusDays(1).atStartOfDay() : null;

        String keyword = expressionSearchDto.getKeyword();

        if (keyword != null && keyword.trim().isEmpty()) {
            keyword = null;
        }

        if(keyword != null ){
            keyword = "%" + keyword + "%";
        }

        EXPRTYPE exprTypeEnum = null;
        if (expressionSearchDto.getExprType() != null && !expressionSearchDto.getExprType().isEmpty()) {
            try {
                exprTypeEnum = EXPRTYPE.valueOf(expressionSearchDto.getExprType().toUpperCase());
            } catch (IllegalArgumentException e) {
                // "WORD", "SENTENCE"가 아닌 다른 값이 들어올 경우를 대비한 방어 코드
                System.out.println("Invalid exprType value in getExpressions: " + expressionSearchDto.getExprType());
            }
        }


        //  모든 필터링을 한 번에 처리
        Page<Expression> expressionPage = expressionRepository.findPageBySearchDto(
                keyword,
                exprTypeEnum,
                expressionSearchDto.getDifficulty(),
                pageable
        );


        // DTO 변환 로직은 그대로 유지 (N+1 문제 해결 버전)
        List<Expression> expressions = expressionPage.getContent();
        if (expressions.isEmpty()) {
            return Page.empty(pageable);
        }
        List<Long> exprIds = expressions.stream().map(Expression::getId).collect(Collectors.toList());
        Set<Long> favoriteIds = exprFavoritesRepository.findFavoriteExpressionIdsByAccountAndExpressionIds(account, exprIds);
        Set<Long> usedIds = exprUsedRepository.findUsedExpressionIdsByAccountAndExpressionIds(account, exprIds);

        return expressionPage.map(expression -> {
            boolean isFavorite = favoriteIds.contains(expression.getId());
            boolean isUsed = usedIds.contains(expression.getId());

            return ExpressionDto.from(expression, isFavorite, isUsed, null);
        });
    }

    // 특정 단어/문장의 상세 정보 조회
    public WordInfoDto getWordDetail(Long exprId){
        Expression expression = expressionRepository.findById(exprId)
                .orElseThrow(()->new IllegalArgumentException("해당 ID에 관한 자세히보기가 없습니다."));
        Optional<WordInfo> wordInfoOptional = wordInfoRepository.findByExpression(expression);
        return wordInfoOptional.map(WordInfoDto::from).orElse(null);
    }

    // 특정 단어/문장에 대한 사용자의 즐겨찾기 상태 토글(없으면 생성,있으면 삭제)
    @Transactional
    public boolean toggleFavoriteStatus(Long accountId, Long exprId){
        Account account = accountRepository.findById(accountId)
                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        account.setId(accountId);
        Expression expression = expressionRepository.findById(exprId)
                .orElseThrow(()->new IllegalArgumentException("해당 ID에 관한 표현이 없습니다."));
        Optional<ExprFavorites> existingFavorite = exprFavoritesRepository.findByAccountAndExpression(account,expression);

        if (existingFavorite.isPresent()) {
            // 이미 즐겨찾기 되어 있으면 삭제
            exprFavoritesRepository.delete(existingFavorite.get());
            return false;
        } else {
            // 아니라면 새로 생성하고 저장
            ExprFavorites newFav = ExprFavorites.of(account, expression);
            exprFavoritesRepository.save(newFav);
            return true;
        }
    }

    // 사용자의 오답 리스트 조회
    public List<IncorrectExprDto> getIncorrectExpressionsAsList(Long accountId) {
        // 1. accountId로 Account 엔티티를 조회합니다.
//        Account account = accountRepository.findById(accountId)
//                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다: " + accountId));
        Account account = new Account();
        account.setId(accountId);
        // 2. 위에서 추가한 메서드를 호출하여 해당 계정의 모든 오답 엔티티 목록을 가져옵니다.
        List<IncorrectExpr> incorrectExprs = incorrectExprRepository.findByAccount(account);

        // 3. Stream API를 사용해 엔티티 목록(List<IncorrectExpr>)을 DTO 목록(List<IncorrectExprDto>)으로 변환합니다.
        return incorrectExprs.stream()
                .map(IncorrectExprDto::from) // DTO에 만들어둔 static 메서드 활용
                .collect(Collectors.toList());
    }

    // '오늘의 복습 추천' 단어/문장 조회
    // QuizService.java

    // '오늘의 복습 추천' 단어/문장 조회 (수정된 버전)
    @Transactional(readOnly = true) // 데이터를 수정하지 않으므로 readOnly = true로 최적화
    public List<ExpressionDto> getDailyRecommendations(Long accountId) {
        Account account = new Account();
        account.setId(accountId);
        LocalDate today = LocalDate.now();

        int minIncorrectCount = 5;
        Pageable limit = PageRequest.of(0, 5);

        List<IncorrectExpr> recommendations = incorrectExprRepository.findTopWordDaily(
                account,
                minIncorrectCount,
                today,
                limit
        );

        // [테스트용 코드]는 그대로 유지하셔도 좋습니다.
        if (recommendations.isEmpty()) {
            System.out.println("### DEBUG: No recommendations found in DB. Creating mock data. ###");
            return List.of(
                    new ExpressionDto("apple", "사과", "WORD", 1, false, false, null),
                    new ExpressionDto("banana", "바나나", "WORD", 2, false, false, null),
                    new ExpressionDto("certain", "확실한", "WORD", 3, false, false, null)
            );
        }

        // DTO 리스트로 변환하여 반환
        return recommendations.stream()
                .map(IncorrectExpr::getExpression)
                .map(expr -> ExpressionDto.from(expr, false, false, null))
                .collect(Collectors.toList());
    }

    //'하루 보지 않기'를 처리하는 메서드


    @Transactional
    public void hideRecommendationForToday(Long accountId, Long expressionId) {
        Account account = new Account();
        account.setId(accountId);

        Expression expression = new Expression();
        expression.setId(expressionId);

        // 오답 노트에서 해당 항목을 찾아서
        IncorrectExpr incorrectExpr = incorrectExprRepository.findByAccountAndExpression(account, expression)
                .orElseThrow(() -> new IllegalArgumentException("해당 오답 노트를 찾을 수 없습니다."));

        // 마지막 추천일을 오늘 날짜로 업데이트합니다.
        incorrectExpr.setLastRecommendedDate(LocalDate.now());
        incorrectExprRepository.save(incorrectExpr); // 명시적으로 저장

        System.out.printf("[추천 숨김] 문제 ID: %d의 마지막 추천일이 오늘로 업데이트되었습니다.%n", expressionId);
    }


    // 정답을 제외한 나머지 단어 뜻에서 지정된 개수만큼 무작위로 오답 보기 생성(퀴즈 및 인쇄)
    public List<String> generateChoices(String correctAnswer, List<String> allMeanings, int count) {
        // 정답 제외
        List<String> filtered = allMeanings.stream()
                .filter(meaning -> !meaning.equalsIgnoreCase(correctAnswer))
                .collect(Collectors.toList());

        Collections.shuffle(filtered);

        // 부족하면 "보기부족"으로 채움
        if (filtered.size() < count) {
            while (filtered.size() < count) {
                filtered.add("※보기부족※");
            }
            return filtered.subList(0, count);
        }

        return filtered.subList(0, count);
    }

    // 사전 페이지의 단어/문장 목록을 페이징하여 조회
    public Page<ExpressionDto> getDictionaryPage(Pageable pageable){
        Page<Expression> expressionPage = expressionRepository.findAll(pageable);
        return expressionPage.map(expression -> ExpressionDto.from(expression));
    }

    // 오답 리스트 등에서 사용될 Pageable 객체를 타입에 따라 생성
    public Pageable createPageable(int page, int size, SORTTYPE sortType){
        Sort sort;
        switch (sortType){
            case LATEST :
                sort = Sort.by("usedTime").descending();
                break;
            case OLDEST:
                sort = Sort.by("usedTime").ascending();
                break;
            case DIFFDESC:
                sort = Sort.by("expression.difficulty").descending();
                break;
            case DIFFASC:
                sort = Sort.by("expression.difficulty").ascending();
                break;
            default:
                sort = Sort.by("usedTime").descending();
        }
        return PageRequest.of(page,size,sort);
    }

    public Map<Long, List<String>> generateChoicesForBatch(List<Expression> expressions, List<String> allMeanings) {
        Map<Long, List<String>> result = new HashMap<>();
        for (Expression expr : expressions) {
            List<String> choices = new ArrayList<>();
            choices.add(expr.getMeaning());
            choices.addAll(generateChoices(expr.getMeaning(), allMeanings, 3));
            Collections.shuffle(choices);
            result.put(expr.getId(), choices);
        }
        return result;
    }

    public Map<String, List<ExpressionDto>> getStudyLog(Long accountId, ExpressionSearchDto searchDto,Pageable  pageable) {
//        Account account = accountRepository.findById(accountId)
//                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다: " + accountId));

        Account account = new Account();
        account.setId(accountId);

        EXPRTYPE exprTypeEnum = null; // 기본값은 null
        if (searchDto.getExprType() != null && !searchDto.getExprType().isEmpty()) {
            try {
                exprTypeEnum = EXPRTYPE.valueOf(searchDto.getExprType().toUpperCase());
            } catch (IllegalArgumentException e) {
                // "ALL" 등 다른 값이 들어올 경우 무시하고 null 상태 유지
                System.out.println("Invalid exprType value: " + searchDto.getExprType());
            }
        }

        if (searchDto.getKeyword() != null && searchDto.getKeyword().trim().isEmpty()) {
            searchDto.setKeyword(null);
        }


        LocalDateTime startDateTime = (searchDto.getStartDate() != null) ? searchDto.getStartDate().atStartOfDay() : null;
        LocalDateTime endDateTime = (searchDto.getEndDate() != null) ? searchDto.getEndDate().plusDays(1).atStartOfDay() : null;

        Sort existingSort = pageable.getSort();
        Sort newSort = existingSort.and(Sort.by(Sort.Direction.DESC, "id"));
        Pageable newPageable = PageRequest.of(pageable.getPageNumber(), pageable.getPageSize(), newSort);

        Page<ExprUsed> usedPage = exprUsedRepository.findUsedBySearchDto(account, searchDto.getKeyword(),
                exprTypeEnum, startDateTime, endDateTime, pageable);

        List<ExprUsed> usedList = usedPage.getContent();

        if (usedList.isEmpty()) {
            return Collections.emptyMap();
        }

        // isFavorite 상태를 확인하기 위해 exprId 리스트 추출
        List<Long> exprIds = usedList.stream()
                .map(used -> used.getExpression().getId())
                .collect(Collectors.toList());

        Set<Long> favoriteIds = exprFavoritesRepository.findFavoriteExpressionIdsByAccountAndExpressionIds(account, exprIds);

        // DTO로 변환
        List<ExpressionDto> dtos = usedList.stream()
                .map(used -> {
                    boolean isFavorite = favoriteIds.contains(used.getExpression().getId());
                    return ExpressionDto.from(used.getExpression(), isFavorite, true, used.getUsedTime().toLocalDate());
                })
                .collect(Collectors.toList());

        // 날짜를 key로, DTO 리스트를 value로 갖는 Map으로 그룹화하여 반환
        return dtos.stream()
                .collect(Collectors.groupingBy(
                        dto -> dto.getDate().toString(), // 날짜를 기준으로 그룹화
                        LinkedHashMap::new,             // 순서가 보장되는 LinkedHashMap 사용
                        Collectors.toList()             // 그룹화된 항목들은 List로 묶음
                ));
    }

}
