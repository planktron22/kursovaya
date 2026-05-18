using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Tiles")]
    public Transform[] tiles;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public int currentTile = 0;
    public bool isMoving = false;
    public float stepDelay = 0.1f;

    [Header("Move Sound")]
    public AudioSource audioSource;
    public AudioClip stepSound;

    [Range(0f, 1f)]
    public float stepSoundVolume = 0.5f;

    [Header("Period Sound")]
    public AudioClip periodSound;

    [Range(0f, 1f)]
    public float periodSoundVolume = 0.7f;

    [Header("Debug Move")]
    public int debugSteps = 1;
    public bool debugMove = false;

    private PlayerStats stats;

    void Start()
    {
        stats = GetComponent<PlayerStats>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (debugMove)
        {
            debugMove = false;

            if (!isMoving)
            {
                Move(debugSteps);
            }
        }
    }

    public void Move(int steps)
    {
        if (isMoving)
            return;

        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogError("�� ��������� ����� ��� �������� ������");
            return;
        }

        StartCoroutine(MoveStepByStep(steps));
    }

    IEnumerator MoveStepByStep(int steps)
    {
        isMoving = true;

        for (int i = 0; i < steps; i++)
        {
            int previousTile = currentTile;

            currentTile = (currentTile + 1) % tiles.Length;

            // ������ ������ ����
            if (previousTile == tiles.Length - 1 && currentTile == 0)
            {
                Debug.Log("������� ������ ����!");

                if (stats != null)
                {
                    stats.DecreaseAge();
                }
            }

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

            // ������� ���� ����
            PlayStepSound();

            // �������� ������� ��� ����������� ������
            Tile tile = tiles[currentTile].GetComponent<Tile>();

            if (tile != null && tile.tileType == TileType.Period)
            {
                PlayPeriodSound();

                if (stats != null)
                {
                    stats.ApplyPeriod();
                }
            }

            yield return new WaitForSeconds(stepDelay);
        }

        // ��������� ����
        Tile finalTile = tiles[currentTile].GetComponent<Tile>();

        if (finalTile != null)
        {
            finalTile.OnPlayerLanded();
        }
        
        if (finalTile != null && finalTile.tileType == TileType.Empty)
        {
            CompetitorAI competitor = FindObjectOfType<CompetitorAI>();
            if (competitor != null && stats != null)
            {
                competitor.SimulateTurn(stats);
            }
        }
        isMoving = false;
    }

    void PlayStepSound()
    {
        if (audioSource == null || stepSound == null)
            return;

        audioSource.PlayOneShot(stepSound, stepSoundVolume * SoundSettings.SfxVolume);
    }

    void PlayPeriodSound()
    {
        if (audioSource == null || periodSound == null)
            return;

        audioSource.PlayOneShot(periodSound, periodSoundVolume * SoundSettings.SfxVolume);
    }

    public TileType GetCurrentTileType()
    {
        if (tiles == null || tiles.Length == 0)
            return TileType.Empty;

        Tile tile = tiles[currentTile].GetComponent<Tile>();

        if (tile != null)
            return tile.tileType;

        return TileType.Empty;
    }

    public void SetCurrentTile(int tileIndex)
    {
        if (tiles == null || tiles.Length == 0)
            return;

        currentTile = Mathf.Clamp(tileIndex, 0, tiles.Length - 1);
        transform.position = tiles[currentTile].position;
    }
}