package com.engjoy.service;

import com.engjoy.constant.CATEGORY;
import com.engjoy.constant.EXPRTYPE;
import com.engjoy.constant.ORDERTYPE;
import com.engjoy.constant.PRINTFORM;
import com.engjoy.dto.PrintContentDto;
import com.engjoy.dto.PrintOptionDto;
import com.engjoy.dto.PrintReviewDto;
import com.engjoy.dto.QuizSettingDto;
import com.engjoy.entity.Expression;
import com.engjoy.repository.ExpressionRepository;
import com.engjoy.repository.WordInfoRepository;
import com.itextpdf.kernel.font.PdfFont;
import com.itextpdf.kernel.font.PdfFontFactory;
import com.itextpdf.kernel.pdf.PdfDocument;
import com.itextpdf.kernel.pdf.PdfWriter;
import com.itextpdf.layout.Document;
import com.itextpdf.layout.element.Paragraph;
import com.itextpdf.layout.properties.TextAlignment;
import lombok.RequiredArgsConstructor;
import org.aspectj.weaver.ast.Expr;
import org.springframework.core.io.ClassPathResource;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Service;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class PrintService {

    private final ExpressionRepository expressionRepository;
    private final WordInfoRepository wordInfoRepository;

    // 사용자의 인쇄 옵션에 따라 PDF생성 후 byte배열로 반환
    public byte[] createPrintablePdf(PrintOptionDto printOptionDto, Long accountId) throws IOException {
        // 인쇄할 Expression 목록 조회
        List<Expression> expressionsToPrint = getExpressionsToPrint(printOptionDto, accountId);

        // --- 👇 테스트용 비상 플랜 추가 ---
        // 2. 만약 인쇄할 목록이 없다면, 전체 단어장에서 20개를 가져와서 테스트
        if (expressionsToPrint.isEmpty()) {
            System.out.println("⚠️ 테스트: 인쇄할 데이터가 없어 전체 단어장에서 20개를 가져옵니다.");
            Pageable limit = PageRequest.of(0, 20); // 너무 많으면 느리니 20개로 제한
            expressionsToPrint = expressionRepository.findRandomExpressions(limit);
        }
        // ---


        // 사용자가 선택한 정렬 적용
        List<Expression> sortedExpressions = applyOrderType(expressionsToPrint, printOptionDto.getPrintOptionDetailDto().getOrderType());
        // 인쇄 형태에 맞게 데이터 목록 생성
        List<PrintContentDto> contents = createContents(sortedExpressions, printOptionDto.getPrintForm());

        return createPdf(contents, printOptionDto);
    }

    // 인쇄 옵션에 따라 인쇄할 목록 가져오기
    private List<Expression> getExpressionsToPrint(PrintOptionDto printOptionDto, Long accountId) {
        if (printOptionDto.isSelectAll()) {
            // '전체 선택'
            QuizSettingDto filters = printOptionDto.getQuizSettingDto();
            // 날짜 범위 설정
            LocalDateTime startDate = null;
            LocalDateTime endDate = null;

            if (filters.getDateRange() != null) {
                switch (filters.getDateRange()) {
                    case TODAY:
                        startDate = LocalDateTime.now().toLocalDate().atStartOfDay();
                        endDate = startDate.plusDays(1);
                        break;
                    case LAST_WEEK:
                        startDate = LocalDateTime.now().minusWeeks(1);
                        endDate = LocalDateTime.now();
                        break;
                    case LAST_MONTH:
                        startDate = LocalDateTime.now().minusMonths(1);
                        endDate = LocalDateTime.now();
                        break;
                    case CUSTOM:
                        if (filters.getStartDate() != null && filters.getEndDate() != null) {
                            startDate = filters.getStartDate().atStartOfDay();
                            endDate = filters.getEndDate().plusDays(1).atStartOfDay();
                        }
                        break;

                }
            }

            EXPRTYPE exprType = null; // 기본값은 null (타입 전체)
            if (filters.getCategory() != null && filters.getCategory() != CATEGORY.MIXED) {
                // MIXED가 아니면, CATEGORY를 EXPRTYPE으로 변환
                exprType = EXPRTYPE.valueOf(filters.getCategory().name());
            }

            // ✅ 항상 새로운 findWithFilters 메서드 하나만 호출
            return expressionRepository.findWithFilters(
                    accountId,
                    exprType, // MIXED일 경우 null이 전달됨
                    startDate,
                    endDate
            );

        } else {
            return expressionRepository.findAllById(printOptionDto.getExprIdsToPrint());
        }
    }

    // Expression을 선택한 정렬 옵션대로 정렬
    private List<Expression> applyOrderType(List<Expression> expressions, ORDERTYPE orderType) {
        List<Expression> sortedExpressions = new ArrayList<>(expressions);
        switch (orderType) {
            case SUFF:
                Collections.shuffle(sortedExpressions);
                break;
            case ABC:
                sortedExpressions.sort((e1, e2) -> e1.getWordText().compareToIgnoreCase(e2.getWordText()));
                break;
            case STAY:
            default:
                break;
        }
        return sortedExpressions;
    }

    // 인쇄 형태에 따라 컨텐츠 생성메서드 호출
    private List<PrintContentDto> createContents(List<Expression> expressions, PRINTFORM printForm) {
        switch (printForm) {
            case EXAM:
                return makeTestSheet(expressions);
            case LIST:
                return makeList(expressions);
            case WORKSHEET:
                return makeWorkSheet(expressions);
            default:
                throw new IllegalArgumentException("지원하지 않는 인쇄 형태입니다.");
        }
    }

    // 시험지 형식에 맞는 데이터 목록 생성
    private List<PrintContentDto> makeTestSheet(List<Expression> expressions){
        List<String> allMeanings = expressionRepository.findAllMeanings(); // 오답 보기용 전체 뜻 목록

        return expressions.stream().map(expression -> {
            List<String> choices = selectRandomWrongChoices(allMeanings, expression.getMeaning(),3);
            choices.add(expression.getMeaning());
            Collections.shuffle(choices);

            return new PrintContentDto(
                    expression.getWordText(), // question
                    expression.getMeaning(),  // answer
                    choices,                  // choices
                    null, null,           // wordText, meaning 은 null
                    null, null, null, null, null // 나머지 필드도 null
            );
        }).collect(Collectors.toList());
    }

    // 단어-뜻 리스트 형식
    private List<PrintContentDto> makeList(List<Expression> expressions) {
        return expressions.stream()
                .map(expr -> new PrintContentDto(
                        null, null, null,     // question, answer, choices 는 null
                        expr.getWordText(),   // wordText
                        expr.getMeaning(),    // meaning
                        null, null, null, null, null // 나머지 필드도 null
                ))
                .collect(Collectors.toList());
    }

    // 워크시트 형식
    private List<PrintContentDto> makeWorkSheet(List<Expression> expressions) {
        return expressions.stream()
                .map(expr -> new PrintContentDto(
                        null, null, null,     // question, answer, choices 는 null
                        expr.getWordText(),   // wordText
                        "________________",   // meaning
                        null, null, null, null, null // 나머지 필드도 null
                ))
                .collect(Collectors.toList());
    }
    // 오답 보기 무작위 추출
    private List<String> selectRandomWrongChoices(List<String> allMeanings, String correctAnswer, int count) {
        List<String> wrongAnswers = new ArrayList<>(allMeanings);
        wrongAnswers.remove(correctAnswer);
        Collections.shuffle(wrongAnswers);
        return wrongAnswers.stream().limit(count).collect(Collectors.toList());
    }

    // 생성된 데이터 목록을 받아 최총 PDF 문서
    private byte[] createPdf(List<PrintContentDto> contents, PrintOptionDto printOptionDto) throws IOException {
        // PDF를 메모리에 생성하기 위한 스트림
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        PdfWriter writer = new PdfWriter(baos);
        PdfDocument pdf = new PdfDocument(writer);
        Document document = new Document(pdf);

        // 1. 한글 폰트 설정 (프로젝트의 /resources/fonts/ 폴더에 폰트 파일이 있어야 함)
        ClassPathResource fontResource = new ClassPathResource("fonts/Pretendard-Regular.ttf");
        PdfFont font = PdfFontFactory.createFont(fontResource.getURL().toString(), "Identity-H");
        document.setFont(font);

        // 2. 문서 제목 추가
        document.add(new Paragraph("나의 단어/문장 학습지")
                .setFontSize(20)
                .setBold()
                .setTextAlignment(TextAlignment.CENTER)
                .setMarginBottom(20)); // 제목 아래 여백 추가

        // 3. 전달받은 콘텐츠 목록을 PDF에 추가
        for (int i = 0; i < contents.size(); i++) {
            PrintContentDto content = contents.get(i);
            String line;

            // 4. 인쇄 형태에 따라 사용할 데이터 필드를 결정
            if (printOptionDto.getPrintForm() == PRINTFORM.EXAM) {
                // 시험지 형식일 경우: 문제(단어)만 표시
                // content.getQuestion()은 이전에 makeTestSheet에서 wordText 값으로 채워졌습니다.
                line = (i + 1) + ". " + content.getQuestion();
            } else {
                // 다른 형식일 경우: 단어와 뜻 모두 표시
                line = (i + 1) + ". " + content.getWordText() + " : " + content.getMeaning();
            }

            document.add(new Paragraph(line).setFontSize(12).setMarginTop(10));

            // 6. 시험지 형식일 경우 객관식 보기를 추가
            if (printOptionDto.getPrintForm() == PRINTFORM.EXAM && content.getChoices() != null) {
                for (int j = 0; j < content.getChoices().size(); j++) {
                    String choiceLine = "   " + (j + 1) + ") " + content.getChoices().get(j);
                    document.add(new Paragraph(choiceLine).setFontSize(11));
                }
            }
        }

        // 7. 문서 작업 완료
        document.close();

        // 8. 생성된 PDF의 byte 배열 반환
        return baos.toByteArray();
    }

}


