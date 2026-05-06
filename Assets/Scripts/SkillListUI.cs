using UnityEngine;

public class SkillListUI : MonoBehaviour
{
    public OpportunityDatabase database;
    public PlayerStats player;

    public GameObject skillPrefab;
    public Transform contentParent;

    void OnEnable()
    {
        Generate();
    }

    public void Generate()
    {
        if (database == null || player == null || skillPrefab == null || contentParent == null)
        {
            Debug.LogError("Не назначены ссылки в SkillListUI!");
            return;
        }

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var skill in database.skills)
        {
            GameObject obj = Instantiate(skillPrefab, contentParent);
            obj.GetComponent<SkillItemUI>().Setup(skill, player);
        }
    }
}