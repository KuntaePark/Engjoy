package com.engjoy.control;

import com.engjoy.dto.ExpressionDto;
import com.engjoy.dto.ExpressionSearchDto;
import com.engjoy.dto.IncorrectExprDto;
import com.engjoy.dto.WordInfoDto;
import com.engjoy.entity.Account;
import com.engjoy.service.ExpressionService;
import lombok.RequiredArgsConstructor;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.web.PageableDefault;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.*;

import java.security.Principal;

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
