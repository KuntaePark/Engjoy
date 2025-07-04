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
    public ResponseEntity<Page<ExpressionDto>> getExpressionPage(
            ExpressionSearchDto searchDto,
            @RequestParam(value = "page", defaultValue = "0") int page,
            @RequestParam(value = "sort", defaultValue = "id,desc") String sort,
            Principal principal) {

        // 1. 정렬 조건 파싱
        String[] sortParams = sort.split(",");
        String sortField = sortParams[0];
        Sort.Direction direction = (sortParams.length > 1 && sortParams[1].equalsIgnoreCase("asc"))
                ? Sort.Direction.ASC : Sort.Direction.DESC;

        // 2. Pageable 객체 생성
        Pageable pageable = PageRequest.of(page, 12, Sort.by(direction, sortField)); // 한 페이지에 12개씩

        // 3. 서비스 호출
        // Long accountId = Long.parseLong(principal.getName()); // 실제 로그인 구현 시 사용
        Long testAccountId = 1L; // 테스트용 계정 ID

        Page<ExpressionDto> expressionPage = expressionService.getExpressions(testAccountId, searchDto, pageable);

        return ResponseEntity.ok(expressionPage);
    }

    // ✅ 오늘의 추천 단어를 제공하는 API 엔드포인트 추가
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

    @GetMapping("/incorrect")
    public String getIncorrectList(Principal principal,
                                   @PageableDefault(size = 10) Pageable pageable,
                                   Model model) {
        if (principal == null) {
            return "redirect:/login";
        }
//        Account account = accountRepository.findByUsername(principal.getName())
//                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Account account = new Account();
        account.setId(1L);
        account.setName(principal.getName());

        Page<IncorrectExprDto> incorrectPage = expressionService.getIncorrectExpressions(account.getId(), pageable);
        model.addAttribute("incorrectPage", incorrectPage);
        return "incorrect";
    }


}
