✨ Show My Life
한 남자의 어린 시절부터 시작되는 인생의 여정을 담은 3D 플랫포머 게임입니다.

<br>

(↑ 이 이미지를 프로젝트의 멋진 스크린샷이나 GIF로 교체하세요)

<br>

📖 프로젝트 소개 (Introduction)
Show My Life는 한 남자의 유년기부터 시작되는 인생의 여정을 따라가는 3D 플랫포머 게임입니다. 플레이어는 주인공이 되어 각 시대를 대표하는 스테이지를 탐험하며 성장과 삶의 의미를 경험하게 됩니다. 이 프로젝트는 Unity 엔진을 사용하여 개발되었으며, 안정적인 시스템 아키텍처 설계와 데이터 기반의 성능 최적화에 중점을 두었습니다.

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
<summary><b>📊 성능 최적화: 데이터 기반의 최적화</b></summary>
<div style="padding-left: 20px; border-left: 2px solid #e0e0e0; margin-left: 10px;">

<b>Unity 프로파일러</b>를 활용하여 렌더링 및 메모리 병목 지점을 과학적으로 분석하고 최적화를 진행했습니다.

<b>텍스처 최적화:</b> Max Texture Size 조절 및 Crunch Compression 적용.

<b>렌더링 최적화:</b> MSAA, 그림자 해상도, SSAO 품질 설정을 조절하여 GPU 부하 감소.

<b>최종 결과:</b> 총 메모리 사용량을 5.45GB에서 2.34GB로 약 57% 절감하여 안정적인 성능을 확보했습니다.
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

기획

정재우

곽범수


개발

손영준

김현종

조성찬

최홍진

<br>

🤝 팀 개발 규칙 (Team Workflow)
원활한 협업과 Git 충돌 방지를 위해 다음과 같은 규칙을 설정하고 준수했습니다.

프리팹(Prefab) 중심의 작업: 씬(Scene) 파일의 동시 수정을 최소화하기 위해, 모든 기능 구현 및 수정은 프리팹 단위로 진행하는 것을 원칙으로 합니다.

씬 수정 전 사전 공지: 씬을 직접 수정해야 할 경우, 반드시 팀 채널에 먼저 알려 다른 팀원의 작업과 충돌이 발생하지 않도록 합니다.

작업 완료 후 공지: 씬 수정 작업이 완료되면 '완료' 공지를 통해 다른 팀원이 안전하게 다음 작업을 이어갈 수 있도록 합니다.
