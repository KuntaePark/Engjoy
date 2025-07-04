package com.engjoy.control;

import com.engjoy.Dto.PasswordResetDto;
import com.engjoy.service.PasswordResetService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.Map;


    @RestController
    @RequestMapping("/api")
    public class ResetController {

        private final PasswordResetService passwordResetService;

        public ResetController(PasswordResetService passwordResetService) {
            this.passwordResetService = passwordResetService;
        }

        // 이메일로 비밀번호 재설정 링크 요청
        @PostMapping("/send-reset-link")
        public ResponseEntity<String> sendResetLink(@RequestBody Map<String, String> payload) {
            String email = payload.get("email");

            boolean sent = passwordResetService.sendResetLink(email);
            if (sent) {
                return ResponseEntity.ok("비밀번호 재설정 링크가 전송되었습니다.");
            } else {
                return ResponseEntity.badRequest().body("해당 이메일을 찾을 수 없습니다.");
            }
        }

        // 실제 비밀번호 변경 요청
        @PostMapping("/reset-password")
        public ResponseEntity<String> resetPassword(@RequestBody PasswordResetDto dto) {
            boolean result = passwordResetService.resetPassword(dto.getToken(), dto.getNewPassword());

            if (result) {
                return ResponseEntity.ok("비밀번호가 성공적으로 변경되었습니다.");
            } else {
                return ResponseEntity.badRequest().body("토큰이 유효하지 않거나 만료되었습니다.");
            }
        }
    }


