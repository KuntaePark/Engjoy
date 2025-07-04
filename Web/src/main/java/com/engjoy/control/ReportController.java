package com.engjoy.control;

import com.engjoy.dto.ReportDataDto;
import com.engjoy.entity.Account;
import com.engjoy.service.ReportService;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;

import java.security.Principal;
import java.time.Year;

@Controller
@RequestMapping("/report")
@RequiredArgsConstructor
public class ReportController {
    private final ReportService reportService;

    @GetMapping
    public String getReportPage(Principal principal, Model model){
        //        Account account = accountRepository.findByUsername(principal.getName())
        //                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));

//        Account account = new Account();
//        account.setId(1L);
//        account.setName(principal.getName());

        Long testAccountId = 1L;

        int currentYear = Year.now().getValue(); // 현재 년도 히트맵

        ReportDataDto report = reportService.getReportData(testAccountId);
        model.addAttribute("report",report);
        model.addAttribute("currentYear", currentYear);
        return "report";
    }
}
