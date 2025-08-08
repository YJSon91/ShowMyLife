using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class SaveManager : MonoBehaviour
{
    private string savePath;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "SaveData.json");
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterSaveManager(this);
    }

    public void Save(Vector3 position, string saveId)
    {
        SaveData data = new(position, saveId);
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(savePath, json);
        }
        catch (System.Exception e)
        {
        }
    }

    public bool TryLoad(out Vector3 position, out string saveId)
    {
        position = Vector3.zero;
        saveId = string.Empty;

        if (!File.Exists(savePath))
            return false;

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonConvert.DeserializeObject<SaveData>(json);
            if (data == null)
                return false;

            position = data.Position;
            saveId = data.SaveId;
            return true;
        }
        catch (System.Exception e)
        {
            return false;
        }
    }

    public void Delete()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }

    public bool Exists()
    {
        return File.Exists(savePath);
    }
}
