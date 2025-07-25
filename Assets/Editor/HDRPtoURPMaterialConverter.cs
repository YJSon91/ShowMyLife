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

            Texture baseMap = FindTexture(CORRECTION_PATH, baseName, new[] { "albedo", "basecolor", "bc" });
            Texture normalMap = FindTexture(CORRECTION_PATH, baseName, new[] { "normal", "n" });
            Texture maskMap = FindTexture(CORRECTION_PATH, baseName, new[] { "mask", "masks", "ao_r_mt", "occlusionroughnessmetallic" });

            // ⬇️ baseMap이 없고, normalMap이 있을 때 → normalMap 이름으로 유추해서 baseMap 다시 탐색
            if (baseMap == null && normalMap != null)
            {
                baseMap = FindTextureFromNormal(CORRECTION_PATH, normalMap, new[] { "albedo", "basecolor", "bc" });
            }

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
            .Replace("mat_", "")
            .Replace("-", "")
            .Trim();
    }

    private static Texture FindTexture(string root, string baseName, string[] suffixes)
    {
        string[] exts = { ".png", ".tga", ".jpg", ".jpeg", ".psd" };
        string baseNorm = NormalizeName(baseName);

        foreach (string file in Directory.GetFiles(root, "*.*", SearchOption.TopDirectoryOnly))
        {
            string lower = file.ToLower();
            if (!exts.Any(ext => lower.EndsWith(ext))) continue;

            string fileName = Path.GetFileNameWithoutExtension(file);
            string fileNorm = NormalizeName(fileName);

            bool suffixMatch = suffixes.Any(suffix => fileNorm.EndsWith(suffix.ToLower()));
            bool baseMatch = fileNorm.StartsWith(baseNorm);

            if (suffixMatch && baseMatch)
            {
                string assetPath = file.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                return AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
            }
        }

        return null;
    }

    private static Texture FindTextureFromNormal(string root, Texture normalMap, string[] baseSuffixes)
    {
        if (normalMap == null) return null;

        string normalPath = AssetDatabase.GetAssetPath(normalMap);
        string normalName = Path.GetFileNameWithoutExtension(normalPath).ToLower();

        string baseNameGuess = normalName
            .Replace("_normal", "")
            .Replace("_n", "")
            .Replace("normal", "")
            .TrimEnd('_');

        string[] exts = { ".png", ".tga", ".jpg", ".jpeg", ".psd" };

        foreach (string file in Directory.GetFiles(root, "*.*", SearchOption.TopDirectoryOnly))
        {
            string lower = file.ToLower();
            if (!exts.Any(ext => lower.EndsWith(ext))) continue;

            string fileName = Path.GetFileNameWithoutExtension(file).ToLower();

            bool suffixMatch = baseSuffixes.Any(suffix => fileName.Contains(suffix));
            bool fuzzyMatch = fileName.Contains(baseNameGuess);

            if (suffixMatch && fuzzyMatch)
            {
                string assetPath = file.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                Debug.Log($"[MaterialFixer] 노말 기반 매칭 성공 → {fileName}");
                return AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
            }
        }

        Debug.LogWarning($"[MaterialFixer] 노말 기반 BaseMap 추정 실패: {normalName}");
        return null;
    }
}
