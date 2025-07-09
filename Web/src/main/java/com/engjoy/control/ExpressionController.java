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

        Sort stableSort = Sort.by(Sort.Direction.DESC, "usedTime").and(Sort.by(Sort.Direction.DESC, "id"));
        Pageable initialPageable = PageRequest.of(0, 20, stableSort);

        Map<String, List<ExpressionDto>> studyLogData = expressionService.getStudyLog(testAccountId, searchDto, initialPageable);

        model.addAttribute("studyLogData", studyLogData);
        model.addAttribute("searchDto", searchDto);
        return "expressions";
    }
    @GetMapping("/api")
    public ResponseEntity<?> getExpressionPage(
            @RequestParam(name = "view", defaultValue = "study_log") String view,
            ExpressionSearchDto searchDto,
            @RequestParam(value = "page", defaultValue = "0") int page,
            // ⭐️ 기본값을 usedTime,desc로 변경
            @RequestParam(value = "sort", defaultValue = "usedTime,desc") String sort,
            Principal principal) {

        Long testAccountId = 1L;

        if ("dictionary".equals(view)) {
            String[] sortParams = sort.split(",");
            Sort dictionarySort = Sort.by(Sort.Direction.fromString(sortParams[1]), sortParams[0]);
            Pageable pageable = PageRequest.of(page, 12, dictionarySort);
            Page<ExpressionDto> expressionPage = expressionService.getExpressions(testAccountId, searchDto, pageable);
            return ResponseEntity.ok(expressionPage);

        } else { //study_log 뷰
            // 프론트에서 받은 sort 문자열(예: "difficulty,desc")을 분리합니다.
            String[] sortParams = sort.split(",");
            String property = sortParams[0]; // 정렬 기준 필드 (예: "difficulty")
            Sort.Direction direction = Sort.Direction.fromString(sortParams[1]); // 정렬 방향 (예: "desc")

            if ("difficulty".equals(property)) {
                property = "expression.difficulty";
            }

            // ⭐️ 2. 분리된 값으로 Sort 객체를 만듭니다.
            Sort requestedSort = Sort.by(direction, property);

            // ⭐️ 3. 날짜 정렬 시에는 안정성을 위해 id를 2차 정렬 기준으로 추가합니다.
            if ("usedTime".equals(property)) {
                requestedSort = requestedSort.and(Sort.by(Sort.Direction.DESC, "id"));
            }

            // 최종적으로 만들어진 정렬 기준을 사용
            Pageable pageable = PageRequest.of(page, 20, requestedSort);
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
