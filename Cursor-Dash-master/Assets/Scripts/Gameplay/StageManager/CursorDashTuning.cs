using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>Allow-listed runtime values. No reflection, asset writes or executable imports.</summary>
public static class CursorDashTuning
{
    public const string DefaultsVersion = "cp14-v2.1-20261024";
    public const string UserKey = "CursorDash.v2.user";
    public const string PresetKey = "CursorDash.v2.presets";
    public const string TutorialKey = "CursorDash.v2.tutorial";
    public const int MaxJsonBytes = 65536, MaxPresets = 8;
    public sealed class Parameter
    {
        public string key, label, group, type, unit, apply, scope, description;
        public double min, max, step;
        public JToken defaultValue;
    }
    public static readonly List<Parameter> Parameters = new List<Parameter>();
    private static readonly Dictionary<string, JToken> requested = new Dictionary<string, JToken>();
    private static readonly Dictionary<string, JToken> effective = new Dictionary<string, JToken>();
    private static readonly Dictionary<string, JToken> liveFallbacks = new Dictionary<string, JToken>();
    private static JObject userValues = new JObject();
    private static HashSet<string> legacyKeys,previousVisualKeys;
    public static bool DevEnabled { get; private set; }
    public static event Action Changed;
    public static Action RuleSessionChanged;
    public static string LastMessage { get; private set; } = "";
    public static string BuildId => Resources.Load<TextAsset>("CursorDash/BuildIdentity")?.text.Trim() ?? "editor-cp14-20261024";
    static CursorDashTuning()
    {
        DefineParameters();
        try { userValues = JObject.Parse(CursorDashStorage.Get(UserKey, "{}")); }
        catch { LastMessage = "일반 설정 저장값을 읽지 못했습니다. 기본값으로 실행하며 기존 데이터는 지우지 않습니다."; }
    }
    private static void Add(string key, string label, string group, double value, double min, double max, double step,
        string unit = "", string apply = "다음 생성", string scope = "개발자", string type = "number", string description = "")
    {
        Parameters.Add(new Parameter { key=key,label=label,group=group,type=type,unit=unit,min=min,max=max,step=step,
            defaultValue=type=="toggle" ? new JValue(value!=0) : new JValue(value),apply=apply,scope=scope,
            description=string.IsNullOrEmpty(description)?label+": "+apply+" 적용. 원본 에셋은 저장하지 않습니다.":description });
    }
    private static void ColorParam(string key, string label, string group, string value, string apply="즉시", string scope="개발자")
    {
        ColorUtility.TryParseHtmlString(value,out Color parsed);
        Parameters.Add(new Parameter {key=key,label=label,group=group,type="color",unit="RGBA",defaultValue="#"+ColorUtility.ToHtmlStringRGBA(parsed),apply=apply,scope=scope,description="RGB/RGBA 16진 색상. "+apply+" 적용"});
    }
    private static void DefineParameters()
    {
        Add("circle.prepare","원형 충전","원형",2.5,0.05,20,0.05,"초");
        Add("circle.morph","원형 흡수·변형","원형",0.15,0.01,3,0.01,"초");
        Add("circle.recoil","원형 반동 시간","원형",0.05,0,1,0.005,"초");
        Add("circle.recoilRatio","반동/최종 월드 길이","원형",0.08,0,0.5,0.01,"비율");
        Add("circle.recoilCurve","반동 곡선","원형",1,0,2,1,"0선형/1부드럽게/2빠르게","다음 생성","개발자","curve");
        Add("circle.dash","원형 전진 시간","원형",0.20,0.01,5,0.01,"초");
        Add("circle.length","커서 기준 길이","원형",1.6,0.2,4,0.05,"로컬");
        Add("circle.width","커서 기준 폭","원형",1.05,0.1,3,0.05,"로컬");
        Add("circle.scale","변형 커서 배율","원형",0.60,0.1,2,0.05,"배");
        Add("circle.morphCurve","형태 변형 곡선","원형",1,0,2,1,"0선형/1부드럽게/2빠르게","다음 생성","개발자","curve");
        Add("circle.impact","원형 폭발 유지","원형",0.4,0.05,3,0.05,"초");
        Add("circle.damage","원형 피해","원형",1,0,10,1,"HP","다음 생성","개발자","integer");
        ColorParam("circle.color","커서 색","원형","#AD1FFF","다음 생성");
        DefineExtendedParameters();
        Add("practice.repeatInterval","훈련 반복 간격","생성",2,.25,10,.05,"초","다음 생성");
        Add("debug.boundary","실제 넥서스 피격 원 표시","시험",0,0,1,1,"","즉시","개발자","toggle");
        DefineVisualParameters();
        DefineFeedbackRevision();
    }
    // Extended checkpoints add only explicitly wired parameters here.
    private static void DefineExtendedParameters()
    {
        Add("circle.spin","원형 테두리 회전 배율","원형",1,0,4,0.1,"배","다음 생성");
        Add("hex.prepare","육각 회전 꺼짐·누락 시 발사까지","육각",3.5,0.05,20,0.05,"초","다음 생성","개발자","number","회전 종료 기준 1/3/5회 충전, 5.5회 변형, 6회 종료에 발사합니다. 이 시간은 회전이 없을 때 발사까지의 안전 대체값입니다.");
        Add("hex.morph","육각 흡수·삼각 변형 상한","육각",0.25,0.01,3,0.01,"초","다음 생성","개발자","number","5.5~6회 구간 안에 변형과 예비동작을 맞춥니다. 기본 실제 변형은 0.175초이며 이 값은 상한입니다.");
        Add("hex.attack","레이저 외곽 도달","육각",0.06,0.01,5,0.01,"초");
        Add("hex.return","반사 반환","육각",0.06,0.005,2,0.005,"초");
        Add("hex.destroy","삼각 파괴 후 풀 복귀","육각",1.333,0.05,5,0.05,"초");
        Add("hex.radius","정삼각형 외접 반경","육각",0.8,0.1,3,0.05,"로컬");
        Add("hex.rotation","삼각형 방향 보정","육각",0,-180,180,1,"도");
        Add("hex.spin","육각 회전·차징 속도","육각",1,0,4,0.1,"배","다음 생성","개발자","number","회전 동작 속도와 충전·변형 시점을 함께 조절합니다. 회전 사이 휴지 간격은 유지하며 0은 기존 준비 시간으로 대체합니다.");
        Add("hex.visualWidth","빔 표시 폭","육각",0.0657304765,0.005,0.5,0.005,"월드");
        Add("hex.hitWidth","빔 방어 판정 폭","육각",0.10112381,0.005,0.5,0.005,"월드");
        Add("hex.damage","육각 피해","육각",1,0,10,1,"HP","다음 생성","개발자","integer");
        Add("nexus.maxHp","넥서스 최대 HP","넥서스",3,1,20,1,"HP","다음 라운드","개발자","integer");
        Add("nexus.solidFraction","넥서스 아트 본체 비율","넥서스",0.59241706,0.1,1,0.01,"비율","다음 라운드");
        Add("nexus.smooth","에너지 수위 보간","넥서스",0.25,0,2,0.01,"초","즉시");
        Add("nexus.waveAmplitude","에너지 물결 높이","넥서스",0.015,0,0.1,0.001,"UI 정규화","즉시");
        Add("nexus.waveSpeed","에너지 물결 속도","넥서스",3,0,15,0.1,"위상/초","즉시");
        Add("nexus.spin","넥서스 장식 회전 배율","넥서스",1,0,4,0.1,"배","다음 라운드");
        Add("fx.enabled","추가 타격 효과 전체","효과 품질",1,0,1,1,"","다음 타격","개발자","toggle");
        Add("fx.particles","추가 파편·스파크","효과 품질",1,0,1,1,"","다음 타격","사용자","toggle");
        Add("fx.distortionSlots","국소 왜곡 동시 상한","효과 품질",4,1,8,1,"개","다음 타격","개발자","integer");
        Add("fx.distortionCap","왜곡 이동량 상한","효과 품질",0.025,0,0.05,0.001,"화면 UV","즉시");
        Add("fx.renderScale","왜곡 패스 해상도","효과 품질",1,0.5,1,0.05,"배","즉시","사용자");
        Add("fx.stopCap","연속 히트스톱 상한","효과 품질",0.1,0.01,0.3,0.005,"초","다음 타격");
        Add("fx.stopGap","히트스톱 사이 최소 진행","효과 품질",0.04,0.01,0.3,0.005,"초","다음 타격");
        Add("fx.shakeCap","누적 흔들림 상한","효과 품질",0.2,0,0.4,0.01,"월드","다음 타격");
        Add("fx.shakeMaxDuration","연속 흔들림 지속 상한","효과 품질",0.35,0.05,1,0.01,"초","다음 타격");
        Add("fx.burstCap","파편 묶음 동시 상한","효과 품질",16,0,32,1,"개","다음 타격","개발자","integer");
        Add("player.speed","최대 이동속도","플레이어",5.75,1,15,0.05,"월드/초","즉시");
        Add("player.acceleration","가속","플레이어",60,1,200,1,"월드/초²","즉시");
        Add("player.release","입력 해제 감속","플레이어",18,1,100,1,"월드/초²","즉시");
        Add("player.turn","방향 회전속도","플레이어",540,90,1800,10,"도/초","즉시");
        Add("player.margin","화면 이동 여백","플레이어",0.08,0,0.25,0.01,"뷰포트","즉시");
        Add("player.visualScale","커서 표시 크기","플레이어",0.5,0.1,1.5,0.05,"로컬","다음 라운드");
        Add("player.colliderX","실제 충돌 폭","플레이어",0.75,0.1,2,0.05,"로컬","다음 라운드");
        Add("player.colliderY","실제 충돌 높이","플레이어",0.75,0.1,2,0.05,"로컬","다음 라운드");
        Add("user.stickRadius","조이스틱 반경","사용자 조작",0.12,0.04,0.25,0.01,"화면 짧은 변 비율","즉시","사용자");
        Add("user.deadZone","조이스틱 데드존","사용자 조작",0.12,0,0.5,0.01,"비율","즉시","사용자");
        Add("user.response","작은 입력 반응 지수","사용자 조작",1,0.5,2,0.05,"지수","즉시","사용자");
        Add("user.shake","화면 흔들림 강도","사용자 효과",1,0,1,0.05,"배","다음 타격","사용자");
        Add("user.distortion","국소 왜곡 강도","사용자 효과",1,0,1,0.05,"배","즉시","사용자");
        Add("user.flash","섬광 강도","사용자 효과",1,0,1,0.05,"배","다음 타격","사용자");
        Add("user.bgm","BGM 음량 (기존 재생 비활성)","오디오",0.5,0,1,0.05,"비율","즉시","사용자");
        Add("user.sfx","SFX 음량","오디오",0.5,0,1,0.05,"비율","즉시","사용자");
        Add("trail.lifetime","잔상 실제 수명","잔상",0.6782609,0.05,10,0.01,"초","안전 재생성");
        Add("trail.hitWidth","잔상 실제 판정 폭","잔상",0.0675,0.005,0.5,0.005,"월드","안전 재생성");
        Add("trail.sample","잔상 샘플 간격","잔상",0.02,0.005,0.2,0.005,"초","안전 재생성");
        Add("trail.distance","실제 샘플 최소 거리","잔상",0.015,0.001,0.2,0.001,"월드","안전 재생성");
        Add("trail.visualWidth","잔상 표시 폭","잔상",0.0675,0.005,0.5,0.005,"월드","즉시");
        Add("trail.alpha","잔상 시작 알파","잔상",1,0,1,0.05,"비율","즉시");
        Add("trail.tailAlpha","잔상 중간 알파","잔상",0.35,0,1,0.05,"비율","즉시");
        Add("trail.fade","잔상 감쇠 프리셋","잔상",1,0,2,1,"0선형/1부드럽게/2빠르게","즉시","개발자","curve");
        Add("spawn.interval","패턴 사이 간격","생성",1.4,0,10,0.05,"초","다음 라운드");
        Add("spawn.jitter","생성 위치 흔들림","생성",0.35,0,1,0.01,"월드","다음 라운드");
        Add("spawn.limit","최대 활성 노드 (기존 풀 한도 내)","생성",128,1,256,1,"개","다음 라운드","개발자","integer");
        Add("spawn.pattern","기존 패턴 선택","생성",0,0,2,1,"0양쪽/1메인/2서브","다음 라운드","개발자","curve");
        Add("spawn.repeat","패턴 반복","생성",1,0,1,1,"","다음 라운드","개발자","toggle");
        Add("spawn.seedEnabled","생성·패턴 고정 seed","생성",0,0,1,1,"","다음 라운드","개발자","toggle");
        Add("spawn.seed","생성 seed (효과 난수 제외)","생성",1234,0,1000000,1,"정수","다음 라운드","개발자","integer");
        Add("debug.timeScale","개발자 시간배율","시험",1,0.1,2,0.05,"배","즉시");
        Add("fx.circleRemove.stop","히트스톱","원형 제거 타격",0.04,0,0.2,0.005,"초","다음 타격","개발자","number");
        Add("fx.circleRemove.shake","셰이크 강도","원형 제거 타격",0.055,0,0.3,0.005,"월드","다음 타격","개발자","number");
        Add("fx.circleRemove.shakeDuration","셰이크 지속","원형 제거 타격",0.14,0.02,0.5,0.01,"초","다음 타격","개발자","number");
        Add("fx.circleRemove.shakeDecay","셰이크 감쇠","원형 제거 타격",1.8,0.5,4,0.1,"지수","다음 타격","개발자","number");
        Add("fx.circleRemove.distortion","국소 왜곡 강도","원형 제거 타격",0.008,0,0.03,0.001,"화면 UV","다음 타격","개발자","number");
        Add("fx.circleRemove.radius","국소 왜곡 반경","원형 제거 타격",0.14,0.02,0.5,0.01,"짧은 변 비율","다음 타격","개발자","number");
        Add("fx.circleRemove.duration","국소 왜곡 지속","원형 제거 타격",0.18,0.02,0.8,0.01,"초","다음 타격","개발자","number");
        Add("fx.circleRemove.flash","전체 섬광 (기본 꺼짐)","원형 제거 타격",0,0,0.12,0.01,"알파","다음 타격","개발자","number");
        Add("fx.circleRemove.count","파편·스파크 수","원형 제거 타격",6,1,24,1,"개","다음 타격","개발자","integer");
        Add("fx.circleRemove.size","파편 크기","원형 제거 타격",0.05,0.01,2,0.01,"월드","다음 타격","개발자","number");
        Add("fx.circleRemove.speed","파편 이동속도","원형 제거 타격",1.5,0,5,0.1,"월드/초","다음 타격","개발자","number");
        Add("fx.circleRemove.life","파편 수명","원형 제거 타격",0.3,0.05,2,0.01,"초","다음 타격","개발자","number");
        Add("fx.circleRemove.volume","사건 음량","원형 제거 타격",0.8,0,1,0.05,"배","다음 타격","개발자","number");
        Add("fx.circleRemove.pitch","사건 음높이","원형 제거 타격",1,0.25,3,0.05,"배","다음 타격","개발자","number");
        ColorParam("fx.circleRemove.color","파편·스파크 색","원형 제거 타격","#BF4DFF","다음 타격");
        Add("fx.hexBlock.stop","히트스톱","육각 방어 타격",0,0,0.2,0.005,"초","다음 타격","개발자","number");
        Add("fx.hexBlock.shake","셰이크 강도","육각 방어 타격",0.018,0,0.3,0.005,"월드","다음 타격","개발자","number");
        Add("fx.hexBlock.shakeDuration","셰이크 지속","육각 방어 타격",0.14,0.02,0.5,0.01,"초","다음 타격","개발자","number");
        Add("fx.hexBlock.shakeDecay","셰이크 감쇠","육각 방어 타격",1.8,0.5,4,0.1,"지수","다음 타격","개발자","number");
        Add("fx.hexBlock.distortion","국소 왜곡 강도","육각 방어 타격",0.002,0,0.03,0.001,"화면 UV","다음 타격","개발자","number");
        Add("fx.hexBlock.radius","국소 왜곡 반경","육각 방어 타격",0.14,0.02,0.5,0.01,"짧은 변 비율","다음 타격","개발자","number");
        Add("fx.hexBlock.duration","국소 왜곡 지속","육각 방어 타격",0.18,0.02,0.8,0.01,"초","다음 타격","개발자","number");
        Add("fx.hexBlock.flash","전체 섬광 (기본 꺼짐)","육각 방어 타격",0,0,0.12,0.01,"알파","다음 타격","개발자","number");
        Add("fx.hexBlock.count","파편·스파크 수","육각 방어 타격",4,1,24,1,"개","다음 타격","개발자","integer");
        Add("fx.hexBlock.size","파편 크기","육각 방어 타격",0.05,0.01,2,0.01,"월드","다음 타격","개발자","number");
        Add("fx.hexBlock.speed","파편 이동속도","육각 방어 타격",1.5,0,5,0.1,"월드/초","다음 타격","개발자","number");
        Add("fx.hexBlock.life","파편 수명","육각 방어 타격",0.3,0.05,2,0.01,"초","다음 타격","개발자","number");
        Add("fx.hexBlock.volume","사건 음량","육각 방어 타격",0.8,0,1,0.05,"배","다음 타격","개발자","number");
        Add("fx.hexBlock.pitch","사건 음높이","육각 방어 타격",1,0.25,3,0.05,"배","다음 타격","개발자","number");
        ColorParam("fx.hexBlock.color","파편·스파크 색","육각 방어 타격","#FFD933","다음 타격");
        Add("fx.triangleBreak.stop","히트스톱","삼각 파괴 타격",0.05,0,0.2,0.005,"초","다음 타격","개발자","number");
        Add("fx.triangleBreak.shake","셰이크 강도","삼각 파괴 타격",0.065,0,0.3,0.005,"월드","다음 타격","개발자","number");
        Add("fx.triangleBreak.shakeDuration","셰이크 지속","삼각 파괴 타격",0.14,0.02,0.5,0.01,"초","다음 타격","개발자","number");
        Add("fx.triangleBreak.shakeDecay","셰이크 감쇠","삼각 파괴 타격",1.8,0.5,4,0.1,"지수","다음 타격","개발자","number");
        Add("fx.triangleBreak.distortion","국소 왜곡 강도","삼각 파괴 타격",0.01,0,0.03,0.001,"화면 UV","다음 타격","개발자","number");
        Add("fx.triangleBreak.radius","국소 왜곡 반경","삼각 파괴 타격",0.14,0.02,0.5,0.01,"짧은 변 비율","다음 타격","개발자","number");
        Add("fx.triangleBreak.duration","국소 왜곡 지속","삼각 파괴 타격",0.18,0.02,0.8,0.01,"초","다음 타격","개발자","number");
        Add("fx.triangleBreak.flash","전체 섬광 (기본 꺼짐)","삼각 파괴 타격",0,0,0.12,0.01,"알파","다음 타격","개발자","number");
        Add("fx.triangleBreak.count","파편·스파크 수","삼각 파괴 타격",6,1,24,1,"개","다음 타격","개발자","integer");
        Add("fx.triangleBreak.size","파편 크기","삼각 파괴 타격",1,0.01,2,0.01,"실루엣 배율","다음 타격","개발자","number");
        Add("fx.triangleBreak.speed","파편 이동속도","삼각 파괴 타격",1.5,0,5,0.1,"월드/초","다음 타격","개발자","number");
        Add("fx.triangleBreak.life","파편 수명","삼각 파괴 타격",0.3,0.05,2,0.01,"초","다음 타격","개발자","number");
        Add("fx.triangleBreak.volume","사건 음량","삼각 파괴 타격",0.8,0,1,0.05,"배","다음 타격","개발자","number");
        Add("fx.triangleBreak.pitch","사건 음높이","삼각 파괴 타격",1,0.25,3,0.05,"배","다음 타격","개발자","number");
        ColorParam("fx.triangleBreak.color","파편·스파크 색","삼각 파괴 타격","#FFD466","다음 타격");
        Add("fx.nexusDamage.stop","히트스톱","넥서스 피격 타격",0.08,0,0.2,0.005,"초","다음 타격","개발자","number");
        Add("fx.nexusDamage.shake","셰이크 강도","넥서스 피격 타격",0.1,0,0.3,0.005,"월드","다음 타격","개발자","number");
        Add("fx.nexusDamage.shakeDuration","셰이크 지속","넥서스 피격 타격",0.14,0.02,0.5,0.01,"초","다음 타격","개발자","number");
        Add("fx.nexusDamage.shakeDecay","셰이크 감쇠","넥서스 피격 타격",1.8,0.5,4,0.1,"지수","다음 타격","개발자","number");
        Add("fx.nexusDamage.distortion","국소 왜곡 강도","넥서스 피격 타격",0.006,0,0.03,0.001,"화면 UV","다음 타격","개발자","number");
        Add("fx.nexusDamage.radius","국소 왜곡 반경","넥서스 피격 타격",0.14,0.02,0.5,0.01,"짧은 변 비율","다음 타격","개발자","number");
        Add("fx.nexusDamage.duration","국소 왜곡 지속","넥서스 피격 타격",0.18,0.02,0.8,0.01,"초","다음 타격","개발자","number");
        Add("fx.nexusDamage.flash","전체 섬광 (기본 꺼짐)","넥서스 피격 타격",0,0,0.12,0.01,"알파","다음 타격","개발자","number");
        Add("fx.nexusDamage.count","파편·스파크 수","넥서스 피격 타격",6,1,24,1,"개","다음 타격","개발자","integer");
        Add("fx.nexusDamage.size","파편 크기","넥서스 피격 타격",0.05,0.01,2,0.01,"월드","다음 타격","개발자","number");
        Add("fx.nexusDamage.speed","파편 이동속도","넥서스 피격 타격",1.5,0,5,0.1,"월드/초","다음 타격","개발자","number");
        Add("fx.nexusDamage.life","파편 수명","넥서스 피격 타격",0.3,0.05,2,0.01,"초","다음 타격","개발자","number");
        Add("fx.nexusDamage.volume","사건 음량","넥서스 피격 타격",0.8,0,1,0.05,"배","다음 타격","개발자","number");
        Add("fx.nexusDamage.pitch","사건 음높이","넥서스 피격 타격",1,0.25,3,0.05,"배","다음 타격","개발자","number");
        ColorParam("fx.nexusDamage.color","파편·스파크 색","넥서스 피격 타격","#1ACCFF","다음 타격");
        ColorParam("hex.color","정삼각형 색","육각","#FFD466","다음 생성");
        ColorParam("hex.beamColor","발사 빔 색","육각","#FF66D9","다음 생성");
        ColorParam("hex.returnColor","반환 빔 색","육각","#FFF299","다음 생성");
        ColorParam("nexus.color","에너지 색","넥서스","#0AB2E6","즉시");
        ColorParam("user.cursorColor","플레이어 커서 색","플레이어","#FFFFFF","즉시","사용자");
        ColorParam("user.trailColor","잔상 색","잔상","#FFFFFF","즉시","사용자");
    }
    private static void DefineVisualParameters()
    {
        legacyKeys=new HashSet<string>(Parameters.Select(p=>p.key));
        Find("hex.radius").label="변형 육각 외접 반경";Find("hex.color").label="변형 육각 색";
        Find("hex.morph").label="육각 흡수·육각 변형";Find("hex.destroy").label="육각 파괴 후 풀 복귀";Find("hex.rotation").label="육각 발사구 방향 보정";
        foreach(var p in Parameters)if(p.group=="삼각 파괴 타격")p.group="육각 반환 파괴";
        foreach(string kind in new[]{"circleRemove","triangleBreak"}){
            Find("fx."+kind+".distortion").max=.08;Find("fx."+kind+".radius").max=1.5;
            string g=kind=="circleRemove"?"원형 제거 타격":"육각 반환 파괴",k="fx."+kind+".";
            Add(k+"frequency","셰이크 주파수",g,26,5,45,1,"Hz","다음 타격");
            Add(k+"rebound","셰이크 되튕김",g,.35,0,1,.05,"배","다음 타격");
            Add(k+"recovery","왜곡 복귀 감쇠",g,2,.5,4,.1,"지수","다음 타격");
            Add(k+"spread","방향 파편 분산",g,65,0,85,1,"±도","다음 타격");
            Add(k+"spin","파편 회전 속도",g,180,0,720,10,"도/초","다음 타격");
        }
        Add("visual.outline","노드 외곽선 두께","노드 그래픽",.035,.01,.12,.005,"로컬","즉시");
        Add("visual.core","중앙 코어 반경","노드 그래픽",.10,.03,.20,.01,"로컬","즉시");
        Add("visual.glow","분리된 약한 발광","노드 그래픽",0,0,.15,.01,"알파","즉시");
        Add("visual.hexShortEdge","육각 짧은 변/긴 변","노드 그래픽",.18,.03,1,.01,"비율","다음 생성");
        Add("visual.energyCurve","육각 피스톤 소모 곡선","노드 그래픽",1,.25,3,.05,"지수","다음 생성");
        Add("visual.successDistortionCap","성공 왜곡 합산 상한","노드 그래픽",.06,0,.08,.005,"화면 UV","다음 타격");
    }
    private static void DefineFeedbackRevision()
    {
        previousVisualKeys=new HashSet<string>(Parameters.Select(p=>p.key));
        Find("visual.core").defaultValue=new JValue(0);Find("visual.core").min=0;
        Find("visual.core").label="중앙점 반경 (0=끄기)";
        Find("hex.beamColor").defaultValue="#FFD466FF";Find("hex.returnColor").defaultValue="#FFD466FF";
        foreach(string kind in new[]{"circleRemove","triangleBreak"}){
            string k="fx."+kind+".";bool circle=kind=="circleRemove";
            Find(k+"stop").defaultValue=new JValue(circle?.065:.075);
            Find(k+"shake").defaultValue=new JValue(circle?.15:.18);
            Find(k+"shakeDuration").defaultValue=new JValue(circle?.22:.25);
            Find(k+"distortion").defaultValue=new JValue(circle?.045:.05);
            Find(k+"distortion").label="렌즈·파동 왜곡 강도";
            Find(k+"radius").defaultValue=new JValue(2.2);Find(k+"radius").max=3;
            Find(k+"radius").label="렌즈 왜곡 반경";
            Find(k+"duration").defaultValue=new JValue(.5);
            Find(k+"duration").label="출렁임·복귀 지속";
            Find(k+"count").defaultValue=new JValue(circle?8:10);
            Find(k+"size").defaultValue=new JValue(circle?.05:.85);
            Find(k+"speed").defaultValue=new JValue(circle?2.4:2.8);
            Find(k+"life").defaultValue=new JValue(circle?.28:.34);
            Find(k+"recovery").defaultValue=new JValue(1.4);
            Find(k+"frequency").defaultValue=new JValue(circle?22:24);
            Find(k+"rebound").defaultValue=new JValue(.55);
        }
        Find("fx.circleRemove.size").description=".05 기준 원형 전용 불규칙 조각 크기 배율. 본체 반경의 .25~.52배 길이를 기준으로 하며 옛 size=1 값도 1.25배로 제한합니다. 승인 육각 파편에는 적용하지 않습니다.";
        Add("visual.rippleStrength","전체 화면 전파 파동","노드 그래픽",.8,0,1.5,.05,"왜곡 배율","다음 타격");
        Add("visual.rippleSpeed","파동 전파 속도","노드 그래픽",6,1,12,.25,"화면 짧은 변/초","다음 타격");
        Add("visual.rippleWidth","파동 폭·파장","노드 그래픽",.35,.10,1,.01,"화면 짧은 변 비율","다음 타격");
        Add("visual.impactExposure","배경·월드 밝기 응답","노드 그래픽",.65,0,1,.05,"강도 (사용자 섬광 배율 적용)","다음 타격");
        // D-021: circle-only reference comparison. Attack clocks and hex defaults remain unchanged.
        Find("fx.circleRemove.stop").defaultValue=new JValue(.05);
        Find("fx.circleRemove.shake").defaultValue=new JValue(.20);
        Find("fx.circleRemove.shakeDuration").defaultValue=new JValue(.35);
        Find("fx.circleRemove.frequency").defaultValue=new JValue(20);
        Find("fx.circleRemove.duration").defaultValue=new JValue(2);
        Find("fx.circleRemove.duration").max=3;
        Find("fx.circleRemove.duration").label="원형 렌즈 복귀 지속";
        Find("fx.circleRemove.recovery").defaultValue=new JValue(2);
        // D-026 circle screen response now uses the approved hex KIND and defaults exactly once.
        // Only circle values change. Explicit stored user presets remain explicit overrides.
        foreach(string field in new[]{"stop","shake","shakeDuration","shakeDecay","frequency","rebound","distortion","radius","duration","recovery","flash"})
            Find("fx.circleRemove."+field).defaultValue=Find("fx.triangleBreak."+field).defaultValue.DeepClone();
        Find("fx.circleRemove.duration").label="원형 게임판 빛·파동 복귀";
        // D-027 circle shards only. Approved hex particles and all screen feedback remain untouched.
        Find("fx.circleRemove.count").defaultValue=new JValue(12);
        Find("fx.circleRemove.life").defaultValue=new JValue(.34);
        // Keep historical JSON keys, while the nexus now snaps to actual health and has no idle oscillator.
        Find("nexus.waveAmplitude").label="피격 순간 수위 눌림";
        Find("nexus.waveAmplitude").description="피해 감소 시에만 .12초 내 안정됩니다. 평상시 물결은 없으며 표시 진폭은 .025 이내입니다.";
        Find("nexus.smooth").label="이전 수위 보간 (미사용)";
        Find("nexus.smooth").description="이전 프리셋 호환 키입니다. 현재 수위는 실제 체력으로 즉시 반영하므로 적용하지 않습니다.";
        Find("nexus.waveSpeed").label="이전 물결 속도 (미사용)";
        Find("nexus.waveSpeed").description="이전 프리셋 호환 키입니다. 평상시 자동 물결과 반복 진동을 사용하지 않습니다.";
        // D-025: keep the legacy export schema and values. C/D now have separate contact profiles.
        foreach(string field in new[]{"count","size","speed","life","color"}){
            var p=Find("fx.nexusDamage."+field);p.label+=" (수동 진단용)";
            p.description="기존 DamageNexus 수동 진단 파편에만 적용합니다. 실제 C 원형/D 레이저 접촉은 NexusImpactVfx의 사건별 프로필을 사용합니다. 공통 히트스톱·화면 반응 설정은 C/D에 계속 적용됩니다.";
        }
    }
    public static Parameter Find(string key) => Parameters.Find(p=>p.key==key);
    public static void EnableDev(bool enabled)
    {
        DevEnabled=enabled;
        if(enabled) RuleSessionChanged?.Invoke();
        Changed?.Invoke();
    }
    public static bool TrySet(string key, JToken value, out string message, bool user=false)
    {
        var p=Find(key);
        if(p==null) {message="지원하지 않는 key: "+key;return false;}
        if(user && p.scope!="사용자") {message="일반 설정에서 변경할 수 없는 항목입니다.";return false;}
        if(!user && !DevEnabled) {message="개발자 도구를 명시적으로 활성화하세요.";return false;}
        if(!Validate(p,value,out message)) return false;
        value=Canonical(p,value);
        if(user) userValues[key]=value.DeepClone();
        else {requested[key]=value.DeepClone();RuleSessionChanged?.Invoke();}
        LastMessage="적용 요청: "+p.label+" ("+p.apply+")";
        Changed?.Invoke(); return true;
    }
    private static JToken Canonical(Parameter p,JToken value) {
        if(p.type=="color"){ColorUtility.TryParseHtmlString((string)value,out Color c);return "#"+ColorUtility.ToHtmlStringRGBA(c);}
        return p.type=="toggle"?value.DeepClone():new JValue((double)value);
    }
    public static bool Equivalent(string key,JToken a,JToken b) {
        if(a==null||b==null)return a==b;
        var p=Find(key);if(p?.type=="color"){ColorUtility.TryParseHtmlString((string)a,out Color ca);ColorUtility.TryParseHtmlString((string)b,out Color cb);return ColorUtility.ToHtmlStringRGBA(ca)==ColorUtility.ToHtmlStringRGBA(cb);}
        if(a.Type==JTokenType.Float||a.Type==JTokenType.Integer)return Math.Abs((double)a-(double)b)<.00001;
        return JToken.DeepEquals(a,b);
    }
    private static bool Validate(Parameter p,JToken v,out string error)
    {
        error=p.label+"의 타입/범위가 올바르지 않습니다.";
        if(v==null) return false;
        if(p.type=="color") {
            if(v.Type!=JTokenType.String) return false;
            string s=(string)v;
            Color c;
            return s!=null && System.Text.RegularExpressions.Regex.IsMatch(s,"^#[0-9a-fA-F]{6}([0-9a-fA-F]{2})?$") && ColorUtility.TryParseHtmlString(s,out c);
        }
        if(p.type=="toggle") return v.Type==JTokenType.Boolean;
        if(v.Type!=JTokenType.Float&&v.Type!=JTokenType.Integer) return false;
        double n=(double)v;
        return !double.IsNaN(n)&&!double.IsInfinity(n)&&n>=p.min&&n<=p.max &&
            ((p.type!="integer"&&p.type!="curve")||Math.Abs(n-Math.Round(n))<0.000001);
    }
    public static bool TrySetText(string key,string text,out string error,bool user=false)
    {
        var p=Find(key);if(p==null){error="지원하지 않는 key";return false;}
        try {
            JToken value=p.type=="color"?new JValue(text):JToken.Parse(text);
            bool ok=TrySet(key,value,out error,user);if(ok)error="";return ok;
        } catch {error="숫자·토글·색 입력 형식을 확인하세요.";return false;}
    }
    private static JToken Read(string key,JToken fallback)
    {
        liveFallbacks[key]=fallback.DeepClone();
        var parameter=Find(key);
        if(Application.isPlaying&&parameter!=null&&parameter.scope=="개발자"&&!Equivalent(key,fallback,parameter.defaultValue))RuleSessionChanged?.Invoke();
        JToken value;
        if(DevEnabled&&requested.TryGetValue(key,out value)) {}
        else if(Find(key)?.scope=="사용자"&&userValues.TryGetValue(key,out value) && Validate(Find(key),value,out _)) {}
        else value=fallback;
        effective[key]=value.DeepClone();
        return value;
    }
    public static float Number(string key,float fallback) => (float)Read(key,new JValue(fallback));
    public static int Integer(string key,int fallback) => Mathf.RoundToInt(Number(key,fallback));
    public static bool Toggle(string key,bool fallback) => (bool)Read(key,new JValue(fallback));
    public static Color ColorValue(string key,Color fallback) {
        Color parsed; return ColorUtility.TryParseHtmlString((string)Read(key,"#"+ColorUtility.ToHtmlStringRGBA(fallback)),out parsed)?parsed:fallback;
    }
    public static JToken Requested(string key) {
        JToken v; if(DevEnabled&&requested.TryGetValue(key,out v)) return v;
        if(Find(key)?.scope=="사용자"&&userValues.TryGetValue(key,out v)&&Validate(Find(key),v,out _))return v;
        if(liveFallbacks.TryGetValue(key,out v))return v;
        return Find(key)?.defaultValue;
    }
    public static JToken Effective(string key) => effective.TryGetValue(key,out var v)?v:Find(key)?.defaultValue;
    public static bool HasConsumed(string key)=>effective.ContainsKey(key);
    public static void Reset(string key=null,string group=null) {
        foreach(var p in Parameters.Where(p=>(key==null||p.key==key)&&(group==null||p.group==group)))requested.Remove(p.key);
        Changed?.Invoke();
    }
    public static float Curve(float t,int preset) => preset==0?Mathf.Clamp01(t):preset==2?1f-Mathf.Pow(1f-Mathf.Clamp01(t),3):Mathf.SmoothStep(0,1,t);
    public static string Export(string name="현재 세션",bool changesOnly=false)
    {
        var values=new JObject();
        foreach(var p in Parameters) {var v=Requested(p.key);if(!changesOnly||!Equivalent(p.key,v,p.defaultValue))values[p.key]=v.DeepClone();}
        return new JObject {["schemaVersion"]=1,["buildId"]=BuildId,["defaultsVersion"]=DefaultsVersion,
            ["presetName"]=name,["createdAt"]=DateTime.UtcNow.ToString("o"),["kind"]=changesOnly?"changes":"full",["values"]=values}.ToString(Formatting.Indented);
    }
    public static bool Import(string json,out string error)
    {
        error=""; if(!DevEnabled){error="Dev 명시 활성화가 필요합니다.";return false;}
        if(json==null||System.Text.Encoding.UTF8.GetByteCount(json)>MaxJsonBytes){error="JSON은 64KiB 이하여야 합니다.";return false;}
        try {
            JObject doc;using(var reader=new JsonTextReader(new System.IO.StringReader(json)){DateParseHandling=DateParseHandling.None})doc=JObject.Load(reader,new JsonLoadSettings {DuplicatePropertyNameHandling=DuplicatePropertyNameHandling.Error});
            var allowed=new[]{"schemaVersion","buildId","defaultsVersion","presetName","createdAt","kind","values"};
            if(doc.Properties().Any(p=>!allowed.Contains(p.Name)))throw new Exception("알 수 없는 최상위 필드");
            if(doc["schemaVersion"]?.Type!=JTokenType.Integer||(int)doc["schemaVersion"]!=1)throw new Exception("지원하지 않는 schemaVersion");
            foreach(string s in new[]{"buildId","defaultsVersion","presetName","createdAt","kind"})if(doc[s]?.Type!=JTokenType.String)throw new Exception(s+" 문자열 필요");
            if((string)doc["defaultsVersion"]!=DefaultsVersion)throw new Exception("기본값 버전 불일치");
            if((string)doc["kind"]!="full"&&(string)doc["kind"]!="changes")throw new Exception("kind 오류");
            if(((string)doc["presetName"]).Length>40)throw new Exception("이름 길이 초과");
            if(!DateTimeOffset.TryParse((string)doc["createdAt"],out _))throw new Exception("작성 시각 오류");
            var vals=doc["values"] as JObject;if(vals==null||vals.Count>Parameters.Count)throw new Exception("values 오류");
            var next=new Dictionary<string,JToken>();
            foreach(var pair in vals){var p=Find(pair.Key);string e;if(p==null||!Validate(p,pair.Value,out e))throw new Exception("지원 key/값 오류: "+pair.Key);next[pair.Key]=Canonical(p,pair.Value);}
            if((string)doc["kind"]=="full"&&next.Count!=Parameters.Count){
                // Exact historical 141- and 157-key full schemas only. Arbitrary omissions remain errors.
                bool original=next.Count==legacyKeys.Count&&legacyKeys.All(next.ContainsKey);
                bool previous=next.Count==previousVisualKeys.Count&&previousVisualKeys.All(next.ContainsKey);
                if(!original&&!previous)throw new Exception("전체 설정의 key가 누락됐습니다. 입력 "+next.Count+" / 이전 "+legacyKeys.Count+" 또는 "+previousVisualKeys.Count+" / 현재 "+Parameters.Count);
                foreach(var p in Parameters.Where(p=>!next.ContainsKey(p.key)))next[p.key]=p.defaultValue.DeepClone();
            }
            // Commit only after every field passes. No data loss on a rejected import.
            requested.Clear();foreach(var pair in next)requested.Add(pair.Key,pair.Value);
            RuleSessionChanged?.Invoke();Changed?.Invoke();LastMessage="가져오기 완료 ("+next.Count+"항목). 적용 시점을 확인하세요.";return true;
        } catch(Exception e){error="가져오기 거부: "+e.Message;LastMessage=error;return false;}
    }
    public static string ChangedReport() {
        var lines=new List<string>{"CursorDash 변경 수치 보고서", "빌드: "+BuildId+" / 기본값: "+DefaultsVersion};
        foreach(var p in Parameters) if(!Equivalent(p.key,Requested(p.key),p.defaultValue))
            lines.Add(p.label+" ["+p.key+"] "+p.defaultValue+" → "+Requested(p.key)+" "+p.unit+" / "+p.apply+" / 현재 유효: "+Effective(p.key));
        return string.Join("\n",lines);
    }
    public static bool ApplyVisualPreset(string name,out string error)
    {
        error="";if(!DevEnabled){error="Dev 명시 활성화 필요";return false;}
        if(name!="baseline"&&name!="strong"){error="지원하지 않는 내장 시안";return false;}
        // Session-only: never writes or replaces a named user preset, user preference or combat parameter.
        foreach(var p in Parameters.Where(p=>p.key.StartsWith("fx.circleRemove.")||p.key.StartsWith("fx.triangleBreak.")||p.key.StartsWith("visual.")))requested[p.key]=p.defaultValue.DeepClone();
        if(name=="strong"){
            SetVisual("circleRemove",.075f,.15f,.22f,.055f,2.4f,.5f,10,.045f,2.4f,.28f,22,.55f,55,220);
            SetVisual("triangleBreak",.085f,.18f,.25f,.06f,2.4f,.5f,12,.9f,2.8f,.34f,24,.55f,65,260);
        }else{
            SetVisual("circleRemove",.04f,.055f,.14f,.008f,.14f,.18f,6,.05f,1.5f,.3f,26,.35f,65,180);
            SetVisual("triangleBreak",.05f,.065f,.14f,.01f,.14f,.18f,6,1,1.5f,.3f,26,.35f,65,180);
            requested["fx.circleRemove.recovery"]=requested["fx.triangleBreak.recovery"]=new JValue(2);
            requested["visual.rippleStrength"]=new JValue(0);requested["visual.impactExposure"]=new JValue(0);
        }
        RuleSessionChanged?.Invoke();Changed?.Invoke();return true;
    }
    private static void SetVisual(string kind,float stop,float shake,float shakeTime,float distortion,float radius,float duration,
        int count,float size,float speed,float life,float frequency,float rebound,float spread,float spin)
    {
        string k="fx."+kind+".";string[] fields={"stop","shake","shakeDuration","distortion","radius","duration","count","size","speed","life","frequency","rebound","spread","spin","recovery"};
        float[] vals={stop,shake,shakeTime,distortion,radius,duration,count,size,speed,life,frequency,rebound,spread,spin,1.4f};
        for(int i=0;i<fields.Length;i++)requested[k+fields[i]]=new JValue(vals[i]);
    }
    private static bool SaveString(string key,string value,out string error) {
        bool ok=CursorDashStorage.Save(key,value,out error);if(!ok)LastMessage=error;return ok;
    }
    public static bool SaveUser(out string error) => SaveString(UserKey,userValues.ToString(Formatting.None),out error);
    private static JObject ReadPresets() => JObject.Parse(CursorDashStorage.Get(PresetKey,"{}"));
    public static string[] PresetNames() {try{return ReadPresets().Properties().Select(p=>p.Name).ToArray();}catch{LastMessage="프리셋 저장소가 손상됐습니다. 기존 값을 지우지 않습니다.";return new string[0];}}
    public static bool SavePreset(string name,out string error) {
        error="";try {name=(name??"").Trim();if(name.Length<1||name.Length>40||name.Any(char.IsControl))throw new Exception("이름은 1~40자");
            var all=ReadPresets();if(all[name]==null&&all.Count>=MaxPresets)throw new Exception("프리셋 최대8개");
            all[name]=Export(name);string packed=all.ToString(Formatting.None);if(System.Text.Encoding.UTF8.GetByteCount(packed)>524288)throw new Exception("저장 한도512KiB");
            return SaveString(PresetKey,packed,out error);
        }catch(Exception e){error="프리셋 저장 실패: "+e.Message;LastMessage=error;return false;}
    }
    public static bool LoadPreset(string name,out string error) {try {var v=ReadPresets()[name];if(v==null)throw new Exception("없는 프리셋");return Import((string)v,out error);}catch(Exception e){error=e.Message;return false;}}
    public static bool DeletePreset(string name,out string error) {try{var all=ReadPresets();all.Remove(name);return SaveString(PresetKey,all.ToString(Formatting.None),out error);}catch(Exception e){error=e.Message;return false;}}
    public static bool DuplicatePreset(string source,string target,out string error){
        error="";try{target=(target??"").Trim();var all=ReadPresets();if(all[source]==null)throw new Exception("원본 프리셋 없음");if(target.Length<1||target.Length>40||target.Any(char.IsControl)||all[target]!=null)throw new Exception("새 이름은 중복 없이 1~40자");if(all.Count>=MaxPresets)throw new Exception("프리셋 최대8개");var doc=JObject.Parse((string)all[source]);doc["presetName"]=target;doc["createdAt"]=DateTime.UtcNow.ToString("o");all[target]=doc.ToString();return SaveString(PresetKey,all.ToString(Formatting.None),out error);}catch(Exception e){error=e.Message;return false;}
    }
    public static void ResetUser(){userValues=new JObject();Changed?.Invoke();}
}
