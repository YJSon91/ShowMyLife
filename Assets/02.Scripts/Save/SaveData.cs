 using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class SaveData
{
    [JsonProperty("x")] public float x;
    [JsonProperty("y")] public float y;
    [JsonProperty("z")] public float z;

    [JsonProperty("saveId")] public string SaveId;

    [JsonIgnore]
    public Vector3 Position
    {
        get => new Vector3(x, y, z);
        set
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
    }

    public SaveData(Vector3 position, string saveId)
    {
        x = position.x;
        y = position.y;
        z = position.z;
        SaveId = saveId;
    }

    public SaveData() { }
}
