using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    public string title;
    public string description;

    public int studyCost;
    public int studyTimeCost;
    public int studyPeriods;
}