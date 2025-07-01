using UnityEngine;

// 연출 테스트
public class EmotionInputHandler : MonoBehaviour
{
    [Tooltip("EmotionDirector 스크립트")]
    [SerializeField] private EmotionDirector emotionDirector;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            emotionDirector?.PlayTopDownEmotion();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            emotionDirector?.PlaySepiaEmotion();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            emotionDirector?.PlaySadness();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            emotionDirector?.ResetEmotion();
        }
    }
}
