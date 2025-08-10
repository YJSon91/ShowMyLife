using UnityEngine;
using System.Collections;
using System.IO;

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
        string path = Path.Combine(Application.persistentDataPath, "SaveData.json");
        bool hasSave = File.Exists(path);

        // 세이브가 없으면(첫 진입) 시네마틱 실행, 있으면 스킵
        if (!hasSave)
        {
            StartCoroutine(PlayWakeUpSequence());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator PlayWakeUpSequence()
    {
        if (emotionDirector == null)
            yield break;
        

        if (player == null)
        {
            // GameManager에서 플레이어 자동 탐색 보조
            var gmPlayer = GameManager.Instance != null ? GameManager.Instance.Player : null;
            player = gmPlayer != null ? gmPlayer.transform : player;

            if (player == null)
                yield break;
        }

        // 조작 비활성화 + 캐릭터 숨김
        emotionDirector.DisablePlayerControl(player);
        emotionDirector.SetPlayerVisible(player, false);

        // 0. 화면 검정
        if (emotionDirector.PostProcessing != null)
            emotionDirector.PostProcessing.ApplyColorFilter(Color.black, 0f);
        yield return null;

        // 1. 시작 위치
        Transform startTarget = emotionDirector.GetLookTargetTransform(5);
        if (startTarget == null)
            yield break;

        // 2. 카메라 초기화
        emotionDirector.ResetThemeCamera();
        emotionDirector.ThemeCamera.SetPosition(startTarget.position);
        emotionDirector.ThemeCamera.SetRotation(new Vector3(-70f, 0f, 0f));
        yield return new WaitForSeconds(lookDuration);

        // 3. 화면 밝히기
        if (emotionDirector.PostProcessing != null)
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

            Vector3 fromPos2 = Camera.main != null ? Camera.main.transform.position : emotionDirector.ThemeCamera.transform.position;
            Quaternion fromRot2 = Camera.main != null ? Camera.main.transform.rotation : emotionDirector.ThemeCamera.transform.rotation;

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
