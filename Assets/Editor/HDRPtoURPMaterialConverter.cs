using UnityEngine;
using UnityEditor;

public class MaterialFixer : EditorWindow
{
    [MenuItem("도구/머테리얼 수정")]
    public static void ConvertHDRPMaterialsToURP()
    {
        // 'Assets/99.Externals' 폴더 내부의 머티리얼만 검색
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/99.Externals" });

        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material hdrpMat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (hdrpMat == null || hdrpMat.shader == null)
                continue;

            string shaderName = hdrpMat.shader.name;

            // Shader Graph 또는 HDRP 기반 셰이더만 변환
            if (!shaderName.StartsWith("Shader Graphs/") && !shaderName.Contains("HDRP"))
                continue;

            // 기존 속성 추출
            Texture baseMap = hdrpMat.GetTexture("_BaseMap");
            Texture normalMap = hdrpMat.GetTexture("_NormalMap");
            Texture rmaMap = hdrpMat.GetTexture("_RMAMap");
            Color baseColor = hdrpMat.HasProperty("_BaseMapTint") ? hdrpMat.GetColor("_BaseMapTint") : Color.white;

            // URP/Lit 셰이더로 변경
            hdrpMat.shader = Shader.Find("Universal Render Pipeline/Lit");

            // 속성 재적용
            if (baseMap) hdrpMat.SetTexture("_BaseMap", baseMap);
            if (normalMap) hdrpMat.SetTexture("_BumpMap", normalMap);
            if (rmaMap) hdrpMat.SetTexture("_MetallicGlossMap", rmaMap);
            hdrpMat.SetColor("_BaseColor", baseColor);
            hdrpMat.SetFloat("_Smoothness", 0.8f);

            EditorUtility.SetDirty(hdrpMat);
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MaterialFixer] 변환 완료: {count}개 머티리얼");
    }
}
