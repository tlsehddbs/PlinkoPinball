
# PlinkoPinball 테이블 게임플레이 아키텍처 가이드

이 문서는 **PlinkoPinball의 테이블 상호작용 시스템 구조**를 설명한다.

---

이 문서는 기획문서를 ChatGPT를 통해 생성한 문서입니다. 추후 문서 정리와 함께 다른 파일과 병합될 수 있습니다. 

---

대상:

- 테이블 요소 구현
- 트리거 개발
- 모듈 시스템
- 점수 시스템
- 스위치 시스템
- 보너스 시스템

목표:

- 책임 분리
- 확장 가능한 구조
- 안정적인 이벤트 처리

---

# 1. 핵심 철학

PlinkoPinball 테이블 아키텍처는 다음 원칙을 따른다.

1. **Trigger는 사실만 감지한다**
2. **Event는 발생 사실을 전달한다**
3. **Global System은 규칙을 적용한다**
4. **Reaction은 로컬 연출/물리를 처리한다**
5. **Switch는 상태만 저장한다**
6. **Module은 상태를 해석한다**
7. **ScoreSystem은 최종 점수를 계산한다**

핵심 흐름:

```
Trigger → TableEvent → EventBus
                         ↓
                     Global Systems
                         ↓
                     Local Reactions
                         ↓
                   Local State Change
                         ↓
                다음 이벤트에 반영
```

---

# 2. 이벤트 파이프라인

모든 테이블 상호작용은 동일한 흐름을 따른다.

```
Ball Interaction
        ↓
Trigger 감지
        ↓
TableEvent 생성
        ↓
TableEventBus.Publish()
        ↓
Global Systems 처리
        ↓
Trigger가 Local Reaction 호출
        ↓
Switch 등 로컬 상태 변화
        ↓
다음 이벤트부터 영향 반영
```

중요 규칙:

> 전역 시스템은 항상 **Local Reaction보다 먼저 실행된다**.

이렇게 해야 점수 계산 기준이 항상 일정하다.

---

# 3. Trigger 종류

현재 시스템에는 3가지 트리거가 있다.

```
Hit
Pass
Zone
```

각각 다른 역할을 가진다.

---

# 3.1 Hit Trigger

가장 중요한 트리거.

사용 대상:

- 범퍼
- 핀
- 타겟
- 대부분의 모듈 요소

실행 흐름:

```
BallHitTrigger
   ↓
TableEvent 생성 (Hit)
   ↓
Event Publish
   ↓
ScoreSystem
GateSystem
TimeSystem
   ↓
Local Reaction
   ├ Impulse
   ├ Flash
   ├ SFX
   └ SwitchState Toggle
```

핵심 규칙:

> Hit 점수 계산은 **기존 스위치 상태 기준**으로 이루어진다.

스위치 토글은 **다음 이벤트부터 반영된다**.

---

# 3.2 Pass Trigger

경로 성공을 의미하는 이벤트.

예:

- Lane
- Orbit
- Ramp

실행 흐름:

```
BallPassTrigger
   ↓
TableEvent 생성 (Pass)
   ↓
Event Publish
   ↓
ScoreSystem
GateSystem
TimeSystem
   ↓
Local Reaction (연출)
```

Pass 이벤트는 보통:

- 점수
- 게이트 충전
- 시간 보너스

를 제공한다.

Switch 입력으로는 사용하지 않는다.

---

# 3.3 Zone Trigger

상태 전환 이벤트.

예:

- Scoop
- Wormhole
- Teleport
- BonusRoom Entry

실행 흐름:

```
BallZoneTrigger
   ↓
TableEvent 생성 (Zone)
   ↓
Event Publish
   ↓
ModeSystem
GateSystem
TimeSystem
   ↓
Local Reaction
   ├ Kicker
   ├ Animation
   └ Effects
```

Zone 이벤트는 **전역 판정이 먼저 수행된다.**

---

# 4. Switch 시스템

Switch는 **테이블 상태 요소**이다.

예:

- Target Bank
- Bonus Light
- Progress Indicator

역할:

```
Hit 입력 수신
↓
ON/OFF 상태 변경
↓
상태 유지
```

Switch는 **새로운 이벤트를 발행하지 않는다.**

---

# 4.1 Switch 입력

Switch는 Hit Trigger의 로컬 반응으로 동작한다.

```
HitTrigger
    ↓
NotifyLocal()
    ↓
SwitchState.OnTableEvent()
    ↓
Switch Toggle
```

---

# 4.2 Switch 적용 시점

Switch 상태 변경은 **Local Reaction 단계에서 발생한다.**

```
Publish Event
↓
ScoreSystem 처리
↓
Switch Toggle
```

따라서

> 새 Switch 상태는 **다음 이벤트부터 점수 계산에 반영된다.**

---

# 5. Module 시스템

Module은 **교체 가능한 테이블 프리팹 묶음**이다.

예:

```
ModuleRoot
   ├ Target
   │   ├ HitTrigger
   │   └ SwitchState
   ├ Bumper
   │   └ HitTrigger
   └ PinCluster
       └ HitTrigger
```

---

# 5.1 ModuleRoot 역할

ModuleRoot는 **상태 해석자**이다.

책임:

- Switch 수집
- Switch 그룹 관리
- 점수 multiplier 계산
- 모듈 상태 초기화

---

# 5.2 Switch 캐싱

모듈이 생성될 때

```
ModuleRoot.Awake()
```

에서 Switch를 수집한다.

```
GetComponentsInChildren<SwitchState>()
```

예:

```
switches = [S1,S2,S3]

groups["bankA"] = [S1,S2,S3]
```

---

# 5.3 Switch 상태 사용

ModuleRoot는 Switch 값을 복사하지 않는다.

대신 **참조를 보관**한다.

```
switchRef.IsOn
```

따라서 항상 최신 상태를 읽는다.

---

# 5.4 Score Multiplier

Module은 점수 multiplier를 제공한다.

```
GetScoreMultiplier()
```

예:

```
baseMultiplier = 1.0

if bankA complete
    +0.5

if special target active
    +0.3
```

---

# 6. ScoreSystem 구조

ScoreSystem은 **최종 점수 계산기**이다.

게임 상태를 직접 관리하지 않는다.

대신 다른 시스템에서 modifier를 받아 계산한다.

---

# 6.1 점수 계산 흐름

```
TableEvent
   ↓
ScoreSystem
   ↓
BaseValue
   ↓
Module Modifier
   ↓
Global Context Modifier
   ↓
Final Score
```

---

# 6.2 점수 공식

```
Score += baseValue
       × moduleMultiplier
       × contextBonus
```

---

# 6.3 Modifier 계층

### BaseValue

Trigger에서 정의

```
baseValue
```

---

### Module Modifier

ModuleRoot 제공

```
GetScoreMultiplier()
```

---

### Context Modifier

게임 전체 상태

예:

- Critical Time
- Bonus Mode
- Upgrade
- Special Event

---

# 7. 이벤트 흐름 예시

## Target Hit

```
Ball
 ↓
HitTrigger
 ↓
TableEvent(Hit)
 ↓
Publish
 ↓
ScoreSystem
    baseValue = 10
    moduleMultiplier = 1.2
    contextBonus = 1.0
 ↓
Score = 12
 ↓
Local Reaction
 ↓
Switch Toggle
 ↓
Bank progress 업데이트
```

Multiplier 변화는 **다음 이벤트부터 적용**된다.

---

## Orbit Pass

```
Ball
 ↓
PassTrigger
 ↓
TableEvent(Pass)
 ↓
ScoreSystem
 ↓
GateSystem
 ↓
Local Reaction
```

---

## Wormhole Entry

```
Ball
 ↓
ZoneTrigger
 ↓
TableEvent(Zone)
 ↓
ModeSystem
 ↓
Bonus Room 진입
 ↓
Local Reaction
```

---

# 8. Module 설계 가이드

Module은 다양한 역할을 가질 수 있다.

---

## Score Module

예:

- Pin Cluster
- Bumper Group

특징:

```
많은 Hit 이벤트
작은 점수 누적
```

---

## Setup Module

예:

- Target Bank
- Drop Target

특징:

```
Switch 기반 진행
Multiplier 조건 생성
```

---

## Path Module

예:

- Orbit Helper
- Lane Modifier

특징:

```
Pass 보상 증가
Gate 충전
Time bonus 제공
```

---

## Risk Module

예:

- Dangerous Lane
- Fast Ramp

특징:

```
높은 보상
높은 위험
```

---

# 9. 전체 시스템 구조

```
Ball
 ↓
Trigger (Hit / Pass / Zone)
 ↓
TableEvent
 ↓
EventBus
 ↓
Global Systems
   ├ ScoreSystem
   ├ GateSystem
   ├ TimeSystem
   └ ModeSystem
 ↓
Local Reactions
   ├ Impulse
   ├ Flash
   ├ SFX
   └ SwitchState
 ↓
Switch State 변경
 ↓
ModuleRoot가 새 상태 읽음
 ↓
다음 이벤트에 multiplier 반영
```

---

# 10. 게임플레이 패턴

PlinkoPinball 점수 흐름:

```
Hit → 상태 준비
Switch → 보상 조건 생성
Pass / Zone → 보상 수확
Score Explosion
```

이 구조는 플레이를 명확하게 만들면서도  
모듈과 업그레이드를 통해 전략성을 제공한다.
