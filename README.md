# 🎮 Engjoy – 게임 기반 영어 학습 플랫폼

> **"플레이하며 영어를 배우는 즐거움"**  
> Unity WebGL과 Spring Boot, node.js를 활용한 실시간 멀티플레이어 영어 학습 게임

![main-demo](docs/images/demo.gif)

---

## 📌 프로젝트 개요
**Engjoy**는 게임을 통해 영어 단어와 문장을 자연스럽게 익히도록 설계된 학습 플랫폼입니다.  
반복 학습 이론(Spaced Repetition)과 자기결정 이론(Self-Determination Theory)을 기반으로, 사용자가 재미있게 학습에 몰입할 수 있도록 제작했습니다.

---

## 🚀 주요 기능
- **회원가입 / 로그인** – Spring Security + CSRF 기반 인증
- **실시간 게임 매칭 & 플레이** – WebSocket 기반 실시간 통신
- **단어장 & 복습 시스템** – 학습 이력 기반 자동 복습
- **랭킹 시스템** – MySQL 윈도우 함수로 효율적 순위 계산

---

## 🛠 기술 스택
| 구분 | 기술 |
|------|------|
| **Frontend** | Unity WebGL, Tailwind CSS |
| **Backend** | Spring Boot, JPA, WebSocket |
| **Database** | MySQL |
| **Infra** | AWS EC2, S3, Nginx |
| **Tools** | Git, GitHub, Figma |

---

## 📂 프로젝트 구조
- Server : node.js 기반 게임 서버 구현 코드
  - npm run [game1/game2/match/lobby]로 실행
- Unity : WebGL 기반 게임 구현 코드(Unity project 파일)
- Web : Spring boot 기반 웹 서버 구현 코드 (Spring boot project 파일)

## 문서 링크
- 요구사항
  - https://docs.google.com/spreadsheets/d/1rnTs1tCQGGGVShEohkv03YSzJllSPNkK/edit?usp=sharing&ouid=115869002334699013787&rtpof=true&sd=true
- 데이터베이스 ERD
  - https://dbdiagram.io/d/project2-6850bb8f3cc77757c81e63f5
