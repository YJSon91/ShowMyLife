using UnityEngine;

public static class SaveManager
{
    public static void Save(Vector3 position, string saveId)
    {
        SaveData data = new(position, saveId);
        SaveLoader.Save(data);
    }

    public static Vector3? Load(out string saveId)
    {
        saveId = string.Empty;

        SaveData data = SaveLoader.Load();
        if (data == null)
            return null;

        saveId = data.SaveId;
        return data.Position;
    }

    public static void Delete()
    {
        SaveLoader.Delete();
    }

    public static bool Exists()
    {
        return SaveLoader.Exists();
    }
}
