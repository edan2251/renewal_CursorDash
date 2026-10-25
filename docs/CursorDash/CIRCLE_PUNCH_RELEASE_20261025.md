# 원형 통과 폭발·육각 기준 화면 반응·최종 방향 반전 — D-026

2026-10-25. **DEPLOYED_PUBLIC_LOADING_VERIFIED / 전투·시각 사용자 검수 대기**. 사용자 요청: 세 항목만 수정하고 검사/WebGL 빌드 성공 후 기존 Pages로 공개한다. **Unity 자동 Play 금지**. 기존 main 사용자 변경/미추적은 보존하고 기존 별도 gh-pages checkout의 실행 산출물만 커밋/정상 push한다. 소스 전체를 커밋하거나 다른 작업을 main에 포함하지 않는다. buildId `circlefx-20261025-60e4c4e52c0d`.

## 변경/보존

- **A 원형 저지**: 옛 시트/Animator의 런타임 재사용을 제거했다. 원본 에셋/클립은 삭제하지 않고 참고 이력으로 보존한다. 같은 ImpactFlat AA 셰이더에 단색 도형으로 처음부터 크게 보이는 두 폭발 덩어리와 진행/양옆 파편을 만든다. 최대 크기는 첫 박자이며 뒤늦게 길게 자라나는 plume이 없다. 원형 전용으로만 바꾼다.
- **B 육각 반환 파괴**: ImpactFragments/셰이더·파괴 경로·수치·타이밍·후처리 식을 보존한다. 육각의 승인 외형/예비/발사/반사/파괴와 CursorForeground/실제 커서 입력·이동·물리/표시 경로를 바꾸지 않는다.
- **C/D 일반 넥서스 접촉**: D-025 프로필과 방향 유지. 이번 최종 방향 변경을 일반 접촉에 적용하지 않는다.
- **최종 넥서스 파괴**: 사용자 후속 답변이 최초 ‘진입 반대’ 설명을 대체했다. **“지그재그 쪽으로 파편 분출을 원함 — 글의 방향 지시 변경”**, 이어 **“지금 넥서스 파편이 터지는것과 반대방향”**으로 확정했다. 현재 코드의 최종 기준축 -incoming을 반전해 +incoming으로 바꾼다. 첨부 아래왼쪽→위오른쪽 공격 예시에서는 **위오른쪽**이 주 분출이다. 이를 ‘진입 반대’라고 잘못 보고하지 않는다. 본체셀/접촉칩/접촉 섬광의 방향만 변경, 보조 불규칙 분산과12셀+24칩·.05초 압축·.20초/.25배 slow·.64초 결과/Retry/Home 정리는 그대로다.

## A 폭발과 커서 가독성

노드 반경 R을 실제 접촉 실루엣으로 구한다. 첫 두 폭발 덩어리는 대략 ±1.27R 옆으로 벌어진 영역이며, 커서 진행 축에는 최소 .15R의 반폭 통로를 둔다. 이 덩어리는 시작부터 충분히 넓고 **.065초 안에 소멸**, 접촉점의 작은 섬광은 **.035초**다. 파편은 기본8개/최대16개, 주2개는 진행축 ±38도, 나머지는 ±44~82도로 양옆을 연다. 주 파편 속도계수2.6~3.1·보조.9~1.7, 감속10~17/s·빠른 축소, 기본 총.28초(옵션 수명은.12~.4초 제한)다. 크기 상한은 몸체 반경22%로 유지한다. 캐주얼 단색/불규칙 또렷한 도형이며 화염/연기/텍스처/넓은 블러를 추가하지 않는다.

실제 CursorVisual은 D-025의 별도 전경 패스를 그대로 사용해 폭발보다 나중에 표시한다. 커서 오브젝트를 앞으로 워프하거나 가짜 커서를 새로 그리지 않으며 입력/이동/충돌을 바꾸지 않는다. 커서가 지나는 통로를 빈 공간으로 남기고 파편을 양옆으로 분리해 통과감을 만든다. 육각과 같은 .075초 히트스톱 자체는 실제 게임 시간 정지이므로, 정지 전후 궤적이 읽히는지는 수동 확인이 필요하다.

## 화면 반응 경로 비교

공통 실행 순서는 접촉/반환 도착에서 사건1회 → 이벤트 기록 → 히트스톱 요청 → 방향 셰이크 → 왜곡/게임판 빛 → 선택적 화면 Image 플래시 → 효과음 → 사건별 파편이다. A는 실제 커서 접촉 순간, B는 반환 레이저가 육각에 도착한 순간이다. 접촉점 작은 Mesh 섬광과 전체 화면 플래시는 다른 효과다.

| 항목 | 이전 A | 승인 B | 이번 A |
|---|---|---|---|
| 히트스톱 | .05초 | .075초 | **.075초** |
| 방향 흔들림 | .20월드/.35초/20Hz | .18월드/.25초/24Hz | **B와 동일** |
| 흔들림 감쇠/되튕김 | 1.8/.55 | 1.8/.55 | 동일 |
| 왜곡 강도/반경 | .045 / 2.2 | .05 / 2.2 | **.05 / 2.2** |
| 복귀 | 원형 lens2초 / recovery2 | 성공 wave.5초 / recovery1.4 | **성공 wave.5초 /1.4** |
| 파동 | 원형 lens 별도 경로, response0 | .8 / 속도6 / 폭.35 | **B와 동일** |
| 게임판 밝기 | 원래 Background_Flash 별도.25초 | CDWorldLight 노출.65, 차가운 빛(.55,.8,1) | **B와 동일 경로/값** |
| 전체 흰 Image 플래시 | fx.circleRemove.flash 기본0 | fx.triangleBreak.flash 기본0 | 기본0 유지. 별도 흰 덮개를 중복 추가하지 않음 |
| 접촉 작은 섬광 | 원형 .055초 + 옛 시트 | 파괴점 Mesh .035초 | 새 단색 Mesh .035초 |

B에서 ‘화면이 번쩍임’으로 보이는 기본 효과는 **게임판 후처리의 노출/파동**이다. 기본 CameraFlash.Image 알파가0이므로 이를 실제 흰색 전체화면 Image 플래시로 오인하지 않는다. UI FlashImage는 씬의 CameraFlash.flashImage(fileID965263941)에 연결되어 있으며, 사용자가 해당 사건 flash를0보다 높이면 기존 흰색/OutQuad/SetUpdate(true)로 사건 duration 동안 동작한다. 이번에도 동일하며 B 코드/설정은 바꾸지 않는다.

CDWorldLight는 최대빛.8 제한, 밝은 선 반응 가중치와 청색 계열 계수를 사용한다. 시간 감쇠는 .5*(1-age)^1.3 + 전파 ring*.7*(1-age)^.7, 기존 성공 파동·recovery 식 그대로다. 숫자/버튼 등 고정 HUD는 해당 게임판 패스 밖이다. User shake/distortion/flash 배율 및 OnVibe/OnFlash 설정, stop 누적상한.1·shake 상한.2/지속상한.35는 그대로 적용한다.

Tuning은 **원형 화면 기본값11개만 B 기본값을 복사**한다. 키/스키마161개는 유지하고 개인 저장 프리셋을 자동 초기화하거나 덮어쓰지 않는다. 과거 프리셋이 A의2초 등 값을 명시했다면 그것은 여전히 명시적 재정의이며, 기본 비교에는 프리셋을 별도 확인해야 한다. A 파편 색/형태·속도/수명과 B 파편은 각각 분리한다.

## 검사와 빌드/배포 기록

- 사전 보호1731파일/main HEAD8ceedbb, 백업 `Cursor-Dash-master/Logs/circle-punch-release-20261025/Before`, `before.json`.
- 참조 C# 컴파일: 런타임90/테스트9/수동 Editor창1, 오류0. Unity Play/공식 실행 PASS로 표시하지 않는다.
- 기존 열려 있던 대상 Unity6000.3.23f1/Web/Cyberpunk를 확인했다. 제한된 프로세스 조회에는 안 보였으나 정상 권한 조회에서 발견해 중복 실행을 중단했다. 배치 실행 산출물은 생성되지 않았고 기존 Editor의 명시적 빌드 메뉴를 사용했다. 사용자 씬 clean/Play false/timeScale1을 읽기 전용 기록했다.
- 원형 신규 시안·통과 가독성/최종 여러 방향/승인B 실제 화면은 사용자 검수 대기다. 자동 Play/공식48개 실행은 NOT_RUN. 이전 공식 PASS를 새 실행으로 집계하지 않는다.
- 실제 Editor 읽기: 양쪽 화면 반응11필드가 모두 일치하며 최종 방향 함수12방향 검사를 Play 없이 실행했다. 최초 진단 helper의 fallback0 기록은 잘못된 측정으로 별도 보존하고, 실제 HitFeedback과 같은 metadata 기본값 fallback을 사용해 다시 읽었다. 게임 런타임 코드는 이 진단 수정으로 바뀌지 않아 재빌드하지 않았다.
- **빌드 Succeeded / errors0 / warnings2 / 79,850,779bytes / 305.61초**. 경고2개는 기존 NexusGameboard의 varying loop gradient 관련 d3d11/gles3 경고이며 이번 셰이더 소스는 보존했다. `build-result.json`과 실제 완료 로그/Editor 상태 교차 확인. 현재 시안 VFX의 실제 장면을 실행한 것은 아니다.
- 런타임 입력1284파일 해시 일치. 로컬19실행파일 HTTP200+전체 바이트 해시 일치/WASM MIME 정상. 로컬 실제 메인 화면/Console warn-error0. 게임 시작/훈련 진입은 하지 않았다.
- 기존 배포 checkout(원래 clean)을 재사용했다. 웹19실행파일만 복사하고 이전 Build의 .data/.framework.js/.loader.js/.wasm **4개만 명시 교체**, .nojekyll 보존. version.json과 최신 index/기존 도구JS만 반영했다. 원래 Pages CP14 이후의 승인된 현재 게임 기준선도 이 빌드의 일부이며, 이번에 별도 새 기능을 추가한 것이 아니다. 소스·로그·백업·개인설정·진단·인증정보는 게시하지 않았다.
- gh-pages **c42faf5cbcb835c06c6392a4560251cffbe361e0 → 7ead4b3fd6fccd9e44b7a796eda121ea54ec0db3**, 정상 fast-forward push. main 원본 HEAD/브랜치 그대로, 소스 전체 커밋/push/merge/rebase 없음. 7개 변경 파일(4개 빌드 파일 rename 표시+index+도구JS+version). 체크아웃 최종 clean.
- [Pages 실행40134356035](https://github.com/edan2251/renewal_CursorDash/actions/runs/40134356035) completed/success, Pages built/해당 commit 일치. [공개 URL](https://edan2251.github.io/renewal_CursorDash/).
- 공개21파일 HTTP200, wasm application/wasm, 공개 텍스트6개 커밋 바이트 일치. canonical 루트 HTML·version.json·**실제 공개 게임의 전체 JSON**이 동일한 새 buildId를 사용한다. 전체 공개 binary를 재다운로드한 해시 검사로 과장하지 않는다.
- 실제 공개 Main/설정 열고 닫기/전체JSON 읽기 PASS. JSON161키·.15/.20/.60 및 A/B 화면 반응11필드 일치. Console warn/error0. 게임 시작·전투·개발자 값 변경/저장·클립보드·휴대폰 검수는 하지 않았다.
- 최종 Native Editor Cyberpunk clean, Play/compile/pause off/timeScale1. 기존 저장7키 동일. TMP imported4개 dirty는 전후 동일하며 Save/Discard/초기화를 하지 않았다. BuildIdentity.txt는 이번 릴리스 식별자로 갱신했고 이전 바이트는 Before 백업과 BuildIdentity-before.bin에 보존했다. 기존 씬/프리팹/원화/기존.meta/설정/패키지는 새 차이0이다.

### release-record 고정 문맥

작업 ID: circle-punch-release-20261025. 프로젝트: CursorDash, 공통 등록ID/프로필 JSON 미제공. PC: 현재 Windows 로컬. 원본 `D:/renewal_CursorDash`, Unity `D:/renewal_CursorDash/Cursor-Dash-master`, branch main/기준8ceedbb+기존 승인 working tree+D026 변경. 대상 릴리스 commit7ead4b3/gh-pages, Unity6000.3.23f1/Built-in/WebGL. 승인자: 사용자 이번 직접 빌드·push·Pages 승인과 최종 방향 변경 답변. Slack/Notion 전달: 미설정/미실행. 독립 Claude QA: NOT_RUN. 사용자 디렉터: 기존 향상/B승인 유지, 이번 A·방향·화면리듬은 시각 검수 대기.
산출물 `D:/renewal_CursorDash/Builds/LocalDevPreview/circlefx-20261025-60e4c4e52c0d`, 파일별 SHA256은 candidate-manifest.json. 복구 기준은 직전 정상 Pages commit c42faf5이며 필요 시 별도 새 복구 커밋 방식으로 진행하고 force push하지 않는다.

[실제 공개 버전 화면](../../Cursor-Dash-master/Logs/circle-punch-release-20261025/public-version.png), [실제 공개 메인](../../Cursor-Dash-master/Logs/circle-punch-release-20261025/public-main.png), public-runtime-export.json / public-http-check.json / public-console.json / release-verification.json이 근거다. 이는 전투 VFX 화면 검수가 아니다.

## 사용자 검수

Unity `Assets/Scenes/Cyberpunk.unity`에서 사용자가 Play→훈련으로 진입한다. 기존 개인 프리셋이 있다면 적용 이름/수치를 먼저 확인한다. 기본 비교는 새 기본 수치로 진행한다.

1. A: 원형 생성 후 좌/우/위/아래에서 충전 중 들이받는다. 첫 순간이 노드보다 넓게 터지고, 커서가 사라지지 않으며 양옆으로 열린 통로로 지나가는지 확인한다. .28초 정도 후 빠르게 정리되고 긴 옛 plume이 남지 않아야 한다.
2. B: 육각 생성→잔상으로 레이저 반환→육각 파괴. 승인한 파편/화면 반응이 그대로인지 확인한다. A/B를 번갈아 실행해 밝기/정지·흔들림의 리듬을 비교한다. 색/파편 모양은 사건마다 다르다.
3. 최종: `Tools→CursorDash→넥서스 수동 검수`에서 훈련 무적 해제·HP15%. 실제 원형 또는 육각 공격을 방치한다. 여러 위치에서 공격시켜 **이전 버전 최종 분출의 반대쪽**으로 나가는지 확인한다. 첨부 아래왼쪽 진입 예시의 새 주 방향은 위오른쪽이다.
4. 일반 C/D(HP최대)의 방향/강도는 그대로여야 한다. 최종만 바뀌며 slow/결과 .64초 리듬 유지. Retry/Home 뒤 시간·입력·새 노드가 정상인지 확인한다.

이 문서는 source/수치 분석과 수동 절차다. 실제 이번 전투 화면을 촬영했다고 주장하지 않는다. 수정한 게임 소스4개는 CircularBreakVfx/HitFeedback/CursorDashTuning/NexusImpactVfx이며, 부수 변경은 관련 테스트2개·빌드 진입 도구·기록 문서다. 다른 승인 게임 규칙/노드 외형/발사/점수·피해 코드는 수정하지 않는다.
