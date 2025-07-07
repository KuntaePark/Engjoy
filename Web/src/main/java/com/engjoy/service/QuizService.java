package com.engjoy.service;

import com.engjoy.dto.*;
import com.engjoy.entity.*;
import com.engjoy.constant.CATEGORY;
import com.engjoy.constant.EXPRTYPE;
import com.engjoy.constant.QUIZCOUNT;
import com.engjoy.repository.*;
import jakarta.servlet.http.HttpSession;
import jakarta.transaction.Transactional;
import lombok.RequiredArgsConstructor;
import org.aspectj.weaver.ast.Expr;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;
import org.springframework.stereotype.Service;

import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.*;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class QuizService {

    private final ExpressionRepository expressionRepository;
    private final ExprUsedRepository exprUsedRepository;
    private final IncorrectExprRepository incorrectExprRepository;
    private final ExpressionService expressionService;
//    private final AccountRepository accountRepository;
    private final ExprFavoritesRepository exprFavoritesRepository;

    // ✅ 세션 키를 역할에 따라 두 개로 분리하여 관리합니다.
    private static final String QUIZ_PAGE_KEY = "QUIZ_PAGE_STATE"; // 퀴즈 문제 정보 저장용
    private static final String QUIZ_GRADED_RESULTS_KEY = "QUIZ_GRADED_RESULTS"; // 채점 결과 누적용

    // 퀴즈 문제 생성
    public QuizPageDto createQuizQuestions(Long accountId, QuizSettingDto quizSettingDto){
        Account account = getAccount(accountId);
        String notifyMsg = null;

        int requestedCount = getQuizCountValue(quizSettingDto.getQuizcount());
        List<Expression> expressions = getExpressionsForQuiz(account, quizSettingDto, requestedCount);

        if (expressions.isEmpty()) {
            return QuizPageDto.from(Collections.emptyList(), "출제할 문제가 없습니다.", 0);
        }
        if(expressions.size() < requestedCount){
            notifyMsg = "복습할 문제가 " + expressions.size() + "개입니다. " + expressions.size() + "문제로 시작합니다.";
        }

        // 즐겨찾기 N+1 문제 해결 (기존 코드 유지)
        List<Long> expressionIds = expressions.stream().map(Expression::getId).collect(Collectors.toList());
        Set<Long> favoriteExprIds = exprFavoritesRepository.findFavoriteExpressionIdsByAccountAndExpressionIds(account, expressionIds);

        // ✅ 성능 개선: 전체 뜻 대신, 오답 보기용 후보 50개만 무작위로 조회
        List<String> distractorPool = expressionRepository.findRandomMeanings(expressionIds, 50);

        List<QuizQuestionDto> questions = expressions.stream()
                .map(expr -> {
                    List<String> choices;
                    String questionText;

                    if (expr.getExprType() == EXPRTYPE.SENTENCE) {
                        questionText = expr.getMeaning();
                        String[] words = expr.getWordText().split(" ");
                        choices = new ArrayList<>(Arrays.asList(words));
                        Collections.shuffle(choices);
                    } else {
                        questionText = expr.getWordText();
                        choices = generateChoicesFromPool(expr.getMeaning(), distractorPool, expr.getExprType());
                    }

                    return QuizQuestionDto.from(
                            expr.getId(),
                            expr.getExprType(),
                            questionText,
                            choices,
                            favoriteExprIds.contains(expr.getId()),
                            expr.getPronAudio()
                    );
                })
                .collect(Collectors.toList());

        return QuizPageDto.from(questions, notifyMsg, questions.size());
    }


    // 퀴즈 채점
    @Transactional
    public QuizGradedDto gradeQuizAnswer(Long accountId, QuizAnsweredDto answeredDto, HttpSession session){
        Account account = getAccount(accountId);
        Expression expr = getExpressions(answeredDto.getExprId());
        boolean isCorrect = checkAnswer(expr,answeredDto);

        if(!isCorrect){
            incrementIncorrectCount(expr.getId(), account);
        }

        QuizGradedDto gradedResult = QuizGradedDto.from(
                expr.getId(),
                isCorrect,
                expr.getExprType() == EXPRTYPE.WORD ? expr.getMeaning() : null,
                expr.getExprType() == EXPRTYPE.SENTENCE ? List.of(expr.getWordText()) : null
        );

        // 세션에 채점 결과를 리스트 형태로 누적 저장
        List<QuizGradedDto> results = (List<QuizGradedDto>) session.getAttribute(QUIZ_GRADED_RESULTS_KEY);
        if (results == null) {
            results = new ArrayList<>();
        }
        results.add(gradedResult);
        session.setAttribute(QUIZ_GRADED_RESULTS_KEY, results);

        System.out.println("✅ [gradeQuizAnswer] 채점 결과 저장됨: " + gradedResult.isCorrect());
        System.out.println("✅ [gradeQuizAnswer] 현재까지 누적된 결과 개수: " + results.size());
        // ---

        return gradedResult;
    }

    // 퀴즈 결과 계산
    public QuizResultDto calculateQuizResult(Long accountId, HttpSession session){

        List<QuizGradedDto> results = (List<QuizGradedDto>) session.getAttribute(QUIZ_GRADED_RESULTS_KEY);
        QuizPageDto quizState = (QuizPageDto) session.getAttribute(QUIZ_PAGE_KEY); // 전체 문제 수

        if (results == null) {
            System.out.println("❌ [calculateQuizResult] 세션에서 결과를 찾을 수 없음 (null)!");
        } else {
            System.out.println("✅ [calculateQuizResult] 세션에서 찾은 결과 개수: " + results.size());
        }

        if(results == null || results.isEmpty()) return QuizResultDto.from(0,0,0);

        List<Long> exprIds = results.stream().map(QuizGradedDto::getExprId).toList();
        Map<Long, Expression> exprMap = expressionRepository.findAllById(exprIds).stream()
                .collect(Collectors.toMap(Expression::getId, e -> e));

        int total = quizState.getQuestions().size();
        int correct = (int) results.stream().filter(QuizGradedDto::isCorrect).count();

        int gold = results.stream().mapToInt(r -> {
            Expression e = exprMap.get(r.getExprId());
            return e == null ? 0 : r.isCorrect()
                    ? calculateNormalReward(e)
                    : calculateIncorrectReward(e, accountId);
        }).sum();

        // 사용한 세션 데이터 정리
        session.removeAttribute(QUIZ_PAGE_KEY);
        session.removeAttribute(QUIZ_GRADED_RESULTS_KEY);
        return QuizResultDto.from(total,correct,gold);
    }

    // 오답 횟수 증가
    @Transactional
    public void incrementIncorrectCount(Long exprId, Account account){
        Expression expr = getExpressions(exprId);
        IncorrectExpr incorrectExpr = incorrectExprRepository
                .findByAccountAndExpression(account,expr)
                .orElseGet(()->{
                    IncorrectExpr newEntity = new IncorrectExpr();
                    newEntity.setAccount(account);
                    newEntity.setExpression(expr);
                    newEntity.setIncorrectCount(0);
                    return newEntity;
                });
        incorrectExpr.setIncorrectCount(incorrectExpr.getIncorrectCount()+1);
        incorrectExpr.setUsedTime(LocalDateTime.now());
        incorrectExprRepository.save(incorrectExpr);
    }

    // 보상 계산 - 정답
    public int calculateNormalReward(Expression expression){
        return expression.getDifficulty() * 10;
    }

    // 보상 계산 - 오답
    public int calculateIncorrectReward(Expression expression, Long accountId){
        Account account = getAccount(accountId);
        return incorrectExprRepository.findByAccountAndExpression(account,expression)
                .map(i -> i.getIncorrectCount() *5)
                .orElse(0);
    }


    private List<String> generateChoicesFromPool(String correctAnswer, List<String> distractorPool, EXPRTYPE exprType) {
        List<String> distractors = new ArrayList<>(distractorPool);

        if(exprType == EXPRTYPE.WORD){
            distractors.remove(correctAnswer); // 정답은 제외
            Collections.shuffle(distractors); // 보기용 오답 섞기
        }
        distractors.remove(correctAnswer);
        Collections.shuffle(distractors);

        List<String> finalChoices = new ArrayList<>();
        finalChoices.add(correctAnswer);
        finalChoices.addAll(distractors.stream().limit(3).collect(Collectors.toList()));

        Collections.shuffle(finalChoices);
        return finalChoices;
    }



    // 퀴즈 개수 매핑
    private int getQuizCountValue(QUIZCOUNT quizCount){
        return switch (quizCount){
            case FIVE -> 5;
            case TEN -> 10;
            case FIFTEEN -> 15;
        };
    }

    // 정답 확인 로직
    // 마침표 앞뒤 공백 제거 + 소문자로 비교
    private boolean checkAnswer(Expression expression, QuizAnsweredDto answeredDto){
        if(expression.getExprType() == EXPRTYPE.WORD){
            return normalizeSentence(expression.getMeaning())
                    .equals(normalizeSentence(answeredDto.getSubmitWord()));
        } else {
            String submittedSentence = String.join(" ", answeredDto.getSubmitSentence());
            String expected = normalizeSentence(expression.getWordText());
            String submitted = normalizeSentence(submittedSentence);
            return expected.equals(submitted);
        }
    }



    // 공백, 마침표 정리하는 헬퍼 메서드 추가
    private String normalizeSentence(String sentence) {
        return sentence
                .replaceAll("\\s+", " ")          // 여러 공백 → 하나로
                .replaceAll("\\s+\\.", ".")       // 마침표 앞 공백 제거
                .trim()
                .toLowerCase();
    }


    // 퀴즈용 Expression 목록 조회
    private List<Expression> getExpressionsForQuiz(Account account, QuizSettingDto quizSettingDto, int requestedCount) {
        LocalDateTime start = quizSettingDto.getStartDate() != null ? quizSettingDto.getStartDate().atStartOfDay() : null;
        LocalDateTime end = quizSettingDto.getEndDate() != null ? quizSettingDto.getEndDate().plusDays(1).atStartOfDay().minusNanos(1) : null;

        List<Expression> expressions;

        // 사용자가 선택한 카테고리에 따라 문제 가져오기
        if (quizSettingDto.getCategory() == CATEGORY.MIXED) {
            long word = exprUsedRepository.countUsedByTypeAndDateRange(account, EXPRTYPE.WORD, start, end);
            long sentence = exprUsedRepository.countUsedByTypeAndDateRange(account, EXPRTYPE.SENTENCE, start, end);
            long total = word + sentence;
            if (total == 0) {
                expressions = Collections.emptyList();
            } else {
                int wordCount = (int) Math.round((double) word / total * requestedCount);
                int sentenceCount = requestedCount - wordCount;
                expressions = new ArrayList<>(getExprList(account, EXPRTYPE.WORD, start, end, wordCount));
                expressions.addAll(getExprList(account, EXPRTYPE.SENTENCE, start, end, sentenceCount));
            }

        } else if (quizSettingDto.getCategory() == CATEGORY.INCORRECT) {
            expressions = incorrectExprRepository.findByAccount(account,
                            PageRequest.of(0, requestedCount, Sort.by("incorrectCount").descending().and(Sort.by("usedTime").ascending())))
                    .map(IncorrectExpr::getExpression).getContent();
        } else {
            EXPRTYPE exprType = quizSettingDto.getCategory() == CATEGORY.WORD ? EXPRTYPE.WORD : EXPRTYPE.SENTENCE;
            expressions = getExprList(account, exprType, start, end, requestedCount);
        }

        // =================================================================
        // ## 2. 테스트용 비상 플랜 추가 ##
        // 만약 위 로직으로 가져온 문제가 하나도 없다면, 테스트를 위해 전체 단어장에서 가져옵니다.
        if (expressions.isEmpty() && requestedCount > 0) {
            System.out.println("⚠️ 테스트: 학습 데이터가 없어 전체 단어장(Expression)에서 문제를 가져옵니다.");

            List<Expression> allExpressions = expressionRepository.findRandomExpressions(PageRequest.of(0, requestedCount));
            Collections.shuffle(allExpressions);

            return allExpressions.stream().limit(requestedCount).collect(Collectors.toList());
        }
        // =================================================================

        // 3. 기존 로직으로 문제를 성공적으로 가져왔다면, 원래의 `expressions`를 반환합니다.
        return expressions;
    }

    // 특정 타입의 Expression 리스트
    private List<Expression> getExprList(Account account, EXPRTYPE exprType, LocalDateTime start, LocalDateTime end, int count){
        return exprUsedRepository.findUsedByDateRangeFetchExpr(account, exprType,start, end, PageRequest.of(0, count))
                .map(ExprUsed::getExpression).getContent();
    }

    // 계정 조회
    private Account getAccount(Long id){
//        return accountRepository.findById(id)
//                .orElseThrow(()->new IllegalArgumentException("계정이 존재하지 않습니다."));
        Account account = new Account();
        account.setId(id);
        return account;
    }

    // Expression 조회
    private Expression getExpressions(Long id){
        return expressionRepository.findById(id)
                .orElseThrow(()->new IllegalArgumentException("Expression이 존재하지 않습니다."));
    }

}
