using UnityEngine;

public class IntroUIFlow : MonoBehaviour
{
    private bool _keyHasBeenPressed = false;

    void Update()
    {
        if (!_keyHasBeenPressed && Input.anyKeyDown)
        {
            _keyHasBeenPressed = true;
            // 키가 눌리면 GameManager에 상태 변경만 요청!
            GameManager.Instance.UpdateGameState(GameManager.GameState.MainMenu);
            // 이 스크립트는 역할을 다했으므로 비활성화
            this.enabled = false;
        }
    }
}
