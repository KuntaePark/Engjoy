package com.engjoy.service;

import com.engjoy.constant.CATEGORY;
import com.engjoy.constant.EXPRTYPE;
import com.engjoy.constant.ORDERTYPE;
import com.engjoy.constant.PRINTFORM;
import com.engjoy.dto.*;
import com.engjoy.entity.Expression;
import com.engjoy.repository.ExprUsedRepository;
import com.engjoy.repository.ExpressionRepository;
import com.engjoy.repository.WordInfoRepository;
import com.itextpdf.kernel.colors.ColorConstants;
import com.itextpdf.kernel.colors.DeviceRgb;
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
import com.itextpdf.layout.properties.VerticalAlignment;
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

    private final ExprUsedRepository exprUsedRepository;
    private final ExpressionRepository expressionRepository;
    private final WordInfoRepository wordInfoRepository;

    // 사용자의 인쇄 옵션에 따라 PDF 생성 후 byte 배열로 반환
    public byte[] createPrintablePdf(PrintOptionDto printOptionDto, Long accountId) throws IOException {
        // 인쇄할 Expression 목록 조회
        List<Expression> expressionsToPrint = getExpressionsToPrint(printOptionDto, accountId);


        // 선택한 정렬 옵션 적용
        List<Expression> sortedExpressions = applyOrderType(
                expressionsToPrint,
                printOptionDto.getPrintOptionDetailDto().getOrderType()
        );

        // 인쇄 형태에 맞게 DTO 목록 생성
        List<PrintContentDto> contents = createContents(
                sortedExpressions,
                printOptionDto.getPrintForm()
        );

        // PDF 생성
        return createPdf(contents, printOptionDto);
    }

    // 인쇄 옵션에 따라 인쇄할 목록 가져오기
    private List<Expression> getExpressionsToPrint(PrintOptionDto printOptionDto, Long accountId) {
        if (!printOptionDto.isSelectAll()) {
            return expressionRepository.findAllById(printOptionDto.getExprIdsToPrint());
        }

        QuizSettingDto filters = printOptionDto.getQuizSettingDto();

        // 1) 클라이언트에서 전달된 날짜(startDate/endDate)를 그대로 사용
        LocalDateTime startDate = null;
        LocalDateTime endDate   = null;
        if (filters.getDateRange() != null) {
            if (filters.getStartDate() != null && filters.getEndDate() != null) {
                startDate = filters.getStartDate().atStartOfDay();
                endDate   = filters.getEndDate().plusDays(1).atStartOfDay();
            }
        }

        // 2) 카테고리 필터 (MIXED 제외)
        EXPRTYPE exprType = null;
        if (filters.getCategory() != null && filters.getCategory() != CATEGORY.MIXED) {
            exprType = EXPRTYPE.valueOf(filters.getCategory().name());
        }

        // 3) 단일 메서드로 필터 적용
        return exprUsedRepository.findWithFilters(
                accountId,
                exprType,
                startDate,
                endDate
        );
    }


    // 정렬 옵션에 따른 리스트 정렬
    private List<Expression> applyOrderType(List<Expression> expressions, ORDERTYPE orderType) {
        List<Expression> sorted = new ArrayList<>(expressions);
        switch (orderType) {
            case SUFF:
                Collections.shuffle(sorted);
                break;
            case ABC:
                sorted.sort((e1, e2) ->
                        e1.getWordText().compareToIgnoreCase(e2.getWordText())
                );
                break;
            default:
                // STAY: 기본 순서 유지
                break;
        }
        return sorted;
    }

    // 인쇄 형태(EXAM, LIST, WORKSHEET)에 따라 DTO 생성
    private List<PrintContentDto> createContents(List<Expression> exprs, PRINTFORM form) {
        switch (form) {
            case EXAM:
                return makeTestSheet(exprs);
            case LIST:
                return makeList(exprs);
            case WORKSHEET:
                return makeWorkSheet(exprs);
            default:
                throw new IllegalArgumentException("지원하지 않는 인쇄 형태입니다.");
        }
    }

    // 시험지(EXAM)용 DTO 생성
    private List<PrintContentDto> makeTestSheet(List<Expression> exprs) {
        List<String> allWords = expressionRepository.findWordTextsByType(EXPRTYPE.WORD);
        List<String> allSentences = expressionRepository.findWordTextsByType(EXPRTYPE.SENTENCE);

        return exprs.stream().map(e -> {
            // 정답 보기를 위한 pool 선택
            List<String> poolSource =
                    e.getExprType() == EXPRTYPE.WORD ? allWords : allSentences;

            // 오답 후보 3개 + 정답 1개
            List<String> choices = selectRandomWrongChoices(
                    new ArrayList<>(poolSource),
                    e.getWordText(),
                    3
            );
            choices.add(e.getWordText());
            Collections.shuffle(choices);

            return new PrintContentDto(
                    e.getMeaning(),
                    e.getWordText(),
                    choices,
                    null, null, null, null, null, null, null
            );
        }).collect(Collectors.toList());
    }

    // 단어-뜻 리스트(LIST)용 DTO 생성
    private List<PrintContentDto> makeList(List<Expression> exprs) {
        return exprs.stream()
                .map(e -> new PrintContentDto(
                        null, null, null,
                        e.getWordText(),
                        e.getMeaning(),
                        null, null, null, null, null
                ))
                .collect(Collectors.toList());
    }

    // 워크시트(WORD → 빈 칸)용 DTO 생성
    private List<PrintContentDto> makeWorkSheet(List<Expression> exprs) {
        return exprs.stream()
                .map(e -> new PrintContentDto(
                        null, null, null,
                        e.getWordText(),
                        "",
                        null, null, null, null, null
                ))
                .collect(Collectors.toList());
    }

    // 오답 후보 무작위 추출
    private List<String> selectRandomWrongChoices(
            List<String> pool,
            String correct,
            int count
    ) {
        pool.remove(correct);
        Collections.shuffle(pool);
        return pool.stream().limit(count).collect(Collectors.toList());
    }

    // PDF 문서 생성
    private byte[] createPdf(List<PrintContentDto> contents, PrintOptionDto opt) throws IOException {
        // 1) PDF, Document 초기화
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        PdfWriter writer = new PdfWriter(baos);
        PdfDocument pdf = new PdfDocument(writer);
        pdf.addNewPage(PageSize.A4);
        Document document = new Document(pdf, PageSize.A4);
        document.setMargins(40, 40, 40, 40);

        // 2) 한글 폰트 및 기본 텍스트 크기 설정
        ClassPathResource fontRes = new ClassPathResource("fonts/Pretendard-Regular.ttf");
        PdfFont koreanFont = PdfFontFactory.createFont(
                fontRes.getURL().getPath(),
                PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED
        );
        document.setFont(koreanFont);

        float baseSize = 11f;
        PrintOptionDetailDto detail = opt.getPrintOptionDetailDto();
        if (detail != null && detail.getFontSize() != null) {
            switch (detail.getFontSize()) {
                case LARGE:  baseSize = 14f; break;
                case SMALL:  baseSize =  8f; break;
                default:     baseSize = 11f; break;
            }
        }
        document.setFontSize(baseSize);

        // --- 헤더 배경 사각형 추가 (페이지 전체 너비) ---
        PdfCanvas bg = new PdfCanvas(pdf.getFirstPage());
        Rectangle pageSize = pdf.getDefaultPageSize();

        // 1) 배경으로 사용할 높이 (원하시는 만큼)
                float headerBgHeight = 60f;

        // 2) 사각형 좌표 계산
                float x      = 0;                                             // 왼쪽 모서리부터
                float y      = pageSize.getHeight() - headerBgHeight;        // 페이지 최상단에서 headerBgHeight 만큼 아래
                float width  = pageSize.getWidth();                          // 페이지 전체 폭

        // 3) 사각형 그리기
                bg.saveState()
                        .setFillColor(new DeviceRgb(199,234,153))  // 원하는 RGB 색상
                        .rectangle(x, y, width, headerBgHeight)
                        .fill()
                        .restoreState();
        // --- 배경 사각형 끝 ---

        document.showTextAligned(
                new Paragraph(detail.getPrintTitle())
                        .setFontSize(20)
                        .setBold()
                        .setFontColor(ColorConstants.DARK_GRAY),
                pageSize.getWidth() / 2f,                  // X: 페이지 가로 중앙
                pageSize.getHeight() - headerBgHeight/2f, // Y: 헤더 높이의 절반 위치
                TextAlignment.CENTER,                   // 가운데 정렬
                VerticalAlignment.MIDDLE
        );


        // 3) 헤더: 제목과 사용자 이름 상자
        if (detail != null) {
            // --- 이름 박스 그리기 (헤더 배경 위에) ---
            if (detail.isUserName()) {
                PdfCanvas canvas = new PdfCanvas(pdf.getFirstPage());
                // 박스 크기
                float boxW = 120f;
                float boxH = 40f;
                // 헤더 높이(…)와 페이지 크기에서 박스 Y 좌표 계산
                float boxX = pageSize.getWidth() - document.getRightMargin() - boxW;
                float boxY = pageSize.getHeight()
                        - headerBgHeight   // 배경 사각형 최상단
                        + (headerBgHeight - boxH) / 2f; // 중앙 정렬

                // (1) 흰색 라운드 사각형 배경 + 검정 테두리
                canvas.saveState()
                        .setFillColor(ColorConstants.WHITE)
                        .roundRectangle(boxX, boxY, boxW, boxH, 4f) // 4pt 반경
                        .fill()
                        .restoreState();

                float paddingLeft = 6f;
                float paddingTop = 6f;
                // (2) 내부에 “Name :” 레이블 텍스트
                document.showTextAligned(
                        new Paragraph("Name :")
                                .setFontSize(12)
                                .setFontColor(new DeviceRgb(140,206,71)),
                        boxX + paddingLeft,
                        boxY + boxH - paddingTop,
                        TextAlignment.LEFT,
                        VerticalAlignment.MIDDLE
                );
            }


        }

        // 4) **공통** 컬럼 레이아웃 설정 (시험지, 리스트/워크시트 모두 사용)
        float gutter       = 20f;
        float headerHeight = 60f;  // 실제 제목+여백 높이에 맞춰 조정
        float colWidth     = (PageSize.A4.getWidth()
                - document.getLeftMargin()
                - document.getRightMargin()
                - gutter) / 2f;
        float colHeight    = PageSize.A4.getHeight()
                - document.getTopMargin()
                - document.getBottomMargin()
                - headerHeight;

        Rectangle leftCol  = new Rectangle(
                document.getLeftMargin(),
                document.getBottomMargin(),
                colWidth, colHeight
        );
        Rectangle rightCol = new Rectangle(
                document.getLeftMargin() + colWidth + gutter,
                document.getBottomMargin(),
                colWidth, colHeight
        );
        document.setRenderer(new ColumnDocumentRenderer(
                document,
                new Rectangle[]{ leftCol, rightCol }
        ));

        // 5) 본문: 시험지(EXAM) vs 리스트/워크시트
        if (opt.getPrintForm() == PRINTFORM.EXAM) {
            // ─── 시험지: 두 컬럼에 자동 배치 ───
            int idx = 1;
            for (PrintContentDto dto : contents) {
                Div block = new Div()
                        .add(new Paragraph(idx++ + ". " + dto.getQuestion())
                                .setFontSize(baseSize + 1)
                                .setBold()
                        )
                        .setMarginBottom(15f)
                        .setKeepTogether(true);

                List<String> choices = dto.getChoices();
                while (choices.size() < 4) choices.add("");
                char[] labels = {'①','②','③','④'};
                for (int i = 0; i < 4; i++) {
                    block.add(new Paragraph(labels[i] + " " + choices.get(i)));
                }
                document.add(block);
            }

        } else {
            // ─── LIST/WORKSHEET 전용 테이블 ───
            Table table = new Table(UnitValue.createPercentArray(new float[]{50, 50}))
                    .useAllAvailableWidth();

            for (PrintContentDto dto : contents) {
                boolean isSentence = dto.getWordText().contains(" ");

                if (isSentence) {
                    // 문장일 경우
                    table.addCell(new Cell(1, 2).add(new Paragraph(dto.getWordText())).setPadding(5));

                    Cell meaningCell = new Cell(1, 2).add(new Paragraph(dto.getMeaning())).setPadding(5);

                    // 워크시트 형식일 때만 답변 셀의 최소 높이를 강제로 지정합니다.
                    if (opt.getPrintForm() == PRINTFORM.WORKSHEET) {
                        // 이 값을 조절하여 빈칸의 높이를 변경할 수 있습니다.
                        meaningCell.setMinHeight(30f);
                    }
                    table.addCell(meaningCell);

                } else {
                    // 일반 단어일 경우
                    table.addCell(new Cell().add(new Paragraph(dto.getWordText())).setPadding(5));
                    table.addCell(new Cell().add(new Paragraph(dto.getMeaning())).setPadding(5));
                }
            }
            document.add(table);
        }
        // 6) 문서 종료
        document.close();
        return baos.toByteArray();
    }
}
