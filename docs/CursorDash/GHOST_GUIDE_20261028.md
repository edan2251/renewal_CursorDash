# D-061 고스트 실습과 최초 피버 안내 인계

> **과거 구현 이력:** 아래 순백색 시각 모사·원형 다음 육각·반대편 동시 생성은 D-062로 대체됐다. 현재 규칙·조절값·실제 Unity 검수는 [AUTONOMOUS_GUIDE_20261028.md](AUTONOMOUS_GUIDE_20261028.md)를 먼저 읽는다. 이번 명시 승인으로 집중 Play 검수를 수행했으므로 아래 당시 NOT_RUN/컴퓨터 조작 미승인을 현재 차단으로 재적용하지 않는다. 공개 도구 숨김·일반처치회로완화·초기화/첫피버 안내는 보존한다.

2026-10-28 / **로컬 구현 완료, 사용자 Play 검수 대기.**

## 범위·기준·보존

현재 요청은 사용자 첨부 `C:/Users/molph/.codex/attachments/b5257b27-0dbf-46a0-ae94-8b86fb761982/붙여넣은 텍스트.txt`의 CP0~CP7이다. 최신 회신으로 실제 Play는 사용자가 직접 담당한다. 지금은 컴퓨터 직접 조작이 필요하지 않다. 추가 화면 조작이 꼭 필요해지면 앱/목적을 먼저 설명하고 질문한다.

이전 D-060 빌드·게시 완료와 이번 작업은 분리한다. 착수 Git은 main `8d26194dc2c7940145c4c394a5d3ad56eb15c9bc`, index는 비어 있었다. 이전 실제 빌드 소스 `2c8ad699fc0138513f8112f2ec0703194438c575`, gh-pages `de9ec5fae6394f2dc64c36166206667b69f4ec53`, buildId `cursordash-20261027-feedback-4694b4ff0c22`는 공개본이다. 이번 변경은 공개본에 포함되지 않는다. 빌드/커밋/수정 커밋/push/배포를 하지 않는다.

작업 전 1808파일의 해시와 Git 상태는 `Cursor-Dash-master/Logs/ghost-tutorial-20261028/before.json`, `git-status-before.txt`에 보관했다. 원본 코드·씬·설정·문서·meta 등 비교/복구 사본은 **`D:/renewal_CursorDash/Backups/ghost-tutorial-20261028`** 이다. 사용자 씬·프리팹·Packages/ProjectSettings, 기존 미추적 Recovery/캡처/Archive/영상/도구·배포 폴더를 정리하거나 되돌리지 않았다.

요청 영상 `C:/Users/molph/Downloads/녹음 2026-10-27 223203.mp4`는 파일 존재(22,441,332바이트)만 확인했다. 디코딩/관람하지 못했다. 33.33~34.13초의 최종 화면을 보았다고 주장하지 않는다. 영상용 루프백 서버는 브라우저에 열지 않고 자체 종료했다. 새 게임 화면/연속 녹화는 없다.

## CP0 — 기존 소유권과 연결 위치

| 역할 | 기존 소유자와 이번 연결 |
|---|---|
| Main/Starting/Playing/사망 뒤 점수 클릭대기/Home | `CursorDashSpaceFlow` 유지. 기존 시작이 끝나 실제 Playing이 될 때만 Practice를 초기화한다. 로고·점수 전환/사망 결과 대기를 재작성하지 않는다. |
| 실습/연습용 공급 | 기존 `CursorDashPractice`를 partial로 확장. 기존 자유 훈련은 남기고 Guided 분기만 추가한다. |
| 실제 생성·회수/공격 취소 | `NodeCreator.SpawnPreviewAt`, `StopNodes`, 기존 풀·Node 실행 토큰 사용. 시범용 템플릿 읽기 함수만 추가한다. |
| 점수·배율·게이지·피버 시계 | 기존 `FeverState`. `PracticeOnly`에서 실제 실습 처치 점수는 표시하되 실제 피버/정식 난이도 시계를 억제한다. |
| HP·기록·시간배율·입력 잠금 | 기존 `StageManager`. 실제 피해의 연습 무적, 완료 전 기록 제외, 연습/안내 시간 제외, 기존 Pause/HitStop의 시간배율 소유권 유지. |
| 링/게이지·회로 | 기존 `FeverNexusRig`/`FeverGaugeView`/`CursorDashCircuits`의 표시 진행만 실습 초기화에 연결한다. HP/충돌 반경은 변경하지 않는다. |
| 완료 저장 | 기존 `CursorDashStorage`/브라우저 bridge. tutorial와 최초 fever 안내를 독립 키로 저장하고 BEST 키는 건드리지 않는다. |

## CP1 — 공개 도구와 설정 숨김

일반 모드에서는 Native OnGUI의 상단 도구바와 WebGL HTML 도구 root 전체를 숨긴다. 초기 snapshot 이전에도 HTML root가 hidden이므로 로딩 중 먼저 보이지 않는다. 예전 생성/확인 버튼 튜토리얼은 다시 표시하지 않는다. 숨긴 상단 도구 영역을 입력 차단 영역으로 사용하지 않는다. `Open(false)`는 설정을 열거나 Pause/입력 잠금을 걸지 않는다.

Pause/Resume/Retry/Home과 데이터·프리셋/JSON/보고서 기능은 보존했다. Pause 안의 BGM/SFX/Vibration 설정 진입만 숨겼다. 실제 투명 클릭 영역과 Canvas 순서는 사용자 Play로 확인해야 한다.

기존 개발 도구 진입은 Editor의 **Tools → CursorDash → 개발자 도구 (Play 중)**, 또는 Editor/Development Build에서 **Ctrl+Shift+D**다. 새 공개 개발 버튼이나 설정 손잡이 패널을 만들지 않았다. Dev 해제 시 기존 패널도 닫아 보이지 않는 입력 잠금이 남지 않게 한다.

## CP2~CP3 — 순백 시범과 실제 2+2 실습

- 기존 시작 연출/입력 인계가 끝난 뒤 원형 실습이 자동 시작된다. 간단한 행동 안내만 표시하며 확인 버튼이 없다.
- 순백 시범은 왼쪽, 실제 기존 색 실습 노드는 오른쪽의 제한된 후보 중 선택한다. 실제 몸체 반경·화면 여백/넥서스/HUD·현재 플레이어와의 겹침을 검사한다. 안전한 위치가 없으면 무리하게 생성하지 않고 재시도한다.
- 시범은 기존 원형/육각 artwork와 cursor sprite·회전 설정/타임라인·생산 선분 접촉/반사 기하를 읽어 그리는 **시각 모사**다. Collider/Node나 실제 잔상을 만들지 않으며 Stage 점수/HP/Feedback/회로를 호출하지 않는다. 로컬 흰 파편만 사용한다. 실제 Physics와 같다는 실행 증거는 아직 없다.
- 실제 원형 접촉 2회만 원형 성공으로 집계한다. 이후 원형 시범·남은 실습·공격·실제 잔상을 정리하고 육각으로 전환한다. 실제 육각은 기존 레이저를 잔상으로 반사해 파괴한 콜백 2회만 성공이다. 직접 접촉을 성공으로 세지 않는다.
- 실제 노드 reference+현재 `ExecutionId`+현재 단계가 모두 맞아야 점수/진행을 수락한다. 이미 수락한 실행·중복 콜백·이전 단계 콜백은 다음 단계를 건너뛰지 못한다.
- 실패한 노드는 기존 풀로 돌리고 .65초 뒤 재공급한다. 처음 단계로 되돌리거나 사망시키지 않는다. 실습 성공의 기존 파편/피드백은 유지한다. 연습 시간/점수는 공식 기록으로 저장하지 않으며 실제 피버도 시작하지 않는다.

**육각 실제 공격·반사·회전 타임라인은 수정하지 않았다.** 시범은 생성 시 현재 설정을 읽는다. 타임라인이 충돌한 경우 실제 생성과 마찬가지로 잘못된 시범을 억지로 발사하지 않는다. 실제 화면에서 시범의 타이밍·가독성/시범과 실습의 구별은 미검증이다.

## CP4 — 실습 종료 초기화

기본 **1.1초**, 진행은 기존 Phase와 연결한 단일 표시 시계다. Pause는 진행을 정지하며 Retry/Home은 이전 Practice를 취소·정리한다.

| 진행 | 기본 환산 시각 | 표시·데이터 |
|---|---:|---|
| 시작 | 0 | 시범·실습·공격·실제 잔상·피드백 정리, 공급/조작 잠시 중지. 실제 커서 위치는 보존. |
| 0~40% | 0~.44초 | 충전 원호/두 번째 점선 링이 안쪽으로 회수. 본체/HP는 유지. |
| 0~43% | 0~.473초 | 회로가 바깥으로 걷힘. |
| 45% | .495초 | 점수0/배율1/정상HP/게이지0/처치·실습 타이머/난수 라운드·연습 효과 초기화. |
| 52~100% | .572~1.1초 | 기존 회로 재구축 표시. |
| 57~100% | .627~1.1초 | 두 번째 링 복원, 기존 회전 impulse800/감쇠.30 연결. |
| 끝 | 1.1초 | 정상 완료만 저장, 연습 모드 해제, 실제 본게임 공급·생존/난이도 시계 시작. |

위 시각은 설정값 환산이며 실측이 아니다. 실습 중 과도한 득점으로도 실제 피버가 발동하지 않게 practice 게이지를 .875 이하로 제한한다. 이는 **연습 전용**이며 정상 게이지·처치량·피버8초 규칙은 유지한다.

## CP5 — 최초 피버 안내

기존 진입 연출 완료 → 첫 Fever 시점에서 공급을 시작하기 전에 안내를 붙였다. 기존 HUD 안에 상호작용 없는 흰 Image와 Medium TMP를 사용한다. 새 Canvas/카메라/Raycaster는 만들지 않는다. 흰 화면은 읽기 구간에 alpha1, 글자는 검정이며 정확히 아래 두 줄이다.

```text
- FEVER TIME -
검은 노드를 마구 파괴하세요!
```

등장 .10 + 유지 .80 + 페이드 .32 + 속도 회복 .24 = **1.46초(비배율 표시 시간)**, 최소 시간배율 .06. 이 시간 동안 `PresentationHold`로 실제 Fever 시계/게이지/공급을 멈춘다. 같은 프레임에 진입 전 남은 delta가 8초를 미리 소비하지 않도록 모델 반복도 중지한다. 종료 후 공급 시작·입력 재설정하고 원래 Fever8초를 소비한다.

Pause/패널/Dev 정지는 기존 Stage 시간 소유자가 우선한다. 안내 완료가 무조건 timeScale1을 대입하지 않는다. 안내 도중 Pause는 안내 진행도 함께 멈추며, Resume/중단 경로는 기존 잠금을 존중한다. Abort/Home/Retry/사망은 notice/hold/view를 정리하고 미완료를 저장하지 않는다. 기존 HUD가 없으면 안내 완료로 저장하지 않고 경고/기존 피버로 진행한다.

### 저장 키와 다시 보기

| 키 | 의미·기존 데이터 보존 |
|---|---|
| `CursorDash.v2.tutorial` | 기존 키 재사용. `completed`만 완료. 정상 2+2 및 초기화 종료 뒤 저장. `skipped`/미완료는 완료가 아니다. |
| `CursorDash.v3.feverGuide` | 신규, 최초 피버 안내의 정상 완료만 `completed`. tutorial/BEST와 독립. C#/WebGL JS 저장 허용 목록을 모두 추가. |
| 기존 `CursorDash_BestPoints_v1_*`, user/presets | 키·값/정산 방식 보존. 실제 저장값을 검사 목적으로 지우거나 덮어쓰지 않았다. |

사용자가 Play 중 **Tools → CursorDash → 고스트 튜토리얼 다시 보기 (기록 보존)** 또는 **최초 피버 안내 다시 보기 (기록 보존)**를 선택하면 기존 완료 키를 지우지 않고 재생한다. 피버 재생은 기존 Dev 기록 제외 경로를 쓴다. 다시보기 자체를 첫 정상 완료 저장 시험으로 간주하지 않는다. 저장 오류는 기존 오류 문자열로 남으며 성공했다고 가장하지 않는다. 실제 실행 종료/재실행 지속성은 아직 NOT_RUN이다.

## CP6 — 일반 처치 회로만 완화

기본 디자인·두께·평상시 flow/피격 우선·피버 처치를 줄이지 않는다. 기존 `CircuitPulseField`를 일반 처치용으로 한 벌 더 준비하고 재사용하며, 매 타격/프레임마다 재질·그래프를 만들지 않는다. 두 풀 합산은 기존 동시4를 넘지 않는다. 중요한 반응은 기존 풀을 쓰며 일반 처치 잔향을 우선 정리한다. 피격의 붉은 wave/Tint 우선 처리는 그대로다.

| 항목 | 시작 값/방식 | 이번 일반 처치 값 |
|---|---|---|
| 전달 범위 | 타격점에서 공간 전면이 전체 독립82경로로 확산 | 최근접 탐색 .11(짧은 화면변 기준), 연결 경로만 |
| 전파 | 공간 속도2.6/폭.32 | 기존 그래프 전달속도1.65 재사용 |
| 국소 잔향/거리 감쇠 | 기존 국소 방어 .34/.40, 처치 공간파동은 거리 기반 수명 | 일반 처치 .19초/.18 |
| 강조 | 기존 최대강조 .44 | 기존강조 × .58, 수학상 상한 .2552 |
| 동시 반응 | 4 | 합산4 유지, Max 합성/피격 우선 |
| 피격/피버/quiet | 기존 값 | 그대로 |

전후 수학 비교: 실제 Graph/Pulse의 세 경로 중점에서 120Hz/3초 표본, 임계 .001. 전체82경로가 강조되던 이전 모델에서 이번 모델의 강조 경로는 6/8/5개였다. 마지막 유의 강조는 각각 .842/.800/.733초 → .467/.467/.450초, 표본 최대 .417/.420/.420 → .204/.204/.204였다. `Logs/ghost-tutorial-20261028/circuit-comparison.json`에 전체 표본을 남겼다. **게임/GPU 화면 비교 자료가 아닌 로직 자료**다. 영상 33.33~34.13초를 재현·관람한 결과가 아니다.

## 조절 위치

**Assets → Resources → CursorDash → VisualPrototype** (`CursorDashVisualProfile`)의 다음 Inspector 항목을 사용한다. 실제 에셋에 저장된 기본값이며 기존 Dev 프리셋 JSON을 강제로 변경하거나 새 개발 도구를 만들지 않는다.

| 그룹 | 필드와 기본값 | 의미 |
|---|---|---|
| Ghost tutorial | `tutorialRespawnSeconds=.65` | 실습 실패/성공 뒤 다음 공급 간격 |
| Ghost tutorial | `tutorialDemoGap=.75` | 시범 처치 파편 정리 뒤 다음 시범 여백 |
| Ghost tutorial | `tutorialGhostScale=.88` | 순백 시범 노드/커서의 상대 크기 |
| Ghost tutorial | `tutorialResetSeconds=1.1` | 실습 완료 후 초기화 표시 전체 길이 |
| First fever notice | `feverNoticeFadeIn=.10`, `feverNoticeHold=.80`, `feverNoticeFadeOut=.32`, `feverNoticeRecovery=.24` | 안내 등장/읽기/퇴장/속도 복귀 길이 |
| First fever notice | `feverNoticeTimeScale=.06` | 안내 중 최소 배율, 실제 피버 시계는 별도 hold |
| Normal kill circuit response only | `circuitOrdinaryHitRange=.11`, `circuitOrdinaryHitSeconds=.19`, `circuitOrdinaryHitGain=.58` | 일반 처치의 근처 탐색/잔향/강조 배율 |

기존 기본값 복원 함수에도 새 일반 회로 값을 포함했다. 실제 공격·충전·비행·반사값을 튜토리얼이나 위 시각 설정이 덮어쓰지 않는다.

## 변경 파일

Unity 루트 아래 기존 수정 23개:

- `Assets/Editor/CursorDashNexusReview.cs`
- `Assets/Plugins/WebGL/CursorDashBridge.jslib`
- `Assets/Resources/CursorDash/VisualPrototype.asset`
- `Assets/Scripts/Gameplay/Object/Node/CircularNodeArtwork.cs`, `HexagonalNodeRings.cs` — 선택적 시범 흰색 인자만 추가, 일반 호출 기본값 유지.
- `Assets/Scripts/Gameplay/StageManager/CursorDashPractice.cs`, `StageManager.cs`, `NodeCreator.cs` — 기존 실습/상태/풀 연결.
- 같은 폴더 `CursorDashFever.cs`, `FeverState.cs`, `FeverGaugeView.cs`, `FeverNexusRig.cs` — 연습/안내 표시·시계·초기화.
- 같은 폴더 `CursorDashRuntimeUI.cs`, `CursorDashSession.cs`, `CursorDashSpaceFlow.cs`, `CursorDashStorage.cs`, `PauseHandler.cs`, `CursorDashVisualProfile.cs`, `CursorDashCircuits.cs`.
- `Assets/WebGLTemplates/CursorDash/TemplateData/cursordash-tools.js`.
- `Assets/Tests/Editor/CursorDashSceneRegression.cs`, `CursorDashSpaceFlowTests.cs`, `CursorDashTestPolishTests.cs`.

새 5개 C#와 해당 `.meta`:

- `Assets/Scripts/Gameplay/StageManager/CursorDashGuideFlow.cs` — 순수 단계/안내 시계.
- 같은 폴더 `CursorDashPractice.Guide.cs` — 기존 Practice partial 확장.
- 같은 폴더 `CursorDashGhostDemonstration.cs` — Collider 없는 시범 소유 리소스.
- 같은 폴더 `CursorDashGuideView.cs` — 기존 HUD 아래 비상호작용 안내.
- `Assets/Tests/Editor/CursorDashGuideTests.cs` — 새 순수 공식 테스트8개.

문서는 root `AGENTS.md`, `docs/CursorDash/STATUS.md`, `MASTER_SPEC.md`, `DECISIONS.md`, `ENVIRONMENT.md`, `VALIDATION.md`, `SOURCES.md`와 본 인계다. 로컬 로그/백업은 별도이며 공개/커밋 대상이 아니다. 기존 테스트의 `skipped` fixture를 `completed`로 바꾸고 숨긴 설정 대신 기존 Dev 모달을 검사하도록 수정했다. 새 사양인 실제2+2/초기화 후 본게임의 핵심 단언으로 옛1+1/훈련 잔류 단언을 대체했으며 삭제·무시는 하지 않았다.

## CP7 — 실제 검사와 미검증

| 상태 | 실제 실행한 것 | 한계 |
|---|---|---|
| PASS | 격리 참조 C# runtime116/test15/Editor2 오류0·기존134경고 | Unity Native 컴파일/에셋 import/Shader 컴파일 아님 |
| PASS | 생산 순수 C# 안내/실습1029단언 | Unity Physics/실제 손 조작·화면·저장 아님 |
| PASS | 기존 생산 순수 회로/시작/되감기4672999 + 집중106885 | 기존 피버·피격 로직 보존 회귀, 실제 GPU 아님 |
| PASS | JS 구문, 모의 DOM/메모리 bridge19단언 | 실제 브라우저/WebGL·파일/클립보드·localStorage 지속성 아님 |
| PASS | CP6 세 원점 전후 계산·회로 cap·짧은 잔향 비교 | 실제 밝기/시각 승인 아님 |
| PASS | 시작1808파일 삭제0, 기존 파일 중 이번 범위30개 변경, 핵심 보존 소스19개 해시 동일·신규meta GUID 중복0·diff 공백 검사 | 메모리 미저장 상태 확인은 아님; HEAD/main·빈 index 유지 |
| NOT_RUN | 새 공식8개 및 수정한 기존 Play/GPU 테스트 실행 | 작성·참조 컴파일까지만 |
| NOT_RUN | 최종 Unity 컴파일/Physics/GPU/Play·사용자 입력·시각 QA·실 저장 재실행 | 사용자 Play 담당으로 인계 |
| NOT_RUN | 모바일/터치/WebGL·화면 비율·장시간 성능 | 빌드/배포 승인 없음, 측정 없음 |

참조 검사에서 처음 Editor 메뉴의 비공개 명령 진입 호출이 컴파일되지 않아 기존 공개 `BrowserCommand` 경계로 수정한 뒤 오류0을 확인했다. 자동 승인 검토가 광범위한 파일 교체/시간·입력 묶음 변경을 거부한 이력은 보존하고, 명시된 최신 승인·백업 근거로 기존 Practice 부분 확장/기존 HUD·Stage 소유자 재사용의 좁은 변경을 적용했다. 거부된 같은 실행을 다른 권한 경로로 우회하거나 미해결 거부를 성공으로 쓰지 않는다.

## 사용자가 열 씬과 수동 순서

최종 실행 씬은 **`Assets/Scenes/Cyberpunk.unity`** 그대로다. 다른 씬을 합치거나 삭제하지 않았다. 사용자 작업을 저장한 뒤 직접 Play한다.

1. 메인/플레이에서 공개 상단 도구·설정/이전 버튼 가이드가 없고 빈 영역 터치가 막히지 않는지 본다. Pause/Resume/Retry/Home은 계속 사용할 수 있어야 한다.
2. 처음 이용하는 저장 상태라면 정상 시작 후 자동 실습한다. 이미 `completed`가 있는 사용자는 위 Tools의 **고스트 튜토리얼 다시 보기**로 재생한다. 기존 BEST/완료 키를 삭제하지 않는다.
3. 왼쪽 흰 시범과 오른쪽 실제 원형을 구분한다. 실제 2회만 집계되는지, 일부러 방치하면 사망 없이 재공급되는지, 시범이 실제 점수/전역 플래시/흔들림을 만들지 않는지 본다.
4. 육각은 직접 접촉 대신 실제 잔상으로 레이저를 반사해 2회 파괴한다. 빠른 연속 성공/모서리에서 정상 전환되는지, 실제 타이밍/반사/승인 파괴가 그대로인지 본다.
5. 약1.1초 게이지/점선 회수·회로 걷기/재구축·점선 고속 복원을 본다. 커서 위치가 튀지 않고 본게임에서 점수0/배율1/정상HP/게이지0/새 시계인지 확인한다.
6. 실습 각 단계/초기화 도중 Pause/Resume·Retry/Home을 시험한다. 오래된 시범/노드/공격/힌트가 다음 라운드에 남지 않아야 한다. 중단은 완료 저장이 아니다.
7. 정상 플레이로 첫 피버에 들어간다. 완전 흰 화면과 정확한 검정 두 줄, 자동 퇴장/속도 복귀, 안내 중 게이지8초/보너스 공급 정지, 종료 후 정상 공급을 확인한다. 안내 도중 Pause/Retry/Home도 확인한다. 다시보기는 Dev 기록 제외 경로이므로 BEST 시험과 분리한다.
8. 다음 피버/다음 시작에 정상 완료한 안내가 다시 강제되지 않는지 확인한다. 재실행 저장 지속성은 사용자가 별도 확인한다. 기존 BEST/설정/프리셋은 지우지 않는다.
9. 일반 단발/연속 처치의 회로만 가까운 짧은 보조 반응인지, 피격 붉은 우선·피버 처치·평상시 flow/숫자 획 가림·승인 파편/점수/Nexus가 그대로인지 비교한다.
10. 체력0 → 기존 파괴/슬로모션 → 이번 점수 클릭대기 → BEST/메인 → 새 시작을 확인한다. 사망 대기 길이/클릭 의미를 바꾸지 않았다.

## 남은 문제·자체 QA·다음 한 작업

CP0~6은 코드 구현/가능한 정적·순수 검사까지 완료했다. CP7의 실제 Play·시각 승인/기기 QA는 남았다. 현재 확인된 Native 실행 실패를 숨기고 완료로 처리한 것은 없지만 **이번 소스로 Native 실행 자체를 하지 않았다.**

자체 QA 후속 후보는 최대3개이며 이번에 추가 구현하지 않는다.

1. **우선1 / 미검증 위험:** 고스트 육각은 실제 설정/기하를 읽는 시각 모사이므로 실제 저프레임·좁은 화면에서 시범 타격/꼬리/반사 가독성을 먼저 확인한다. 코드 검사만으로 물리·사용자 이해를 판정할 수 없다.
2. **우선1 / 미검증 위험:** 기존 HUD 안의 흰 안내 범위/Pause 정렬과 숨긴 WebGL 상단 클릭 영역은 실제 화면 비율·손 터치로 확인한다. 모의 DOM 검사는 투명 영역·마스크/GPU 보장이 아니다.
3. **우선2 / 시각 취향:** 순백 시범 크기.88과 첫 안내1.46초는 초기 시안이다. 실제 플레이어보다 시범이 강하게 보이거나 안내가 길게 느껴지면 해당 값만 다음 검수 후 조정한다. 지금은 시각 승인으로 취급하지 않는다.

다음 한 작업은 **사용자 Cyberpunk Play 검수**다. 노드 겹침·새 Flash Background·별도 사이버 재구축·새 설정 패널·타이틀 변경·추가 배포는 시작하지 않는다. 소스/에셋/문서는 미커밋으로 남긴다.
