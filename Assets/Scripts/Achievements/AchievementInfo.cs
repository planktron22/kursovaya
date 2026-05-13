using System;

[Serializable]
public class AchievementInfo
{
    public string id;
    public string title;
    public string description;
    public bool unlocked;

    public AchievementInfo(string id, string title, string description)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.unlocked = false;
    }
}
