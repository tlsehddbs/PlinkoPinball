# PlinkoPinball

> **네온 픽셀 스타일의 3D 핀볼 스코어 어택 게임**  
> 모듈을 조합하고, 업그레이드를 선택하고, 라운드가 거듭될수록 더 위험한 배수 구조로 점수를 폭발시키는  
> **로그라이크 감성의 기록 갱신형 핀볼**

![Status](https://img.shields.io/badge/status-in%20development-2563eb?style=for-the-badge)
![Version](https://img.shields.io/badge/version-v0.2-7c3aed?style=for-the-badge)
![Engine](https://img.shields.io/badge/engine-Unity-111827?style=for-the-badge)
![Language](https://img.shields.io/badge/language-C%23-059669?style=for-the-badge)

---

! 이 문서는 기획 문서를 ChatGPT를 사용하여 GitHub Readme.md 스타일로 정리하여 생성한 문서입니다.

---

## About

**PlinkoPinball**은 클래식 핀볼의 손맛 위에  
**랜덤 모듈 구성**, **업그레이드 선택**, **위험-보상 구조**, **점수 폭증의 쾌감**을 더한 게임입니다.

플레이어는 매 라운드마다 테이블 위 모듈을 활용해 점수 기대값을 키우고,  
더 큰 리스크를 감수하며 기록 갱신을 노리게 됩니다.

이 프로젝트는 단순한 핀볼이 아니라 다음 경험을 동시에 목표로 합니다.

- 순간 판단이 필요한 **액션성**
- 빌드 완성도를 높이는 **전략성**
- 배수와 조건이 꼬일수록 터지는 **도박적 쾌감**

---

## Core Fantasy

PlinkoPinball이 주는 핵심 경험은 아래와 같습니다.

- **"이번 런은 뭔가 터질 것 같은데?"**
- **"조금만 더 욕심내면 기록을 깰 수 있을 것 같은데?"**
- **"위험한 모듈 조합이 오히려 대박을 만든다"**

즉, 이 게임은  
**정교한 핀볼 조작** 위에 **불확실한 기대값의 재미**를 얹는 프로젝트입니다.

---

## Key Features

### 점수가 폭발하는 핀볼

단순히 공을 오래 살리는 게임이 아니라,  
**배수**, **모듈 조건**, **상황 보너스**가 겹치며 점수가 커지는 구조를 지향합니다.

### 라운드마다 달라지는 모듈 구성

테이블은 고정된 맵이 아니라,  
**모듈 슬롯 기반 구조**를 통해 매 플레이마다 다른 흐름을 만들 수 있습니다.

### 전략적인 업그레이드 선택

업그레이드는 단순 수치 증가가 아니라,  
현재 런의 모듈 구성과 시너지를 만드는 방향으로 작동합니다.

### 공정한 도박성

운빨만으로 결정되지 않되,  
**욕심낼수록 기대값이 출렁이는 구조**를 통해 긴장감을 만듭니다.

### 명확한 피드백

무엇이 켜졌고, 어떤 배수가 적용됐고, 왜 점수가 커졌는지  
플레이어가 즉시 체감할 수 있어야 합니다.

---

## Gameplay Loop

```text
모듈 장착
  ↓
런치
  ↓
Hit / Pass / Zone 발생
  ↓
점수 / 게이트 / 시간 처리
  ↓
라운드 종료
  ↓
업그레이드 선택
  ↓
다음 라운드
```

한 라운드 안에서는 핀볼 액션의 긴장감이,  
라운드 바깥에서는 빌드 완성의 재미가 작동합니다.

---

## Architecture Philosophy

이 프로젝트의 시스템 설계는  
**단순한 이벤트 흐름**과 **명확한 책임 분리**를 중심으로 구성됩니다.

### 1. 이벤트는 단순하게

테이블 이벤트는 오직 3종만 사용합니다.

- `Hit`
- `Pass`
- `Zone`

이벤트 종류를 최소화해 시스템 전체를 이해하기 쉽고,  
확장 가능하게 유지합니다.

### 2. Trigger는 사실만 발행

Trigger는 해석하지 않습니다.  
그저 **"무슨 일이 일어났는가"** 만 전달합니다.

룰과 점수 계산은 전부 상위 시스템이 담당합니다.

### 3. Module은 룰 제공자

Module은 이벤트를 다시 만들지 않습니다.  
자신의 상태를 바탕으로 배수와 조건 충족 여부를 계산해 제공합니다.

즉, Module은 판정자가 아니라 **Modifier Provider**입니다.

### 4. ScoreSystem은 최종 적용자

모든 점수 계산은 중앙의 `ScoreSystem`이 수행합니다.

이 구조 덕분에 다음 장점이 생깁니다.

- 룰 해석 위치가 명확함
- 디버깅이 쉬움
- 모듈 교체와 확장이 안전함

---

## Event Model

### `TableEvent`

```csharp
public struct TableEvent
{
    public string eventId;
    public TableEventType eventType; // Hit | Pass | Zone
    public int baseValue;
    public string[] tags;
    public Transform source;
    public Vector3 position;
}
```

### Event Flow

```text
Trigger
  ↓
TableEventBus.Publish
  ↓
Global Systems
(Score / Gate / Time)
+ Local Reactions
```

이 구조의 핵심은  
**이벤트 스트림을 작고 예측 가능하게 유지하는 것**입니다.

---

## Switch Design

기존 구조에서 Switch는 복잡한 이벤트 요소가 아니라,  
이제 아주 단순한 **로컬 상태 오브젝트**입니다.

### `SwitchState`

```csharp
public class SwitchState : MonoBehaviour
{
    public bool IsOn;
    public string groupId;

    public void Toggle() { ... }
}
```

### 동작 원칙

- `HitTrigger`가 로컬 반응 전달
- `SwitchState`가 상태 변경
- 추가 이벤트는 발행하지 않음

즉, Switch는 상태만 들고 있고,  
그 상태를 해석하는 것은 `ModuleRoot`입니다.

---

## Module System

`ModuleRoot`는 모듈 단위 시스템의 중심입니다.

### 역할

- 하위 `SwitchState` 자동 수집
- 그룹 관리
- 배수 계산
- 조건 충족 판정
- `Reset` 처리

### 수집 전략

```csharp
GetComponentsInChildren<SwitchState>(true);
```

`Awake` 시점에 1회만 수집하고 캐싱하여  
런타임 탐색 비용을 피합니다.

### 왜 중요한가?

이 구조 덕분에 모듈은

- 독립적으로 교체 가능하고
- 슬롯 시스템에 자연스럽게 결합되며
- 빌드 다양성을 만들기 쉬워집니다

---

## Score Calculation

점수 계산은 다음 흐름으로 정리됩니다.

```text
Score += baseValue × ModuleMultiplier × ContextBonus
```

### 역할 분리

- `Trigger` → 사실 전달
- `ModuleRoot` → multiplier 제공
- `ScoreSystem` → 최종 계산 및 적용

이 방식은 시스템이 커질수록 장점이 커집니다.  
특히 **이벤트 폭증 방지**, **책임 분리**, **확장 안정성** 면에서 강합니다.

---

## Module Slot System

PlinkoPinball의 테이블은 고정형이 아니라  
**모듈 슬롯 기반 구조**를 전제로 합니다.

### 특징

- 특정 위치에 모듈 프리팹 장착
- 런마다 랜덤 구성 가능
- 모듈 교체에 안전한 구조 유지

### 구조 원칙

- `ModuleRoot`는 프리팹 최상위에 위치
- 교체 시에도 내부 구조가 자연스럽게 유지됨

---

## Upgrade Direction

업그레이드는 이벤트 구조를 직접 흔들기보다,  
`ModuleRoot`가 제공하는 값에 영향을 주는 방향으로 설계됩니다.

예시:

- `ModuleControl`
- `MultiplierEngine`

이렇게 하면 업그레이드가 난잡해지지 않고,  
모듈과 점수 구조 위에서 자연스럽게 시너지를 만들 수 있습니다.

---

## Tech Stack

현재 프로젝트 기준으로 예상 / 사용 중인 기술 스택입니다.

- **Engine**: Unity
- **Language**: C#
- **Physics**: Unity Physics
- **Rendering**: 3D + Neon Pixel Style
- **Architecture**: Event Bus + ModuleRoot 중심 구조
- **Data Handling**: ScriptableObject 기반 확장 고려
- **Version Control**: Git + GitHub

> 실제 사용 버전에 맞춰 Unity Version, URP/HDRP, Input System 등은 추후 구체화할 수 있습니다.

---

## Project Structure

```text
PlinkoPinball/
├─ Assets/
│  ├─ Animations/
│  ├─ Materials/
│  ├─ Textures/
│  ├─ Audios/
│  │  ├─ BGM
│  │  └─ SFX
│  ├─ Prefabs/
│  │  ├─ Modules/
│  │  ├─ Table/
│  │  └─ UI/
│  ├─ Scenes/
│  │  ├─ Bootstrap/
│  │  ├─ Gameplay/
│  │  └─ Testbeds/
│  ├─ Scripts/
│  │  ├─ Core/
│  │  │  ├─ Input/
│  │  │  ├─ Services/
│  │  │  └─ TableEvents/
│  │  ├─ Debugging/
│  │  ├─ Gameplay/
│  │  │  ├─ Components/
│  │  │  │  ├─ Reactions/
│  │  │  │  └─ Triggers/
│  │  ├─ Modules/
│  │  ├─ Score/
│  │  ├─ Systems/
│  │  ├─ UI/
│  │  └─ Utilities/
│  └─ Resources/
├─ Plugins/
├─ Settings/
├─ Packages/
├─ ProjectSettings/
└─ README.md
```

### 구조 설명

- **Core**: 게임 전반에서 공통으로 쓰는 핵심 로직
- **Events**: `TableEvent`, `EventBus`, 이벤트 관련 타입
- **Modules**: `ModuleRoot`, `SwitchState`, 모듈별 규칙
- **Score**: 점수 계산, 배수 적용, 스코어 반영
- **Systems**: Gate, Time, Round, Upgrade 등 글로벌 시스템
- **ScriptableObjects**: 데이터 중심 설계 확장을 위한 에셋 폴더

---

## Development Notes

이 프로젝트는 다음 방향을 중요하게 봅니다.

- **직관적인 액션**
- **명확한 시스템 책임 분리**
- **모듈 기반 확장성**
- **반복 플레이에서 달라지는 기대값**
- **기록 갱신 욕구를 자극하는 점수 구조**

---

## Roadmap

- [x] 기본 핀볼 테이블 프로토타입 구축
- [x] `Hit / Pass / Zone` 이벤트 흐름 정리
- [ ] `ModuleRoot` 기반 모듈 시스템 구현
- [ ] 점수 배수 및 컨텍스트 보너스 구조 구현
- [ ] 업그레이드 선택 루프 구현
- [ ] 라운드 진행 및 웜홀 종료 구조 구현
- [ ] 네온 픽셀 스타일 비주얼 정리
- [ ] 기록 갱신 중심 밸런싱

---

## License

라이선스는 추후 결정 예정입니다.

<!--
추천:
MIT / Apache-2.0 / Proprietary 중 프로젝트 방향에 맞게 선택
-->
