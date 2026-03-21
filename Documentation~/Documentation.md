# 유니티 모듈형 행동 시스템 - 자료

---

## 1. 개요

이 문서는 유니티 모듈형 행동 시스템의 구조와 설계 철학, 그리고 실제 사용 방법을 설명합니다.

이 시스템은 게임 오브젝트의 행동을 **데이터와 로직 단위로 분리**하여 관리하는 것을 목표로 합니다.  
기존의 거대한 MonoBehaviour 중심 설계에서 벗어나, 행동을 모듈 단위로 구성하고 조합할 수 있도록 설계되었습니다.

핵심 설계 목표는 다음과 같습니다.

- 책임 분리 (Separation of Concerns)
- 높은 재사용성
- 런타임 성능 최적화 (GC 최소화)
- 인스펙터 기반 확장 가능 구조

각 행동은 다음 세 가지 구성 요소로 이루어집니다.

| 구성 요소 | 역할 |
|------------|------|
| Setting | 불변 또는 준불변 데이터 |
| Context | 런타임 상태 데이터 |
| Processor | 실제 실행 로직 |

이 구조는 데이터와 실행 흐름을 명확히 분리하여 유지보수성을 극대화합니다.

---

## 2. 자세한 사용법

---

## 2.1 패키지 설치

유니티 패키지 매니저를 통해 Git URL 방식으로 패키지를 설치합니다.

![패키지 설치 이미지](./Documentation~/스크린샷%202026-03-03%20174631.png)

```
https://github.com/Armangi1312/unity-am-modular-behavior-system.git
```

패키지 매니저에서 **Add package from Git URL**을 선택한 뒤 위 주소를 입력합니다.

---

## 2.2 행동 구성

유니티 인스펙터에서 Setting, Processor, Context를 추가하여 행동을 구성합니다.

Processor의 조합과 순서에 따라 하나의 캐릭터 동작이 완성됩니다.  
각 Processor는 독립적으로 동작하지만, Context를 공유하여 상호 작용합니다.

---

### 구성 요소 상세 설명

#### 1. Setting

- 런타임 중 변경되지 않는 데이터
- 설계 의도를 표현하는 설정 값
- 디자이너가 조정하는 튜닝 데이터

예시:
- 이동 속도
- 점프 높이
- 공격 쿨타임

Setting은 가능한 한 `private set`을 사용하여 불변성을 유지하는 것이 좋습니다.

---

#### 2. Context

- 런타임 동안 지속적으로 변경되는 상태 데이터
- Processor 간 데이터 전달 매개체
- 행동의 현재 상태 표현

예시:
- 현재 속도
- 입력 방향
- 현재 체력

Context는 상태 저장소이며, 가능한 한 단순한 데이터 구조를 유지하는 것이 성능상 유리합니다.

---

#### 3. Processor

- 실제 행동 로직 실행 단위
- 특정 InvokeTiming에 따라 호출됨
- Setting과 Context를 참조하여 동작 수행

Processor는 단일 책임 원칙을 따르는 것이 좋습니다.  
하나의 Processor는 하나의 기능만 수행하도록 설계하는 것이 유지보수에 유리합니다.

---

# 3. 코드 작성 안내

---

## 3.1 Setting 작성

```csharp
[Serializable]
public class 이름 : ISetting
{
    [field: SerializeField]
    public float Value { get; private set; }
}
```

Setting은 런타임에 변경되지 않는 데이터를 보관합니다.  
가능한 경우 `private set`을 사용하여 외부 수정 방지를 권장합니다.

---

## 3.2 Context 작성

```csharp
[Serializable]
public class 이름 : IContext
{
    [field: SerializeField]
    public float Value { get; set; }
}
```

Context는 런타임 상태를 저장합니다.  
성능이 중요한 경우 자동 프로퍼티 대신 일반 필드를 사용하는 것도 고려할 수 있습니다.

---

## 3.3 Processor 작성

```csharp
[Serializable]
[RequireSetting(typeof(-))]
[RequireContext(typeof(-))]
public class 이름 
    : Processor<필요한Setting인터페이스, 필요한Context인터페이스>
{
    public override void Initialize(
        Registry<필요한Setting인터페이스> settingRegistry,
        Registry<필요한Context인터페이스> contextRegistry)
    {
        var setting = settingRegistry.Get<필요한Setting인터페이스>();
        var context = contextRegistry.Get<필요한Context인터페이스>();
    }

    public override void Process()
    {
    }
}
```

### RequireSetting / RequireContext

이 어트리뷰트는 다음을 수행합니다.

- 의존성 명시
- 자동 등록
- 구조적 안전성 확보

Processor가 필요로 하는 Setting과 Context를 명확히 선언함으로써 초기화 누락을 방지합니다.

---

# 4. 이동 Processor 예시

(기존 코드 유지)

```
[이전 코드 블록 그대로 유지]
```

---

# 5. Controller

## 5.1 Controller의 역할

Controller는 여러 Processor를 그룹화하여 하나의 실행 단위로 관리하는 컴포넌트입니다.

Controller는 다음을 담당합니다.

- Processor 초기화
- Setting/Context Registry 구성
- 실행 순서 관리
- InvokeTiming에 따른 실행 제어

즉, Controller는 실행 흐름의 오케스트레이터 역할을 합니다.

---

## 5.2 Controller 구조

Controller는 다음과 같은 형태로 상속받아 사용합니다.

```csharp
public class 이름  : Controller<필요한Setting인터페이스, 필요한Context인터페이스>
{
}
```

Controller는 내부적으로:

- 모든 Setting을 Registry에 등록
- 모든 Context를 Registry에 등록
- Processor의 Require 정보를 기반으로 의존성 검증
- 각 Processor의 Initialize 호출
- 지정된 타이밍에 Process 호출

---

## 5.3 실행 흐름

- Controller 생성
- Setting/Context 구성
- Processor 등록
- Initialize 단계
- InvokeTiming에 따라 Process 실행

Processor는 서로 직접 참조하지 않습니다.  
Context를 통해 간접적으로 상호 작용합니다.

이 구조는 의존성을 최소화하고 테스트 가능성을 높입니다.

---

## 5.4 Controller 설계 장점

- 행동 단위의 명확한 분리
- 실행 흐름의 중앙 집중 관리
- 모듈 교체 용이
- 디자이너 친화적 구조
- GC 최소화 설계 가능

Controller는 단순한 MonoBehaviour가 아니라,  
행동 실행 파이프라인의 관리 계층입니다.