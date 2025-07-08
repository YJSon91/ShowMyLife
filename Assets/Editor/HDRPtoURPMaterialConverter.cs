using UnityEngine;
using UnityEditor;

public class MaterialFixer : EditorWindow
{
    [MenuItem("도구/머테리얼 수정")]
    public static void ConvertAllMaterialsToURP()
    {
        // 'Assets/99.Externals' 경로 아래의 모든 머테리얼(.mat) 검색
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/99.Externals" });

        int convertedCount = 0;

        foreach (string guid in guids)
        {
            // GUID를 경로로 변환 → 머티리얼 로드
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null || mat.shader == null)
                continue;

            string shaderName = mat.shader.name;

            if (shaderName == "Universal Render Pipeline/Lit")
                continue;

            // 변환 대상 조건: Shader Graph / HDRP / InternalError / Built-in Standard
            bool isConvertible =
                shaderName.StartsWith("Shader Graphs/") ||
                shaderName.Contains("HDRP") ||
                shaderName == "Hidden/InternalErrorShader" ||
                shaderName.Contains("Standard");

            if (!isConvertible)
                continue;

            // 기존 텍스처 추출
            Texture baseMap = mat.GetTexture("_BaseMap")
                            ?? mat.GetTexture("_MainTex")
                            ?? mat.GetTexture("_Albedo");

            Texture normalMap = mat.GetTexture("_NormalMap")
                              ?? mat.GetTexture("_BumpMap");

            Texture metallicMap = mat.GetTexture("_MetallicGlossMap")
                                ?? mat.GetTexture("_SpecGlossMap")
                                ?? mat.GetTexture("_RMAMap");

            // 색상 추출
            Color baseColor = Color.white;
            if (mat.HasProperty("_BaseColor")) baseColor = mat.GetColor("_BaseColor");
            else if (mat.HasProperty("_Color")) baseColor = mat.GetColor("_Color");
            else if (mat.HasProperty("_BaseMapTint")) baseColor = mat.GetColor("_BaseMapTint");

            // Smoothness 추출
            float smoothness = 0.8f;
            if (mat.HasProperty("_Smoothness")) smoothness = mat.GetFloat("_Smoothness");

            // 셰이더를 URP/Lit으로 교체
            mat.shader = Shader.Find("Universal Render Pipeline/Lit");

            // 속성 재설정
            if (baseMap) mat.SetTexture("_BaseMap", baseMap);
            if (normalMap) mat.SetTexture("_BumpMap", normalMap);
            if (metallicMap) mat.SetTexture("_MetallicGlossMap", metallicMap);

            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);

            // 변경 사항 표시
            EditorUtility.SetDirty(mat);
            convertedCount++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[MaterialFixer] 변환 완료: {convertedCount}개 머티리얼");
    }
}

