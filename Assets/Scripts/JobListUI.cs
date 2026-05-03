using UnityEngine;

public class JobListUI : MonoBehaviour
{
    public OpportunityDatabase database;
    public PlayerStats player;

    public GameObject jobItemPrefab;
    public Transform contentParent;

    void OnEnable()
    {
        GenerateList();
    }

    public void GenerateList()
    {
        if (database == null || contentParent == null || jobItemPrefab == null)
        {
            Debug.LogError("Не назначены ссылки!");
            return;
        }

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var job in database.jobs)
        {
            GameObject obj = Instantiate(jobItemPrefab, contentParent);

            JobItemUI item = obj.GetComponent<JobItemUI>();
            item.Setup(job, player);
        }
    }
}