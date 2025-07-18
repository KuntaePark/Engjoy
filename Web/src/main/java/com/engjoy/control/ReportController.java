package com.engjoy.control;

import com.engjoy.dto.IncorrectExprDto;
import com.engjoy.dto.ReportDataDto;
import com.engjoy.entity.Account;
import com.engjoy.repository.AccountRepository;
import com.engjoy.service.ReportService;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.ResponseBody;

import java.security.Principal;
import java.time.Year;
import java.util.List;

@Controller
@RequestMapping("/report")
@RequiredArgsConstructor
public class ReportController {
    private final ReportService reportService;
    private final AccountRepository accountRepository;

    @GetMapping
    public String getReportPage(Principal principal, Model model){
        if (principal == null) {
            return "redirect:/login";
        }
        String userEmail = principal.getName();
        Account account = accountRepository.findByEmail(principal.getName())
                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Long accountId = account.getId();

        int currentYear = Year.now().getValue(); // 현재 년도 히트맵

        ReportDataDto report = reportService.getReportData(accountId);
        model.addAttribute("report",report);
        model.addAttribute("currentYear", currentYear);
        return "report";
    }

    @GetMapping("/wrong")
    @ResponseBody
    public List<IncorrectExprDto> getWrongAnswers(Principal principal){
        if (principal == null) {
            throw new SecurityException("로그인이 필요합니다.");
        }
        String userEmail = principal.getName();
        Account account = accountRepository.findByEmail(principal.getName())
                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Long accountId = account.getId();
        return reportService.getWrongList(accountId);
    }
}
