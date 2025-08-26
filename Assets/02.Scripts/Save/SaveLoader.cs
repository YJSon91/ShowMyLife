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
        }
        catch (System.Exception e)
        {

        }
    }

    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
            return null;

        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonConvert.DeserializeObject<SaveData>(json);
        }
        catch (System.Exception e)
        {
            return null;
        }
    }

    public static void Delete()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public static bool Exists()
    {
        return File.Exists(SavePath);
    }
}

