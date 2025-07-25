using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class MaterialFixer : EditorWindow
{
    private const string CORRECTION_PATH = "Assets/99.Externals/Correction";

    [MenuItem("도구/머테리얼 수정")]
    public static void ConvertAllMaterialsToURP()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { CORRECTION_PATH });

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

            // Correction 폴더 내에서만 텍스처 검색
            Texture baseMap = FindTexture(CORRECTION_PATH, baseName, new[] { "albedo", "basecolor", "bc" });
            Texture normalMap = FindTexture(CORRECTION_PATH, baseName, new[] { "normal", "n" });
            Texture maskMap = FindTexture(CORRECTION_PATH, baseName, new[] { "mask", "masks", "ao_r_mt", "occlusionroughnessmetallic" });

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

            Color baseColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", mat.HasProperty("_Smoothness") ? mat.GetFloat("_Smoothness") : 0.8f);

            EditorUtility.SetDirty(mat);
            convertedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MaterialFixer] 변환 완료: {convertedCount}개 머티리얼");
    }

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

    private static Texture FindTexture(string root, string baseName, string[] suffixes)
    {
        string[] exts = { ".png", ".tga", ".jpg", ".jpeg", ".psd" };
        string baseNorm = NormalizeName(baseName);

        foreach (string file in Directory.GetFiles(root, "*.*", SearchOption.AllDirectories))
        {
            string lower = file.ToLower();
            if (!exts.Any(ext => lower.EndsWith(ext))) continue;

            string fileName = Path.GetFileNameWithoutExtension(file);
            string fileNorm = NormalizeName(fileName);

            // 수정된 조건: 끝에 붙거나, 포함만 되어도 허용
            bool suffixMatch = suffixes.Any(suffix =>
                fileNorm.EndsWith(suffix.ToLower()) || fileNorm.Contains(suffix.ToLower())
            );

            bool baseMatch = fileNorm.Contains(baseNorm) || baseNorm.Contains(fileNorm);

            if (suffixMatch && baseMatch)
            {
                string assetPath = file.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                return AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
            }
        }

        return null;
    }
}
