# PlinkoPinball

> **Retro Analog Mainframe Pinball × Plinko Roguelite**  
> 제한된 전력이 꺼지기 전까지 거대한 메인보드에서 회로 기물을 활성화하고,  
> Compression Progress를 누적해 Plinko 계산 보드에서 보상을 해소하는  
> **하이브리드 아케이드 빌드 성장형 게임**

![Status](https://img.shields.io/badge/status-in%20development-2563eb?style=for-the-badge)
![Version](https://img.shields.io/badge/version-v0.3-7c3aed?style=for-the-badge)
![Engine](https://img.shields.io/badge/engine-Unity-111827?style=for-the-badge)
![Language](https://img.shields.io/badge/language-C%23-059669?style=for-the-badge)
![Platform](https://img.shields.io/badge/platform-PC%20First-f59e0b?style=for-the-badge)

---

## Table of Contents

- [Overview](#overview)
- [Core Fantasy](#core-fantasy)
- [Design Pillars](#design-pillars)
- [Game Design Spec](#game-design-spec)
  - [Core Loop](#core-loop)
  - [Pinball Phase](#pinball-phase)
  - [Packet Compression System](#packet-compression-system)
  - [Chipset Module System](#chipset-module-system)
  - [Plinko Phase](#plinko-phase)
  - [Error Pin System](#error-pin-system)
  - [Meta Progression](#meta-progression)
- [Theme & Naming Spec](#theme--naming-spec)
- [Gameplay Data Flow](#gameplay-data-flow)
- [Technical Architecture](#technical-architecture)
  - [Layer Overview](#layer-overview)
  - [Trigger Layer](#trigger-layer)
  - [TableEvent](#tableevent)
  - [EventBus](#eventbus)
  - [Global Systems](#global-systems)
  - [Local Reactions](#local-reactions)
  - [Module / Chipset Architecture](#module--chipset-architecture)
  - [Score Pipeline](#score-pipeline)
  - [Performance Rules](#performance-rules)
- [Project Structure](#project-structure)
- [Roadmap](#roadmap)
- [License](#license)

---

## Overview

**PlinkoPinball**은 클래식 핀볼의 물리적 손맛 위에  
**Plinko 보상 페이즈**, **Packet Compression**, **Chipset Module**, **Error Pin**, **영구 성장 구조**를 결합한 게임입니다.

플레이어는 핀볼 라운드 중 최종 보상을 직접 받지 않습니다.  
대신 제한된 전력이 꺼지기 전까지 회로 기물을 활성화해 Plinko 보상 해소를 위한 준비값을 누적합니다.

라운드 종료 후에는 그 결과가 **Reward Snapshot**으로 고정되고,  
별도 **Plinko Phase**에서 실제 Currency 보상이 정산됩니다.

---

## Core Fantasy

플레이어는 거대한 **Retro Analog Mainframe**의 운영자입니다.

핀볼 테이블은 거대한 메인보드이며,  
공은 회로 위를 흐르는 **Pulse Core**입니다.

플레이어는 제한된 **Power Reserve**가 꺼지기 전까지:

- Capacitor를 충전하고
- MOSFET을 스위칭하고
- Data Bus를 통과시키고
- Signal Bridge를 연결하고
- Fan을 회전시켜 열과 흐름을 제어하며
- Chipset 효과를 누적합니다.

이 과정은 최종 보상이 아니라 **연산 준비**입니다.

실제 보상은 라운드 종료 후 Plinko 계산 보드에서 해소됩니다.

---

## Design Pillars

### 1. Pinball is Preparation

핀볼은 최종 보상 정산 페이즈가 아닙니다.

핀볼은 다음을 담당합니다.

- Throughput 획득
- Compression Progress 누적
- Plinko Start Ball 확보
- Chipset 효과 축적
- Reward Snapshot 준비

---

### 2. Plinko is Resolution

Plinko는 준비된 보상을 실제로 해소하는 별도 페이즈입니다.

핀볼 중 Plinko Board 상태를 직접 변경하지 않습니다.

---

### 3. Score is Not Currency

Score는 영구 성장 재화가 아닙니다.

테마상 Score는 **Throughput**이며, 다음 역할만 가집니다.

- 즉각적 피드백
- 액션 도파민
- 런 퍼포먼스 측정

---

### 4. Growth Comes from Currency

실제 성장은 Plinko 결과로 획득한 Currency를 통해 이루어집니다.

---

### 5. Keep the Architecture Event-Driven

모든 테이블 상호작용은 다음 흐름을 유지합니다.

```text
Trigger
↓
TableEvent
↓
Global System
↓
Local Reaction
```

---

# Game Design Spec

## Core Loop

```text
Power On
↓
Pinball Round
↓
Throughput 획득
↓
Compression Progress 누적
↓
Chipset 효과 축적
↓
Power Shutdown
↓
Reward Snapshot 생성
↓
Plinko Phase
↓
Currency 획득
↓
Permanent Upgrade
↓
Next Round / Reboot
```

---

## Pinball Phase

### Objective

핀볼 라운드의 핵심 목표는 **전원이 꺼지기 전까지 최대한 많은 Plinko 기대값을 준비하는 것**입니다.

플레이어는 다음을 수행합니다.

- Power Reserve 유지
- Throughput 획득
- Compression Progress 누적
- Plinko 시작 Ball 확보
- Chipset 효과 누적
- Plinko Snapshot 품질 향상

---

## Time System

### Time = Power Reserve

기존 Time은 테마상 **Power Reserve**입니다.

- 지속적으로 감소
- Critical 상태 존재
- 0이 되면 Pinball Phase 종료
- 전력 고갈은 시스템 Shutdown으로 해석

---

## Score System

### Score = Throughput

점수는 현재 런의 처리량입니다.

- 보상 재화가 아님
- 영구 성장에 직접 사용하지 않음
- HUD 피드백과 런 퍼포먼스 측정용

---

## Packet Compression System

기물 Hit이 즉시 Plinko Ball을 생성하지 않습니다.

대신 Hit은 **Compression Progress**를 증가시킵니다.

```text
Table Object Hit
↓
Compression Progress + Gain
↓
Threshold 도달
↓
Plinko Start Ball +1
↓
Progress 잔여분 유지 또는 초기화
```

### Example

초기 상태:

```text
10 hits → 1 Plinko Ball
```

업그레이드 후:

```text
3 hits → 1 Plinko Ball
```

### Purpose

Packet Compression은 Pinball 성과와 Plinko 기대값을 연결하는 핵심 브릿지입니다.

장점:

- 기물을 많이 맞춘 보상이 명확함
- Plinko Ball 인플레이션을 제어함
- 영구 업그레이드 체감이 좋음
- 밸런싱 값이 단순함

### Upgrade Hooks

- Compression Threshold 감소
- 특정 기물 Compression Gain 증가
- Critical 상태에서 Compression Gain 보정
- 특정 Chipset 활성 시 Compression 보너스

---

## Chipset Module System

기존 Module은 테마상 **Special Purpose Chipset**입니다.

Chipset은 런 중 활성화되는 회로 효과이며,  
직접 보상을 지급하지 않고 **Plinko Snapshot Modifier**를 누적합니다.

### Current Chipsets

| Chipset | Effect | Duration |
|---|---|---|
| Amplifier Chipset | Global Pin Multiplier 증가 | Current Run |
| Overclock Chipset | Global Slot Multiplier 증가 | Current Run |
| Error Correction Chipset | Error Pin 생성 확률 감소 | Current Run |
| Memory Chipset | Pin Value Bonus 증가 | Current Run |
| Processor Chipset | Slot Value Bonus 증가 | Current Run |

---

### Chipset Design Rules

Chipset 효과는 Plinko에서 단순하게 적용 가능해야 합니다.

허용되는 방향:

- Global Pin Multiplier
- Global Slot Multiplier
- Pin Value Bonus
- Slot Value Bonus
- Error Pin Rate 감소
- Additional Ball
- 단순 확률 보정

지양하는 방향:

- 복잡한 Plinko path control
- ball route 강제 조작
- slot defense
- packet sorting
- 실시간 Plinko Board 변형
- Pinball Runtime과 Plinko Runtime 직접 연결

---

## Plinko Phase

Plinko는 반드시 Pinball 종료 후 별도 페이즈로 실행됩니다.

```text
Pinball End
↓
Reward Snapshot 생성
↓
Plinko Board 생성 / 초기화
↓
Snapshot Modifier 적용
↓
Plinko Ball 드랍
↓
Pin / Slot 결과 계산
↓
Currency 지급
```

### Board Structure

- 고정형 보드
- 고정 핀 구조
- 고정 슬롯 구조

### Run Variable Elements

- Plinko Start Ball Count
- Pin Bonus
- Slot Bonus
- Global Pin Multiplier
- Global Slot Multiplier
- Pin Value Bonus
- Slot Value Bonus
- Error Pin Rate

---

## Reward Snapshot

Pinball 중 획득한 Plinko 관련 값은 즉시 적용되지 않습니다.

반드시 Snapshot으로 고정된 뒤 Plinko Phase에서 사용됩니다.

### Snapshot Data

```text
PlinkoRewardSnapshot
├─ startBallCount
├─ globalPinMultiplier
├─ globalSlotMultiplier
├─ pinValueBonus
├─ slotValueBonus
├─ errorPinRate
├─ boostedPinIds
├─ boostedSlotIds
└─ runSeed
```

### Snapshot Rules

MUST:

- Pinball End 시점에 생성
- Plinko Resolver는 Snapshot만 참조
- Pinball Runtime 상태를 직접 참조하지 않음

MUST NOT:

- Pinball 중 Plinko Board를 직접 변경
- Pinball Runtime 상태를 Plinko Runtime에서 직접 조회
- Plinko 중 Chipset 상태를 다시 계산

---

## Error Pin System

Error Pin은 Plinko의 variance / risk layer입니다.

Plinko Board 생성 시 일부 Pin이 메모리 오류처럼 Error 상태가 될 수 있습니다.

### Pin Types

| Pin Type | Meaning | Effect |
|---|---|---|
| Normal Pin | 기본 회로 접점 | 기본 보상 |
| Boosted Pin | 강화 회로 접점 | Pin Bonus 적용 |
| Error Pin | 메모리 오류 핀 | 보상 감소 / multiplier 감소 |

### Error Pin Behavior

Error Pin에 Ball이 닿으면 데이터가 깨지는 느낌을 줍니다.

권장 효과:

```text
currentBallRewardMultiplier *= errorPenaltyMultiplier
```

예시:

```text
1.0x → 0.5x
```

### Error Correction Chipset

Error Correction Chipset은 Error Pin 생성 확률을 낮춥니다.

```text
finalErrorPinRate = baseErrorPinRate - errorCorrectionReduction
```

Clamp:

```text
finalErrorPinRate = Mathf.Clamp(finalErrorPinRate, minErrorPinRate, maxErrorPinRate)
```

---

## Meta Progression

Plinko 결과로 Currency를 획득합니다.

Currency는 영구 성장에 사용됩니다.

### Upgrade Categories

#### Pinball Upgrade

- Power Reserve 증가
- Actuator 반응성 증가
- 특정 기물 Throughput 증가

#### Compression Upgrade

- Compression Threshold 감소
- Compression Gain 증가
- 특정 기물 Compression 효율 증가

#### Plinko Upgrade

- Base Pin Value 증가
- Base Slot Value 증가
- 기본 Error Pin Rate 감소
- Start Ball Count 증가

#### Chipset Upgrade

- Chipset Unlock
- Chipset Tier 강화
- Chipset 효과량 증가
- Chipset 등장 조건 완화

---

## Pin / Slot Bonus Duration Rule

런 중 획득한 Chipset Bonus는 기본적으로 이번 런 한정입니다.

영구 업그레이드는 기본값을 올립니다.

예시:

```text
Memory Chipset
→ Current Run Pin Value Bonus +20%

Permanent Upgrade
→ Base Pin Value +5%
```

```text
Processor Chipset
→ Current Run Slot Value Bonus +25%

Permanent Upgrade
→ Base Slot Value +5%
```

이 구조는 런 중 빌드 감각과 장기 성장감을 분리합니다.

---

# Theme & Naming Spec

## Main Theme

**Retro Analog Mainframe**

## Sub Theme

**Reactor / Power Reserve / Mainboard Circuit**

---

## Pinball Table

핀볼 테이블은 거대한 메인보드입니다.

### Ball

| Gameplay Name | Theme Name |
|---|---|
| Ball | Pulse Core |

---

## Table Objects

| Pinball Object | Theme Name | Gameplay Role |
|---|---|---|
| Pop Bumper | Capacitor | 기본 Hit, Compression Progress 증가 |
| Target | MOSFET | 정밀 Hit, Chipset 조건 충족 |
| Drop Target | MOSFET | 순차 목표, Compression / Chipset 조건 |
| Sling | Coil | 반발, 속도 유지, 액션 피드백 |
| Orbit | Data Bus | 경로 통과, 연속 흐름, Compression Gain |
| Flipper | Actuator | 플레이어 조작 |
| Ramp | Signal Bridge | 고가치 경로, Chipset 조건 |
| Spinner | Fan | 회전 Hit, 누적 Gain, 시각적 포인트 |

---

## Plinko Board

Plinko Board는 계산/해결 보드입니다.

| Plinko Element | Theme Meaning |
|---|---|
| Ball | Resolved Pulse / Data Packet |
| Pin | Memory / Circuit Contact |
| Boosted Pin | Stabilized / Amplified Contact |
| Error Pin | Corrupted Memory Contact |
| Slot | Computation Output Port |

---

# Gameplay Data Flow

## Full Runtime Flow

```text
[Pinball Object Collision]
        ↓
[Trigger]
        ↓
[TableEvent 생성]
        ↓
[TableEventBus.Publish]
        ↓
┌───────────────────────────────┐
│ Global Systems                 │
│ - ScoreSystem                  │
│ - TimeSystem                   │
│ - CompressionSystem            │
│ - ChipsetSystem                │
│ - PlinkoRewardSystem           │
└───────────────────────────────┘
        ↓
[Local Reactions]
- Impulse
- Flash
- SFX
- Animation
- Toggle
        ↓
[Pinball End / Power Shutdown]
        ↓
[PlinkoRewardSnapshot 생성]
        ↓
[PlinkoBoardResolver]
        ↓
[CurrencyRewardSystem]
        ↓
[PermanentUpgradeSystem]
```

---

## Pinball Event Example

```text
Pulse Core hits Capacitor
↓
BallHitTrigger detects collision
↓
TableEvent {
  eventId: "hit.capacitor.small",
  eventType: Hit,
  baseValue: 100,
  tags: ["score", "compression"],
  source: Capacitor Transform,
  position: hit point,
  time: current time,
  ball: Rigidbody
}
↓
ScoreSystem
- "score" tag 확인
- Throughput 증가
↓
CompressionSystem
- "compression" tag 확인
- Compression Progress 증가
↓
Local Reaction
- Capacitor flash
- impulse
- click sfx
```

---

## Chipset Activation Example

```text
Pulse Core passes Signal Bridge
↓
TableEvent {
  eventId: "pass.signalBridge.left",
  eventType: Pass,
  tags: ["score", "chipset", "compression"]
}
↓
ChipsetSystem
- 관련 ModuleRoot / Chipset State 확인
- 조건 충족 시 Amplifier Chipset charge 증가
↓
PlinkoRewardSystem
- Global Pin Multiplier modifier 누적
↓
Snapshot에 반영될 pending reward 저장
```

---

## Compression Example

```text
Compression Threshold = 10
Current Progress = 9

Capacitor Hit
↓
Compression Gain +1
↓
Progress = 10
↓
Plinko Start Ball +1
↓
Progress = 0
```

업그레이드 후:

```text
Compression Threshold = 3
Current Progress = 2

MOSFET Hit
↓
Compression Gain +1
↓
Progress = 3
↓
Plinko Start Ball +1
```

---

## Plinko Resolution Example

```text
Snapshot:
- startBallCount = 8
- globalPinMultiplier = 1.3
- globalSlotMultiplier = 1.5
- pinValueBonus = 5
- slotValueBonus = 10
- errorPinRate = 0.08

Plinko Board 생성
↓
Error Pin 배치
↓
Boosted Pin / Slot 적용
↓
Ball 드랍
↓
Pin Hit Reward 계산
↓
Slot Reward 계산
↓
Currency 지급
```

---

# Technical Architecture

## Layer Overview

테이블 상호작용 시스템은 책임을 분리합니다.

```text
Trigger
↓
Event
↓
Global System
↓
Local Reaction
↓
Module / Chipset State
```

> 주의: 실제 구현에서는 Reaction과 System의 호출 순서를 프로젝트 코드에 맞게 유지하되,  
> 핵심 원칙은 "게임 규칙은 Global System, 연출은 Local Reaction"입니다.

---

## Trigger Layer

Trigger는 물리 상호작용을 감지합니다.

### Responsibilities

- 충돌 또는 Trigger 감지
- TableEvent 생성
- TableEventBus에 Publish
- 캐싱된 Local Reaction 호출
- ball reference 주입

### Must Not

Trigger는 절대 게임 룰을 직접 처리하지 않습니다.

금지:

- 점수 직접 수정
- 시간 직접 수정
- Module / Chipset 직접 수정
- Compression Progress 직접 수정
- Currency 지급
- Plinko Board 수정

---

## Trigger Types

### BallHitTrigger

사용 대상:

- Capacitor
- MOSFET
- Coil
- Plinko Pin

Unity Callback:

```csharp
OnCollisionEnter
```

EventType:

```csharp
Hit
```

---

### BallPassTrigger

사용 대상:

- Data Bus
- Signal Bridge
- Ramp / Orbit Route

Unity Callback:

```csharp
OnTriggerEnter
```

EventType:

```csharp
Pass
```

---

### BallZoneTrigger

사용 대상:

- Drain
- Critical Zone
- Special Area

Unity Callback:

```csharp
OnTriggerEnter / OnTriggerExit
```

EventType:

```csharp
Zone
```

---

## TableEvent

`TableEvent`는 사실 데이터 전달 전용입니다.

```csharp
public struct TableEvent
{
    public string eventId;
    public TableEventType eventType;
    public int baseValue;
    public string[] tags;
    public Transform source;
    public Vector3 position;
    public float time;
    public Rigidbody ball;
}
```

### Event ID Rule

`eventId`는 프리팹 이름이 아니라 이벤트 의미를 표현해야 합니다.

Good:

```text
hit.capacitor.small
hit.mosfet.leftBank
pass.dataBus.right
pass.signalBridge.upper
spin.fan.center
zone.drain
```

Bad:

```text
Bumper_01
Target(Clone)
NewPrefab3
```

---

## EventBus

`TableEventBus`는 모든 TableEvent를 전달하는 중앙 시스템입니다.

### Rules

- Global System은 `OnEnable`에서 구독
- `OnDisable`에서 구독 해제
- Publish는 GC 발생을 최소화
- 이벤트는 단순하고 예측 가능해야 함

Example:

```csharp
TableEventBus.Publish(in tableEvent);
```

---

## Global Systems

Global System은 TableEventBus를 구독하고 게임 규칙을 처리합니다.

### Current / Planned Systems

| System | Responsibility |
|---|---|
| ScoreSystem | Throughput 계산 |
| TimeSystem | Power Reserve 감소 / Critical 처리 |
| CompressionSystem | Compression Progress 처리 |
| ChipsetSystem | Chipset 조건 / 상태 처리 |
| PlinkoRewardSystem | Snapshot에 들어갈 보상값 누적 |
| CurrencyRewardSystem | Plinko 결과 Currency 지급 |
| UpgradeSystem | 영구 업그레이드 적용 |
| GateSystem | Gate / Route 상태 처리 |

---

## Local Reactions

Local Reaction은 동일 오브젝트 단위의 반응입니다.

예:

- Impulse
- Flash
- SFX
- Animation
- Toggle
- SwitchState

### Rules

- Reaction은 게임 규칙을 계산하지 않음
- Trigger는 Awake에서 Reaction 목록 캐싱
- 런타임 중 반복 `GetComponents` 호출 금지
- Local Reaction은 시각/물리/음향 피드백에 집중

---

## Module / Chipset Architecture

기존 Module 구조는 유지합니다.

```text
Hit
↓
Switch
↓
Module
↓
Future Event / Snapshot Modifier
```

테마상 해석:

```text
Pulse Core Interaction
↓
Switch / Route State
↓
Chipset State
↓
Plinko Snapshot Modifier
```

### Implemented Core

- SwitchState
- ModuleState
- ModuleRoot
- ModuleRootLookupCache

### ModuleRoot Responsibilities

- 하위 SwitchState 수집
- 스위치 그룹 관리
- 조건 충족 판정
- modifier 제공
- 상태 Reset
- Chipset 효과 계산을 위한 상태 제공

### Lookup Rule

이벤트 발생 시 `source`에서 ModuleRoot를 찾을 수 있습니다.

```csharp
source.GetComponentInParent<ModuleRoot>();
```

단, 결과는 반드시 캐싱합니다.

권장:

```csharp
Dictionary<Transform, ModuleRoot>
```

또는 기존:

```text
ModuleRootLookupCache
```

### Policy

Switch 변경은 현재 이벤트에 즉시 적용하지 않습니다.

```text
Event A 발생
↓
Switch State 변경
↓
Event A 점수 / 보상에는 미적용
↓
Event B부터 적용
```

이 정책은 결정성을 높이고 디버깅을 쉽게 만듭니다.

---

## Score Pipeline

ScoreSystem은 `tags`에 `"score"`가 있는 이벤트만 처리합니다.

```text
TableEvent 수신
↓
tags contains "score"
↓
ModuleRoot 조회
↓
moduleMultiplier / contextBonus 조회
↓
Throughput 계산
```

예시 공식:

```text
Throughput += baseValue × moduleMultiplier × contextBonus
```

### Non-Score Tags

예:

- timeBonus
- timePenalty
- teleport
- mode
- collect
- plinkoReward
- compression
- chipset
- warning
- critical

---

## Compression Pipeline

CompressionSystem은 `compression` 태그가 있는 이벤트를 처리합니다.

```text
TableEvent 수신
↓
tags contains "compression"
↓
Compression Gain 계산
↓
Progress 증가
↓
Threshold 확인
↓
Plinko Start Ball 증가
```

### Data-Driven Values

권장 ScriptableObject 필드:

```csharp
[CreateAssetMenu(menuName = "PlinkoPinball/Balance/Compression Settings")]
public sealed class CompressionSettings : ScriptableObject
{
    public int baseThreshold = 10;
    public int minThreshold = 3;
    public float defaultGain = 1f;
    public List<ObjectCompressionGain> objectGains;
}
```

---

## Plinko Resolver Pipeline

PlinkoBoardResolver는 Snapshot만 참조합니다.

```text
PlinkoRewardSnapshot
↓
Board Setup
↓
Error Pin Roll
↓
Boosted Pin / Slot Setup
↓
Ball Drop
↓
Pin Hit Calculation
↓
Slot Landing Calculation
↓
CurrencyRewardSystem
```

### Resolver Must Not

- Pinball Runtime 상태 조회
- ChipsetSystem 직접 조회
- ModuleRoot 직접 조회
- 실시간 테이블 상태 참조

---

## Performance Rules

- Trigger는 Reaction을 `Awake`에서 캐싱
- 플레이 중 `GetComponents` 반복 호출 금지
- ModuleRoot 탐색 결과 캐싱
- Ball 판별은 가능하면 Layer 기반
- EventBus 구독은 `OnEnable` / `OnDisable`에서 관리
- ScriptableObject로 밸런싱 값을 외부화

---

## Debugging Tools

권장 디버그 컴포넌트:

| Tool | Purpose |
|---|---|
| GlobalEventLogger | TableEvent 흐름 확인 |
| ReactionLogger | Local Reaction 실행 확인 |
| ModuleDebugger | ModuleRoot / SwitchState 상태 확인 |
| CompressionDebugger | Compression Progress / Threshold 확인 |
| SnapshotDebugger | PlinkoRewardSnapshot 값 확인 |
| PlinkoBoardDebugger | Error Pin / Boosted Pin 배치 확인 |

---

## Namespace Guide

권장 네임스페이스:

```text
PlinkoPinball.Core.TableEvents
PlinkoPinball.Gameplay.Components.Triggers
PlinkoPinball.Gameplay.Components.Reactions
PlinkoPinball.Gameplay.Modules
PlinkoPinball.Gameplay.Systems
PlinkoPinball.Gameplay.Plinko
PlinkoPinball.Gameplay.Progression
PlinkoPinball.Debugging
```

---

# Data-Driven Balancing

가능한 모든 밸런싱 값은 ScriptableObject로 이동 가능한 구조를 유지합니다.

## Balance Data Candidates

### Compression

- base threshold
- min threshold
- gain per object type
- upgrade scaling

### Chipset

- effect amount
- activation condition
- tier scaling
- duration rule

### Plinko

- base pin value
- base slot value
- error pin rate
- error penalty multiplier
- boosted pin count
- boosted slot count

### Meta Upgrade

- cost curve
- max level
- effect per level
- unlock condition

---

# Project Structure

```text
PlinkoPinball/
├─ Assets/
│  ├─ Docs/
│  │  ├─ README.md
│  │  ├─ GameDesignSpec.md
│  │  └─ PlinkoPinball_Table_Architecture_Guide.md
│  ├─ Animations/
│  ├─ Materials/
│  ├─ Textures/
│  ├─ Audio/
│  │  ├─ BGM/
│  │  └─ SFX/
│  ├─ Prefabs/
│  │  ├─ Table/
│  │  ├─ Modules/
│  │  ├─ Plinko/
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
│  │  ├─ Gameplay/
│  │  │  ├─ Components/
│  │  │  │  ├─ Triggers/
│  │  │  │  └─ Reactions/
│  │  │  ├─ Modules/
│  │  │  ├─ Systems/
│  │  │  ├─ Plinko/
│  │  │  └─ Progression/
│  │  ├─ UI/
│  │  ├─ Debugging/
│  │  └─ Utilities/
│  └─ Resources/
├─ Packages/
├─ ProjectSettings/
└─ README.md
```

---

# Roadmap

## Done

- [x] 기본 핀볼 프로토타입
- [x] TableEvent 구조
- [x] TableEventBus
- [x] BallHitTrigger
- [x] BallPassTrigger
- [x] BallZoneTrigger
- [x] ITableEventReaction
- [x] ScoreSystem
- [x] TimeSystem
- [x] HUD
- [x] InputRouter
- [x] Scene Flow
- [x] Module Core

---

## Next Priorities

- [ ] PlinkoRewardSnapshot
- [ ] CompressionSystem
- [ ] PlinkoBoardResolver
- [ ] Error Pin System
- [ ] CurrencyRewardSystem
- [ ] PermanentUpgradeSystem
- [ ] ModuleSlot
- [ ] ModuleDirector
- [ ] Module Tier System
- [ ] Data Driven Balancing
- [ ] Content Expansion

---

# Development Rules

## Must Keep

- Event 기반 구조
- Snapshot 기반 Plinko
- Trigger 책임 제한
- Module / Chipset 느슨한 결합
- Data-driven balancing
- SRP 중심 코드 구조

---

## Must Not

- Pinball 중 Plinko 진입
- Pinball에서 Currency 직접 지급
- Score를 영구 성장 재화로 사용
- Trigger에서 Score / Time / Compression 직접 수정
- Trigger에서 Module / Chipset 직접 참조
- Plinko 메타 루프 완성 전 불필요한 대형 시스템 추가
- 복잡한 Plinko path control
- 복잡한 packet sorting / routing simulation
- 과도한 VFX 중심 설계
- 전체 네온 테마화

---

# Tech Stack

- Unity
- C#
- Unity Physics
- Event Driven Architecture
- ScriptableObject Data Design
- Stylized 3D
- Pixelation Shader Pipeline
- Minimal Glow UI Effects

---

# License

추후 결정 예정입니다.

<!--
추천:
- Proprietary: 상용 출시 중심
- MIT: 오픈소스 실험 프로젝트
- Apache-2.0: 명시적 특허 조항 필요 시
-->

