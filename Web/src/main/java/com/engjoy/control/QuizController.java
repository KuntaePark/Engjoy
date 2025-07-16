package com.engjoy.control;

import com.engjoy.dto.*;
import com.engjoy.entity.Account;
import com.engjoy.repository.AccountRepository;
import com.engjoy.service.QuizService;
import jakarta.servlet.http.HttpSession;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.mvc.support.RedirectAttributes;

import java.security.Principal;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

@Controller
@RequestMapping("/quiz")
@RequiredArgsConstructor
public class QuizController {
    private final QuizService quizService;
    private static final String QUIZ_STATE = "QUIZ_STATE";
    private final AccountRepository accountRepository;

    // 퀴즈 설정 페이지
    @GetMapping("/setting")
    public String quizSettingPage(Model model){
        return "quizSetting";
    }


    // 퀴즈 생성 및 시작
    @PostMapping("/start")
    public String getQuiz(QuizSettingDto quizSettingDto,
                          Principal principal,
                          HttpSession session,
                          RedirectAttributes redirectAttributes){
        System.out.println("[Controller] 폼에서 받은 카테고리: " + quizSettingDto.getCategory());

        if (principal == null) {
            return "redirect:/login";
        }
        String userEmail = principal.getName();
        Account account = accountRepository.findByEmail(principal.getName())
                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Long accountId = account.getId();

        QuizPageDto quizState = quizService.createQuizQuestions(accountId,quizSettingDto);

        if (quizState.getQuestions().isEmpty()) {
            // 3. 문제가 없으면, 알림 메시지를 담아서 설정 페이지로 "돌려보냄"
            redirectAttributes.addFlashAttribute("notifyMsg", quizState.getNotifyMsg());
            return "redirect:/quiz/setting";
        }

        session.setAttribute("QUIZ_STATE", quizState);

        if (quizState.getNotifyMsg() != null ) {
            redirectAttributes.addFlashAttribute("notifyMsg", quizState.getNotifyMsg());
        }

        return "redirect:/quiz/take";
    }

    // 퀴즈 풀이 페이지
    @GetMapping("/take")
    public String takeQuizPage(
            @RequestParam(name = "index", defaultValue = "0") int index,
            HttpSession session,Model model){
        QuizPageDto quizState = (QuizPageDto) session.getAttribute(QUIZ_STATE);

        if (quizState == null || quizState.getQuestions().isEmpty()) {
            return "redirect:/quiz/setting";
        }
        List<QuizQuestionDto> allQuestions = quizState.getQuestions();

        // 인덱스 검사 (문제 더 있는지 확인)
        if(index >= allQuestions.size()){
            return "redirect:/quizResult";
        }
        // 현재 인덱스에 해당하는 문제 꺼내기
        QuizQuestionDto currentQuestion = allQuestions.get(index);

        model.addAttribute("question",currentQuestion);
        model.addAttribute("currentIndex",index);
        model.addAttribute("totalQuestions",allQuestions.size()); // 진행 상태 표시용

        return "take";
    }
    // 답안 제출 및 채점
    @PostMapping("/submit")
    @ResponseBody
    public ResponseEntity<QuizGradedDto> submitQuiz(@RequestBody QuizAnsweredDto quizAnsweredDto,
                                                    Principal principal,
                                                    HttpSession session){
        Object quizState = session.getAttribute(QUIZ_STATE);
        if(quizState==null){
            return ResponseEntity.badRequest().build();
        }
        if (principal == null) {
            return ResponseEntity.status(HttpStatus.UNAUTHORIZED).build();
        }
        String userEmail = principal.getName();
        Account account = accountRepository.findByEmail(principal.getName())
                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Long accountId = account.getId();
        QuizGradedDto gradedResult = quizService.gradeQuizAnswer(accountId, quizAnsweredDto, session);
        return ResponseEntity.ok(gradedResult);
    }

    // 퀴즈 최종 결과
    @GetMapping("/result")
    @ResponseBody
    public QuizResultDto quizResult(Principal principal, HttpSession session){
        if (principal == null) {
            throw new SecurityException("로그인이 필요합니다.");
        }
        String userEmail = principal.getName();
        Account account = accountRepository.findByEmail(principal.getName())
                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Long accountId = account.getId();

        QuizResultDto finalResult = quizService.calculateQuizResult(accountId,session);

        return finalResult;
    }

    // 퀴즈 중단 후 세션 초기화
    @GetMapping("/exit")
    public String exitQuiz(HttpSession session){
        session.removeAttribute(QUIZ_STATE);
        session.removeAttribute("QUIZ_GRADED_RESULTS");
        return "redirect:/expressions";
    }

    @GetMapping("/check-availability")
    @ResponseBody
    public ResponseEntity<Map<String, Object>> checkQuizAvailability(
            QuizSettingDto quizSettingDto, Principal principal) {

        if (principal == null) {
            return ResponseEntity.status(HttpStatus.UNAUTHORIZED).build();
        }
        String userEmail = principal.getName();
        Account account = accountRepository.findByEmail(principal.getName())
                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Long accountId = account.getId();

        Map<String, Object> response = new HashMap<>();

        // 1. 기존의 createQuizQuestions 메서드를 그대로 호출하여 퀴즈 정보를 미리 받아옵니다.
        QuizPageDto quizInfo = quizService.createQuizQuestions(accountId, quizSettingDto);

        // 2. 받아온 퀴즈 정보(quizInfo)를 바탕으로 상태를 결정합니다.
        if (quizInfo.getQuestions().isEmpty()) {
            // [UNAVAILABLE]: 문제가 아예 없는 경우
            response.put("status", "UNAVAILABLE");
            response.put("message", quizInfo.getNotifyMsg()); // "출제할 문제가 없습니다."

        } else if (quizInfo.getNotifyMsg() != null) {
            // [AVAILABLE_PARTIAL]: 문제는 있지만 부족한 경우 (notifyMsg가 생성됨)
            response.put("status", "AVAILABLE_PARTIAL");
            response.put("message", quizInfo.getNotifyMsg()); // "N개 뿐입니다..."

        } else {
            // [AVAILABLE_FULL]: 문제가 충분한 경우
            response.put("status", "AVAILABLE_FULL");
            response.put("message", "퀴즈를 시작하시겠습니까?");
        }

        return ResponseEntity.ok(response);
    }


}