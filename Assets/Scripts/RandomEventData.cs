using UnityEngine;

[CreateAssetMenu(fileName = "New Random Event", menuName = "Game/Random Event")]
public class RandomEventData : ScriptableObject
{
    public string title;
    public string description;

    [Range(1, 100)]
    public int probability = 10;

    public RandomEventType eventType;

    public int balanceAmount;
    public int moodAmount;

    public int percentAmount;

    public int timePenalty;
    public int durationPeriods;

    public OpportunityData realtyToAdd;
}