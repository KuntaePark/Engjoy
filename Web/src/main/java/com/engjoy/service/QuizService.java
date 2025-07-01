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

    private static final String QUIZ_STATE_SESSION_KEY = "QUIZ_STATE";

    // 퀴즈 문제 생성
    public QuizPageDto createQuizQuestions(Long accountId, QuizSettingDto quizSettingDto){
        Account account = getAccount(accountId);
        String notifyMsg = null;

        int requestedCount = getQuizCountValue(quizSettingDto.getQuizcount()); // 사용자가 퀴즈 설정시 선택한 개수
        List<Expression> expressions = getExpressionsForQuiz(account,quizSettingDto, requestedCount); // 최근 일주일간 사용한 표현 5개 가져오기

        if(expressions.size() < requestedCount){
            notifyMsg = "복습할 문제가" + expressions.size() + "개입니다." + expressions.size() + "문제로 시작합니다.";
        }

        Collections.shuffle(expressions);
        List<QuizQuestionDto> questions = expressions.stream()
                .map(expr->QuizQuestionDto.from(
                        expr,
                        generateQuizChoices(expr),
                        exprFavoritesRepository.findByAccountAndExpression(account,expr).isPresent()
                ))
                .collect(Collectors.toList());
        return QuizPageDto.from(questions,notifyMsg,questions.size());
    }

    // 퀴즈 채점
    @Transactional
    public QuizGradedDto gradeQuizAnswer(Long accountId, QuizAnsweredDto answeredDto){
        Account account = getAccount(accountId);
        Expression expr = getExpressions(answeredDto.getExprId());

        boolean isCorrect = checkAnswer(expr,answeredDto);

        if(!isCorrect){
            incrementIncorrectCount(expr.getId(), account);
        }

        return QuizGradedDto.from(
                expr.getId(),
                isCorrect,
                expr.getExprType() == EXPRTYPE.WORD ? expr.getMeaning() : null,
                expr.getExprType() == EXPRTYPE.SENTENCE ? List.of(expr.getMeaning()) : null
        );
    }

    // 퀴즈 결과 계산
    public QuizResultDto calculateQuizResult(Long accountId, HttpSession session){
        List<QuizGradedDto> results = (List<QuizGradedDto>)  session.getAttribute(QUIZ_STATE_SESSION_KEY);
        if(results == null || results.isEmpty()) return QuizResultDto.from(0,0,0);

        int total = results.size();
        int correct = (int) results.stream().filter(QuizGradedDto::isCorrect).count();
        int gold = results.stream().mapToInt(r -> {
            Expression e = getExpressions(r.getExprId());
            return e == null ? 0 : r.isCorrect()
                    ? calculateNormalReward(e)
                    : calculateIncorrectReward(e, accountId);
        }).sum();

        session.removeAttribute(QUIZ_STATE_SESSION_KEY);
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

    // 보기 선택지 생성
    public List<String> generateQuizChoices(Expression expression){
        List<String> choices = new ArrayList<>();
        choices.add(expression.getMeaning());
        choices.addAll(expressionService.generateChoices(expression.getMeaning(),3));
        Collections.shuffle(choices);
        return choices;
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
    private boolean checkAnswer(Expression expression, QuizAnsweredDto answeredDto){
        if(expression.getExprType() == EXPRTYPE.WORD){
            return expression.getMeaning().equalsIgnoreCase(answeredDto.getSubmitWord());
        }else{
            return answeredDto.getSubmitSentence() != null &&
                    !answeredDto.getSubmitSentence().isEmpty() &&
                    expression.getMeaning().equalsIgnoreCase(answeredDto.getSubmitSentence().get(0));
        }
    }

    // 퀴즈용 Expression 목록 조회
    private List<Expression> getExpressionsForQuiz(Account account, QuizSettingDto quizSettingDto, int requestedCount) {
        LocalDateTime start = quizSettingDto.getStartDate() != null ? quizSettingDto.getStartDate().atStartOfDay() : null;
        LocalDateTime end = quizSettingDto.getEndDate() != null ? quizSettingDto.getEndDate().plusDays(1).atStartOfDay().minusNanos(1) : null;

        if (quizSettingDto.getCategory() == CATEGORY.MIXED) {
            long word = exprUsedRepository.countUsedByTypeAndDateRange(account, EXPRTYPE.WORD, start, end);
            long sentence = exprUsedRepository.countUsedByTypeAndDateRange(account, EXPRTYPE.SENTENCE, start, end);
            long total = word + sentence;
            if (total == 0) return Collections.emptyList();

            int wordCount = (int) Math.round((double) word / total * requestedCount);
            int sentenceCount = requestedCount - wordCount;

            List<Expression> expressions = new ArrayList<>(getExprList(account,EXPRTYPE.WORD, start, end, wordCount));
            expressions.addAll(getExprList(account,EXPRTYPE.SENTENCE, start, end,sentenceCount));
            return expressions;

        }

        if (quizSettingDto.getCategory() == CATEGORY.INCORRECT){
            return incorrectExprRepository.findByAccount(account,
                    PageRequest.of(0, requestedCount, Sort.by("incorrectCount").descending().and(Sort.by("usedTime").ascending())))
                    .map(IncorrectExpr::getExpression).getContent();
        }

        EXPRTYPE exprType = quizSettingDto.getCategory() == CATEGORY.WORD ? EXPRTYPE.WORD : EXPRTYPE.SENTENCE;
        return getExprList(account,exprType,start, end, requestedCount);
    }

    // 특정 타입의 Expression 리스트
    private List<Expression> getExprList(Account account, EXPRTYPE exprType, LocalDateTime start, LocalDateTime end, int count){
        return exprUsedRepository.findUsedByDateRange(account, exprType,start, end, PageRequest.of(0, count))
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
