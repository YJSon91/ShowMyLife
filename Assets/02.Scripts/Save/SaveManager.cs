using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "SaveData.json");

    public static void Save(string id, Vector3 pos)
    {
        SaveData data = new SaveData
        {
            saveId = id,
            x = pos.x,
            y = pos.y,
            z = pos.z
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"[SaveManager] 저장 완료 → {SavePath}");
    }

    public static Vector3? Load(out string saveId)
    {
        saveId = null;

        if (!File.Exists(SavePath))
            return null;

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data == null)
            return null;

        saveId = data.saveId;
        return new Vector3(data.x, data.y, data.z);
    }

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }
}
