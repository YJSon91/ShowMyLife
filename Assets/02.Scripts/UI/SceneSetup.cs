using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    void Awake()
    {
        // 이 오브젝트에 붙어있는 IntroUIFlow 컴포넌트를 찾습니다.
        IntroUIFlow introFlow = GetComponent<IntroUIFlow>();

        // 만약 IntroUIFlow가 있다면 (그리고 비활성화 상태라면)
        if (introFlow != null)
        {
            // 스크립트를 활성화시켜 일을 시작하도록 합니다.
            introFlow.enabled = true;
            Debug.Log("[SceneSetup] IntroScene이므로, IntroUIFlow를 활성화합니다.");
        }
    }
}
