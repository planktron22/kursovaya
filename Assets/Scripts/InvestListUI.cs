using UnityEngine;

public class InvestListUI : MonoBehaviour
{
    public OpportunityDatabase database;
    public PlayerStats player;

    public GameObject investPrefab;
    public Transform contentParent;

    void OnEnable()
    {
        Generate();
    }

    public void Generate()
    {
        if (database == null || player == null || investPrefab == null || contentParent == null)
        {
            Debug.LogError("Не назначены ссылки в InvestListUI!");
            return;
        }

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var invest in database.invests)
        {
            GameObject obj = Instantiate(investPrefab, contentParent);

            InvestItemUI item = obj.GetComponent<InvestItemUI>();
            item.Setup(invest, player);
        }
    }
}