# Phase 3 Setup Guide

## Unity Editor 작업 가이드 - Core Game Loop

### 1. GameManager 업데이트

**GameManager GameObject 선택**:

1. **컴포넌트 추가**:
   - Add Component → MatchDetector (MatchDetector.cs)
   - Add Component → GravityChecker (GravityChecker.cs)
   - Add Component → DestructionHandler (DestructionHandler.cs)
   - Add Component → BubblePoolManager (BubblePoolManager.cs)

2. **MatchDetector 설정**:
   - Min Match Count: 3 (기본값)

3. **DestructionHandler 설정**:
   - Destruction Delay: 0.1
   - Destruction Duration: 0.3
   - Destruction Curve: 기본 EaseInOut
   - Fall Duration: 1.0
   - Fall Gravity: 10
   - Fall Rotation Speed: 360

4. **BubblePoolManager 설정**:
   - Bubble Prefab: Bubble 프리팹 드래그
   - Initial Pool Size: 50
   - Pool Parent: 자동 생성됨

### 2. BubbleShooter 업데이트

**BubbleShooter GameObject 선택 → Inspector**:

1. **새로운 References 연결**:
   - Pool Manager: GameManager의 BubblePoolManager 드래그
   - Match Detector: GameManager의 MatchDetector 드래그
   - Gravity Checker: GameManager의 GravityChecker 드래그
   - Destruction Handler: GameManager의 DestructionHandler 드래그

2. **Shooter Settings 추가**:
   - Bubble Prefab: Bubble 프리팹 드래그 (풀 없을 때 대비)
   - Shoot Speed: 10

### 3. Bubble Prefab에 Layer 설정

**Bubble Prefab 선택**:
- Layer: Bubble (Layer 8)
- Tag: Untagged (또는 "Bubble" 태그 생성)

### 4. BubbleSpawner 비활성화

**GameManager → BubbleSpawner 컴포넌트**:
- Enable Click To Spawn: **체크 해제**
- 또는 컴포넌트 전체 비활성화 (체크박스 해제)

이제 BubbleShooter가 버블 생성을 담당합니다.

### 5. 테스트

**Play Mode 실행**:

#### 기본 게임 사이클 테스트

1. **버블 발사**:
   - 마우스 홀드 → 조준
   - 릴리즈 → 버블 발사
   - 궤적을 따라 애니메이션 이동
   - 육각형 그리드에 스냅

2. **매칭 감지**:
   - 같은 색상 3개 연결 시 매칭
   - Console: "Match found! X [Color] bubbles"

3. **파괴 애니메이션**:
   - 매칭된 버블들 스케일 축소
   - 0.3초 후 사라짐

4. **중력 체크**:
   - 최상단과 연결 끊긴 버블 감지
   - Console: "Gravity check: X bubbles will fall"

5. **낙하 애니메이션**:
   - 중력 영향으로 가속 낙하
   - 회전하며 아래로 떨어짐
   - 화면 밖으로 사라짐

#### 테스트 시나리오

**시나리오 1: 단순 매칭**
1. 같은 색상 버블 2개 배치
2. 같은 색상으로 3번째 발사
3. 결과: 3개 모두 파괴

**시나리오 2: 체인 파괴**
1. 여러 색상 혼합 배치
2. 가운데 색상 매칭으로 파괴
3. 결과: 위에 있던 다른 색상들도 낙하

**시나리오 3: 벽 반사**
1. 벽 쪽으로 조준 (노란 가이드라인)
2. 반사각으로 버블 배치
3. 결과: 반사 후 정확한 위치 배치

**시나리오 4: 복잡한 구조**
1. 여러 줄 버블 배치 (Phase 1 테스트 패턴 사용)
2. 중간 버블 제거
3. 결과: 연결 끊긴 상단 버블들 낙하

### 6. Console 로그 확인

**정상 동작 시 로그**:
```
Spawned [Color] bubble at Hex(x, y)
Shot fired! Direction: ..., Reflection: ...
Bubble placed at Hex(x, y)
Match found! 3 [Color] bubbles
Destroying 3 bubbles
Gravity check: 5 bubbles will fall
Making 5 bubbles fall
```

### 7. Scene View 디버깅

**Gizmos 표시**:
- 매칭된 버블: 와이어 구체 (MatchDetector)
- 떨어질 버블: 와이어 큐브 (GravityChecker)
- 청록색 라인: 발사 궤적
- 노란 구체: 벽 반사 지점

### 8. 문제 해결

**버블이 발사되지 않는 경우**:
- BubblePoolManager에 Bubble Prefab 연결 확인
- BubbleShooter에 모든 참조 연결 확인
- Bubble Prefab에 Bubble.cs 컴포넌트 확인

**매칭이 감지되지 않는 경우**:
- 같은 색상 3개 이상 인접 확인
- MatchDetector의 Min Match Count 확인 (기본 3)
- Bubble의 Type이 올바르게 설정되었는지 확인

**버블이 떨어지지 않는 경우**:
- GravityChecker가 연결되었는지 확인
- 최상단(r=0)에 버블이 있는지 확인
- DestructionHandler의 Fall Duration 확인

**애니메이션이 이상한 경우**:
- Shoot Speed 조정 (기본 10)
- Destruction Duration 조정 (기본 0.3)
- Fall Gravity 조정 (기본 10)

### 9. 성능 확인

**Profiler 확인** (Window → Analysis → Profiler):
- BFS 알고리즘 (MatchDetector): <5ms
- DFS 알고리즘 (GravityChecker): <5ms
- Object Pooling: 메모리 할당 최소화
- 프레임: 60 FPS 유지

### 10. 다음 단계

Phase 3 완료 체크리스트:
- [ ] 버블 발사 애니메이션 (궤적 따라 이동)
- [ ] 육각형 그리드에 스냅
- [ ] 같은 색상 3개 매칭 감지
- [ ] 매칭된 버블 파괴 애니메이션
- [ ] 중력 체크 (DFS)
- [ ] 연결 끊긴 버블 낙하 애니메이션
- [ ] 완전한 게임 사이클: Shoot → Place → Match → Destroy → Gravity → Fall

완료되면 **Phase 4 (Polish & Game Feel)** 진행!

### 추가 팁

**BubbleSpawner 재활성화 (디버깅용)**:
- 초기 버블 패턴 생성 시 유용
- Enable Click To Spawn 체크
- BubbleShooter와 동시 사용 시 충돌 가능

**디버그 키**:
- C: 모든 버블 제거
- R: 씬 리로드
- D: 디버그 시각화 토글 (BubbleSpawner)
