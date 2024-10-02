using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public int health;
    public int attackPower;
    public int movementRange;
    public float speed;
    public int detectionRange;
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

    void Update()
    {
        if (ifTurn)
        {
            // 高亮可移动的格子
            HighlightReachableCells();

            // 检测玩家是否在索敌范围内
            if (IsPlayerInDetectionRange())
            {
                // 寻找距离玩家最近的可到达格子
                GridCell targetCell = FindClosestCellToPlayer();
                if (targetCell != null)
                {
                    // 计算路径并开始移动
                    path = FindPath(currentCell, targetCell);
                    if (path != null && path.Count > 0)
                    {
                        StartCoroutine(MoveAlongPath());
                        // 结束当前回合
                        ifTurn = false;
                    }
                }
            }
            else
            {
                foreach (GridCell cell in reachableCells)
                {
                    cell.cellState = CellState.Normal;
                    cell.UpdateVisual();
                }
                // 玩家不在索敌范围内，可以执行其他行为（例如原地待机）
                ifTurn = false;
            }
        }
    }

    void HighlightReachableCells()
    {
        // 清除之前的高亮
        foreach (GridCell cell in reachableCells)
        {
            cell.cellState = CellState.Normal;
            cell.UpdateVisual();
        }
        reachableCells.Clear();

        // 使用广度优先搜索计算可到达的格子
        Queue<GridCell> frontier = new Queue<GridCell>();
        Dictionary<GridCell, int> costSoFar = new Dictionary<GridCell, int>();

        frontier.Enqueue(currentCell);
        costSoFar[currentCell] = 0;

        // 将当前格子加入可到达的格子列表，并设置状态
        reachableCells.Add(currentCell);
        currentCell.SetCellState(CellState.Highlighted);

        while (frontier.Count > 0)
        {
            GridCell current = frontier.Dequeue();

            foreach (GridCell next in gridManager.GetNeighbors(current))
            {
                if (!next.isWalkable || next.occupant != null)
                {
                    continue;
                }

                int newCost = costSoFar[current] + 1;
                if (newCost > movementRange)
                {
                    continue;
                }

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    frontier.Enqueue(next);
                    reachableCells.Add(next);

                    next.cellState = CellState.Highlighted;
                    next.UpdateVisual();
                }
            }
        }
    }

    List<GridCell> FindPath(GridCell start, GridCell goal)
    {
        List<GridCell> path = new List<GridCell>();

        PriorityQueue<GridCell> frontier = new PriorityQueue<GridCell>();
        frontier.Enqueue(start, 0);

        Dictionary<GridCell, GridCell> cameFrom = new Dictionary<GridCell, GridCell>();
        Dictionary<GridCell, int> costSoFar = new Dictionary<GridCell, int>();

        cameFrom[start] = null;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            GridCell current = frontier.Dequeue();

            if (current == goal)
            {
                break;
            }

            foreach (GridCell next in gridManager.GetNeighbors(current))
            {
                // 考虑障碍物和占据者
                if (!next.isWalkable || (next.occupant != null && next != goal))
                {
                    continue;
                }

                int newCost = costSoFar[current] + 1;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    int priority = newCost + Heuristic(goal, next);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        // 构建路径
        if (!cameFrom.ContainsKey(goal))
        {
            // 无法到达目标
            return null;
        }

        GridCell temp = goal;
        while (temp != start)
        {
            path.Add(temp);
            temp = cameFrom[temp];
        }
        path.Reverse();
        return path;
    }

    // 启动协程，按照路径移动
    IEnumerator MoveAlongPath()
    {
        foreach (GridCell cell in path)
        {
            // 更新格子的占据信息
            currentCell.occupant = null;
            cell.occupant = gameObject;
            currentCell = cell;

            // 更新朝向
            //UpdateFacingDirection(transform.position, cell.GetCenterPosition());

            // 平滑移动到下一个格子
            Vector3 startPosition = transform.position;
            Vector3 endPosition = cell.GetCenterPosition();
            float elapsedTime = 0f;
            float journeyLength = Vector3.Distance(startPosition, endPosition);
            float journeyTime = journeyLength / speed;

            while (elapsedTime < journeyTime)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / journeyTime));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.position = endPosition;
        }

        // 移动结束后，清空路径和高亮
        path.Clear();
        foreach (GridCell cell in reachableCells)
        {
            cell.cellState = CellState.Normal;
            cell.UpdateVisual();
        }
        reachableCells.Clear();
    }

    int Heuristic(GridCell a, GridCell b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // 更新角色朝向
    void UpdateFacingDirection(Vector3 fromPosition, Vector3 toPosition)
    {
        Vector3 direction = toPosition - fromPosition;
        direction.y = 0;  // 忽略Y轴

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    // 检测玩家是否在索敌范围内
    bool IsPlayerInDetectionRange()
    {
        // 获取玩家位置
        Character player = FindObjectOfType<Character>();
        if (player != null)
        {
            int distance = Mathf.Abs(currentCell.x - player.currentCell.x) + Mathf.Abs(currentCell.y - player.currentCell.y);
            return distance <= detectionRange;
        }
        return false;
    }

    // 寻找距离玩家最近的可到达格子
    GridCell FindClosestCellToPlayer()
    {
        Character player = FindObjectOfType<Character>();
        if (player == null) return null;

        GridCell playerCell = player.currentCell;
        GridCell closestCell = null;
        int shortestDistance = int.MaxValue;

        // 遍历可到达的格子，找到距离玩家最近的格子
        foreach (GridCell cell in reachableCells)
        {
            // 计算从 cell 到玩家所在格子的路径
            List<GridCell> pathToPlayer = FindPath(cell, playerCell);
            if (pathToPlayer == null)
            {
                // 如果无法从该格子到达玩家，跳过
                continue;
            }

            int distance = pathToPlayer.Count;
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestCell = cell;
            }
        }
        return closestCell;
    }

    private List<GridCell> detectionRangeCells = new List<GridCell>();
    public void ShowDetectionRange()
    {
        Queue<GridCell> frontier = new Queue<GridCell>();
        HashSet<GridCell> visited = new HashSet<GridCell>();

        frontier.Enqueue(currentCell);
        visited.Add(currentCell);

        while (frontier.Count > 0)
        {
            GridCell current = frontier.Dequeue();

            foreach (GridCell next in gridManager.GetNeighbors(current))
            {
                if (visited.Contains(next))
                {
                    continue;
                }

                visited.Add(next);

                // 墙体（Blocked）优先显示，不覆盖
                if (next.cellState == CellState.Blocked)
                {
                    continue;
                }

                next.SetCellState(CellState.Selected);  // 黄色表示索敌范围
                detectionRangeCells.Add(next);

                // 索敌范围可以穿墙
                int distance = Mathf.Abs(next.x - currentCell.x) + Mathf.Abs(next.y - currentCell.y);
                if (distance < detectionRange)
                {
                    frontier.Enqueue(next);
                }
            }
        }
    }

    public void ClearDetectionRange()
    {
        foreach (GridCell cell in detectionRangeCells)
        {
            // 只在状态为 Selected 时重置，以免覆盖其他状态
            if (cell.cellState == CellState.Selected)
            {
                cell.cellState = CellState.Normal;
                cell.UpdateVisual();
            }
        }
        detectionRangeCells.Clear();
    }

    void EndTurn()
    {
        // 清除可到达的格子状态
        reachableCells.Clear();
        // 清除索敌范围显示（如果有）
        ClearDetectionRange();
        // 结束回合
        ifTurn = false;
    }
}