using UnityEngine;

public class RealtyListUI : MonoBehaviour
{
    public OpportunityDatabase database;
    public PlayerStats player;

    public GameObject realtyPrefab;
    public Transform contentParent;

    void OnEnable()
    {
        Generate();
    }

    public void Generate()
    {
        if (database == null || realtyPrefab == null || contentParent == null)
        {
            Debug.LogError("Не назначены ссылки в RealtyListUI!");
            return;
        }

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var item in database.realty)
        {
            GameObject obj = Instantiate(realtyPrefab, contentParent);

            obj.GetComponent<RealtyItemUI>().Setup(item, player);
        }
    }
}