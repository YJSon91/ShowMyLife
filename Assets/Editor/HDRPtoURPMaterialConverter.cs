using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

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

            // 변환 가능한 셰이더들
            bool isConvertible =
                shaderName.StartsWith("Shader Graphs/") ||
                shaderName.Contains("HDRP") ||
                shaderName == "Hidden/InternalErrorShader" ||
                shaderName.Contains("Standard") ||
                shaderName.StartsWith("Unreal/") ||
                shaderName == "Nimikko/MasterShader";

            if (!isConvertible)
                continue;

            string materialName = Path.GetFileNameWithoutExtension(path);
            string baseName = materialName.StartsWith("M_") ? materialName.Substring(2) : materialName;

            string folder = Path.GetDirectoryName(path).Replace("Materials", "Textures");

            // 이름 기반 텍스처 찾기
            Texture baseMap = FindTexture(folder, baseName, new[] { "BC", "BaseColor" });
            Texture normalMap = FindTexture(folder, baseName, new[] { "N", "Normal" });
            Texture maskMap = FindTexture(folder, baseName, new[] { "AO_R_MT", "OcclusionRoughnessMetallic" });

            // 셰이더 교체
            mat.shader = Shader.Find("Universal Render Pipeline/Lit");

            // 텍스처 할당
            if (baseMap) mat.SetTexture("_BaseMap", baseMap);
            else Debug.LogWarning($"[MaterialFixer] BaseMap 없음: {path}");

            if (normalMap)
            {
                mat.SetTexture("_BumpMap", normalMap);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (maskMap)
            {
                mat.SetTexture("_MaskMap", maskMap);
                mat.EnableKeyword("_MASKMAP");
                mat.SetFloat("_Metallic", 1f);
            }

            // 색상/속성 복사
            Color baseColor = Color.white;
            if (mat.HasProperty("_BaseColor")) baseColor = mat.GetColor("_BaseColor");
            else if (mat.HasProperty("_Color")) baseColor = mat.GetColor("_Color");
            mat.SetColor("_BaseColor", baseColor);

            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", mat.GetFloat("_Smoothness"));
            else
                mat.SetFloat("_Smoothness", 0.8f);

            EditorUtility.SetDirty(mat);
            convertedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MaterialFixer] 변환 완료: {convertedCount}개 머티리얼");
    }

    // 이름 기반 텍스처 검색
    private static Texture FindTexture(string folder, string baseName, string[] suffixes)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { folder });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(assetPath).ToLower();
            string baseLower = baseName.ToLower();

            foreach (string suffix in suffixes)
            {
                if (fileName.Contains(baseLower) && fileName.Contains(suffix.ToLower()))
                {
                    Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                    if (tex != null)
                        return tex;
                }
            }
        }
        return null;
    }
}
