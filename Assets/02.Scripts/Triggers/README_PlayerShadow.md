# 플레이어 그림자 시스템 사용법

## 📋 개요
플레이어가 공중에 있을 때 땅에 자연스러운 그림자를 생성하는 시스템입니다.

## 🚀 빠른 시작

### 1. 기본 설정
1. **빈 GameObject 생성** (이름: "PlayerShadow")
2. **PlayerShadowProjector 스크립트 추가**
3. **Ground 레이어 설정** (땅 오브젝트들을 Ground 레이어로 설정)

### 2. 자동 설정
- 스크립트가 자동으로 Projector 컴포넌트를 생성합니다
- 플레이어를 자동으로 찾아서 연결합니다
- 기본 그림자 머티리얼을 자동으로 생성합니다

## ⚙️ 설정 옵션

### 그림자 설정
- **Shadow Scale**: 그림자 기본 크기 (기본값: 1.0)
- **Max Shadow Distance**: 그림자가 생성되는 최대 높이 (기본값: 20)
- **Shadow Alpha**: 그림자 투명도 (기본값: 0.6)
- **Ground Layer Mask**: 땅으로 인식할 레이어

### 그림자 품질
- **Use Smooth Shadow**: 부드러운 그림자 가장자리 사용
- **Shadow Fade Speed**: 그림자 페이드 인/아웃 속도

## 🎮 게임플레이 연동

### 자동 동작
- 플레이어가 점프하면 그림자 자동 생성
- 땅에 착지하면 그림자 자동 숨김
- 높이에 따른 그림자 크기 자동 조절

### 수동 제어
```csharp
// 그림자 투명도 조절
shadowProjector.SetShadowAlpha(0.8f);

// 그림자 크기 조절
shadowProjector.SetShadowScale(1.5f);

// 그림자 활성화/비활성화
shadowProjector.SetShadowEnabled(false);
```

## 🌟 고급 기능

### PlayerShadowManager 사용
1. **빈 GameObject 생성** (이름: "PlayerShadowManager")
2. **PlayerShadowManager 스크립트 추가**
3. **전역 그림자 설정 관리**

```csharp
// 모든 그림자 투명도 설정
PlayerShadowManager.Instance.SetAllShadowsAlpha(0.7f);

// 그림자 시스템 전체 비활성화
PlayerShadowManager.Instance.SetShadowSystemEnabled(false);

// 고품질 모드 활성화
PlayerShadowManager.Instance.SetShadowQuality(true);
```

### 이벤트 시스템
```csharp
// 그림자 시스템 상태 변화 감지
PlayerShadowManager.Instance.OnShadowSystemToggled += (enabled) => {
    Debug.Log($"그림자 시스템: {(enabled ? "활성화" : "비활성화")}");
};

// 그림자 품질 변화 감지
PlayerShadowManager.Instance.OnShadowQualityChanged += (quality) => {
    Debug.Log($"그림자 품질: {quality}");
};
```

## 🔧 문제 해결

### 그림자가 보이지 않는 경우
1. **Ground 레이어 확인**: 땅 오브젝트가 Ground 레이어로 설정되어 있는지 확인
2. **레이어 마스크 확인**: Ground Layer Mask가 올바르게 설정되어 있는지 확인
3. **플레이어 태그 확인**: 플레이어가 "Player" 태그를 가지고 있는지 확인

### 성능 최적화
1. **Max Shadow Distance 조절**: 불필요하게 큰 값으로 설정하지 않기
2. **Shadow Fade Speed 조절**: 너무 빠른 페이드 효과는 성능에 부담
3. **Use Smooth Shadow**: 필요하지 않으면 비활성화

## 📱 플랫폼별 최적화

### 모바일
- **Shadow Scale**: 0.8 이하로 설정
- **Use Smooth Shadow**: 비활성화 권장
- **Shadow Fade Speed**: 3 이하로 설정

### PC
- **Shadow Scale**: 1.0 이상으로 설정
- **Use Smooth Shadow**: 활성화 권장
- **Shadow Fade Speed**: 5 이상으로 설정

## 🎨 커스터마이징

### 커스텀 셰이더
- `SimpleShadowShader.shader`: 기본 그림자용
- `PlayerShadowShader.shader`: 고품질 그림자용

### 커스텀 머티리얼
```csharp
// 커스텀 머티리얼 적용
Material customMaterial = new Material(Shader.Find("Custom/SimpleShadow"));
customMaterial.SetColor("_Color", new Color(0, 0, 0, 0.8f));
shadowProjector.material = customMaterial;
```

## 📊 성능 모니터링

### 디버그 정보
- **OnGUI**: 실시간 그림자 시스템 상태 표시
- **Console**: 그림자 생성/제거 로그
- **Gizmos**: 그림자 위치 및 레이캐스트 시각화

### 성능 체크리스트
- [ ] 그림자가 불필요하게 많이 생성되지 않는가?
- [ ] 레이캐스트가 과도하게 자주 호출되지 않는가?
- [ ] 머티리얼 인스턴스가 적절히 관리되는가?

## 🔄 업데이트 히스토리

### v1.0.0
- 기본 그림자 시스템 구현
- 자동 플레이어 탐지
- 높이 기반 그림자 크기 조절
- 부드러운 페이드 효과

### v1.1.0
- PlayerShadowManager 추가
- 전역 그림자 설정 관리
- 이벤트 시스템 구현
- 성능 최적화 옵션 추가
