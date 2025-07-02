package com.engjoy.control;

import com.engjoy.dto.*;
import com.engjoy.service.QuizService;
import jakarta.servlet.http.HttpSession;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.*;

import java.security.Principal;
import java.util.List;

@Controller
@RequestMapping("/quiz")
@RequiredArgsConstructor
public class QuizController {
    private final QuizService quizService;
    private static final String QUIZ_STATE = "QUIZ_STATE";

    // 퀴즈 설정 페이지
    @GetMapping("/setting")
    public String quizSettingPage(Model model){
        return "quizSetting";
    }

    // 퀴즈 생성 및 시작
    @PostMapping("/start")
    public String getQuiz(QuizSettingDto quizSettingDto,
                          Principal principal,
                          HttpSession session){

//        Account account = accountRepository.findByUsername(principal.getName())
//                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Long testAccountId = 1L;

        QuizPageDto quizState = quizService.createQuizQuestions(testAccountId,quizSettingDto);
        session.setAttribute("QUIZ_STATE", quizState);
        return "redirect:/quiz/take";
    }

    // 퀴즈 풀이 페이지
    @GetMapping("/take")
    public String takeQuizPage(
            @RequestParam(name = "index", defaultValue = "0") int index,
            HttpSession session,Model model){
        QuizPageDto quizState = (QuizPageDto) session.getAttribute(QUIZ_STATE);

        if(quizState == null ){
            return "redirect:/quizSetting";
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
        //        Account account = accountRepository.findByUsername(principal.getName())
//                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Long testAccountId = 1L;
        QuizGradedDto gradedResult = quizService.gradeQuizAnswer(testAccountId, quizAnsweredDto, session);
        return ResponseEntity.ok(gradedResult);
    }

    // 퀴즈 최종 결과
    @GetMapping("/result")
    @ResponseBody
    public QuizResultDto quizResult(Principal principal, HttpSession session){
//        Account account = accountRepository.findByUsername(principal.getName())
//                .orElseThrow(() -> new IllegalArgumentException("사용자를 찾을 수 없습니다."));
        Long testAccountId = 1L;
        QuizResultDto finalResult = quizService.calculateQuizResult(testAccountId,session);

        return finalResult;
    }

    // 퀴즈 중단 후 세션 초기화
    @GetMapping("/exit")
    public String exitQuiz(HttpSession session){
        session.removeAttribute(QUIZ_STATE);
        return "redirect:/expressions";
    }

}
