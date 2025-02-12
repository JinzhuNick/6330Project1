using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    public int attackPower;
    public int maxHealth;
    public int health;
    public int movementRange;
    public float speed;  // 控制移动速度

    public bool ifTurn = false;  // 控制角色是否可以移动
    public bool ifEndMove = false; // 角色是否已经移动完成
    public bool ifAttack = false; // 角色是否进入攻击阶段
    

    public GridManager gridManager;
    public GridCell currentCell;

    private List<GridCell> reachableCells = new List<GridCell>();  // 可到达的格子列表
    private List<GridCell> path = new List<GridCell>();  // 角色当前的移动路径

    public GameObject damageTextPrefab;
    public GameObject AttackPhase;
    public GameObject MovePhase;

    public Animator animator;




    // 技能列表
    private List<Skill> skills = new List<Skill>();
    // 当前选择的技能
    public Skill selectedSkill = null;

    public Button skill1Button;
    public Button skill2Button;
    public Button skill3Button;
    public Button skipButton;
    public Button cancelButton;

    //Events
    public delegate void OnCharacterHurt(float maxHealth, float currentHealth, GameObject hurtObject);
    public static event OnCharacterHurt onCharacterHurt;
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        // 绑定按钮事件
        skill1Button.onClick.AddListener(() => SelectSkill(0));
        skill2Button.onClick.AddListener(() => SelectSkill(1));
        skill3Button.onClick.AddListener(() => SelectSkill(2));
        skipButton.onClick.AddListener(() => SkipAttack());
        //cancelButton.onClick.AddListener(CancelAttack);

        // 初始化技能
        skills.Add(new NormalAttackSkill());
        skills.Add(new Skill1());
        skills.Add(new Skill2()); 

        // 初始化角色所在的格子
        Vector3 position = transform.position;
        int x = Mathf.FloorToInt(position.x / gridManager.cellSize + 0.5f);
        int y = Mathf.FloorToInt(position.z / gridManager.cellSize + 0.5f);

        currentCell = gridManager.GetCell(x, y);
        if (currentCell != null)
        {
            currentCell.occupant = gameObject;
            transform.position = currentCell.GetCenterPosition();
        }
        else
        {
            Debug.LogError("角色初始化位置不在任何格子上！");
        }

        health = maxHealth;
        onCharacterHurt?.Invoke((float)maxHealth, (float)health, this.gameObject);
    }

    void Update()
    {
        if (ifTurn)
        {
            // 高亮可移动的格子
            HighlightReachableCells();

            // 显示敌人的索敌范围
            ShowEnemyDetectionRanges();

            ifEndMove = true;
            // 处理鼠标输入
            HandleMouseInput();

        }

        if (ifAttack)
        {
            // 攻击逻辑
            HandleAttackInput();
        }

        if(ifEndMove)
        {
            MovePhase.SetActive(true);
        }
        else MovePhase.SetActive(false);

        if(ifAttack)
        {
            AttackPhase.SetActive(true);
        }
        else AttackPhase.SetActive(false);
    }

    // 高亮可到达的格子
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

    // 处理鼠标输入
    void HandleMouseInput()
    {
        if (GameManager.Instance.ifClickable == true && Input.GetMouseButtonDown(0))
        {
            // 发射一条射线检测鼠标点击的位置
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GridCell clickedCell = GetCellFromPosition(hit.point);
                if (clickedCell != null && reachableCells.Contains(clickedCell))
                {
                    if(clickedCell == currentCell)
                    {
                        foreach (GridCell cell in reachableCells)
                        {
                            cell.ClearCellStates();
                            //cell.AddCellState(CellState.Normal);
                            cell.UpdateVisual();
                        }
                        reachableCells.Clear();
                        ClearEnemyDetectionRanges();
                        ifTurn = false;
                        Debug.Log("关闭ifturn");
                        ifEndMove = false;
                        return;
                    }
                    // 计算路径并开始移动
                    path = FindPath(currentCell, clickedCell);
                    if (path != null && path.Count > 0)
                    {
                        StartCoroutine(MoveAlongPath());
                        // 结束当前回合
                        ifTurn = false;
                    }
                }
            }
        }
    }

    // 根据世界坐标获取对应的格子
    public GridCell GetCellFromPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / gridManager.cellSize + 0.5f);
        int y = Mathf.FloorToInt(position.z / gridManager.cellSize + 0.5f);
        return gridManager.GetCell(x, y);
    }

    // 寻路算法（A*）
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
                    int priority = newCost + Heuristic(goal, next);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        // 构建路径
        GridCell temp = goal;
        while (temp != start)
        {
            if (cameFrom.ContainsKey(temp))
            {
                path.Add(temp);
                temp = cameFrom[temp];
            }
            else
            {
                // 无法到达目标
                return null;
            }
        }
        path.Reverse();
        return path;
    }

    // 启动协程，按照路径移动
    IEnumerator MoveAlongPath()
    {
        GameManager.Instance.ifClickable = false;
        animator.SetBool("isRunning", true);
        foreach (GridCell cell in path)
        {
            // 更新格子的占据信息
            currentCell.occupant = null;
            cell.occupant = gameObject;
            currentCell = cell;

            // 更新朝向
            UpdateFacingDirection(transform.position, cell.GetCenterPosition());

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

        // 移动结束后，清空路径和高亮以及敌人的索敌范围
        path.Clear();
        foreach (GridCell cell in reachableCells)
        {
            cell.ClearCellStates();
            //cell.AddCellState(CellState.Normal);
            cell.UpdateVisual();
        }
        animator.SetBool("isRunning", false);
        reachableCells.Clear();
        ClearEnemyDetectionRanges();
        ifEndMove = false;
        GameManager.Instance.ifClickable = true;
    }

    // 清除敌人索敌范围
    void ClearEnemyDetectionRanges()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            enemy.ClearDetectionRange();
        }
    }

    // 更新角色朝向
    public void UpdateFacingDirection(Vector3 fromPosition, Vector3 toPosition)
    {
        Vector3 direction = toPosition - fromPosition;
        direction.y = 0;  // 忽略Y轴

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    // 计算启发式函数（曼哈顿距离）
    int Heuristic(GridCell a, GridCell b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // 显示所有敌人的索敌范围
    void ShowEnemyDetectionRanges()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            enemy.ShowDetectionRange();
        }
    }

    void HandleAttackInput()
    {
        if (GameManager.Instance.ifClickable)
        {
            // 键盘选择技能
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSkill(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSkill(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSkill(2);

            // 跳过攻击
            if (Input.GetKeyDown(KeyCode.S)) SkipAttack();

            // 取消攻击
            if (Input.GetKeyDown(KeyCode.X)) CancelAttack();
        }

        // 处理技能释放
        if (selectedSkill != null)
        {
            selectedSkill.UpdateSkill(this);
        }

    }

    void SelectSkill(int index)
    {
        if (index >= 0 && index < skills.Count)
        {
            CancelAttack(); // 先取消当前技能
            selectedSkill = skills[index];
            selectedSkill.OnSelect(this);
        }
    }

    void CancelAttack()
    {
        if (selectedSkill != null)
        {
            selectedSkill.OnCancel(this);
            selectedSkill = null;
        }
    }

    void SkipAttack()
    {
        CancelAttack();
        ifAttack = false;
        // 其他跳过逻辑
        Debug.Log("Skipped Attack");
    }
    // 受到伤害
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"玩家受到 {damage} 点伤害，剩余血量：{health}");
        onCharacterHurt?.Invoke((float)maxHealth, (float)health, this.gameObject);
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
            GameObject damageTextObj = Instantiate(damageTextPrefab, transform.position+new Vector3(1,-1,0), Quaternion.Euler(45, 45, 0));
            //damageTextObj.transform.SetParent(this.gameObject.transform);

            // 设置父对象为场景中的 Canvas（如果有）
            /*
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                damageTextObj.transform.SetParent(canvas.transform, false);
            }
            */

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

        Debug.Log("玩家死亡！");
        // 可以添加游戏结束逻辑
    }
}