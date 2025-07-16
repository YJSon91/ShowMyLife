using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class SaveLoader
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "SaveData.json");

    public static void Save(SaveData data)
    {
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(SavePath, json);
            Debug.Log("[SaveLoader] 저장 성공");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoader] 저장 실패: {e.Message}");
        }
    }

    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[SaveLoader] 저장 파일 없음");
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonConvert.DeserializeObject<SaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoader] 불러오기 실패: {e.Message}");
            return null;
        }
    }
}
