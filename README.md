✨ Show My Life
한 남자의 어린 시절부터 시작되는 인생의 여정을 담은 3D 플랫포머 게임입니다.

<br>

<img width="1536" height="1024" alt="ee8413b6-a946-4c4b-9cd8-103934ba308c" src="https://github.com/user-attachments/assets/a3a10d8e-5a1a-4d2b-adf4-febc9f6c17cd" />
(https://github.com/user-attachments/assets/a4b54573-d8bb-48ea-bd75-c48e9c0e1bc0)

<br>

📖 프로젝트 소개 (Introduction)
Show My Life는 한 남자의 유년기부터 시작되는 인생의 여정을 따라가는 3D 플랫포머 게임입니다. 플레이어는 주인공이 되어 각 시대를 대표하는 스테이지를 탐험하며 성장과 삶의 의미를 경험하게 됩니다. 이 프로젝트는 Unity 엔진과 URP(Universal Render Pipeline)를 사용하여 개발되었으며, 안정적인 시스템 아키텍처 설계와 데이터 기반의 성능 최적화에 중점을 두었습니다.

<br>

🌟 주요 기능 (Key Features)
인생의 여정을 담은 스테이지: 유년기, 청소년기 등 각 시대를 대표하는 테마로 구성된 3D 플랫포머 스테이지.

데이터 기반의 대사 시스템: JSON 파일을 통해 관리되는 유연한 대사 시스템으로 깊이 있는 스토리텔링을 제공합니다.

연출이 가미된 시네마틱 카메라: 달리 줌, 레일 카메라 등 다양한 카메라 연출을 통해 몰입감 높은 플레이 경험을 선사합니다.

세이브 & 로드: 체크포인트 기반의 저장 및 이어하기 기능을 통해 언제든지 게임을 다시 시작할 수 있습니다.

<br>

🛠️ 기술적 특징 (Technical Highlights)
이 프로젝트는 확장성과 유지보수성을 고려하여 다음과 같은 시스템 아키텍처를 구축했습니다.

<details>
<summary><b>🏛️ GameManager: 상태 머신 기반의 총괄 관리자</b></summary>
<div style="padding-left: 20px; border-left: 2px solid #e0e0e0; margin-left: 10px;">

<b>싱글톤 패턴</b>과 <b>상태 머신(State Machine)</b>을 적용하여 게임의 복잡한 상태(메인 메뉴, 플레이, 일시정지 등)를 중앙에서 총괄합니다.

하위 매니저(UIManager, SoundManager 등)들이 스스로를 등록하는 시스템으로 매니저 간의 <b>느슨한 결합(Loose Coupling)</b>을 유지하여 확장성을 높였습니다.
</div>
</details>

<details>
<summary><b>🎨 UIManager: 제네릭 기반의 모듈식 UI 관리</b></summary>
<div style="padding-left: 20px; border-left: 2px solid #e0e0e0; margin-left: 10px;">

<b>제네릭 메서드</b>(Show&lt;T&gt;, Hide&lt;T&gt;)를 활용하여 UI 패널을 타입 기반으로 관리, 코드의 재사용성과 유지보수성을 극대화했습니다.

모든 UI 스크립트는 UiBase를 상속받아 각자의 동작을 책임지도록 <b>모듈화</b>하여 독립성을 보장했습니다.
</div>
</details>

<details>
<summary><b>⌨️ Input System: 이벤트 기반의 입력 처리</b></summary>
<div style="padding-left: 20px; border-left: 2px solid #e0e0e0; margin-left: 10px;">

Unity의 새로운 <b>Input System</b>을 도입하여, InputReader가 입력을 감지하고 C# <b>이벤트(event)</b>를 통해 다른 시스템에 알리는 방식을 사용했습니다.

이를 통해 입력 처리 로직과 실제 동작 로직(PlayerMovement, SoundManager 등)을 분리하여 코드의 결합도를 낮췄습니다.
</div>
</details>

<details>
<summary><b>📊 성능 최적화: 데이터 기반의 최적화</b></summary>
<div style="padding-left: 20px; border-left: 2px solid #e0e0e0; margin-left: 10px;">

<b>Unity 프로파일러</b>를 활용하여 렌더링 및 메모리 병목 지점을 과학적으로 분석하고 최적화를 진행했습니다.

<b>텍스처 최적화:</b> Max Texture Size 조절 및 Crunch Compression 적용.

<b>렌더링 최적화:</b> MSAA, 그림자 해상도, SSAO 품질 설정을 조절하여 GPU 부하 감소.

<b>오브젝트 풀링(Object Pooling)</b>: 짧은 수명·다량 생성되는 오브젝트를 사전에 생성·재사용하여 Instantiate/Destroy로 인한 메모리 할당 및 GC 스파이크를 방지.

<b>오클루전 컬링(Occlusion Culling)</b>: 카메라에 가려진 오브젝트의 렌더링을 자동으로 생략하여 드로우콜 및 픽셀 연산량 절감.

<b>스테이지 기반 토글</b>: 현재 플레이 구간에 필요한 오브젝트만 활성화하고 나머지는 비활성화하여 불필요한 CPU 로직 실행 차단.

<b>최종 결과:</b> 총 메모리 사용량을 5.45GB에서 2.34GB로 약 57% 절감하여 안정적인 성능을 확보했습니다.
</div>
</details>
<details>
<summary><b>📹 EmotionDirector: 시네마틱 연출 시스템</b></summary>
<div style="padding-left: 20px; border-left: 2px solid #e0e0e0; margin-left: 10px;">

Cinemachine을 활용하여 스크립트 기반 카메라 연출을 완전 제어.  

레일 기반 카메라 연출: 돌리 줌(줌인·줌아웃)과 트랙 이동을 통합 관리하여 장면 전환과 몰입도를 극대화.  

시야 훑기(Sweep), 타겟 응시(POV 연동) 등 다양한 연출 패턴을 모듈화하여 재사용 가능.  

플레이어 시점 카메라와 테마 카메라 간의 전환, 보간 시간 제어, 시야각(FOV) 변화까지 코드로 정밀하게 제어.  

연출 중 플레이어 조작 제한 및 복원, 색상 필터(PostProcessing) 적용, TimeScale 조정 등 복합적인 연출 효과를 하나의 매니저에서 통합 관리.  

스테이지별로 연출 시퀀스를 스크립트로 정의하여 씬 전환 없이도 자연스럽게 이어지는 시네마틱 구현.  

</div>
</details>

<details>
<summary><b>💾 SaveManager: 세이브 & 로드 시스템</b></summary>
<div style="padding-left: 20px; border-left: 2px solid #e0e0e0; margin-left: 10px;">

JSON 직렬화를 활용하여 플레이어 위치, 스테이지 진행 상태 등을 저장.  

체크포인트 기반 자동 저장 구조로, 씬 전환 없이 게임 중간에 복원 가능.  

MonoBehaviour 기반 매니저 구조로 GameManager와 연동.  

</div>
</details>

<details>
<summary><b>🛠 DebugWindow: 범용 디버그 허브</b></summary>
<div style="padding-left: 20px; border-left: 2px solid #e0e0e0; margin-left: 10px;">

카테고리별 디버그 버튼 등록 가능.  

게임 내 다양한 테스트(씬 로드, 오브젝트 토글, 데이터 초기화 등)를 실시간 실행.  

팀 전체가 공용으로 활용할 수 있는 확장 가능한 구조.  

</div>
</details>

<details>
<summary><b>🎨 URPMaterialConverter: 머티리얼 자동 변환 툴</b></summary>
<div style="padding-left: 20px; border-left: 2px solid #e0e0e0; margin-left: 10px;">

에디터 확장 기능(EditorWindow)으로 HDRP/기타 셰이더를 URP 표준 셰이더로 일괄 변환.  

Texture 매핑 자동화 및 손실 최소화.  

대규모 외부 에셋 호환성을 빠르게 확보.  

</div>
</details>

<br>




🎮 시작하기 (Getting Started)
조작법

이동: W, A, S, D

달리기: Shift

점프: Space

일시정지: ESC

<br>

🧑‍💻 팀원 및 역할 (Team & Roles)

| 이름 | 역할 | 담당 업무 |
|------|------|-----------|
|ㅁㅁㅁㅁㅁㅁㅁㅁ|                                        |                              |
| <nobr>[**정재우**]()</nobr> | 메인기획 | 내용 |
| <nobr>[**곽범수**]()</nobr> | 서브기획 | 초등학교 & 고등학교 스테이지 기획, 중간피드백, 예산관리(에셋 구매, 피드백 신청자 참가비) |
| <nobr>[**손영준**](CONTRIBUTIONS/Youngjun.md)</nobr> | 메인팀장 | 게임 메인시스템 제작, UI 및 대화시스템 제작, 대사 업데이트 툴 제작 |
| <nobr>[**최홍진**](https://github.com/ghdwlsdl1/ShowMyLife-Codes.git)</nobr> | 서브팀장 | 맵 제작 및 배치, 세이브 시스템 제작, 시네마틱 시스템 제작, 범용 오브젝트 풀링 제작, 범용 디버그 허브 제작, 머티리얼 자동 변환 툴 제작 |
| <nobr>[**김현종**](CONTRIBUTIONS/Hyunjong.md)</nobr> | 팀원 | 내용 |
| <nobr>[**조성찬**](CONTRIBUTIONS/Sungchan.md)</nobr> | 팀원 | 내용 |

<br>

🤝 팀 개발 규칙 (Team Workflow)
원활한 협업과 Git 충돌 방지를 위해 다음과 같은 규칙을 설정하고 준수했습니다.

프리팹(Prefab) 중심의 작업: 씬(Scene) 파일의 동시 수정을 최소화하기 위해, 모든 기능 구현 및 수정은 프리팹 단위로 진행하는 것을 원칙으로 합니다.

씬 수정 전 사전 공지: 씬을 직접 수정해야 할 경우, 반드시 팀 채널에 먼저 알려 다른 팀원의 작업과 충돌이 발생하지 않도록 합니다.

작업 완료 후 공지: 씬 수정 작업이 완료되면 '완료' 공지를 통해 다른 팀원이 안전하게 다음 작업을 이어갈 수 있도록 합니다.
