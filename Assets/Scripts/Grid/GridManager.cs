using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public int gridWidth = 10;    // 横向格子数量
    public int gridHeight = 10;   // 纵向格子数量
    public float cellSize = 1f;  // 每个格子的大小
    

    [SerializeField]
    public List<GridCell> gridCellsList = new List<GridCell>();

    void Start()
    {
        CreateGridObjects();
    }

    public void CreateGrid()
    {
#if UNITY_EDITOR
        if (gridCellsList.Count > 0)
        {
            if (!UnityEditor.EditorUtility.DisplayDialog("警告", "这将重置当前的格子列表，是否继续？", "是", "否"))
            {
                return;
            }
        }
#endif
        gridCellsList.Clear();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridCell cell = new GridCell(x, y, cellSize);
                gridCellsList.Add(cell);
            }
        }
    }
    // 新增方法，在游戏开始时创建格子对象
    public void CreateGridObjects()
    {
        // 清理已有的格子对象
        foreach (GridCell cell in gridCellsList)
        {
            if (cell.cellObject != null)
            {
                Destroy(cell.cellObject);
                cell.cellObject = null;
            }
        }

        // 实例化格子对象
        foreach (GridCell cell in gridCellsList)
        {
            // 创建格子对象
            GameObject cellObj = Instantiate(cellPrefab, cell.GetCenterPosition(), Quaternion.identity, transform);
            cellObj.name = $"Cell_{cell.x}_{cell.y}";
            cellObj.transform.localScale = new Vector3(cellSize*0.7f, 0.1f, cellSize * 0.7f);

            // 保存格子对象的引用
            cell.cellObject = cellObj;

            // 根据初始状态设置格子颜色
            UpdateCellVisual(cell);
        }
    }
    public GridCell GetCell(int x, int y)
    {
        return gridCellsList.Find(c => c.x == x && c.y == y);
    }

    void OnDrawGizmos()
    {
        if (gridCellsList == null || gridCellsList.Count == 0)
        {
            CreateGrid();
        }

        foreach (GridCell cell in gridCellsList)
        {
            // 设置格子颜色
            Gizmos.color = cell.isWalkable ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(cell.GetCenterPosition(), new Vector3(cellSize, 0.1f, cellSize));

            // 根据格子状态绘制边框
            switch (cell.cellState)
            {
                case CellState.Normal:
                    Gizmos.color = Color.white;
                    break;
                case CellState.Highlighted:
                    Gizmos.color = Color.yellow;
                    break;
                case CellState.Selected:
                    Gizmos.color = Color.blue;
                    break;
                case CellState.Blocked:
                    Gizmos.color = Color.black;
                    break;
            }
            Gizmos.DrawWireCube(cell.GetCenterPosition(), new Vector3(cellSize, 0.1f, cellSize));
        }
    }

    // 更新格子的视觉表现
    public void UpdateCellVisual(GridCell cell)
    {
        if (cell.cellObject != null)
        {
            Renderer renderer = cell.cellObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = GetCellColor(cell);
            }
        }
    }

    // 根据格子的状态获取颜色
    public Color GetCellColor(GridCell cell)
    {
        if (!cell.isWalkable)
        {
            return new Color(1, 0, 0, 0.4f); // 红色
        }
        else
        {
            switch (cell.cellState)
            {
                case CellState.Normal:
                    return new Color(1, 1, 1, 0.1f); // 白色，透明度降低
                case CellState.Highlighted:
                    return new Color(0, 1, 0, 0.4f); // 绿色
                case CellState.Selected:
                    return new Color(1, 1, 0, 0.4f); // 黄色
                case CellState.Blocked:
                    return new Color(1, 0, 0, 0.4f); // 红色
                default:
                    return new Color(1, 1, 1, 0.1f); // 白色，透明度降低
            }
        }
    }

    public List<GridCell> GetNeighbors(GridCell cell)
    {
        List<GridCell> neighbors = new List<GridCell>();

        int x = cell.x;
        int y = cell.y;

        // 上下左右四个方向
        GridCell neighbor;

        // 上
        neighbor = GetCell(x, y + 1);
        if (neighbor != null)
            neighbors.Add(neighbor);

        // 下
        neighbor = GetCell(x, y - 1);
        if (neighbor != null)
            neighbors.Add(neighbor);

        // 左
        neighbor = GetCell(x - 1, y);
        if (neighbor != null)
            neighbors.Add(neighbor);

        // 右
        neighbor = GetCell(x + 1, y);
        if (neighbor != null)
            neighbors.Add(neighbor);

        return neighbors;
    }
}
