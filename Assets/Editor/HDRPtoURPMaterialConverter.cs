using UnityEngine;
using UnityEditor;

public class MaterialFixer : EditorWindow
{
    [MenuItem("도구/머테리얼 수정")]
    public static void ConvertAllMaterialsToURP()
    {
        // 'Assets/99.Externals' 폴더 내부의 모든 머티리얼 검색
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/99.Externals" });

        int convertedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null || mat.shader == null)
                continue;

            string shaderName = mat.shader.name;

            // 이미 URP Lit인 경우 스킵
            if (shaderName == "Universal Render Pipeline/Lit")
                continue;

            // 모든 비-URP 셰이더 포함 (Shader Graph, HDRP, InternalErrorShader 등)
            bool isConvertible =
                shaderName.StartsWith("Shader Graphs/") ||
                shaderName.Contains("HDRP") ||
                shaderName == "Hidden/InternalErrorShader";

            if (!isConvertible)
                continue;

            // 기존 속성 추출 (없는 경우 무시됨)
            Texture baseMap = mat.GetTexture("_BaseMap");
            Texture normalMap = mat.GetTexture("_NormalMap") ?? mat.GetTexture("_BumpMap");
            Texture metallicMap = mat.GetTexture("_MaskMap") ?? mat.GetTexture("_MetallicGlossMap") ?? mat.GetTexture("_RMAMap");
            Color baseColor = Color.white;

            if (mat.HasProperty("_BaseColor")) baseColor = mat.GetColor("_BaseColor");
            else if (mat.HasProperty("_BaseMapTint")) baseColor = mat.GetColor("_BaseMapTint");

            float smoothness = 0.8f;
            if (mat.HasProperty("_Smoothness")) smoothness = mat.GetFloat("_Smoothness");

            // URP Lit 셰이더로 전환
            mat.shader = Shader.Find("Universal Render Pipeline/Lit");

            // 속성 재적용
            if (baseMap) mat.SetTexture("_BaseMap", baseMap);
            if (normalMap) mat.SetTexture("_BumpMap", normalMap);
            if (metallicMap) mat.SetTexture("_MetallicGlossMap", metallicMap);

            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);

            EditorUtility.SetDirty(mat);
            convertedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MaterialFixer] 변환 완료: {convertedCount}개 머티리얼");
    }
}
