using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class SaveManager : MonoBehaviour
{
    private string savePath;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "SaveData.json");
        Debug.Log($"[SaveManager] 경로 초기화됨: {savePath}");
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterSaveManager(this);
        }
        else
        {
            Debug.LogWarning("[SaveManager] GameManager 인스턴스를 찾을 수 없습니다.");
        }
    }

    public void Save(Vector3 position, string saveId)
    {
        SaveData data = new(position, saveId);
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            Debug.Log($"[SaveManager] 직렬화 완료:\n{json}");
            File.WriteAllText(savePath, json);
            Debug.Log($"[SaveManager] 저장 성공: {saveId} at {position}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] 저장 실패: {e.Message}");
        }
    }

    public bool TryLoad(out Vector3 position, out string saveId)
    {
        position = Vector3.zero;
        saveId = string.Empty;

        if (!File.Exists(savePath))
        {
            Debug.LogWarning("[SaveManager] 저장 파일 없음");
            return false;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonConvert.DeserializeObject<SaveData>(json);
            if (data == null)
                return false;

            position = data.Position;
            saveId = data.SaveId;
            Debug.Log($"[SaveManager] 로드 성공: {saveId} at {position}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] 로드 실패: {e.Message}");
            return false;
        }
    }

    public void Delete()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("[SaveManager] 저장 파일 삭제됨");
        }
    }

    public bool Exists()
    {
        return File.Exists(savePath);
    }
}
