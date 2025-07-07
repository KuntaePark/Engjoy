package com.engjoy.service;



import com.engjoy.constant.CATEGORY;
import com.engjoy.constant.EXPRTYPE;
import com.engjoy.constant.ORDERTYPE;
import com.engjoy.constant.PRINTFORM;
import com.engjoy.dto.*;
import com.engjoy.entity.Expression;
import com.engjoy.repository.ExpressionRepository;
import com.engjoy.repository.WordInfoRepository;
import com.itextpdf.kernel.colors.ColorConstants;
import com.itextpdf.kernel.font.PdfFont;
import com.itextpdf.kernel.font.PdfFontFactory;
import com.itextpdf.kernel.geom.PageSize;
import com.itextpdf.kernel.geom.Rectangle;
import com.itextpdf.kernel.pdf.PdfDocument;
import com.itextpdf.kernel.pdf.PdfWriter;
import com.itextpdf.kernel.pdf.canvas.PdfCanvas;
import com.itextpdf.layout.ColumnDocumentRenderer;
import com.itextpdf.layout.Document;
import com.itextpdf.layout.element.Cell;
import com.itextpdf.layout.element.Div;

import com.itextpdf.layout.element.Paragraph;

import com.itextpdf.layout.element.Table;

import com.itextpdf.layout.properties.TextAlignment;

import com.itextpdf.layout.properties.UnitValue;

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



// 항상 새로운 findWithFilters 메서드 하나만 호출

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



// — 시험지(EXAM) DTO 생성 —

    private List<PrintContentDto> makeTestSheet(List<Expression> exprs) {

// 1) 전체 영단어 pool

        List<String> allWords = expressionRepository.findWordTextsByType(EXPRTYPE.WORD);

        List<String> allSentences = expressionRepository.findWordTextsByType(EXPRTYPE.SENTENCE);



        return exprs.stream().map(e -> {

// 2) 현재 문제(e)의 타입에 따라 사용할 보기 pool을 선택 (수정)

            List<String> choicePool;

            if (e.getExprType() == EXPRTYPE.WORD) {

                choicePool = new ArrayList<>(allWords);

            } else { // SENTENCE 또는 기타

                choicePool = new ArrayList<>(allSentences);

            }



// 3) 선택된 pool에서 오답 후보 3개 + 정답 1개 추출

            List<String> pool = selectRandomWrongChoices(choicePool, e.getWordText(), 3);

            pool.add(e.getWordText());

            Collections.shuffle(pool);



            return new PrintContentDto(

                    /* question */ e.getMeaning(),

                    /* answer */ e.getWordText(), // 정답 확인을 위해 answer 필드에도 단어/문장 추가

                    /* choices */ pool,

                    null, null, null, null, null, null, null

            );

        }).collect(Collectors.toList());

    }



// 단어-뜻 리스트 형식

    private List<PrintContentDto> makeList(List<Expression> expressions) {

        return expressions.stream()

                .map(expr -> new PrintContentDto(

                        null, null, null, // question, answer, choices 는 null

                        expr.getWordText(), // wordText

                        expr.getMeaning(), // meaning

                        null, null, null, null, null // 나머지 필드도 null

                ))

                .collect(Collectors.toList());

    }



// 워크시트 형식

    private List<PrintContentDto> makeWorkSheet(List<Expression> expressions) {

        return expressions.stream()

                .map(expr -> new PrintContentDto(

                        null, null, null, // question, answer, choices 는 null

                        expr.getWordText(), // wordText

                        "", // meaning

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



// 생성된 데이터 목록을 받아 최종 PDF 문서

    private byte[] createPdf(List<PrintContentDto> contents, PrintOptionDto opt) throws IOException {
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        PdfWriter writer = new PdfWriter(baos);
        PdfDocument pdf = new PdfDocument(writer);
        Document document = new Document(pdf, PageSize.A4);
        float margin = 40;
        document.setMargins(margin, margin, margin, margin);

        ClassPathResource fontResource = new ClassPathResource("fonts/Pretendard-Regular.ttf");
        PdfFont koreanFont = PdfFontFactory.createFont(fontResource.getURL().getPath(), PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
        document.setFont(koreanFont);

        // --- ▼▼▼ 헤더 및 폰트 크기 적용 로직 수정 ▼▼▼ ---
        PrintOptionDetailDto detailDto = opt.getPrintOptionDetailDto();

        // 1. 사용자가 선택한 폰트 크기 값에 따라 기본 크기를 결정합니다.
        float baseFontSize = 11f; // '보통' 사이즈를 기본값으로 설정
        if (detailDto != null && detailDto.getFontSize() != null) {
            switch (detailDto.getFontSize()) {
                case LARGE:
                    baseFontSize = 14f;
                    break;
                case SMALL:
                    baseFontSize = 8f;
                    break;
                case MED: // FONTSIZE Enum이 MED라면 MED로 수정
                default:
                    baseFontSize = 11f;
                    break;
            }
        }
        // 2. 문서 전체의 기본 폰트 크기를 설정합니다.
        document.setFontSize(baseFontSize);


        // 3. 헤더를 그립니다. (제목, 이름 등 일부는 고정 크기를 사용)
        if (detailDto != null) {
            String title = detailDto.getPrintTitle();
            if (title != null && !title.trim().isEmpty()) {
                document.add(new Paragraph(title)
                        .setTextAlignment(TextAlignment.CENTER)
                        .setFontSize(20) // 제목은 고정 크기
                        .setBold()
                        .setMarginBottom(5));
            }

            if (detailDto.isUserName()) {
                float nameBoxWidth = 120;
                float nameBoxHeight = 20;
                float nameBoxX = pdf.getDefaultPageSize().getWidth() - document.getRightMargin() - nameBoxWidth;
                float nameBoxY = pdf.getDefaultPageSize().getHeight() - document.getTopMargin() - 30;

                document.add(new Paragraph("Name: ")
                        .setFontSize(12) // 이름 라벨은 고정 크기
                        .setFixedPosition(nameBoxX - 40, nameBoxY + 5, 40)
                        .setTextAlignment(TextAlignment.RIGHT));

                PdfCanvas canvas = new PdfCanvas(pdf.getFirstPage());
                canvas.setStrokeColor(ColorConstants.BLACK);
                canvas.rectangle(nameBoxX, nameBoxY, nameBoxWidth, nameBoxHeight);
                canvas.stroke();
                canvas.release();
            }

            document.add(new Paragraph(" ").setFontSize(12).setMarginBottom(20));
        }
        // --- ▲▲▲ 로직 수정 완료 ▲▲▲ ---


        if (opt.getPrintForm() == PRINTFORM.EXAM) {
            // 시험지(EXAM): 2단 레이아웃
            float gutter = 20;
            float columnWidth = (PageSize.A4.getWidth() - document.getLeftMargin() - document.getRightMargin() - gutter) / 2;
            Rectangle[] columns = new Rectangle[]{
                    new Rectangle(document.getLeftMargin(), document.getBottomMargin(), columnWidth, PageSize.A4.getHeight() - document.getTopMargin() - document.getBottomMargin()),
                    new Rectangle(document.getLeftMargin() + columnWidth + gutter, document.getBottomMargin(), columnWidth, PageSize.A4.getHeight() - document.getTopMargin() - document.getBottomMargin())
            };
            document.setRenderer(new ColumnDocumentRenderer(document, columns));

            int idx = 1;
            for (PrintContentDto item : contents) {
                // 4. 문제와 보기의 글자 크기가 위에서 설정한 baseFontSize를 따르도록 수정합니다.
                Div questionBlock = new Div()
                        .add(new Paragraph(idx++ + ". " + item.getQuestion()).setFontSize(baseFontSize + 1).setBold())
                        .setMarginBottom(15f);
                List<String> choices = item.getChoices();
                while (choices.size() < 4) {
                    choices.add("");
                }

                char[] numLabels = {'①', '②', '③', '④'};
                for (int j = 0; j < 4; j++) {
                    questionBlock.add(new Paragraph(numLabels[j] + " " + choices.get(j))); // setFontSize 제거하여 문서 기본값 상속
                }
                document.add(questionBlock);
            }
        } else {
            // 리스트/워크시트: 2단 레이아웃 (자동으로 문서 기본 폰트 크기가 적용됩니다)
            Table table = new Table(UnitValue.createPercentArray(new float[]{35, 15, 35, 15})).useAllAvailableWidth();
            int totalItems = contents.size();
            int rows = (int) Math.ceil(totalItems / 2.0);
            for (int i = 0; i < rows; i++) {
                PrintContentDto leftItem = contents.get(i);
                table.addCell(new Cell().add(new Paragraph(leftItem.getWordText())).setPadding(5));
                table.addCell(new Cell().add(new Paragraph(leftItem.getMeaning())).setPadding(5));
                int rightIndex = i + rows;
                if (rightIndex < totalItems) {
                    PrintContentDto rightItem = contents.get(rightIndex);
                    table.addCell(new Cell().add(new Paragraph(rightItem.getWordText())).setPadding(5));
                    table.addCell(new Cell().add(new Paragraph(rightItem.getMeaning())).setPadding(5));
                } else {
                    table.addCell(new Cell().add(new Paragraph("")));
                    table.addCell(new Cell().add(new Paragraph("")));
                }
            }
            document.add(table);
        }
        document.close();
        return baos.toByteArray();
    }




}