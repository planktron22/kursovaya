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

        switch (tileType)
        {
            case TileType.Empty:
                break;

            case TileType.Period:
                break;

            case TileType.Community:
                break;

            case TileType.Studying:
                break;

            case TileType.Luck:
                break;

            case TileType.Unluck:
                break;

            case TileType.UltraLuck:
                break;

            case TileType.UltraUnluck:
                break;
        }
    }
}