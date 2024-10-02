using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    public int health;
    public int attackPower;
    public int movementRange;
    public float speed;
    public bool ifTurn = false;

    protected GridManager gridManager;
    protected GridCell currentCell;
    protected List<GridCell> reachableCells = new List<GridCell>();
    protected List<GridCell> path = new List<GridCell>();

    public virtual void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        // 初始化单位所在的格子
        Vector3 position = transform.position;
        int x = Mathf.FloorToInt(position.x / gridManager.cellSize + 1);
        int y = Mathf.FloorToInt(position.z / gridManager.cellSize + 1);

        currentCell = gridManager.GetCell(x, y);
        if (currentCell != null)
        {
            currentCell.occupant = gameObject;
            transform.position = currentCell.GetCenterPosition();
        }
        else
        {
            Debug.LogError("单位初始化位置不在任何格子上！");
        }
    }

    public virtual void Update()
    {
        if (ifTurn)
        {
            // 子类实现具体的行为
        }
    }

    // 公共方法，例如高亮可到达的格子、移动等
}