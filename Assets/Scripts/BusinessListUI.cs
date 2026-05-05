using UnityEngine;

public class BusinessListUI : MonoBehaviour
{
    public OpportunityDatabase database;
    public PlayerStats player;

    public GameObject businessPrefab;
    public Transform contentParent;

    void OnEnable()
    {
        Generate();
    }

    void Generate()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var item in database.businesses)
        {
            if (item.type != OpportunityType.Business)
                continue;

            GameObject obj = Instantiate(businessPrefab, contentParent);

            obj.GetComponent<BusinessItemUI>().Setup(item, player);
        }
    }
}