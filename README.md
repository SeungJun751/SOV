# 🎵 Sound of Vista

> **5.1채널 공간 입체음향을 활용한 힐링 퍼즐 게임**

[![Unity](https://img.shields.io/badge/Unity-6-black?logo=unity)](https://unity.com/)
[![ICT Award](https://img.shields.io/badge/🥇-ICT%20어워드코리아%20금상-gold)](https://github.com/SeungJun751/SOV)

**Sound of Vista**는 5.1채널 공간 오디오 기술과 IoT 하체 운동 기구를 결합한 Unity 3D 힐링 퍼즐 게임입니다.  
플레이어는 봉인된 소리를 해방시키는 3가지 사운드 기반 퍼즐을 풀 수 있으며, 특정 공간에서 실제 운동 기구로 게임 내 이동을 제어할 수 있습니다.

---

## 📌 프로젝트 개요

- **개발 기간**: 2024.09 ~ 2025.09 (12개월)
- **개발 인원**: 4명 (프로그래머 2명, 디자이너 2명)
- **본인 역할**: Lead Developer (클라이언트 개발, 시스템 설계)
- **개발 환경**: Unity 6, C# (.NET), Firebase, Google Gemini API
- **수상 내역**: 
  - 🥇 **ICT 어워드코리아 2025 금상** (전자신문사)
  - 🏆 성결대학교 캡스톤디자인(2) 우수상 (2025-1학기)

**📺 플레이 영상**: [YouTube](https://youtube.com/watch?v=example)  
**🌐 포트폴리오**: [김승준 Portfolio](https://seungjun751.github.io)

---

## ✨ 핵심 기능

### 🎧 5.1채널 공간 오디오 시스템
- **Audio Mixer 기반 독립 제어**: Master/BGM/SFX 볼륨 개별 조정
- **32개 AudioSource 풀링**: Object Pooling으로 효율적인 3D 사운드 재생
- **거리 기반 우선순위**: 가까운 소리만 재생하여 성능 최적화

### 🧩 3가지 사운드 퍼즐
1. **순서 기억 퍼즐**: 5개 기둥에서 3D 사운드 시퀀스 재생 → 플레이어가 순서대로 선택
2. **멜로디 매칭 퍼즐**: 물 높이로 8개 음계(도~도) 조절 → 노트 큐브 소리와 일치시키기
3. **동물 소리 경로 추적**: 이동하는 3D 사운드 경로를 Line Renderer로 시각화 → 순서대로 나무 연결

### 🏃‍♂️ IoT 운동 기구 연동
- **Firebase Realtime Database**: 하체 운동 기구의 회전 센서 데이터를 실시간 수신
- **Quaternion 기반 회전 감지**: 360도 1바퀴 회전 시 게임 내 이동 거리 증가
- **Android → Firebase → Unity** 3단계 중계 시스템으로 PC 블루투스 제약 극복

### 🤖 Google Gemini 2.0 Flash 챗봇
- **힐링 대화 시스템**: 게임 내에서 AI와 대화하며 감정 위로 기능 제공

---

## 🛠 기술 스택

| 분류 | 기술 |
|------|------|
| **엔진** | Unity 6 |
| **언어** | C# (.NET Framework) |
| **오디오** | Audio Mixer (5.1 Surround), Spatial Audio, AudioSource Pooling |
| **네트워크** | Firebase Realtime Database, Google Gemini API |
| **디자인 패턴** | Singleton, Object Pooling, ScriptableObject |
| **AI Tool** | Gemini |
| **기타** | Coroutine, NavMesh, Cinemachine |

---

## 🎯 담당 파트 (본인 기여도: 약 60%)

### 1️⃣ 오디오 시스템 설계
- **[`AudioManager.cs`](Script/01.Manager/AudioManager.cs)**: 싱글톤 기반 BGM/SFX 제어, 씬별 BGM 자동 전환
- **[`SpatialAudioManager.cs`](Script/01.Manager/SpatialAudioManager.cs)**: 32개 AudioSource 풀 관리, 거리 기반 우선순위 재생

```csharp
// 핵심 로직: 거리 기반 우선순위 재생
var sortedSounds = virtualSounds.OrderBy(
    sound => Vector3.Distance(sound.position, listenerTransform.position)
).ToList();

foreach (VirtualSound vs in sortedSounds) {
    if (Time.time >= vs.nextPlayTime && Vector3.Distance(vs.position, listenerTransform.position) <= vs.maxDistance) {
        AudioSource availableSource = audioSourcePool.FirstOrDefault(s => !s.gameObject.activeSelf);
        if (availableSource != null) {
            availableSource.transform.position = vs.position;
            availableSource.clip = vs.clip;
            availableSource.Play();
        }
    }
}
```

### 2️⃣ 퍼즐 시스템 구현
- **[`Puzzle1.cs`](Script/04.Scene_1/Puzzle1.cs)**: 사운드 시퀀스 퍼즐, 카메라 전환, 플레이어 입력 제어
- **[`MelodyPuzzleManager.cs`](Script/05.Scene_2/MelodyPuzzleManager.cs)**: 멜로디 매칭 퍼즐, 8개 음계 시스템
- **[`PlayerLineDrawer.cs`](Script/Scene_3/PlayerLineDrawer.cs)**: 동물 소리 경로 추적, Line Renderer 시각화, Undo 기능

### 3️⃣ Firebase + IoT 연동
- **[`FirebaseCycleReceiver.cs`](Script/08.WalkScene/FirebaseCycleReceiver.cs)**: 회전 센서 데이터 실시간 리스닝, Quaternion 기반 회전 감지

```csharp
float deltaAngle = Quaternion.Angle(lastRotation, currentRotation);
accumulatedAngle += deltaAngle;

if (accumulatedAngle >= 360f) {
    RotationCount++;
    accumulatedAngle -= 360f;
    Debug.Log($"1바퀴 회전 감지! 총 회전 수: {RotationCount}");
}
```

### 4️⃣ 플레이어 제어 시스템
- **[`PlayerControlBase.cs`](Script/02.Player/PlayerControlBase.cs)**: 1인칭 이동, 점프, 수중 모드, 리스폰
- **[`CameraController.cs`](Script/02.Player/CameraController.cs)**: 마우스 감도 제어, PlayerPrefs 저장

---

## 🚀 기술적 도전과 해결

### 1️⃣ 오디오 믹서 볼륨 초기화 문제
**🔴 문제**  
씬 전환 후 BGM 볼륨이 -80dB로 초기화되어 소리가 들리지 않음.  
`AudioMixer.SetFloat()`가 즉시 적용되지 않고 프레임 지연 발생.

**✅ 해결**  
Coroutine을 활용하여 **1프레임 대기 후 볼륨 복구** 로직 구현.

```csharp
private IEnumerator Coroutine_ResetAudioMixer() {
    audioMixer.SetFloat("BGMVolume", -80f);  // 일단 뮤트
    yield return null;                       // 1프레임 대기
    SetBGMVolume(targetBgmVolume);          // 저장된 볼륨 복구
}
```

**📈 결과**: 씬 전환 시 안정적인 오디오 재생 보장

---

### 2️⃣ 5.1채널 공간 음향 출력 문제
**🔴 문제**  
Unity에서 Audio를 3D로 설정하고 Project Settings에서 5.1채널로 지정했지만,  
실제 스피커에서는 **스테레오(2채널)로만 출력**되는 문제 발생.

**✅ 해결**  
1. PC와 5.1채널 스피커 간 **오디오 인터페이스 드라이버 재설치**
2. Windows 사운드 설정에서 출력 형식을 **5.1 Surround**로 변경
3. Unity Audio Listener와 스피커 채널 매핑 확인

**📈 결과**: 5.1채널 공간 오디오 정상 출력, 몰입감 대폭 향상

---

### 3️⃣ PC-블루투스 운동기구 연동 불가 문제
**🔴 문제**  
Unity와 모바일은 Bluetooth 직접 연결이 가능하지만,  
PC(Windows)에서는 Unity 프로젝트가 블루투스 운동기구와 **직접 통신 불가**.

**✅ 해결**  
**3단계 중계 시스템** 구축:
1. **Android 앱**: 블루투스 운동기구의 회전 센서 값 수신
2. **Firebase Realtime Database**: 모바일 → Firebase 실시간 업로드
3. **Unity PC 클라이언트**: Firebase SDK로 리스닝 → 게임 내 이동으로 변환

```
Bluetooth 운동기구 → Android 앱 → Firebase → Unity PC
```

**📈 결과**: PC 환경에서도 IoT 운동 기구 완벽 연동 성공

---

## 📂 프로젝트 구조

```
SOV/
├── Scripts/
│   ├── 01.Manager/          # 핵심 매니저 시스템
│   │   ├── AudioManager.cs          # BGM/SFX 제어, 씬별 자동 전환
│   │   ├── SpatialAudioManager.cs   # 3D 사운드 풀링 시스템
│   │   ├── InteractionManager.cs    # F키 상호작용 시스템
│   │   └── LoadingManager.cs        # 씬 로딩 애니메이션
│   ├── 02.Player/           # 플레이어 제어
│   │   ├── PlayerControlBase.cs     # 이동, 점프, 수중 모드
│   │   └── CameraController.cs      # 1인칭 카메라
│   ├── 04.Scene_1/          # 퍼즐 1: 사운드 시퀀스
│   │   └── Puzzle1.cs
│   ├── 05.Scene_2/          # 퍼즐 2: 멜로디 매칭
│   │   ├── MelodyPuzzleManager.cs
│   │   ├── WaterSound.cs
│   │   └── NoteCube.cs
│   ├── Scene_3/             # 퍼즐 3: 경로 추적
│   │   ├── AnimalSoundTrailPuzzle.cs
│   │   └── PlayerLineDrawer.cs
│   └── 08.WalkScene/        # IoT 연동
│       └── FirebaseCycleReceiver.cs
└── README.md
```

---

## 🎮 플레이 방법

### 조작법
- **W/A/S/D**: 이동
- **마우스**: 시점 회전
- **F**: 상호작용
- **Space**: 점프
- **T**: AI 챗봇 열기 (힐링 존)
- **Esc**: 옵션 메뉴

### 퍼즐 공략
1. **Scene 1 - 사운드 시퀀스 퍼즐**
   - 중앙 오브젝트에 접근 → F키로 시퀀스 재생
   - 4개 기둥에서 들린 소리 순서대로 F키 누르기

2. **Scene 2 - 멜로디 매칭 퍼즐**
   - 노트 큐브에 F키로 소리 확인
   - 물 높이를 마우스로 조절하여 같은 음계 맞추기
   - 모든 물 웅덩이가 정답이면 문 열림

3. **Scene 3 - 동물 소리 경로 추적**
   - 동물 울음소리가 이동하는 경로를 기억
   - F키로 나무를 순서대로 연결 (Line으로 시각화)
   - Z키로 마지막 선택 취소 가능

---

## 🔧 빌드 및 실행

### 필수 요구사항
- Unity 6
- .NET Framework 4.x
- 5.1채널 스피커 (권장)

### 5.1채널 오디오 설정
1. **Windows 사운드 설정**
   - 제어판 → 소리 → 재생 → 스피커 → 속성
   - 고급 탭 → 5.1 Surround 선택

2. **Unity Project Settings**
   - Edit → Project Settings → Audio
   - Default Speaker Mode: **5.1 Surround**

---

**본 프로젝트는 유료 에셋을 사용함의 따라 소스코드만 공개하는 점 양해부탁드립니다.**

**전체 프로젝트 실행을 원하실 경우 [이메일](email:ksjun0541@gmail.com)로 문의 부탁드립니다.**

---

## 🤝 기여자

| 이름 | 역할 | 담당 |
|------|------|------|
| **김승준** | Lead Developer | 오디오 시스템, 퍼즐, IoT 연동 |
| 팀원A | Developer | UI/UX, 캐릭터 제어 |
| 팀원B | Designer | 3D 모델링, 레벨 디자인 |
| 팀원C | Designer | 사운드 디자인, BGM 제작 |

---

## 📧 Contact

- **Name**: 김승준 (Kim Seung-jun)
- **Email**: [ksjun0541@gmail.com](mailto:ksjun0541@gmail.com)
- **GitHub**: [@SeungJun751](https://github.com/SeungJun751)
- **Portfolio**: [https://seungjun751.github.io](https://seungjun751.github.io)

---

<p align="center">
  <strong>⭐ 이 프로젝트가 도움이 되었다면 Star를 눌러주세요! ⭐</strong>
</p>
