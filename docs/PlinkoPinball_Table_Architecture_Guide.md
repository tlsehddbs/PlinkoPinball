# PlinkoPinball 테크니컬 아키텍처 가이드

## 1. 문서 목적

이 문서는 PlinkoPinball 프로젝트에서 **핀볼 테이블 상호작용 시스템의
공식 아키텍처 규칙**을 정의한다.

목표:

• 테이블 요소의 **재사용 가능한 구조** 구축\
• 게임 규칙의 **중앙 집중 처리**\
• 연출과 물리 반응의 **오브젝트 단위 조립**\
• 테이블 모듈의 **랜덤 교체 및 확장 안정성 확보**\
• 디버깅과 밸런싱의 **데이터 기반 운영**

이 문서는 다음 요소를 구현할 때 반드시 참고해야 한다.

- 테이블 트리거
- 범퍼 / 레인 / 타겟 / 스쿱
- 스위치 시스템
- 모듈 시스템
- 점수 / 시간 / 게이트 시스템
- TableEvent

이 문서와 다른 방식으로 구현해야 할 경우 반드시 문서화가 필요하다.

---

## 2. 전체 아키텍처 개요

테이블 상호작용 시스템은 다음 **5개 레이어**로 구성된다.

Trigger → Event → Reaction → System → Module

각 레이어의 책임은 명확히 분리되어야 한다.

|레이어|역할|
|---|---|
|Trigger|물리 상호작용 감지|
|Event|발생 사실 데이터|
|Reaction|로컬 오브젝트 반응|
|System|전역 게임 규칙|
|Module|테이블 모듈 상태 제공|

이 구조를 지키면 **시스템 결합도가 낮아지고, 확장성이 높아지며, 디버깅이 쉬워진다.**

---

## 3. 이벤트 흐름

모든 테이블 상호작용은 다음 흐름을 따른다.

Ball Interaction\
↓\
Trigger 감지\
↓\
TableEvent 생성\
↓\
TableEventBus.Publish\
↓\
System 처리 (점수 / 시간 / 게이트)\
↓\
Local Reaction 실행

이 구조는 **게임 규칙과 오브젝트 연출을 완전히 분리**한다.

---

## 4. Trigger 레이어

Trigger는 공과 테이블 요소의 상호작용을 감지한다.

Trigger의 책임:

- 충돌 또는 트리거 감지
- TableEvent 데이터 생성
- EventBus에 이벤트 발행
- 로컬 Reaction 호출

Trigger가 하면 안 되는 것:

- 점수 계산
- 타이머 변경
- 보너스 시작
- 게이트 개방

Trigger는 **관찰자(observer)** 역할만 수행해야 한다.

---

## 5. Trigger 종류

### BallHitTrigger

공과 물리 충돌을 감지한다.

사용 예:

- 범퍼
- 타겟
- 핀
- 슬링샷

Unity Callback

OnCollisionEnter

EventType

Hit

---

### BallPassTrigger

공이 특정 경로를 통과할 때 감지한다.

사용 예:

- 인레인 / 아웃레인
- 오빗
- 램프

Unity Callback

OnTriggerEnter

EventType

Pass

---

### BallZoneTrigger

특정 영역 진입 / 이탈을 감지한다.

사용 예:

- 스쿱
- 텔레포트
- 보너스룸 입구

Unity Callback

OnTriggerEnter / OnTriggerExit

EventType

Zone

---

## 6. TableEvent 구조

TableEvent는 다음 데이터를 가진다.

eventId : string\
eventType : Hit \| Pass \| Zone\
baseValue : int\
tags : string\[\]\
source : Transform\
position : Vector3\
time : float\
ball : Rigidbody

예시 eventId

hit.bumper.small\
pass.orbit.right\
zone.enter.wormhole

eventId는 **프리팹 이름이 아니라 이벤트 의미를 표현**해야 한다.

---

## 7. EventBus

TableEventBus는 모든 이벤트를 전달하는 중앙 시스템이다.

사용 예:

TableEventBus.Publish(in e)

규칙:

• System은 OnEnable에서 구독\
• OnDisable에서 해제\
• Publish는 GC 발생을 최소화해야 한다

---

## 8. Reaction 레이어

Reaction은 **오브젝트 단위 반응**을 정의한다.

예:

ImpulseOnHit\
FlashOnEvent\
SfxOnEvent\
AnimatorOnEvent\
SwitchState

Trigger는 Awake에서 Reaction 목록을 캐싱해야 한다.

---

## 9. Switch 시스템

Switch는 **테이블 상태 요소**이다.

예:

- 타겟 뱅크
- 보너스 라이트
- 레인 상태

Switch 특징:

• On / Off 상태 유지\
• Hit 이벤트로 상태 변경\
• 이벤트를 추가로 발행하지 않음

Switch는 **상태 저장자(state container)** 역할만 수행한다.

---

## 10. Module 시스템

Module은 교체 가능한 테이블 파트이다.

모든 모듈 프리팹은 **ModuleRoot 컴포넌트**를 가져야 한다.

ModuleRoot 역할:

• SwitchState 수집\
• 스위치 그룹 관리\
• 점수 multiplier 제공\
• 모듈 상태 리셋

예시 구조

ModuleRoot\
├ Bumper\
│ └ HitTrigger\
├ TargetBank\
│ └ SwitchState\
└ Lane\
└ PassTrigger

---

## 11. Module 탐색

이벤트 발생 시 해당 모듈을 찾아야 한다.

방법:

source.GetComponentInParent`<ModuleRoot>`{=html}()

이 결과는 캐싱해야 한다.

예:

Dictionary\<Transform, ModuleRoot\>

모듈이 없으면 multiplier는 1이다.

---

## 12. 점수 처리 파이프라인

ScoreSystem 처리 흐름

TableEvent 수신\
↓\
tags에 score 포함 확인\
↓\
ModuleRoot 조회\
↓\
multiplier 조회\
↓\
최종 점수 계산

점수 공식

Score += baseValue × moduleMultiplier × contextBonus

---

## 13. 성능 가이드

Trigger는 Reaction을 Awake에서 캐싱한다.

게임 플레이 중에는 GetComponents 호출 금지.

공 판별은 tag 대신 **Layer 기반**으로 수행한다.

---

## 14. 디버깅 도구

추천 디버그 컴포넌트

GlobalEventLogger\
ReactionLogger\
ModuleDebugger

이 도구들은 이벤트 흐름과 모듈 상태를 시각화한다.

---

## 15. 네임스페이스 구조

PlinkoPinball.Core.TableEvents\
PlinkoPinball.Gameplay.Components.Triggers\
PlinkoPinball.Gameplay.Components.Reactions\
PlinkoPinball.Gameplay.Modules\
PlinkoPinball.Gameplay.Systems\
PlinkoPinball.Debugging

이 구조는 Gameplay 시스템을 명확히 분리한다.

---

## 16. 아키텍처 보장

이 구조를 유지하면 다음이 보장된다.

• 테이블 요소 모듈화\
• 모듈 랜덤 교체 안정성\
• Switch 상태 결정성\
• 단순한 이벤트 흐름\
• 독립적인 시스템

이는 다음 확장에 안전하다.

- 모듈 Tier 시스템
- 업그레이드 시스템
- 보너스룸
- 고급 점수 시스템
- 멀티볼
