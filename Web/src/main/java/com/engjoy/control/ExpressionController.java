package com.engjoy.control;

import com.engjoy.dto.ExpressionDto;
import com.engjoy.dto.ExpressionSearchDto;
import com.engjoy.dto.IncorrectExprDto;
import com.engjoy.dto.WordInfoDto;
import com.engjoy.entity.Account;
import com.engjoy.service.ExpressionService;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.data.domain.Sort;
import org.springframework.data.web.PageableDefault;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.*;

import java.security.Principal;
import java.util.List;
import java.util.Map;

@Controller
@RequestMapping("/expressions")
@RequiredArgsConstructor
public class ExpressionController {
    private final ExpressionService expressionService;
//    private final AccountRepository accountRepository;

    @GetMapping
    public String getExpressions(Principal principal,
                                 @ModelAttribute ExpressionSearchDto searchDto,
                                 @PageableDefault(size = 10, sort = "id") Pageable pageable,
                                 Model model) {
//        if (principal == null) {
//            return "redirect:/login";
//        }
        // Principal에서 username을 얻어 Account 엔티티를 조회
//        Account account = accountRepository.findByUsername(principal.getName())
//                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
//        Account account = new Account();
//        account.setId(1L);
//        account.setName(principal.getName());

        Long testAccountId = 1L;
        Page<ExpressionDto> expressionPage = expressionService.getExpressions(testAccountId, searchDto, pageable);
        model.addAttribute("expressionPage", expressionPage);
        model.addAttribute("searchDto", searchDto);
        return "expressions";
    }

    @GetMapping("/api")
    public ResponseEntity<?> getExpressionPage(
            @RequestParam(name = "view", defaultValue = "study_log") String view,
            ExpressionSearchDto searchDto,
            @RequestParam(value = "page", defaultValue = "0") int page,
            @RequestParam(value = "sort", defaultValue = "id,desc") String sort,
            Principal principal) {

        // 테스트용 계정 ID
        Long testAccountId = 1L;

        if ("dictionary".equals(view)) {
            // "사전" 보기 요청 시: 기존 로직을 그대로 실행하여 Page<ExpressionDto> 반환

            // 1. 정렬 조건 파싱
            String[] sortParams = sort.split(",");
            String sortField = sortParams[0];
            Sort.Direction direction = (sortParams.length > 1 && sortParams[1].equalsIgnoreCase("asc"))
                    ? Sort.Direction.ASC : Sort.Direction.DESC;

            // 2. Pageable 객체 생성
            Pageable pageable = PageRequest.of(page, 12, Sort.by(direction, sortField));

            // 3. 서비스 호출
            Page<ExpressionDto> expressionPage = expressionService.getExpressions(testAccountId, searchDto, pageable);

            return ResponseEntity.ok(expressionPage);

        } else {
            // "학습 기록" 보기 요청 시 (기본값): 날짜별로 그룹화된 Map<String, List<ExpressionDto>> 반환

            // 학습 기록에 맞는 Pageable 생성 및 새로운 서비스 메서드 호출
            Pageable pageable = PageRequest.of(page, 5, Sort.by("usedTime").descending()); // 예: 한 페이지에 5일치 기록
            Map<String, List<ExpressionDto>> studyLog = expressionService.getStudyLog(testAccountId, searchDto, pageable);

            return ResponseEntity.ok(studyLog);
        }
    }

    // 오늘의 추천 단어를 제공하는 API 엔드포인트 추가
    @GetMapping("/api/recommendations")
    public ResponseEntity<List<ExpressionDto>> getRecommendations(Principal principal) {
        // Long accountId = Long.parseLong(principal.getName()); // 실제 로그인 구현 시 사용
        Long testAccountId = 1L; // 테스트용 ID

        List<ExpressionDto> recommendations = expressionService.getDailyRecommendations(testAccountId);

        return ResponseEntity.ok(recommendations);
    }

    @GetMapping("/detail/{exprId}")
    @ResponseBody
    public ResponseEntity<WordInfoDto> getWordDetailModal(@PathVariable Long exprId) {
        WordInfoDto wordDetail = expressionService.getWordDetail(exprId);
        return ResponseEntity.ok(wordDetail);
    }

    @PostMapping("/favorite/{exprId}")
    @ResponseBody
    public ResponseEntity<Boolean> toggleFavoriteStatus(Principal principal,
                                                        @PathVariable Long exprId) {
        if (principal == null) {
            return ResponseEntity.status(401).build(); // 401 Unauthorized
        }
//        Account account = accountRepository.findByUsername(principal.getName())
//                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Account account = new Account();
        account.setId(1L);
        account.setName(principal.getName());

        boolean isFavorite = expressionService.toggleFavoriteStatus(account.getId(), exprId);
        return ResponseEntity.ok(isFavorite);
    }

    @GetMapping("/api/wrong-answers")
    public ResponseEntity<List<IncorrectExprDto>> getWrongAnswers(Principal principal) {
        //        Account account = accountRepository.findByUsername(principal.getName())
//                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Long testAccountId = 1L;

        List<IncorrectExprDto> incorrectExpressions = expressionService.getIncorrectExpressionsAsList(testAccountId);

        return ResponseEntity.ok(incorrectExpressions);
    }


}
