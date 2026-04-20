using UnityEngine;

public class Dice : MonoBehaviour
{
    public PlayerMovement player;
    public UIManager UI;

    public void RollDice()
    {
        if (player.isMoving)
        {
            Debug.Log("Идет перемещение...");
            return;
        }

        if (UI.isPanelOpen)
        {
            Debug.Log("Панель открыта!");
            return;
        }

        int roll = Random.Range(1, 7);
        Debug.Log("Выпало: " + roll);

        player.Move(roll);
    }
}