using UnityEngine;

/// <summary>
/// 그림자 모양 타입
/// </summary>
public enum ShadowShape
{
    Circle,     // 원형
    Square,     // 사각형
    Ellipse     // 타원형
}

/// <summary>
/// 플레이어 공중 그림자 생성 및 관리 시스템
/// </summary>
public class PlayerShadowProjector : MonoBehaviour
{
    [Header("그림자 설정")]
    [SerializeField] private GameObject shadowObject;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask groundLayerMask = 1; // Ground 레이어
    
    [Header("플레이어 연동")]
    [SerializeField] private bool usePlayerMovementController = true; // PlayerMovementController와 연동
    [SerializeField] private PlayerMovementController playerMovementController; // 직접 참조
    
    [Header("그림자 크기 조절")]
    [SerializeField] private float shadowScale = 1f;
    [SerializeField] private float maxShadowDistance = 20f;
    
    [Header("박스캐스트 설정")]
    [SerializeField] private float boxCastWidth = 0.6f; // 박스캐스트 너비 (x축)
    [SerializeField] private float boxCastDepth = 0.6f; // 박스캐스트 깊이 (z축)
    [SerializeField] private float boxCastHeight = 0.05f; // 박스캐스트 높이 (y축)
    [SerializeField] private float shadowCheckDistance = 15f; // 그림자 감지 거리 (기본값: 15f)
    
    [Header("그림자 투명도")]
    [SerializeField] private float shadowAlpha = 0.6f;
    
    [Header("그림자 품질")]
    [SerializeField] private bool useSmoothShadow = true;
    [SerializeField] private float shadowFadeSpeed = 5f;
    
    [Header("그림자 자동 제어")]
    [SerializeField] private bool enableGroundDetection = true; // 지면 감지 기능 활성화
    
    [Header("그림자 모양")]
    [SerializeField] private ShadowShape shadowShape = ShadowShape.Circle;
    [SerializeField] private bool useSoftEdges = true;
    [SerializeField] private bool useQuadForCircle = true; // 원형도 Quad 사용 (권장)
    
    private Renderer shadowRenderer;
    private Material shadowMaterial;
    private bool isPlayerGrounded;
    private float currentAlpha;
    private float targetAlpha;
    
    private void Awake()
    {
        InitializeShadowProjector();
    }
    
    private void Start()
    {
        // 플레이어 자동 탐색
        if (playerTransform == null)
        {
            FindPlayer();
        }
        
        // 초기 알파값 설정
        currentAlpha = 0f;
        targetAlpha = 0f;
    }
    
    private void FindPlayer()
    {
        // GameManager를 통해 플레이어 찾기
        if (GameManager.Instance?.Player != null)
        {
            playerTransform = GameManager.Instance.Player.transform;
            
            // PlayerMovementController도 함께 찾기
            if (usePlayerMovementController && playerMovementController == null)
            {
                playerMovementController = playerTransform.GetComponent<PlayerMovementController>();
                if (playerMovementController == null)
                {
                    playerMovementController = playerTransform.GetComponentInParent<PlayerMovementController>();
                }
            }
        }
        else
        {
            // 태그로 플레이어 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                
                // PlayerMovementController도 함께 찾기
                if (usePlayerMovementController && playerMovementController == null)
                {
                    playerMovementController = player.GetComponent<PlayerMovementController>();
                    if (playerMovementController == null)
                    {
                        playerMovementController = player.GetComponentInParent<PlayerMovementController>();
                    }
                }
            }
        }
        
        if (playerTransform == null)
        {
            Debug.LogWarning("[PlayerShadowProjector] 플레이어를 찾을 수 없습니다!");
        }
        
        if (usePlayerMovementController && playerMovementController == null)
        {
            Debug.LogWarning("[PlayerShadowProjector] PlayerMovementController를 찾을 수 없습니다!");
        }
    }
    
    private void InitializeShadowProjector()
    {
        if (shadowObject == null)
        {
            // 그림자 오브젝트 자동 생성
            CreateShadowObject();
        }
        
        // 그림자 렌더러 및 머티리얼 설정
        SetupShadowRenderer();
    }
    
    private void CreateShadowObject()
    {
        // 선택된 모양에 따라 그림자 오브젝트 생성
        switch (shadowShape)
        {
            case ShadowShape.Circle:
                if (useQuadForCircle)
                {
                    // 원형 그림자 (Quad 사용 - 물리적 문제 없음)
                    shadowObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    // 원형 텍스처를 위한 머티리얼 설정은 SetupShadowRenderer에서 처리
                }
                else
                {
                    // 원형 그림자 (Cylinder 사용 - 물리 콜라이더 제거 필요)
                    shadowObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    shadowObject.transform.localScale = new Vector3(1f, 0.01f, 1f);
                }
                break;
                
            case ShadowShape.Square:
                // 사각형 그림자 (Quad)
                shadowObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                break;
                
            case ShadowShape.Ellipse:
                if (useQuadForCircle)
                {
                    // 타원형 그림자 (Quad 사용)
                    shadowObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    shadowObject.transform.localScale = new Vector3(1.5f, 1f, 1f);
                }
                else
                {
                    // 타원형 그림자 (Cylinder 사용)
                    shadowObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    shadowObject.transform.localScale = new Vector3(1.5f, 0.01f, 1f);
                }
                break;
                
            default:
                shadowObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                break;
        }
        
        shadowObject.name = "PlayerShadow";
        shadowObject.transform.SetParent(transform);
        shadowObject.transform.localPosition = Vector3.zero;
        shadowObject.transform.localRotation = Quaternion.Euler(90, 0, 0); // 땅에 평행하게
        
        // 그림자 오브젝트를 투명하게 설정
        shadowObject.layer = LayerMask.NameToLayer("TransparentFX");
        
        // 물리 콜라이더 제거 (그림자는 시각적으로만 보여야 함)
        Collider shadowCollider = shadowObject.GetComponent<Collider>();
        if (shadowCollider != null)
        {
            DestroyImmediate(shadowCollider);
        }
        
        // 그림자 렌더러 참조
        shadowRenderer = shadowObject.GetComponent<Renderer>();
    }
    
    private void SetupShadowRenderer()
    {
        if (shadowRenderer == null) return;
        
        // 그림자용 머티리얼 생성
        shadowMaterial = new Material(Shader.Find("Custom/SimpleShadow"));
        if (shadowMaterial == null)
        {
            // 커스텀 셰이더가 없으면 기본 셰이더 사용
            shadowMaterial = new Material(Shader.Find("Standard"));
            Debug.LogWarning("[PlayerShadowProjector] Custom/SimpleShadow 셰이더를 찾을 수 없어 Standard 셰이더를 사용합니다.");
        }
        
        // 그림자 모양에 따른 텍스처 설정
        if (shadowShape == ShadowShape.Circle && useQuadForCircle)
        {
            // 원형 텍스처 생성 및 적용
            Texture2D circleTexture = CreateCircleTexture();
            shadowMaterial.mainTexture = circleTexture;
        }
        
        // 그림자 머티리얼 설정
        shadowMaterial.SetColor("_Color", new Color(0, 0, 0, shadowAlpha));
        shadowMaterial.SetFloat("_Mode", 3); // Transparent 모드
        shadowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        shadowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        shadowMaterial.SetInt("_ZWrite", 0);
        shadowMaterial.DisableKeyword("_ALPHATEST_ON");
        shadowMaterial.EnableKeyword("_ALPHABLEND_ON");
        shadowMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        shadowMaterial.renderQueue = 3000; // Transparent 큐
        
        shadowRenderer.material = shadowMaterial;
    }
    
    /// <summary>
    /// 원형 텍스처 생성
    /// </summary>
    private Texture2D CreateCircleTexture()
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size);
        
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = 0f;
                
                if (distance <= radius)
                {
                    // 원형 영역 내부
                    if (useSoftEdges)
                    {
                        // 부드러운 가장자리
                        float edgeDistance = radius - distance;
                        alpha = Mathf.Clamp01(edgeDistance / (radius * 0.1f)); // 10% 영역에서 페이드
                    }
                    else
                    {
                        // 선명한 가장자리
                        alpha = 1f;
                    }
                }
                
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        
        return texture;
    }
    
    private void Update()
    {
        if (playerTransform == null) return;
        
        UpdateShadowPosition();
        UpdateShadowVisibility();
        UpdateShadowFade();
    }
    
    private void UpdateShadowPosition()
    {
        if (!enableGroundDetection)
        {
            // 지면 감지가 비활성화된 경우 항상 그림자 표시
            isPlayerGrounded = false;
            targetAlpha = shadowAlpha;
            return;
        }
        
        // PlayerMovementController와 연동하여 지면 상태 확인
        if (usePlayerMovementController && playerMovementController != null)
        {
            // PlayerMovementController의 지면 상태 직접 사용
            isPlayerGrounded = playerMovementController.IsGrounded;
            
            if (isPlayerGrounded)
            {
                // 땅에 닿아있으면 그림자 숨김
                targetAlpha = 0f;
                return;
            }
            
                         // 공중에 있을 때만 그림자 위치 계산
             Vector3 playerPosition = playerTransform.position;
             RaycastHit hit;
             
             // 박스캐스트로 지면 확인 (PlayerMovementController와 동일한 방식)
             Vector3 boxCenter = playerPosition;
             Vector3 boxHalfExtents = new Vector3(boxCastWidth / 2f, boxCastHeight / 2f, boxCastDepth / 2f);
             Quaternion orientation = playerTransform.rotation;
             
                          if (Physics.BoxCast(boxCenter, boxHalfExtents, Vector3.down, out hit, orientation, shadowCheckDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
             {
                 // 그림자 위치는 플레이어 발 아래에 고정 (x, z는 플레이어 위치, y는 감지된 지면 높이)
                 Vector3 shadowPosition = new Vector3(
                     playerTransform.position.x,  // 플레이어 x 위치
                     hit.point.y + 0.1f,         // 감지된 지면 높이 + 약간 위
                     playerTransform.position.z   // 플레이어 z 위치
                 );
                 transform.position = shadowPosition;
                 
                 // 그림자 크기를 높이에 따라 조절
                 float heightRatio = Mathf.Clamp01((shadowCheckDistance - hit.distance) / shadowCheckDistance);
                 float currentScale = shadowScale * (0.5f + heightRatio * 0.5f);
                 
                 if (shadowObject != null)
                 {
                     shadowObject.transform.localScale = Vector3.one * currentScale;
                 }
                 
                 targetAlpha = shadowAlpha;
             }
            else
            {
                // 땅이 감지되지 않으면 그림자 숨김
                targetAlpha = 0f;
            }
        }
        else
        {
                         // 기존 방식 (PlayerMovementController 없을 때) - 박스캐스트 사용
             Vector3 playerPosition = playerTransform.position;
             RaycastHit hit;
             
             // 박스캐스트로 지면 확인
             Vector3 boxCenter = playerPosition;
             Vector3 boxHalfExtents = new Vector3(boxCastWidth / 2f, boxCastHeight / 2f, boxCastDepth / 2f);
             Quaternion orientation = playerTransform.rotation;
             
                          if (Physics.BoxCast(boxCenter, boxHalfExtents, Vector3.down, out hit, orientation, maxShadowDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
             {
                 // 그림자 위치는 플레이어 발 아래에 고정 (x, z는 플레이어 위치, y는 감지된 지면 높이)
                 Vector3 shadowPosition = new Vector3(
                     playerTransform.position.x,  // 플레이어 x 위치
                     hit.point.y + 0.1f,         // 감지된 지면 높이 + 약간 위
                     playerTransform.position.z   // 플레이어 z 위치
                 );
                 transform.position = shadowPosition;
                 
                 // 그림자 크기를 높이에 따라 조절
                 float heightRatio = Mathf.Clamp01((maxShadowDistance - hit.distance) / maxShadowDistance);
                 float currentScale = shadowScale * (0.5f + heightRatio * 0.5f);
                 
                 if (shadowObject != null)
                 {
                     shadowObject.transform.localScale = Vector3.one * currentScale;
                 }
                 
                 // 플레이어가 공중에 있음
                 isPlayerGrounded = false;
                 targetAlpha = shadowAlpha;
             }
            else
            {
                // 땅이 감지되지 않으면 그림자 숨김
                isPlayerGrounded = true;
                targetAlpha = 0f;
            }
        }
    }
    
    private void UpdateShadowVisibility()
    {
        // PlayerMovementController와 연동된 경우 지면 상태는 이미 UpdateShadowPosition에서 처리됨
        // 여기서는 추가적인 가시성 로직만 처리
        if (usePlayerMovementController && playerMovementController != null)
        {
            // PlayerMovementController와 연동된 경우 추가 처리 불필요
            return;
        }
        
        // 기존 방식 (PlayerMovementController 없을 때)
        if (enableGroundDetection && isPlayerGrounded)
        {
            targetAlpha = 0f;
        }
        else if (!isPlayerGrounded)
        {
            targetAlpha = shadowAlpha;
        }
    }
    
    private void UpdateShadowFade()
    {
        // 부드러운 페이드 효과
        if (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
        {
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * shadowFadeSpeed);
            
            if (shadowMaterial != null)
            {
                Color currentColor = shadowMaterial.GetColor("_Color");
                currentColor.a = currentAlpha;
                shadowMaterial.SetColor("_Color", currentColor);
            }
        }
        
        // 그림자 오브젝트 활성화/비활성화
        if (shadowObject != null)
        {
            shadowObject.SetActive(currentAlpha > 0.01f);
        }
    }
    
    /// <summary>
    /// 그림자 투명도 동적 조절
    /// </summary>
    public void SetShadowAlpha(float alpha)
    {
        shadowAlpha = Mathf.Clamp01(alpha);
        targetAlpha = shadowAlpha;
    }
    
    /// <summary>
    /// 그림자 크기 동적 조절
    /// </summary>
    public void SetShadowScale(float scale)
    {
        shadowScale = Mathf.Max(0.1f, scale);
        if (shadowObject != null)
        {
            shadowObject.transform.localScale = Vector3.one * shadowScale;
        }
    }
    
    /// <summary>
    /// 그림자 최대 거리 조절
    /// </summary>
    public void SetMaxShadowDistance(float distance)
    {
        maxShadowDistance = Mathf.Max(1f, distance);
    }
    
    /// <summary>
    /// 그림자 페이드 속도 조절
    /// </summary>
    public void SetShadowFadeSpeed(float speed)
    {
        shadowFadeSpeed = Mathf.Max(0.1f, speed);
    }
    
    /// <summary>
    /// 그림자 활성화/비활성화
    /// </summary>
    public void SetShadowEnabled(bool enabled)
    {
        if (shadowObject != null)
        {
            shadowObject.SetActive(enabled);
        }
        
        if (!enabled)
        {
            targetAlpha = 0f;
        }
    }
    
    /// <summary>
    /// 그림자 모양 변경
    /// </summary>
    public void SetShadowShape(ShadowShape newShape)
    {
        if (shadowShape == newShape) return;
        
        shadowShape = newShape;
        
        // 기존 그림자 오브젝트 제거
        if (shadowObject != null)
        {
            DestroyImmediate(shadowObject);
        }
        
        // 새로운 모양으로 그림자 재생성
        CreateShadowObject();
        SetupShadowRenderer();
    }
    
    /// <summary>
    /// 부드러운 가장자리 사용 여부 설정
    /// </summary>
    public void SetSoftEdges(bool useSoft)
    {
        useSoftEdges = useSoft;
        
        if (shadowMaterial != null)
        {
            // 부드러운 가장자리를 위한 머티리얼 설정
            if (useSoft)
            {
                shadowMaterial.EnableKeyword("_ALPHABLEND_ON");
                shadowMaterial.renderQueue = 3000;
            }
            else
            {
                shadowMaterial.DisableKeyword("_ALPHABLEND_ON");
                shadowMaterial.renderQueue = 2000;
            }
        }
    }
    

    
    /// <summary>
    /// 지면 감지 기능 활성화/비활성화
    /// </summary>
    public void SetGroundDetectionEnabled(bool enabled)
    {
        enableGroundDetection = enabled;
        
        if (!enabled)
        {
            // 지면 감지가 비활성화되면 항상 그림자 표시
            isPlayerGrounded = false;
            targetAlpha = shadowAlpha;
        }
        else
        {
            // 지면 감지가 활성화되면 현재 상태에 따라 그림자 표시/숨김
            if (isPlayerGrounded)
            {
                targetAlpha = 0f;
            }
            else
            {
                targetAlpha = shadowAlpha;
            }
        }
    }
    
    /// <summary>
    /// 현재 지면 상태 강제 설정 (디버깅용)
    /// </summary>
    public void ForceGroundState(bool grounded)
    {
        isPlayerGrounded = grounded;
        
        if (enableGroundDetection && grounded)
        {
            targetAlpha = 0f;
        }
        else if (!grounded)
        {
            targetAlpha = shadowAlpha;
        }
    }
    
    /// <summary>
    /// PlayerMovementController 연동 설정
    /// </summary>
    public void SetPlayerMovementControllerIntegration(bool enabled)
    {
        usePlayerMovementController = enabled;
        
        if (enabled && playerMovementController == null)
        {
            // 자동으로 PlayerMovementController 찾기
            FindPlayer();
        }
    }
    
         /// <summary>
     /// PlayerMovementController 직접 할당
     /// </summary>
     public void SetPlayerMovementController(PlayerMovementController controller)
     {
         playerMovementController = controller;
         usePlayerMovementController = controller != null;
     }
     
     /// <summary>
     /// 박스캐스트 크기 설정
     /// </summary>
     public void SetBoxCastSize(float width, float depth, float height)
     {
         boxCastWidth = Mathf.Max(0.1f, width);
         boxCastDepth = Mathf.Max(0.1f, depth);
         boxCastHeight = Mathf.Max(0.01f, height);
     }
     
     /// <summary>
     /// 박스캐스트 너비 설정
     /// </summary>
     public void SetBoxCastWidth(float width)
     {
         boxCastWidth = Mathf.Max(0.1f, width);
     }
     
     /// <summary>
     /// 박스캐스트 깊이 설정
     /// </summary>
     public void SetBoxCastDepth(float depth)
     {
         boxCastDepth = Mathf.Max(0.1f, depth);
     }
     
     /// <summary>
     /// 그림자 감지 거리 설정
     /// </summary>
     public void SetShadowCheckDistance(float distance)
     {
         shadowCheckDistance = Mathf.Max(1f, distance);
     }
    
    /// <summary>
    /// 현재 그림자 상태 정보 반환
    /// </summary>
    public bool IsShadowVisible => currentAlpha > 0.01f;
    public bool IsPlayerGrounded => isPlayerGrounded;
    public float CurrentShadowAlpha => currentAlpha;
    public ShadowShape CurrentShadowShape => shadowShape;
    public bool IsUsingSoftEdges => useSoftEdges;
    public bool IsGroundDetectionEnabled => enableGroundDetection;
    public bool IsPlayerMovementControllerIntegrated => usePlayerMovementController && playerMovementController != null;
    
         // 디버그 정보 표시
     private void OnDrawGizmosSelected()
     {
         if (playerTransform == null) return;
         
         // 박스캐스트 시각화
         Gizmos.color = Color.yellow;
         Vector3 boxCenter = playerTransform.position;
         Vector3 boxHalfExtents = new Vector3(boxCastWidth / 2f, boxCastHeight / 2f, boxCastDepth / 2f);
         Vector3 endPosition = boxCenter + Vector3.down * shadowCheckDistance; // shadowCheckDistance 사용
         
         // 시작 박스
         Gizmos.DrawWireCube(boxCenter, boxHalfExtents * 2);
         // 끝 박스
         Gizmos.DrawWireCube(endPosition, boxHalfExtents * 2);
         // 연결선
         Gizmos.DrawLine(boxCenter, endPosition);
         
         // 그림자 위치 표시
         if (transform.position != Vector3.zero)
         {
             Gizmos.color = Color.red;
             Gizmos.DrawWireSphere(transform.position, 0.5f);
         }
     }
}
