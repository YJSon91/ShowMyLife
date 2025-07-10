using UnityEngine;

public class IntroUIFlow : MonoBehaviour
{
    private bool _keyHasBeenPressed = false;

    private void Update()
    {
        // 아직 키가 눌리지 않았고, 아무 키나 눌렸다면
        if (!_keyHasBeenPressed && Input.anyKeyDown)
        {
            _keyHasBeenPressed = true;

            // 1. GameManager에게 상태 변경만 요청합니다.
            GameManager.Instance.UpdateGameState(GameManager.GameState.MainMenu);

            // 2. 자신의 임무가 끝났으므로, 이 컴포넌트를 즉시 파괴합니다.
            Destroy(this);
        }
    }
}
