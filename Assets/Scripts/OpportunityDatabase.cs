using UnityEngine;

public class OpportunityDatabase : MonoBehaviour
{
    public OpportunityData[] jobs;
    public OpportunityData[] businesses;
    public OpportunityData[] realty;
    public OpportunityData[] invests;

    public SkillData[] skills;
    public PersonData[] people;

    public RandomEventData[] luckEvents;
    public RandomEventData[] unluckEvents;
    public RandomEventData[] ultraLuckEvents;
    public RandomEventData[] ultraUnluckEvents;

    public OpportunityData FindOpportunityById(string id)
    {
        foreach (var item in jobs)
            if (item.id == id) return item;

        foreach (var item in businesses)
            if (item.id == id) return item;

        foreach (var item in realty)
            if (item.id == id) return item;

        foreach (var item in invests)
            if (item.id == id) return item;

        return null;
    }

    public SkillData FindSkillById(string id)
    {
        foreach (var skill in skills)
            if (skill.id == id) return skill;

        return null;
    }

    public PersonData FindPersonById(string id)
    {
        foreach (var person in people)
            if (person.id == id) return person;

        return null;
    }
}