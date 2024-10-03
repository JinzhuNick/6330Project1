using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int health;
    public int minAttack = 2;
    public int maxAttack = 3 ;
    public int movementRange;
    public float speed;
    public int detectionRange;

    public bool ifTurn = false;
    public bool ifEndMove = false; // 角色是否已经移动完成
    public bool ifAttack = false; // 敌人是否进入攻击阶段


    protected GridManager gridManager;
    protected GridCell currentCell;
    protected List<GridCell> reachableCells = new List<GridCell>();
    protected List<GridCell> path = new List<GridCell>();

    public GameObject damageTextPrefab;

    public virtual void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        // 初始化单位所在的格子
        Vector3 position = transform.position;
        int x = Mathf.FloorToInt(position.x / gridManager.cellSize + 0.5f);
        int y = Mathf.FloorToInt(position.z / gridManager.cellSize + 0.5f);

        currentCell = gridManager.GetCell(x, y);

        Debug.Log("敌人初始化格子："+x +","+y);
        

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
            ifEndMove = true;

            // 检测玩家是否在索敌范围内
            if (IsPlayerInDetectionRange())
            {
                // 检测是否已经与玩家相邻
                if (IsPlayerAdjacent())
                {
                    // 已经在玩家旁边，可以攻击或结束回合
                    Debug.Log("敌人已在玩家旁边，结束回合");
                    EndTurn();
                }
                else
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
                    else
                    {
                        // 无法找到路径或路径为空，结束回合
                        Debug.Log("无法找到路径，结束回合");
                        EndTurn();
                    }
                }
            }
            else
            {
                foreach (GridCell cell in reachableCells)
                {
                    cell.ClearCellStates();
                    cell.UpdateVisual();
                }
                // 玩家不在索敌范围内，可以执行其他行为
                ifTurn = false;
                ifEndMove = false;
            }
        }
        if (ifAttack)
        {
            // 攻击逻辑
            StartCoroutine(PerformAttack());
        }
    }

    void HighlightReachableCells()
    {
        // 清除之前的高亮
        foreach (GridCell cell in reachableCells)
        {
            cell.RemoveCellState(CellState.Highlighted);
        }
        reachableCells.Clear();
        /*
        foreach (GridCell cell in reachableCells)
        {
            cell.cellState = CellState.Normal;
            cell.UpdateVisual();
        }
        */
        reachableCells.Clear();

        // 使用广度优先搜索计算可到达的格子
        Queue<GridCell> frontier = new Queue<GridCell>();
        Dictionary<GridCell, int> costSoFar = new Dictionary<GridCell, int>();

        frontier.Enqueue(currentCell);
        costSoFar[currentCell] = 0;

        // 将当前格子加入可到达的格子列表，并设置状态
        reachableCells.Add(currentCell);
        currentCell.AddCellState(CellState.Highlighted);

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

                    //next.cellState = CellState.Highlighted;
                    next.AddCellState(CellState.Highlighted);
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
            //cell.cellState = CellState.Normal;
            cell.RemoveCellState(CellState.Highlighted);
            cell.RemoveCellState(CellState.Selected);
            cell.UpdateVisual();
        }
        reachableCells.Clear();
        ifEndMove = false;
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
                if (next.isWalkable == false)
                {
                    continue;
                }

                //next.SetCellState(CellState.Selected);  // 黄色表示索敌范围
                next.AddCellState(CellState.Selected);
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
            if (cell.IsStateInCellStates(CellState.Selected))
            {
                cell.ClearCellStates();
                cell.UpdateVisual();
            }
        }
        detectionRangeCells.Clear();
    }

    void EndTurn()
    {
        // 清除可到达的格子状态
        foreach (GridCell cell in reachableCells)
        {
            cell.ClearCellStates();
            cell.UpdateVisual();
        }
        reachableCells.Clear();
        // 清除索敌范围显示（如果有）
        ClearDetectionRange();
        // 结束回合
        ifTurn = false;
        ifEndMove = false;
    }

    bool IsPlayerAdjacent()
    {
        Character player = FindObjectOfType<Character>();
        if (player != null)
        {
            int distance = Mathf.Abs(currentCell.x - player.currentCell.x) + Mathf.Abs(currentCell.y - player.currentCell.y);
            return distance == 1;
        }
        return false;
    }

    Character CheckForPlayerInRange()
    {
        foreach (GridCell neighbor in gridManager.GetNeighbors(currentCell))
        {
            if (neighbor.occupant != null)
            {
                Character player = neighbor.occupant.GetComponent<Character>();
                if (player != null)
                {
                    return player;
                }
            }
        }
        return null;
    }

    IEnumerator PerformAttack()
    {
        ifAttack = false; // 防止重复进入

        // 检测周围是否有玩家
        Character player = CheckForPlayerInRange();
        if (player != null)
        {
            // 前摇
            yield return new WaitForSeconds(0.1f);

            // 计算伤害
            int damage = Random.Range(minAttack, maxAttack + 1);
            player.TakeDamage(damage);

            // 后摇
            yield return new WaitForSeconds(0.5f);

            // 攻击结束
            EndTurn();
        }
        else
        {
            // 没有玩家在攻击范围内，结束回合
            EndTurn();
        }
    }

    //受到伤害
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"敌人受到 {damage} 点伤害，剩余血量：{health}");
        ShowDamageText(damage);
        if (health <= 0)
        {
            Die();
        }
    }

    void ShowDamageText(int damage)
    {
        if (damageTextPrefab != null)
        {
            // 在角色位置实例化伤害数字
            GameObject damageTextObj = Instantiate(damageTextPrefab, transform.position + new Vector3(1, -1, 0), Quaternion.Euler(45, 45, 0));

            // 设置父对象为场景中的 Canvas（如果有）
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                damageTextObj.transform.SetParent(canvas.transform, false);
            }

            // 设置伤害数字
            FloatingText floatingText = damageTextObj.GetComponent<FloatingText>();
            if (floatingText != null)
            {
                floatingText.SetText("-" + damage.ToString());
            }
        }
        else
        {
            Debug.LogWarning("无法找到 DamageTextPrefab 预制件。");
        }
    }

    void Die()
    {
        // 死亡处理
        currentCell.occupant = null;
        Destroy(gameObject);
    }
}