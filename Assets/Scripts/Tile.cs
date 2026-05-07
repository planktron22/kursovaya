using UnityEngine;

public enum TileType
{
    Empty,
    Period,
    Community,
    Studying,
    Luck,
    Unluck,
    UltraLuck,
    UltraUnluck
}

public class Tile : MonoBehaviour
{
    public TileType tileType;

    public void OnPlayerLanded()
    {
        Debug.Log("Игрок попал на: " + tileType);

        UIManager ui = FindObjectOfType<UIManager>();
        RandomEventManager eventManager = FindObjectOfType<RandomEventManager>();

        switch (tileType)
        {
            case TileType.Empty:
                break;

            case TileType.Period:
                break;

            case TileType.Community:

                PlayerStats player = FindObjectOfType<PlayerStats>();

                if (player != null)
                {
                    player.TryMeetRandomPerson();
                }

                ui.ShowPanel(tileType);

                break;

            case TileType.Studying:
                ui.ShowPanel(tileType);
                break;

            case TileType.Luck:
            case TileType.Unluck:
            case TileType.UltraLuck:
            case TileType.UltraUnluck:

                if (eventManager != null)
                {
                    eventManager.TriggerEvent(tileType);
                }

                ui.ShowPanel(tileType);

                break;
        }
    }
}