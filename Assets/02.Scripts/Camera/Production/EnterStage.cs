 using UnityEngine;
using System.Collections;

public class EnterStage : MonoBehaviour
{
    [Tooltip("연출 매니저")]
    [SerializeField] private EmotionDirector emotionDirector;

    [Tooltip("플레이어 오브젝트")]
    [SerializeField] private Transform player;

    [Tooltip("천장응시 시간")]
    [SerializeField] private float lookDuration = 2f;

    [Tooltip("일어나는 시간")]
    [SerializeField] private float moveDuration = 2f;

    [Tooltip("응시 타겟")]
    [SerializeField] private Transform endLookTarget;

    [Tooltip("눈뜨는 시간")]
    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        StartCoroutine(PlayWakeUpSequence());
    }

    private IEnumerator PlayWakeUpSequence()
    {
        if (player == null)
        {
            Debug.LogWarning("[EnterStage] Player가 연결되지 않았습니다.");
            yield break;
        }

        // 조작 비활성화 + 캐릭터 숨김
        emotionDirector.DisablePlayerControl(player);
        emotionDirector.SetPlayerVisible(player, false);

        // 0. 화면 검정
        emotionDirector.PostProcessing.ApplyColorFilter(Color.black, 0f);
        yield return null;

        // 1. 시작 위치
        Transform startTarget = emotionDirector.GetLookTargetTransform(5);
        if (startTarget == null) yield break;

        // 2. 카메라 초기화
        emotionDirector.ResetThemeCamera();
        emotionDirector.ThemeCamera.SetPosition(startTarget.position);
        emotionDirector.ThemeCamera.SetRotation(new Vector3(-70f, 0f, 0f));
        yield return new WaitForSeconds(lookDuration);

        // 3. 화면 밝히기
        emotionDirector.PostProcessing.ResetToDefault(fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        // 4. 일어나는 이동 연출
        Vector3 from = startTarget.position;
        Vector3 to = from + startTarget.TransformDirection(Vector3.forward * 2f) + Vector3.up * 1f;
        Quaternion fromRot1 = Quaternion.Euler(-70f, 0f, 0f);
        Quaternion toRot1 = Quaternion.identity;

        emotionDirector.PlayMoveAndRotateToNeutral(from, to, fromRot1, toRot1, moveDuration);
        yield return new WaitForSeconds(moveDuration + 0.5f);

        // 5. 응시 연출
        if (endLookTarget != null)
        {
            emotionDirector.ThemeCamera.SmoothLookAt(endLookTarget, 1f);
            yield return new WaitForSeconds(1.5f);
            yield return new WaitForEndOfFrame();

            Vector3 fromPos2 = Camera.main.transform.position;
            Quaternion fromRot2 = Camera.main.transform.rotation;

            Vector3 toPos = endLookTarget.position;
            Quaternion toRot = fromRot2;

            emotionDirector.PlayMoveAndRotateToNeutral(fromPos2, toPos, fromRot2, toRot, 1.5f);
        }

        // 6. 복원
        yield return new WaitForSeconds(2f);
        emotionDirector.SetPlayerVisible(player, true);
        emotionDirector.ResetToDefault();
        emotionDirector.EnablePlayerControl(player);
        gameObject.SetActive(false);
    }
}
