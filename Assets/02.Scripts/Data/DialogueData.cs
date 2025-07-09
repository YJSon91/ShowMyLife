using System.Collections.Generic;


[System.Serializable]
public class DialogueData
{
    // 이제 모든 타입이 동일한 구조를 가집니다.
    public List<Dialogue> Start { get; set; }
    public List<Dialogue> End { get; set; }
    public List<Dialogue> Fall { get; set; }
    public List<Dialogue> Cheer { get; set; }
    // ... 다른 타입들도 필요하면 List<Dialogue>로 추가 ...
}
[System.Serializable]
public class Dialogue
{
    public string id;
    public string text;
}


