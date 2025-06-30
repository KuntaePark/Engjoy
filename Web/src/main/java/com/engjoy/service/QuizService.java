// QuizService.java
package com.engjoy.service;

import com.engjoy.dto.QuizAnsweredDto;
import com.engjoy.dto.QuizGradedDto; // 업데이트된 DTO import
import com.engjoy.dto.QuizPageDto;
import com.engjoy.dto.QuizQuestionDto;
import com.engjoy.dto.QuizResultDto; // 업데이트된 DTO import
import com.engjoy.dto.QuizSettingDto;
import com.engjoy.entity.Account;
import com.engjoy.entity.ExprUsed;
import com.engjoy.entity.Expression;
import com.engjoy.entity.IncorrectExpr;
import com.engjoy.constant.CATEGORY;
import com.engjoy.constant.EXPRTYPE;
import com.engjoy.constant.QUIZCOUNT;
import com.engjoy.repository.AccountRepository;
import com.engjoy.repository.ExprFavoritesRepository;
import com.engjoy.repository.ExprUsedRepository;
import com.engjoy.repository.ExpressionRepository;
import com.engjoy.repository.IncorrectExprRepository;
import jakarta.servlet.http.HttpSession;
import jakarta.transaction.Transactional;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Sort;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class QuizService {

    private final ExpressionRepository expressionRepository;
    private final ExprUsedRepository exprUsedRepository;
    private final IncorrectExprRepository incorrectExprRepository;
    private final ExpressionService expressionService;
    private final AccountRepository accountRepository;
    private final ExprFavoritesRepository exprFavoritesRepository;

    private static final String QUIZ_STATE_SESSION_KEY = "QUIZ_STATE";

    /** 퀴즈 문제 생성 */
    public QuizPageDto createQuizQuestions(Long accountId, QuizSettingDto quizSettingDto) {
        Account account = getAccount(accountId);
        String notifyMsg = null;

        int requestedCount = getQuizCountValue(quizSettingDto.getQuizcount());
        List<Expression> expressions = getExpressionsForQuiz(account, quizSettingDto, requestedCount);


        if (expressions.size() < requestedCount) {
            notifyMsg = "복습할 문제가 " + expressions.size() + "개입니다. " + expressions.size() + "문제로 시작합니다.";
        }

        Collections.shuffle(expressions);
        List<QuizQuestionDto> questions = expressions.stream()
                .map(expr -> QuizQuestionDto.from(
                        expr,
                        generateQuizChoices(expr),
                        exprFavoritesRepository.findByAccountAndExpression(account, expr).isPresent()
                ))
                .collect(Collectors.toList());

        return QuizPageDto.from(questions, notifyMsg, questions.size());
    }

    /** 정답 채점 */
    @Transactional
    public QuizGradedDto gradeQuizAnswer(Long accountId, QuizAnsweredDto answerDto) {
        Account account = getAccount(accountId);
        Expression expr = getExpression(answerDto.getExprId());

        boolean isCorrect = checkAnswer(expr, answerDto);
        if (!isCorrect) incrementIncorrectCount(expr.getId(), account);

        exprUsedRepository.save(ExprUsed.builder()
                .account(account).expression(expr).useTime(LocalDateTime.now()).build());

        // QuizGradedDto.from 메서드 사용 (메시지 필드는 DTO의 from 메서드에서 null로 처리됨)
        return QuizGradedDto.from(expr.getId(), isCorrect,
                expr.getExprType() == EXPRTYPE.WORD ? expr.getMeaning() : null,
                expr.getExprType() == EXPRTYPE.SENTENCE ? List.of(expr.getMeaning()) : null);
    }

    /** 최종 결과 계산 */
    public QuizResultDto calculateQuizResult(Long accountId, HttpSession session) {
        List<QuizGradedDto> results = (List<QuizGradedDto>) session.getAttribute(QUIZ_STATE_SESSION_KEY);
        if (results == null || results.isEmpty()) return QuizResultDto.from(0, 0, 0);

        int total = results.size();
        // QuizResultDto의 필드명 변경: correctAnswers -> correctCount
        int correct = (int) results.stream().filter(QuizGradedDto::isCorrect).count();
        int gold = results.stream().mapToInt(r -> {
            Expression e = getExpression(r.getExprId());
            return e == null ? 0 : r.isCorrect()
                    ? calculateNormalReward(e)
                    : calculateIncorrectReward(e, accountId);
        }).sum();

        session.removeAttribute(QUIZ_STATE_SESSION_KEY);
        // QuizResultDto.from 메서드 사용 (correctCount 필드명 반영)
        return QuizResultDto.from(total, correct, gold);
    }

    /** 오답 횟수 증가 */
    @Transactional
    public void incrementIncorrectCount(Long exprId, Account account) {
        Expression expr = getExpression(exprId);
        IncorrectExpr incorrect = incorrectExprRepository
                .findByAccountAndExpression(account, expr)
                .orElseGet(() -> IncorrectExpr.builder().account(account).expression(expr).incorrectCount(0).build());

        incorrect.setIncorrectCount(incorrect.getIncorrectCount() + 1);
        incorrectExprRepository.save(incorrect);
    }

    /** 일반 퀴즈 보상 계산 */
    public int calculateNormalReward(Expression expr) {
        return expr.getDifficulty() * 10;
    }

    /** 오답 퀴즈 보상 계산 */
    public int calculateIncorrectReward(Expression expr, Long accountId) {
        Account account = getAccount(accountId);
        return incorrectExprRepository.findByAccountAndExpression(account, expr)
                .map(i -> i.getIncorrectCount() * 5).orElse(0);
    }

    /** 퀴즈 보기 생성 */
    public List<String> generateQuizChoices(Expression expr) {
        List<String> choices = new ArrayList<>();
        choices.add(expr.getMeaning());
        choices.addAll(expressionService.generateChoices(expr.getMeaning(), 3));
        Collections.shuffle(choices);
        return choices;
    }

    /** 퀴즈 개수 Enum -> int 변환 */
    private int getQuizCountValue(QUIZCOUNT quizCount) {
        return switch (quizCount) {
            case FIVE -> 5;
            case TEN -> 10;
            case FIFTEEN -> 15;
        };
    }

    /** 정답 확인 로직 */
    private boolean checkAnswer(Expression expr, QuizAnsweredDto dto) {
        if (expr.getExprType() == EXPRTYPE.WORD) {
            return expr.getMeaning().equalsIgnoreCase(dto.getSubmitWord());
        } else {
            return dto.getSubmitSentence() != null &&
                    !dto.getSubmitSentence().isEmpty() &&
                    expr.getMeaning().equalsIgnoreCase(dto.getSubmitSentence().get(0));
        }
    }

    /** 퀴즈용 Expression 리스트 가져오기 */
    private List<Expression> getExpressionsForQuiz(Account account, QuizSettingDto dto, int requestedCount) {
        LocalDateTime start = dto.getStartDate() != null ? dto.getStartDate().atStartOfDay() : null;
        LocalDateTime end = dto.getEndDate() != null ? dto.getEndDate().plusDays(1).atStartOfDay().minusNanos(1) : null;

        if (dto.getCategory() == CATEGORY.MIXED) {
            long wordCount = exprUsedRepository.countUsedByTypeAndDateRange(account, EXPRTYPE.WORD, start, end);
            long sentenceCount = exprUsedRepository.countUsedByTypeAndDateRange(account, EXPRTYPE.SENTENCE, start, end);
            long total = wordCount + sentenceCount;

            if (total == 0) return Collections.emptyList();

            int wordCountForQuiz = (int) Math.round((double) word / total * requestedCount);
            int sentenceCountForQuiz = requestedCount - wordCountForQuiz;

            return combine(
                    getExprList(account, EXPRTYPE.WORD, start, end, wordCountForQuiz),
                    getExprList(account, EXPRTYPE.SENTENCE, start, end, sentenceCountForQuiz)
            );

        } else if (dto.getCategory() == CATEGORY.INCORRECT) {
            return incorrectExprRepository.findByAccount(account,
                            PageRequest.of(0, requestedCount, Sort.by("incorrectCount").descending().and(Sort.by("usedTime").ascending())))
                    .map(IncorrectExpr::getExpression).getContent();
        }

        EXPRTYPE type = dto.getCategory() == CATEGORY.WORD ? EXPRTYPE.WORD : EXPRTYPE.SENTENCE;
        return getExprList(account, type, start, end, requestedCount);
    }

    /** 공통된 Expression 목록 조회 */
    private List<Expression> getExprList(Account account, EXPRTYPE type, LocalDateTime start, LocalDateTime end, int count) {
        return exprUsedRepository.findUsedByDateRange(account, type, start, end, PageRequest.of(0, count))
                .map(ExprUsed::getExpression).getContent();
    }

    /** 유틸: 리스트 병합 */
    private List<Expression> combine(List<Expression> a, List<Expression> b) {
        List<Expression> result = new ArrayList<>(a);
        result.addAll(b);
        return result;
    }

    /** Account 엔티티 조회 */
    private Account getAccount(Long id) {
        return accountRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("계정이 존재하지 않습니다."));
    }

    /** Expression 엔티티 조회 */
    private Expression getExpression(Long id) {
        return expressionRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Expression not found: " + id));
    }
}
