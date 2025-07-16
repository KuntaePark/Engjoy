package com.engjoy.control;

import com.engjoy.dto.PrintOptionDto;
import com.engjoy.entity.Account;
import com.engjoy.repository.AccountRepository;
import com.engjoy.service.PrintService;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;

import java.io.IOException;
import java.security.Principal;

@Controller
@RequestMapping("/print")
@RequiredArgsConstructor
public class PrintController {
    private final PrintService printService;
    private final AccountRepository accountRepository;

    // 인쇄 옵션 설정 페이지
    @GetMapping("/setting")
    public String showPrintSettingPage(){
        return "printSetting";
    }

    // 사용자가 선택한 옵션을 받아 PDF 생성 및 다운로드
    @PostMapping("/download")
    public ResponseEntity<byte[]> downloadPdf(
            @RequestBody PrintOptionDto printOptionDto,
            Principal principal
            ) throws IOException{
        if (principal == null) {
            return ResponseEntity.status(HttpStatus.UNAUTHORIZED).build();
        }
        String userEmail = principal.getName();
        Account account = accountRepository.findByEmail(principal.getName())
                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Long accountId = account.getId();

        byte[] pdfBytes = printService.createPrintablePdf(printOptionDto, accountId);

        // HTTP 응답 헤더 설정
        HttpHeaders headers = new HttpHeaders();
        // 1. 콘텐츠 타입을 PDF로 지정
        headers.setContentType(MediaType.APPLICATION_PDF);
        // 2. 브라우저가 파일을 다운로드하도록 설정하고, 파일 이름 제안
        headers.setContentDispositionFormData("attachment", "my_word_list.pdf");

        // 생성된 PDF 바이트 배열을 HTTP 응답 본문에 담아 반환
        return ResponseEntity.ok()
                .headers(headers)
                .body(pdfBytes);
    }


}
