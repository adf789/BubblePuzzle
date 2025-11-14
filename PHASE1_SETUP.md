# Phase 1 Setup Guide

## Unity Editor 작업 가이드

### 1. Bubble Prefab 생성

1. **빈 GameObject 생성**
   - Hierarchy → 우클릭 → Create Empty
   - 이름: `Bubble`

2. **컴포넌트 추가**
   - Add Component → Sprite Renderer
   - Add Component → Circle Collider 2D
   - Add Component → Scripts → Bubble (Bubble.cs)

3. **SpriteRenderer 설정**
   - Sprite: Unity 기본 Circle 스프라이트 (또는 임시 Sprite)
   - Color: White (스크립트에서 색상 변경)
   - Sorting Layer: Default
   - Order in Layer: 0

4. **CircleCollider2D 설정**
   - Radius: 0.5
   - Is Trigger: 체크 해제

5. **Bubble 스크립트 설정**
   - Bubble Type: Red (기본값)
   - Sprite Renderer: 자동 연결됨
   - Circle Collider: 자동 연결됨

6. **Prefab 저장**
   - Assets 폴더에 `Prefabs/Bubbles/` 폴더 생성
   - Hierarchy의 Bubble을 폴더로 드래그
   - Hierarchy에서 Bubble 삭제

### 2. Game Scene 설정

1. **GameManager 생성**
   - Hierarchy → Create Empty
   - 이름: `GameManager`
   - Add Component → GameManager (GameManager.cs)
   - Add Component → BubbleGrid (BubbleGrid.cs)

2. **BubbleGrid 설정**
   - Hex Size: 0.6 (버블 크기에 맞게 조정)
   - Grid Origin: (0, 0) 또는 화면 중앙 상단

3. **Camera 설정**
   - Main Camera 선택
   - Position: (0, 0, -10)
   - Size: 10 (1024x1920 해상도에 맞게 조정)
   - Background: 검은색 또는 원하는 배경색

4. **Game Area Walls 생성**
   - Hierarchy → 2D Object → Sprite → Square (좌측 벽)
     - 이름: `WallLeft`
     - Position: (-5.12, 0, 0) (화면 너비의 절반)
     - Scale: (0.2, 20, 1)
     - Add Component → Box Collider 2D
     - Tag: "Wall" (Tag 생성 필요)
     - Layer: Wall (Layer 생성 필요)

   - 위 과정 반복 (우측 벽)
     - 이름: `WallRight`
     - Position: (5.12, 0, 0)
     - Scale: (0.2, 20, 1)
     - Tag: "Wall"
     - Layer: Wall

### 3. Layers 설정

1. **Edit → Project Settings → Tags and Layers**

2. **Tags 추가**
   - "Wall" 추가

3. **Layers 추가**
   - Layer 8: `Bubble`
   - Layer 9: `Wall`
   - Layer 10: `BubbleShooter`

4. **Collision Matrix 설정**
   - Edit → Project Settings → Physics 2D
   - Layer Collision Matrix:
     - Bubble ↔ Bubble: 체크
     - Bubble ↔ Wall: 체크
     - Wall ↔ BubbleShooter: 체크
     - BubbleShooter ↔ Bubble: 체크 해제

### 4. 테스트용 스크립트 추가 (선택사항)

테스트를 위해 수동으로 버블을 배치하는 스크립트를 GameManager에 추가:

1. **GameManager 선택**
2. **Inspector에서 스크립트 수정 또는 확장 클래스 생성**
3. **Play Mode에서 마우스 클릭 시 버블 생성하도록 테스트**

### 5. 실행 및 테스트

1. **Play 버튼 클릭**
2. **Console 확인**: "BubblePuzzle Game Started" 메시지
3. **Scene View**: Grid Origin에 빨간 구체 표시
4. **디버그 키 테스트**:
   - C 키: 그리드 클리어
   - R 키: 씬 리로드

### 6. 다음 단계

Phase 1 완료 후:
- [ ] 버블이 육각형 패턴으로 수동 배치 확인
- [ ] Gizmos로 헥스 그리드 시각화 확인
- [ ] 5가지 색상 버블 생성 테스트

완료되면 Phase 2 (Shooter & Aiming)로 진행합니다.
