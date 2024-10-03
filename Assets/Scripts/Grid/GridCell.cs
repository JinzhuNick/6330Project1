using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GridCell
{
    public int x;
    public int y;
    float cellSize;
    public Vector3 worldPosition;

    public GameObject occupant = null;      // 当前占据的玩家或怪物
    public Direction facingDirection;       // 朝向

    public bool isWalkable = true;          // 是否可移动

    public CellState cellState = CellState.Normal;  // 格子状态，用于渲染不同的边框颜色
    private HashSet<CellState> cellStates = new HashSet<CellState>();

    public GridCell(int x, int y, float size)
    {
        this.x = x;
        this.y = y;
        this.cellSize = size;
        worldPosition = new Vector3(x * size, 0, y * size);
    }
    public Vector3 GetCenterPosition()
    {
        return worldPosition;
    }

    [HideInInspector]
    public GameObject cellObject;  // 用于在场景中显示格子的对象

    public void UpdateVisual()
    {
        if (cellObject != null)
        {
            Renderer renderer = cellObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                GridManager gridManager = GameObject.FindObjectOfType<GridManager>();
                if (gridManager != null)
                {
                    renderer.material.color = gridManager.GetCellColor(this);
                }
            }
        }
    }

    /*
    public void SetCellState(CellState newState)
    {
        // 定义状态优先级
        int currentPriority = GetStatePriority(cellState);
        int newPriority = GetStatePriority(newState);

        if (newPriority >= currentPriority)
        {
            cellState = newState;
            UpdateVisual();
        }
    }
    */

    // 添加状态
    public void AddCellState(CellState newState)
    {
        if (cellStates == null)
        {
            cellStates = new HashSet<CellState>();
        }
        cellStates.Add(newState);
        UpdateVisual();
    }

    // 移除状态
    public void RemoveCellState(CellState state)
    {
        if (cellStates.Contains(state))
        {
            cellStates.Remove(state);
            UpdateVisual();
        }
    }

    // 清除所有状态
    public void ClearCellStates()
    {
        cellStates.Clear();
        UpdateVisual();
    }

    // 获取当前的状态集合
    public HashSet<CellState> GetCellStates()
    {
        if (cellStates == null)
        {
            cellStates = new HashSet<CellState>();
        }
        return cellStates;
    }

    public bool IsStateInCellStates(CellState state)
    {
        return GetCellStates().Contains(state);
    }
    private int GetStatePriority(CellState state)
    {
        switch (state)
        {
            case CellState.Blocked:
                return 3;  // 最高优先级
            case CellState.Selected:
                return 2;
            case CellState.Highlighted:
                return 1;
            case CellState.Normal:
                return 0;
            default:
                return 0;
        }
    }

}

public enum Direction
{
    North,
    East,
    South,
    West
}

public enum CellState
{
    Normal,
    Highlighted,
    Selected,
    Blocked,
    Active
}