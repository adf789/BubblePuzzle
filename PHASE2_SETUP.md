# Phase 2 Setup Guide

## Unity Editor 작업 가이드 - Shooter & Aiming

### 1. BubbleShooter GameObject 생성

1. **Hierarchy → Create Empty**
   - 이름: `BubbleShooter`
   - Position: (0, -8, 0) - 화면 하단 중앙

2. **컴포넌트 추가**
   - Add Component → BubbleShooter (BubbleShooter.cs)
   - Add Component → TrajectoryCalculator (TrajectoryCalculator.cs)

3. **TrajectoryCalculator 설정**
   - Max Distance: 20
   - Wall Layer: Wall (Layer 9)
   - Bubble Layer: Bubble (Layer 8)

### 2. AimGuide 생성 (LineRenderer)

1. **BubbleShooter 자식으로 빈 GameObject 생성**
   - Hierarchy → BubbleShooter 우클릭 → Create Empty
   - 이름: `AimGuide`

2. **컴포넌트 추가**
   - Add Component → Line Renderer
   - Add Component → AimGuide (AimGuide.cs)

3. **LineRenderer 설정**
   - Positions: 비우기 (스크립트에서 자동 설정)
   - Width: 0.1
   - Color: White (또는 원하는 색상)
   - Material: Default-Line (또는 Sprites-Default)
   - Sorting Layer: Default
   - Order in Layer: 10

4. **AimGuide 스크립트 설정**
   - Line Renderer: 자동 연결
   - Line Width: 0.1
   - Normal Color: White
   - Reflection Color: Yellow
   - Line Material: Sprites-Default

### 3. PreviewFrame 생성

1. **BubbleShooter 자식으로 빈 GameObject 생성**
   - Hierarchy → BubbleShooter 우클릭 → Create Empty
   - 이름: `PreviewFrame`

2. **컴포넌트 추가**
   - Add Component → PreviewFrame (PreviewFrame.cs)

3. **PreviewFrame 설정**
   - Frame Size: 0.6 (버블 크기와 동일)
   - Frame Color: White (Alpha: 0.5)
   - Line Width: 0.05

### 4. BubbleShooter 스크립트 연결

**BubbleShooter GameObject 선택 → Inspector**:

- **References**:
  - Main Camera: Main Camera 드래그
  - Trajectory Calculator: 자동 연결됨
  - Aim Guide: AimGuide 자식 드래그
  - Preview Frame: PreviewFrame 자식 드래그
  - Bubble Grid: GameManager의 BubbleGrid 드래그

- **Shooter Settings**:
  - Shooter Transform: BubbleShooter 자신 드래그
  - Min Aim Distance: 1.0

- **Debug**:
  - Show Debug Gizmos: 체크 (Scene View에서 궤적 표시)

### 5. GameManager 업데이트

**GameManager GameObject 선택**:
- BubbleSpawner의 `Enable Click To Spawn` 체크 해제 (조준과 충돌 방지)

### 6. Material 설정 (선택사항)

**LineRenderer용 Material 생성**:

1. **Project → Assets 우클릭 → Create → Material**
   - 이름: `LineMaterial`

2. **Material 설정**
   - Shader: Sprites/Default 또는 Unlit/Color
   - Color: White

3. **AimGuide에 적용**
   - AimGuide → Line Material에 LineMaterial 드래그

### 7. 테스트

**Play Mode 실행**:

1. **마우스 홀드**:
   - 좌클릭 홀드 → 조준선 표시
   - 벽에 닿으면 노란색으로 변경 (반사)
   - 마우스 방향으로 궤적 표시

2. **Hexagonal Preview**:
   - 조준선 끝에 육각형 프레임 표시
   - 비어있는 위치: 하얀색 프레임
   - 이미 버블이 있는 위치: 빨간색 프레임

3. **Wall Reflection**:
   - 벽에 조준 → 반사각 계산 (최대 1회)
   - 반사 지점에 노란 구체 표시 (Gizmos)

4. **마우스 릴리즈**:
   - 버튼 놓으면 발사 (현재는 로그만 출력)
   - 조준선과 프레임 숨김

5. **Scene View Gizmos**:
   - 청록색: 궤적 라인
   - 녹색 구체: Shooter 위치
   - 파란 화살표: 조준 방향
   - 노란 구체: 벽 반사 지점
   - 빨간/녹색 구체: 최종 도착 지점

### 8. 디버그 출력 확인

**Console 로그**:
- "Started aiming" - 조준 시작
- "Stopped aiming" - 조준 종료
- "Shot fired! Direction: ..., Reflection: ..." - 발사 정보

### 9. 문제 해결

**조준선이 안 보이는 경우**:
- LineRenderer Material 확인
- Sorting Order 확인 (10 이상)
- AimGuide 스크립트 연결 확인

**반사가 안 되는 경우**:
- Wall Layer 설정 확인 (Layer 9)
- Wall GameObject의 Layer 확인
- TrajectoryCalculator의 Wall Layer Mask 확인

**프레임이 안 보이는 경우**:
- PreviewFrame의 Frame Size 확인
- LineRenderer가 자동 생성되었는지 확인
- BubbleGrid 연결 확인

### 10. 다음 단계

Phase 2 완료 체크리스트:
- [ ] 마우스 홀드로 조준선 표시
- [ ] 벽 반사 계산 (최대 1회)
- [ ] 육각형 프레임 표시
- [ ] 빈/차있는 위치 색상 구분
- [ ] Scene View에서 Gizmos 표시

완료되면 **Phase 3 (Core Game Loop)** 진행!
