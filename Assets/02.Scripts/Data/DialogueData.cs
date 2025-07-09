using System.Collections.Generic;

[System.Serializable]
public class FallDialogues
{
    public List<Dialogue> Low;
    public List<Dialogue> Middle;
    public List<Dialogue> High;
}

// 기존 DialogueData 수정
[System.Serializable]
public class DialogueData
{
    public FallDialogues Fall; // fall의 타입을 FallDialogues로 변경
    public List<Dialogue> Cheer;
    public List<Dialogue> Process;
    public List<Dialogue> Reach;
}
