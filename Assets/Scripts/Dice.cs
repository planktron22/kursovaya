using UnityEngine;

public class Dice : MonoBehaviour
{
    public PlayerMovement player;

    public void RollDice()
    {
        if (player.isMoving)
        {
            Debug.Log("Идет перемещение...");
            return;
        }

        int roll = Random.Range(1, 7);
        Debug.Log("Выпало: " + roll);

        player.Move(roll);
    }
}