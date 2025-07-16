using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class SaveManager
{
    private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "SaveData.json");

    public static void Save(Vector3 position, string saveId)
    {
        SaveData data = new(position, saveId);
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

        try
        {
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"[SaveManager] 저장 완료 → {saveId} at {position}");
        }
        catch (IOException e)
        {
            Debug.LogError($"[SaveManager] 저장 실패: {e.Message}");
        }
    }

    public static Vector3? Load(out string saveId)
    {
        saveId = string.Empty;

        if (!File.Exists(SaveFilePath))
            return null;

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SaveData data = JsonConvert.DeserializeObject<SaveData>(json);

            if (data != null)
            {
                saveId = data.SaveId;
                return data.Position;
            }
        }
        catch (JsonException e)
        {
            Debug.LogError($"[SaveManager] 로드 실패 (JSON 오류): {e.Message}");
        }
        catch (IOException e)
        {
            Debug.LogError($"[SaveManager] 로드 실패 (IO 오류): {e.Message}");
        }

        return null;
    }

    public static void Delete()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            Debug.Log("[SaveManager] 저장 파일 삭제됨");
        }
    }

    public static bool Exists()
    {
        return File.Exists(SaveFilePath);
    }
}
