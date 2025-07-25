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
            if (mat == null || mat.shader == null) continue;

            string shaderName = mat.shader.name;
            if (shaderName == "Universal Render Pipeline/Lit") continue;

            bool isConvertible =
                shaderName.StartsWith("Shader Graphs/") ||
                shaderName.Contains("HDRP") ||
                shaderName == "Hidden/InternalErrorShader" ||
                shaderName.Contains("Standard") ||
                shaderName.StartsWith("Unreal/") ||
                shaderName == "Nimikko/MasterShader";

            if (!isConvertible) continue;

            string materialName = Path.GetFileNameWithoutExtension(path);
            string baseName = NormalizeName(materialName);
            string folder = Path.GetDirectoryName(path);
            string texturesFolder = folder.Replace("Materials", "Textures");

            // 텍스처 연결 시 이름 정규화 비교
            Texture baseMap = FindTexture("Assets/99.Externals", baseName, new[] { "Albedo", "BaseColor", "BC" });
            Texture normalMap = FindTexture("Assets/99.Externals", baseName, new[] { "Normal", "N" });
            Texture maskMap = FindTexture("Assets/99.Externals", baseName, new[] { "Mask", "Masks", "AO_R_MT", "OcclusionRoughnessMetallic" });

            // 셰이더 교체
            mat.shader = Shader.Find("Universal Render Pipeline/Lit");

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

            // 색상/속성 유지
            Color baseColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", mat.HasProperty("_Smoothness") ? mat.GetFloat("_Smoothness") : 0.8f);

            EditorUtility.SetDirty(mat);
            convertedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MaterialFixer] 변환 완료: {convertedCount}개 머티리얼");
    }

    // 이름 정규화
    private static string NormalizeName(string name)
    {
        return name.ToLower()
            .Replace("mi_", "")
            .Replace("ml_", "")
            .Replace("m_", "")
            .Replace("material_", "")
            .Replace("default", "")
            .Replace("_mat", "")
            .Replace("_01", "")
            .Replace("mat_", "")
            .Replace("mat", "")
            .Replace("_", "")
            .Trim();
    }

    // 비교
    private static Texture FindTexture(string root, string baseName, string[] suffixes)
    {
        string[] exts = { ".png", ".tga", ".jpg", ".jpeg", ".psd" };
        string[] files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories);

        string baseNorm = NormalizeName(baseName);

        foreach (string file in files)
        {
            if (!exts.Any(e => file.ToLower().EndsWith(e))) continue;

            string fileName = Path.GetFileNameWithoutExtension(file);
            string fileNorm = NormalizeName(fileName);

            bool suffixMatch = suffixes.Any(suf => fileNorm.Contains(suf.ToLower()));
            bool baseMatch = fileNorm.Contains(baseNorm) || baseNorm.Contains(fileNorm);

            if (suffixMatch && baseMatch)
            {
                string assetPath = file.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                if (tex != null)
                    return tex;
            }
        }

        return null;
    }
}
