# CursorDash CP1~CP4 GitHub Pages 테스트 배포 기록

상태: **DEPLOYED_PUBLIC_RUNTIME_VERIFIED** (2026-10-24 KST).
최종 주소: **https://edan2251.github.io/renewal_CursorDash/**

## 고정 문맥과 승인

공통 `release-record.ko.md` 양식의 문맥을 프로젝트 배포 지시에 맞춰 기록한다.

| 항목 | 값 |
|---|---|
| 작업 ID | deploy-cp14-20261024-131718 |
| 프로젝트 ID / 프로필 | 사용자 지정 CursorDash / 공통 프로필 JSON·등록 ID는 이번 요청에서 미제공 |
| PC | 현재 로컬 Windows PC |
| 원본 Git / Unity 경로 | D:/renewal_CursorDash / D:/renewal_CursorDash/Cursor-Dash-master |
| 원본 branch / HEAD | main / 8ceedbb7ead4c2b97417739f62fd7904e1a51497 (유지) |
| 원격 main | 413f8427293c66e275963172ecfdada8a3c0a8e2 (전후 동일) |
| 배포 저장소 / branch | edan2251/renewal_CursorDash / gh-pages |
| 배포 전 SHA | 296a394bc48ee230f824df547390e26a8d83a8b6 |
| 배포 후 SHA | **c42faf5cbcb835c06c6392a4560251cffbe361e0** |
| Unity / 타깃 | 기존6000.3.23f1 / WebGL, Built-in |
| Slack / 별도 외부 전달 | 미설정 / 수행하지 않음 |
| 독립 Claude QA | 이번 배포에서는 NOT_RUN. 이전 사용자 CP1~CP4 결과 확인 및 이번 직접 배포 승인으로 진행 |
| 승인자 / 근거 | 사용자 직접 요청 + _incoming/CursorDash_CP14_GitHubPages_Deploy.md, D-017 |

공유 lastactiveproject/다른 프로젝트 연결을 사용하지 않았다. 이번 승인은 이 버전의 실행 산출물 gh-pages 정상 push·공개 검증에 한정한다. main 소스 커밋/push/merge/rebase와 새 기능·UI 리디자인은 수행하지 않았다.

## 빌드 재사용과 패키지

- buildId: **cp14-20261024-031826-fe9e3b0bcb8d**
- defaultsVersion: **cp14-v2.1-20261024**
- 검증된 빌드: `D:/renewal_CursorDash/Builds/LocalDevPreview/cp14-20261024-031826-fe9e3b0bcb8d`
- 원래 빌드 결과: Succeeded / errors0 / warnings0, 79,591,684bytes.
- **재빌드하지 않음**: 기존 기록의 소스/셰이더/템플릿/설정85개 SHA256이 현재와 모두 일치했다. 후보19개 파일의 목록/크기/SHA256을 확보했다. 무압축 .data/.wasm과 buildId별 파일명, 상대 경로로 Pages 호환성이 있었다.
- 배포 체크아웃: `D:/renewal_CursorDash/Deployments/cp14-pages-20261024-131718/checkout`. 최신 원격 gh-pages의 별도 shallow checkout이며 원본 폴더의 브랜치를 바꾸거나 원본 .git에서 worktree/브랜치를 생성하지 않았다.
- 후보19개 실행 파일을 명시적으로 복사했다. `.nojekyll`을 보존하고 buildId/defaultsVersion 두 필드만 담은 `version.json`을 추가해 최종21개 파일이다. 기존 사이트에는 CNAME/기타 사용자 사이트 파일이 없었다.
- 불필요해진 **Build/play.data / play.framework.js / play.loader.js / play.wasm 4개만** 배포 체크아웃에서 명시적으로 교체했다. .git·루트 전체 삭제/clean/reset/stash/force push/전체 add는 하지 않았다.
- 새 빌드 파일명이 달라 이전 play.* 캐시와 혼합되지 않는다. 새 수치 도구 JS/CSS를 포함했고 해당 경로는 이전 배포에 없었다. 압축/프레임워크 최적화로 범위를 확대하지 않았다.
- 공개 파일에는 Logs/백업/원본 C#/개인 프리셋/진단 덤프/인증정보/로컬 경로 보고서를 넣지 않았다. 공개 텍스트6개에서 로컬 workspace·localhost 의존 및 토큰 표식을 검사했으며 발견하지 않았다. 인증파일/토큰 값은 읽거나 복사하지 않았다.
- Unity 생성 framework.js의 공백 한 줄 경고는 생성 원본 그대로 보존했다. 앱 HTML/도구 JS/CSS/버전 JSON의 diff check는 통과했다.

실행 산출물 SHA256 목록은 [배포 manifest](D:/renewal_CursorDash/Cursor-Dash-master/Logs/deploy-cp14-20261024-131718/deployment-runtime-manifest.json), 원래 후보는 [candidate manifest](D:/renewal_CursorDash/Cursor-Dash-master/Logs/deploy-cp14-20261024-131718/candidate-build-manifest.json)에 있다. 로컬 진단은 공개 checkout 밖에만 보관했다.

## 커밋과 실제 Pages 결과

`Deploy CursorDash CP1-CP4 WebGL test build` 커밋은 배포 전 SHA의 직접 후속 커밋이다.
`git push origin HEAD:refs/heads/gh-pages` 정상 fast-forward push 성공. force/이력 재작성 없음.

- [배포 커밋](https://github.com/edan2251/renewal_CursorDash/commit/c42faf5cbcb835c06c6392a4560251cffbe361e0)
- [Pages 실행 40061981463](https://github.com/edan2251/renewal_CursorDash/actions/runs/40061981463): **completed / success**, 대상 headSha 일치.
- Pages builds/latest: **built**, commit 일치. 생성 UTC2026-10-24T04:20:54Z, 완료04:21:17Z (KST13:21:17).
- 기존 legacy Pages source **gh-pages / 루트**와 공개 URL/권한을 유지했다. 설정 변경 없음.
- [공개 버전 정보](https://edan2251.github.io/renewal_CursorDash/version.json)와 **실제 게임 런타임에서 내보낸 JSON**의 buildId/defaultsVersion이 모두 일치했다.
- 공개 파일21개 HEAD는 전부 **HTTP200**, WASM Content-Type은 **application/wasm**. 현재 로더/도구 파일 경로를 확인했고 브라우저에서 로딩·셰이더 표시가 성공했다. 전체 공개 바이너리를 별도로 재다운로드해 SHA를 재계산한 검증으로 부풀리지는 않는다.

## 공개 주소에서 실제 확인한 기능

| 항목 | 결과 / 방법 |
|---|---|
| 메인·게임 시작 | PASS. 새 공개 탭에서 로딩/메인 표시, 화면의 시작 영역을 실제 클릭해 최초 안내 진입. Dev 자동 활성화 없음 |
| 정규 게임 | PASS. 기본 정규 게임 버튼→Cyberpunk, HP3/3·노드 생성·무적false·기록 가능. 설정을 열어 시간0으로 안전 정지 |
| 훈련·개발자 패널 | PASS. 훈련→개발자 도구, 기록 제외·무적 표시·141개 JSON 항목 |
| 원형 확정 기본값 | PASS. 실제 런타임 JSON의 변형.15 / 전진.20 / 커서.60. .45 실험값은 배포 기본값에 없음 |
| 대표 수치 실제 적용 | PASS. UI로 전진.30 요청 시 실제.20/다음 생성 대기→실제 원형 생성 후 실제.30. 안전 재생성·시험으로 시간1·활성 노드 확인. 배포 기본값/에셋 변경 아님 |
| 한국어 보고서·복사 | PASS. 항목명/key/기본→요청/단위/시점/현재 유효값·buildId 포함. 클립보드 복사 성공 응답과 실제 읽은 텍스트의 완전 일치 확인 |
| JSON | PASS. 공개 런타임 전체141키·변경 JSON 내보내기, HTML 텍스트 가져오기로 원래 요청값 복원. 공개에서 파일 선택/다운로드·프리셋 새로고침까지 다시 시험한 결과로 표현하지 않음(동일 빌드의 이전 로컬 검증은 유지) |
| 세션 실험 정리 | PASS. 전진.20으로 복원 후 새 원형의 실제.20 확인, 노드 정리·Dev 해제·메인 복귀. 개인 브라우저 저장값을 빌드에 복사하지 않음 |
| 반응형 | PASS_DESKTOP_RESPONSIVE. 요청 viewport390×844의 좁은 화면에서 HTML 수치/소수 입력·스크롤 레이아웃 확인. 브라우저 배율에 따라 실제 CSS viewport 약355px. 임시 override는 복원 |
| 오류/네트워크 | PASS. 수집한 공개 브라우저 Console warn/error0건, 실제 JS/WASM/텍스트/셰이더 로딩 성공. 알려진 파일21개 HTTP200. 모든 미래 요청/기기를 보증하지 않음 |
| 실제 휴대폰 | NOT_RUN. 데스크톱 앱 브라우저/반응형과 구분 |

기존 공식27 PASS/0 FAIL/0 SKIP은 이전 실행(완료 UTC2026-10-23T17:59:31.4580605Z)을 **재사용**했다. 게임 소스/판정을 바꾸지 않았으므로 다시 실행하지 않았고 새 실행으로 집계하지 않는다. 이번에는 Unity 실행/재컴파일/Play/테스트/재빌드를 호출하지 않았다.
독립 검증 함수2개·과거 USER_VERIFIED와 합산하지 않는다. 화면 클릭과 HTML 버튼 검증은 실제 UI 조작이다. 노드 생성·안전 시험은 도구가 제공하는 내부 보조 명령으로, 사람이 전투를 성공한 결과와 구분한다.

### 실제 공개 화면과 로그

- [메인](D:/renewal_CursorDash/Cursor-Dash-master/Logs/deploy-cp14-20261024-131718/Public/01-main.jpg)
- [시작 클릭 후 최초 안내](D:/renewal_CursorDash/Cursor-Dash-master/Logs/deploy-cp14-20261024-131718/Public/02-first-guide.jpg)
- [훈련 원형 생성](D:/renewal_CursorDash/Cursor-Dash-master/Logs/deploy-cp14-20261024-131718/Public/03-training-circle.jpg)
- [실제.30 적용·한국어 보고서](D:/renewal_CursorDash/Cursor-Dash-master/Logs/deploy-cp14-20261024-131718/Public/04-live-report.jpg)
- [정규 게임·설정 정지](D:/renewal_CursorDash/Cursor-Dash-master/Logs/deploy-cp14-20261024-131718/Public/05-normal-game.jpg)
- [좁은 화면 수치 도구](D:/renewal_CursorDash/Cursor-Dash-master/Logs/deploy-cp14-20261024-131718/Public/06-portrait-panel.jpg)
- [실제 런타임 기본값JSON](D:/renewal_CursorDash/Cursor-Dash-master/Logs/deploy-cp14-20261024-131718/Public/runtime-defaults.json), [기능별 근거](D:/renewal_CursorDash/Cursor-Dash-master/Logs/deploy-cp14-20261024-131718/Public/browser-checks.json), [Console](D:/renewal_CursorDash/Cursor-Dash-master/Logs/deploy-cp14-20261024-131718/Public/console.json)

## 사용법·남은 검수·알려진 항목

공개 주소 **훈련/설정 → 개발자 활성화(또는 개발자 도구) → 항목 변경 → 안전 재생성·시험**.
패널을 닫은 뒤 상단 훈련/메인으로 전환한다. 기본 정규 게임은 패널 안의 같은 이름 버튼으로 시작한다.
다음 생성/다음 라운드 대기와 실제 값을 비교하고 **변경 보고서 → 클립보드 복사** 또는 전체/변경JSON으로 조정값을 전달한다.
[141개 항목과 사용법](D:/renewal_CursorDash/docs/CursorDash/TUNING_GUIDE.md).

localhost/127.0.0.1과 **github.io의 저장소는 별개**다. 서로 옮길 값은 JSON으로 내보내고 대상 주소에서 가져온다. 주소 이동을 위해 저장소 전체 삭제/최고기록 초기화를 하지 않는다. github.io에서는 같은 origin의 저장 영역을 쓸 수 있으므로 다른 프로젝트 키를 건드리지 않고 CursorDash.v2의 허용 키만 쓴다.

실제 휴대폰에서 남은 항목: 터치·멀티터치의 게임/UI 입력 분리, 주소창/안전영역/가상 키보드, 소수·음수 입력/스크롤/닫은 뒤 새 입력, 값 적용·프리셋 저장/새로고침, 파일 가져오기/내보내기·복사 대안, 반환 가독성·왜곡·발열/지속 FPS. 공개 주소에서 바로 시험할 수 있다. 이번에 실기기 검수 완료로 표시하지 않는다.

확인된 기존 수치 표기 차이: `fx.nexusDamage.distortion`은 메타데이터 기본 .006, HitFeedback의 미지정 fallback은 .008이다. 실제 피격 뒤 보고서에는 사용자가 편집하지 않은 이 차이도 나타났다. 기존 TUNING_GUIDE의 상속된 값 포함 규칙과 함께 기록하며, 이번 배포에서 게임 코드/수치를 수정하지 않았다. 기본값 정합성은 별도 후속 항목이다.
메인 BESTTIME11.53 표시도 기존 후보 런타임에서 관찰했다. 현재 MainSceneController는 MapInformation.highScore와 해당 PlayerPrefs의 최댓값을 사용한다. 후보 생성 당시 메모리 MapInformation11.5285225/디스크0의 차이는 이전 CP14 결과에 보존돼 있다. 새 브라우저 최고기록을 배포용 소스에 복사하거나 초기화한 작업은 없다.
BGM 재생 비활성·실기기 미검수·이전 Editor의 폰트 참조 메시지는 기존 제한으로 유지한다. 공개 브라우저에서는 해당 폰트/셰이더 오류를 관찰하지 않았다. 원인 미확정인 Editor 메시지를 새로 해결했다고 표현하지 않는다.

## 보존·종료·복구 지점

원본 main HEAD/브랜치와 기존20개 tracked 변경·283개 미추적 파일을 사전에 기록했다. 사전303개 파일 SHA256과 종료 상태를 비교하고, 허용한 로컬 문서 갱신만 별도로 분류했다. 기존 Unity 게임 파일/프리팹/씬/패키지/설정은 이번 배포에서 수정하지 않았다. 사용자 미저장 상태를 저장·폐기하거나 Unity를 재시작하지 않았다.
추가된 별도 배포 checkout과 로컬 문서/검증 로그는 원본 main에 stage/commit/push하지 않았다. 원본 폴더에서 전체 add하지 않는다.
최종 보존 근거는 실행 폴더의 `preservation-final.json`, 소스 일치는 `source-match.json`과 `source-match-final.json`을 따른다.

문제가 재현될 경우 복구 기준은 **296a394bc48ee230f824df547390e26a8d83a8b6**이다. 사용자 승인 범위 내 실제 배포 회귀 복구는 그 실행 산출물을 적용한 **새 후속 커밋**으로 수행하며 force push/이력 재작성은 하지 않는다. 이번 공개 검증에서는 롤백이 필요한 회귀가 확인되지 않았다.
다음 한 작업은 사용자의 공개 버전 모바일/시각 검수와 조정 보고서 전달이다. 새 기능·리디자인·후속 배포로 자동 진행하지 않는다.
