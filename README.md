# Unity Modular Behavior System

## 1. 소개

이 유니티용 패키지는 **모듈형 행동 시스템**을 제공합니다.  
게임 오브젝트에 다양한 행동을 쉽게 추가하고 관리할 수 있도록 설계되었습니다.

각 행동은 독립적인 모듈로 구성되어 있어 **재사용성**과 **유지보수성**이 뛰어납니다.

시스템은 다음 세 가지 핵심 요소로 구성됩니다:

- **Setting**  
  런타임에 불변 또는 준불변 데이터를 보관합니다.

- **Context**  
  런타임 동안 현재 상태를 나타냅니다.

- **Processor**  
  실제 행동 로직을 실행하는 단위입니다.

---

## 2. 주요 기능

### 유니티 인스펙터 지원



- 인스펙터에서 Setting, Processor, Context를 쉽게 구성할 수 있습니다.
- 디자이너와 개발자가 협업하여 행동을 직관적으로 조정할 수 있습니다.

---

### 유연한 행동 구성

- Processor 구성을 변경하여 다양한 행동을 생성할 수 있습니다.
- 높은 확장성과 재사용성을 제공합니다.

---

### 런타임 GC 최소화

- 런타임 중 GC 발생을 거의 유발하지 않습니다.
- 프레임 드랍 방지 및 성능 향상에 도움을 줍니다.

---

## 3. 사용 방법

### 3.1 패키지 설치

유니티 패키지 매니저에서 Git URL을 통해 설치합니다.

```
https://github.com/Armangi1312/unity-am-modular-behavior-system.git
```

패키지 매니저에서 "**Add package from Git URL**"을 선택 후 위 주소를 입력하세요.

![패키지 설치 이미지](https://raw.githubusercontent.com/Armangi1312/unity-am-modular-behavior-system/main/Documentation~/Images/SC2.png)

---

### 3.2 행동 구성

인스펙터에서 Setting, Processor, Context를 추가하여 행동을 구성합니다.

![컨트롤러 이미지](https://raw.githubusercontent.com/Armangi1312/unity-am-modular-behavior-system/main/Documentation~/Images/SC1.png)

#### Processor 관리

- `+` 버튼: Processor 추가
- `-` 버튼: Processor 제거
- 드래그로 순서 변경 가능 (우선순위 조정)

> *런타임 중에는 수정할 수 없습니다.*

---

### Setting / Context 추가

- Processor와 동일한 방식으로 추가할 수 있습니다.
- Processor가 요구하는 Setting과 Context는 자동으로 추가됩니다.

---

## 4. 문서 및 예제

자세한 코드 설명과 예제는 아래 문서를 참고하세요

[Documentation](./Documentation~/Documentation.md)