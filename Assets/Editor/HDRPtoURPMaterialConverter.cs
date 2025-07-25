using UnityEngine;
using UnityEditor;

public class MaterialFixer : EditorWindow
{
    [MenuItem("도구/머테리얼 수정")]
    public static void ConvertAllMaterialsToURP()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/99.Externals" });

        int convertedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null || mat.shader == null)
                continue;

            string shaderName = mat.shader.name;

            if (shaderName == "Universal Render Pipeline/Lit")
                continue;

            bool isConvertible =
                shaderName.StartsWith("Shader Graphs/") ||
                shaderName.Contains("HDRP") ||
                shaderName == "Hidden/InternalErrorShader" ||
                shaderName.Contains("Standard") ||
                shaderName.StartsWith("Unreal/");

            if (!isConvertible)
                continue;

            // 텍스처 추출
            Texture baseMap = mat.GetTexture("_BaseMap")
                            ?? mat.GetTexture("_MainTex")
                            ?? mat.GetTexture("_Albedo")
                            ?? mat.GetTexture("BC");

            Texture normalMap = mat.GetTexture("_NormalMap")
                              ?? mat.GetTexture("_BumpMap")
                              ?? mat.GetTexture("N");

            Texture maskMap = mat.GetTexture("_MaskMap")
                            ?? mat.GetTexture("_MetallicGlossMap")
                            ?? mat.GetTexture("_SpecGlossMap")
                            ?? mat.GetTexture("_RMAMap")
                            ?? mat.GetTexture("AO_R_MT");

            // 색상 추출
            Color baseColor = Color.white;
            if (mat.HasProperty("_BaseColor")) baseColor = mat.GetColor("_BaseColor");
            else if (mat.HasProperty("_Color")) baseColor = mat.GetColor("_Color");
            else if (mat.HasProperty("_BaseMapTint")) baseColor = mat.GetColor("_BaseMapTint");

            float smoothness = 0.8f;
            if (mat.HasProperty("_Smoothness")) smoothness = mat.GetFloat("_Smoothness");

            // 셰이더 교체
            mat.shader = Shader.Find("Universal Render Pipeline/Lit");

            // 텍스처 재설정
            if (baseMap) mat.SetTexture("_BaseMap", baseMap);
            if (normalMap)
            {
                mat.SetTexture("_BumpMap", normalMap);
                mat.EnableKeyword("_NORMALMAP");
            }
            if (maskMap)
            {
                mat.SetTexture("_MaskMap", maskMap);
                mat.EnableKeyword("_MASKMAP");
                mat.SetFloat("_Metallic", 1.0f);
            }

            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);

            EditorUtility.SetDirty(mat);
            convertedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MaterialFixer] 변환 완료: {convertedCount}개 머티리얼");
    }
}
