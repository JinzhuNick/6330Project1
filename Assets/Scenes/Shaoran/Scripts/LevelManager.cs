using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static Grid grid;
    public GameObject gridIndicator;
    private List<GameObject> gridIndicators = new List<GameObject>();
    public GameObject player;

    [Header("Grid Settings")]
    public int gridRows;
    public int gridCols;
    public float cellSize;

    [Header("Enemies to Spawn")]
    public GameObject[] enemies;

    [Header("Enemy Spawn Locations")]
    public int[] enemySpawnX;
    public int[] enemySpawnY;

    [Header("Player Spawn Location")]
    public int playerSpawnX;
    public int playerSpawnY;

    public void Awake()
    {
        grid = new Grid(gridRows , gridCols, cellSize);
    }
    public void Start()
    {
        Initialize();
    }

    private void Initialize() 
    {
        SpawnGrids();
        SpawnEnemies();
        SpawnPlayer();
    }
    public void SpawnGrids()
    {
        for (int x = 0; x < grid.gridArray.GetLength(0); x++)
        {
            for (int z = 0; z < grid.gridArray.GetLength(1); z++)
            {
                gridIndicators.Add(Instantiate(gridIndicator, grid.GetWorldPosition(x, 0, z), transform.rotation) as GameObject);
                //Instantiate(gridCollider, grid.GetWorldPosition(x, 0.2f, z), transform.rotation);
            }
        }
    }

    public void SpawnEnemies() 
    {
        for (int i = 0; i < enemies.Length; i++)
        {
                Instantiate(enemies[i], grid.GetWorldPosition(enemySpawnX[i], 1, enemySpawnY[i]), Quaternion.identity);
        }
    }

    public void SpawnPlayer() 
    {
        Instantiate(player, grid.GetWorldPosition(playerSpawnX,1f,playerSpawnY), Quaternion.identity);
    }
}
