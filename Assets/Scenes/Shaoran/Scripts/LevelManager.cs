using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static Grid grid;
    public GameObject gridIndicator;
    public List<GameObject> gridIndicators = new List<GameObject>();

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
    public void Awake()
    {
        grid = new Grid(15 , 15, 2f);
    }
    public void Start()
    {
        SpawnGrids();
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
}
