using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class SkillItemUI : MonoBehaviour
{
    public Text titleText;
    public Text descriptionText;
    public Text costText;
    public Text timeText;
    public Text periodsText;
    public Text statusText;

    public Button studyButton;

    private SkillData skill;
    private PlayerStats player;

    private CultureInfo culture = new CultureInfo("ru-RU");

    string Format(int value)
    {
        return value.ToString("N0", culture);
    }

    public void Setup(SkillData data, PlayerStats stats)
    {
        skill = data;
        player = stats;

        titleText.text = skill.title;
        descriptionText.text = skill.description;
        costText.text = "Стоимость: " + Format(skill.studyCost) + " р.";
        timeText.text = "Время: " + skill.studyTimeCost + " ч.";
        periodsText.text = "Срок: " + skill.studyPeriods + " периодов";

        UpdateStatus();
    }

    void UpdateStatus()
    {
        if (player.HasSkill(skill))
        {
            statusText.text = "Изучено";
            studyButton.gameObject.SetActive(false);
            return;
        }

        PlayerSkillStudy study = GetStudy();

        if (study != null)
        {
            statusText.text = "Изучается. Осталось: " + study.remainingPeriods;
            studyButton.gameObject.SetActive(false);
            return;
        }

        statusText.text = "Не изучено";
        studyButton.gameObject.SetActive(true);
    }

    PlayerSkillStudy GetStudy()
    {
        foreach (var study in player.studyingSkills)
        {
            if (study.skill == skill)
                return study;
        }

        return null;
    }

    public void OnStudyClicked()
    {
        player.StartStudyingSkill(skill);
        UpdateStatus();
    }
}