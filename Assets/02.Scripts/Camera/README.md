# 카메라 인풋 시스템 설정 가이드

## 개요
이 문서는 Unity의 새로운 Input System을 사용하여 카메라 컨트롤을 구현하는 방법을 설명합니다.

## 구성 요소
- `CameraController.cs`: 카메라 움직임과 플레이어 추적을 담당하는 메인 컨트롤러
- `CameraInputSetup.cs`: 인풋 시스템과 카메라 컨트롤러를 연결하는 설정 스크립트
- `Player_Input.inputactions`: 인풋 액션 정의 파일 (Look, Zoom 액션 포함)

## 설정 방법

### 1. 인풋 액션 설정
`Player_Input.inputactions` 파일에 다음 액션들이 정의되어 있습니다:
- **Look**: 마우스 이동을 감지하여 카메라 회전 제어 (Vector2 타입)
- **Zoom**: 마우스 휠 스크롤을 감지하여 줌 인/아웃 제어 (Axis 타입)

### 2. 카메라 오브젝트 설정
1. 카메라 오브젝트에 `CameraController` 컴포넌트 추가
2. 카메라 오브젝트에 `CameraInputSetup` 컴포넌트 추가
3. `CameraController`의 설정값 조정:
   - **Target**: 카메라가 따라갈 대상 (일반적으로 플레이어)
   - **Distance**: 카메라와 타겟 사이의 거리
   - **Height**: 카메라 높이 오프셋
   - **Mouse Sensitivity**: 마우스 감도

### 3. 플레이어 인풋 컴포넌트 설정
1. 플레이어 오브젝트에 `PlayerInput` 컴포넌트가 있는지 확인
2. `PlayerInput` 컴포넌트에서 다음과 같이 설정:
   - **Actions**: `Player_Input` (Resources 폴더에 있는 에셋)
   - **Default Map**: "Player"
   - **Behavior**: "Invoke Unity Events"
3. 이벤트 연결:
   - **Look** 이벤트를 `CameraInputSetup.OnLook`에 연결
   - **Zoom** 이벤트를 `CameraInputSetup.OnZoom`에 연결

## 주요 기능

### 카메라 회전
- 마우스 이동으로 카메라 회전
- 수직 각도 제한 (위/아래 회전 제한)
- 부드러운 회전 보간

### 줌 기능
- 마우스 휠로 줌 인/아웃
- 최소/최대 거리 제한
- 부드러운 줌 보간

### 플레이어 추적
- 플레이어를 화면 정중앙에 고정
- 충돌 감지로 벽 통과 방지
- 부드러운 회전 처리

## 문제 해결
- **카메라가 회전하지 않는 경우**: 
  - `PlayerInput` 컴포넌트의 이벤트가 올바르게 연결되었는지 확인
  - 콘솔에 "PlayerInput 컴포넌트를 찾을 수 없습니다" 경고가 표시되면 플레이어 오브젝트에 `PlayerInput` 컴포넌트가 있는지 확인

- **마우스 감도가 너무 높거나 낮은 경우**:
  - `CameraController`의 Mouse Sensitivity 값 조정

- **카메라가 벽을 통과하는 경우**:
  - Collision Layers 설정 확인
  - Collision Offset 값 증가

## 커스터마이징
- **마우스 반전**: Invert Y Axis, Invert X Axis 옵션 사용
- **회전 속도**: Rotation Speed 값 조정
- **줌 속도**: Zoom Speed 값 조정

## 코드 예시
```csharp
// Look 액션 처리
public void OnLook(InputAction.CallbackContext context)
{
    Vector2 lookInput = context.ReadValue<Vector2>();
    // 카메라 회전 처리
}

// Zoom 액션 처리
public void OnZoom(InputAction.CallbackContext context)
{
    float zoomInput = context.ReadValue<float>();
    // 줌 처리
}
``` 