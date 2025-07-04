package com.engjoy.service;

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
//    private final AccountRepository accountRepository;

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

        // --- 👇 디버깅을 위한 로그 추가 ---
        System.out.println("--- Repository 호출 직전 파라미터 확인 ---");
        System.out.println("keyword: " + keyword);
        System.out.println("exprType: " + expressionSearchDto.getExprType());
        System.out.println("difficulty: " + expressionSearchDto.getDifficulty());
        System.out.println("-----------------------------------------");

        //  모든 필터링을 한 번에 처리
        Page<Expression> expressionPage = expressionRepository.findPageBySearchDto(
                keyword,
                expressionSearchDto.getExprType(),
                expressionSearchDto.getDifficulty(),
                pageable
        );

        // --- 👇 테스트용 비상 플랜 추가 ---
        // 2. 만약 위 쿼리 결과가 비어있다면, 학습 이력과 상관없이 전체 단어장에서 데이터를 가져오기
        if (expressionPage.isEmpty()) {
            System.out.println("⚠️ 테스트: 학습 이력이 없어 전체 단어장에서 데이터를 가져옵니다.");
            // 필터 조건 없이 findAll로 모든 단어를 페이징해서 가져옴
            expressionPage = expressionRepository.findAll(pageable);
        }
        // ---

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

            // 이 부분은 이제 필요 없으므로 삭제하거나, 다른 용도로 사용합니다.
            // LocalDate date = ...

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
//        Account account = accountRepository.findById(accountId)
//                .orElseThrow(()->new IllegalArgumentException("해당 ID가 존재하지 않습니다."));
        Account account = new Account();
        account.setId(accountId);
        Expression expression = expressionRepository.findById(exprId)
                .orElseThrow(()->new IllegalArgumentException("해당 ID에 관한 표현이 없습니다."));
        Optional<ExprFavorites> existingFavorite = exprFavoritesRepository.findByAccountAndExpression(account,expression);

        if (existingFavorite.isPresent()){
            exprFavoritesRepository.delete(existingFavorite.get());
            return false;
        }else{
            ExprFavorites newFavorite = ExprFavorites.of(account,expression);
            return true;
        }
    }

    // 사용자의 오답 리스트를 페이징하여 조회
    public Page<IncorrectExprDto> getIncorrectExpressions(Long accountId, Pageable pageable){
//        Account account = accountRepository.findById(accountId)
//                .orElseThrow(()->new IllegalArgumentException("해당 ID가 존재하지 않습니다."));
        Account account = new Account();
        account.setId(accountId);

        Page<IncorrectExpr> incorrectExprs = incorrectExprRepository.findByAccount(account,pageable);
        return incorrectExprs.map(IncorrectExprDto::from);
    }

    // '오늘의 복습 추천' 단어/문장 조회
    @Transactional
    public List<ExpressionDto> getDailyRecommendations(Long accountId) {
        Account account = new Account();
        account.setId(accountId);
        LocalDate today = LocalDate.now();

        // 오답 횟수 기준 설정
        int minIncorrectCount = 5;
        // 최대 5개의 결과를 가져오도록 Pageable 설정
        Pageable limit = PageRequest.of(0, 5);
        // Repository 메서드 호출
        List<IncorrectExpr> recommendations = incorrectExprRepository.findTopWordDaily(
                account,
                minIncorrectCount,
                today,
                limit
        );

        // ✅ 2. [테스트용 코드] 만약 DB에서 가져온 결과가 비어있다면,
        if (recommendations.isEmpty()) {
            System.out.println("### DEBUG: No recommendations found in DB. Creating mock data. ###");
            // 가짜 추천 단어 DTO 3개를 만들어서 반환합니다.
            return List.of(
                    new ExpressionDto("apple", "사과", "WORD", 1, false, false, null),
                    new ExpressionDto("banana", "바나나", "WORD", 2, false, false, null),
                    new ExpressionDto("certain", "확실한", "WORD", 3, false, false, null)
            );
        }

        // 추천된 단어들의 '마지막 추천일'을 오늘로 업데이트
        if (!recommendations.isEmpty()) {
            recommendations.forEach(IncorrectExpr::updateLastRecommendedDate);
            // incorrectExprRepository.saveAll(recommendations); // 필요 시 주석 해제
        }
        // DTO 리스트로 변환하여 반환
        return recommendations.stream()
                .map(IncorrectExpr::getExpression)
                .map(expr -> ExpressionDto.from(expr, false, false, null))
                .collect(Collectors.toList());
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


}
