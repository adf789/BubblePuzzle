# Phase 4 Setup Guide

## Unity Editor 작업 가이드 - Polish & Game Feel

### 1. UI Canvas 생성

**Hierarchy → UI → Canvas**:

1. **Canvas 설정**:
   - Render Mode: Screen Space - Overlay
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1024 x 1920
   - Match: 0.5 (Width and Height)

2. **Canvas 이름**: `GameCanvas`

### 2. UI 요소 생성

#### Score Text

**Canvas 우클릭 → UI → Text - TextMeshPro**:
- 이름: `ScoreText`
- Position: (0, 900, 0)
- Width/Height: 400 x 100
- Text: "Score: 0"
- Font Size: 48
- Alignment: Center
- Color: White

#### Combo Text

**Canvas 우클릭 → UI → Text - TextMeshPro**:
- 이름: `ComboText`
- Position: (0, 800, 0)
- Width/Height: 300 x 80
- Text: "Combo x0"
- Font Size: 36
- Alignment: Center
- Color: Yellow
- **초기 상태**: 비활성화 (체크박스 해제)

#### Combo Bar

**Canvas 우클릭 → UI → Image**:
- 이름: `ComboBarBackground`
- Position: (0, 730, 0)
- Width/Height: 300 x 20
- Color: Dark Gray (50, 50, 50)

**ComboBarBackground 자식으로 Image 생성**:
- 이름: `ComboBarFill`
- Anchor: Stretch (양쪽 끝까지)
- Position: (0, 0, 0)
- Width/Height: 0 (Stretch로 자동)
- Image Type: Filled
- Fill Method: Horizontal
- Fill Origin: Left
- Fill Amount: 1.0
- Color: Yellow

#### Bubbles Remaining Text

**Canvas 우클릭 → UI → Text - TextMeshPro**:
- 이름: `BubblesText`
- Position: (0, -900, 0)
- Width/Height: 300 x 60
- Text: "Bubbles: 0"
- Font Size: 32
- Alignment: Center
- Color: White

### 3. GameUI 컴포넌트 설정

**GameCanvas 선택 → Add Component → GameUI**:

**UI References**:
- Score Text: ScoreText 드래그
- Combo Text: ComboText 드래그
- Bubbles Remaining Text: BubblesText 드래그
- Combo Bar: ComboBarFill 드래그

**Settings**:
- Show Combo: 체크
- Combo Decay Time: 3.0

### 4. GameManager 업데이트

**GameManager GameObject 선택**:

1. **컴포넌트 추가**:
   - Add Component → LevelManager (LevelManager.cs)

2. **새로운 References 연결**:
   - Game UI: GameCanvas의 GameUI 드래그
   - Level Manager: 자동 연결됨
   - Match Detector: 이미 연결됨
   - Gravity Checker: 이미 연결됨

### 5. LevelManager 설정

**GameManager → LevelManager 컴포넌트**:

**References**:
- Bubble Grid: GameManager의 BubbleGrid 드래그
- Pool Manager: GameManager의 BubblePoolManager 드래그
- Bubble Prefab: Bubble 프리팹 드래그 (풀 백업용)

**Current Level**: 비워둠 (ScriptableObject로 생성 예정)

### 6. Level Data 생성 (ScriptableObject)

**Project → Assets 우클릭 → Create → BubblePuzzle → Level Data**:

1. **첫 번째 레벨 생성**:
   - 이름: `Level1`
   - Level Number: 1
   - Level Name: "Level 1"
   - Rows: 5
   - Columns Per Row: 7
   - Available Colors: Red, Blue, Green (3개만)
   - Target Score: 500
   - Max Shots: 20

2. **더 어려운 레벨 생성** (선택사항):
   - `Level2`: 6 rows, Red/Blue/Green/Yellow (4개 색상)
   - `Level3`: 7 rows, 모든 5개 색상

### 7. Level Pattern 설정 (선택사항)

**Level1 선택 → Inspector**:

**Initial Pattern → Rows 설정**:
```
Element 0: "RRGBBRR"
Element 1: "BGGRRGB"
Element 2: "RRBGBBR"
Element 3: "GBRRBBG"
Element 4: "RRGBBGR"
```

패턴 규칙:
- R = Red
- B = Blue
- G = Green
- Y = Yellow
- P = Purple
- `-` 또는 공백 = Empty

### 8. LevelManager에 레벨 연결

**GameManager → LevelManager**:
- Current Level: Level1 ScriptableObject 드래그

### 9. 테스트

**Play Mode 실행**:

#### UI 테스트

1. **Score 표시**:
   - 버블 매칭 시 점수 증가
   - 기본: 버블 1개당 10점
   - 콤보: 콤보 레벨당 5점 추가

2. **Combo 시스템**:
   - 매칭 성공 시 콤보 증가
   - Combo Text 표시 (노란색)
   - Combo Bar 감소 (3초 후 리셋)
   - 3초 내 다시 매칭 시 콤보 유지

3. **Bubbles Remaining**:
   - 현재 그리드의 버블 개수 표시
   - 파괴/낙하 시 업데이트

4. **Level System**:
   - 시작 시 Level1의 패턴 생성
   - 20발 제한
   - 500점 달성 시 레벨 클리어
   - Console: "Level Complete!"

#### 점수 계산 예시

**시나리오 1: 단순 매칭**
- 빨간 버블 3개 매칭
- 점수: 3 × 10 = 30점
- 콤보: x1

**시나리오 2: 콤보 연속**
- 1차 매칭: 3개 → 30점 (콤보 x1)
- 3초 내 2차 매칭: 4개 → 40 + 5 = 45점 (콤보 x2)
- 3초 내 3차 매칭: 5개 → 50 + 10 = 60점 (콤보 x3)
- 총점: 135점

**시나리오 3: 낙하 보너스**
- 매칭 후 5개 버블 낙하
- 보너스: 5 × 5 = 25점

### 10. Console 로그 확인

**정상 동작 시 로그**:
```
BubblePuzzle Game Started
Level loaded: Level 1
Match found! 3 Red bubbles
Score: +30 (Match: 30, Combo: 0)
Gravity check: 2 bubbles will fall
Score: +10 (Fall Bonus)
Level Complete! Shots used: 15/20
```

### 11. 문제 해결

**UI가 보이지 않는 경우**:
- Canvas → Render Mode 확인
- TextMeshPro Import 확인 (Window → TextMeshPro → Import TMP Essential Resources)
- GameUI 컴포넌트 연결 확인

**점수가 올라가지 않는 경우**:
- GameManager → GameUI 연결 확인
- BubbleShooter → GameManager 연결 확인
- Console 로그 "OnMatchScored" 확인

**레벨이 로드되지 않는 경우**:
- LevelManager → Current Level 연결 확인
- Level Data의 패턴 문자열 확인
- BubblePoolManager 프리팹 연결 확인

**콤보가 작동하지 않는 경우**:
- ComboText 초기 비활성화 상태 확인
- ComboBar Fill Amount = 1.0 확인
- Combo Decay Time > 0 확인

### 12. VFX & SFX 추가 (선택사항)

#### Particle Effects

**파괴 이펙트**:
1. Hierarchy → Effects → Particle System
2. 이름: `BubblePopEffect`
3. Duration: 0.5
4. Start Lifetime: 0.3
5. Start Speed: 2
6. Start Size: 0.2
7. Emission → Rate: 20
8. Shape → Sphere
9. Color over Lifetime: Fade out
10. Prefab으로 저장

**DestructionHandler 컴포넌트**:
- Pop Effect Prefab: BubblePopEffect 연결
- 파괴 시 Instantiate하여 재생

#### Sound Effects

**AudioSource 추가**:
1. GameManager → Add Component → Audio Source
2. Play On Awake: 비활성화
3. Audio Clips 준비:
   - shoot.wav
   - pop.wav
   - fall.wav
   - combo.wav

**SoundManager 스크립트** (선택사항):
- 사운드 재생 관리
- 볼륨 조절
- 음소거 기능

### 13. 최적화 체크리스트

**Profiler 확인** (Window → Analysis → Profiler):
- [ ] CPU: UI 업데이트 <1ms
- [ ] Memory: UI 할당 최소화
- [ ] Rendering: Canvas 배치 수 <5
- [ ] GC: UI 업데이트 시 GC 미발생

**성능 목표**:
- 60 FPS 유지
- UI 업데이트 <1ms
- 레벨 로드 <100ms
- 메모리 증가 <10MB

### 14. 완료 체크리스트

Phase 4 완료 확인:
- [ ] Score UI 표시 및 업데이트
- [ ] Combo 시스템 작동 (3초 타이머)
- [ ] Combo Bar 시각화
- [ ] Bubbles Remaining 카운터
- [ ] Level Data ScriptableObject 생성
- [ ] 레벨 로딩 시스템
- [ ] 승리 조건 체크 (점수/샷 수)
- [ ] GameManager와 UI 통합

### 15. 다음 단계 (추가 개선)

**추가 가능한 기능**:
- 🎆 Particle Effects (파괴, 낙하)
- 🔊 Sound Effects & BGM
- 🎬 트위닝 애니메이션 (DOTween)
- 🏆 승리/패배 화면
- 📊 하이스코어 시스템
- 🎮 레벨 선택 화면
- ⚙️ 설정 메뉴 (음량, 난이도)
- 💾 세이브/로드 시스템

완성된 게임을 즐기세요! 🎉
