using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform[] tiles;
    public float moveSpeed = 5f;
    private int currentTile = 0;
    public bool isMoving = false;
    public float stepDelay = 0.1f;

    public void Move(int steps)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveStepByStep(steps));
        }
    }

    IEnumerator MoveStepByStep(int steps)
    {
        isMoving = true;

        for (int i = 0; i < steps; i++)
        {
            currentTile = (currentTile + 1) % tiles.Length;

            Vector3 targetPos = tiles[currentTile].position;

            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            transform.position = targetPos;
            yield return new WaitForSeconds(stepDelay);
        }

        Tile tile = tiles[currentTile].GetComponent<Tile>();

        if (tile != null)
        {
            tile.OnPlayerLanded();
        }

        isMoving = false; 
    }
}