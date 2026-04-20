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

        switch (tileType)
        {
            case TileType.Empty:
                break;

            case TileType.Period:
                break;

            case TileType.Community:
                ui.ShowPanel(tileType);
                break;

            case TileType.Studying:
                ui.ShowPanel(tileType);
                break;

            case TileType.Luck:
                ui.ShowPanel(tileType);
                break;

            case TileType.Unluck:
                ui.ShowPanel(tileType);
                break;

            case TileType.UltraLuck:
                ui.ShowPanel(tileType);
                break;

            case TileType.UltraUnluck:
                ui.ShowPanel(tileType);
                break;
        }
    }
}