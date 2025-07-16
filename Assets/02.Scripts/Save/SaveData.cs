using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class SaveData
{
    [JsonProperty("position")]
    public Vector3 Position { get; set; }

    [JsonProperty("saveId")]
    public string SaveId { get; set; }

    public SaveData(Vector3 position, string saveId)
    {
        Position = position;
        SaveId = saveId;
    }
}
