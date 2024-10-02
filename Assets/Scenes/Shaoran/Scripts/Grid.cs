using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using UnityEngine;
//using CodeMonkey.Utils;
using static UnityEngine.Rendering.DebugUI;

public class Grid
{
    public int width;
    public int height;
    public float cellSize;
    public int[,] gridArray;
    public TextMesh[,] debugTextArray;
    public bool[,] isOcupiedArray;
    public LevelManager levelManager;
    public bool[,] isMonsterArray;
    public Font font;
    public Grid(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        debugTextArray = new TextMesh[width, height]; ;
        gridArray = new int[width, height];
        isOcupiedArray = new bool[width, height];
        isMonsterArray = new bool[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int z = 0; z < gridArray.GetLength(1); z++)
            {
                isOcupiedArray[x, z] = false;
            }
        }
    }

    public Vector3 GetWorldPosition(int x,float y, int z)
    {
        float xWorldPosition;
        float yWorldPosition;

        xWorldPosition = x * cellSize ;
        yWorldPosition = z * cellSize ;
        return new Vector3(xWorldPosition, y, yWorldPosition);
    }

    private void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.RoundToInt(worldPosition.x / cellSize);
        y = Mathf.RoundToInt(worldPosition.z / cellSize);
    }

    public bool GetValueBool(Vector3 worldPosition)
    {
        int x, z;
        GetXY(worldPosition, out x, out z);

        return isOcupiedArray[x, z];
    }

    public bool GetValueBool(int x, int z)
    {
        return isOcupiedArray[x, z];
    }

    public bool GetValueBoolMonster(int x, int z)
    {
        return isMonsterArray[x, z];
    }

    public void SetValueBoolMonster(int x, int z, bool value)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
        {
            isMonsterArray[x, z] = value;
        }
    }

    public void SetValueBoolMonster(Vector3 worldPosition, bool value)
    {
        int x, z;
        GetXY(worldPosition, out x, out z);
        SetValueBoolMonster(x, z, value);
        //debugTextArray[x, z].text = gridArray[x, z].ToString();
        Debug.Log(x + "," + z + "is occupied: " + value);
    }

    public int GetValueInt(Vector3 worldPosition)
    {
        int x, z;
        GetXY(worldPosition, out x, out z);

        return gridArray[x, z];
    }

    public void SetValue(int x, int z, int value)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
        {
            gridArray[x, z] = value;
            if (value == 0) { debugTextArray[x, z].text = ""; }
            else { debugTextArray[x, z].text = gridArray[x, z].ToString(); }
        }
    }

    public void SetValueBool(int x, int z, bool value)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
        {
            isOcupiedArray[x, z] = value;
        }
    }

    public void SetValue(Vector3 worldPosition, int value)
    {
        int x, z;
        GetXY(worldPosition, out x, out z);
        SetValue(x, z, value);
        debugTextArray[x, z].text = gridArray[x, z].ToString();
    }

    public void SetValueBool(Vector3 worldPosition, bool value)
    {
        int x, z;
        GetXY(worldPosition, out x, out z);
        SetValueBool(x, z, value);
        //debugTextArray[x, z].text = gridArray[x, z].ToString();
        Debug.Log(x + "," + z + "is occupied: " + value);
    }

    public void AddValue(Vector3 worldPosition, int value)
    {
        int x, z;
        GetXY(worldPosition, out x, out z);

        gridArray[x, z] += value;
        if (gridArray[x, z] != 0) { debugTextArray[x, z].text = gridArray[x, z].ToString(); }
    }

    public void AddValue(int x, int z, int value)
    {
        gridArray[x, z] += value;
        if (gridArray[x, z] != 0) { debugTextArray[x, z].text = gridArray[x, z].ToString(); }
        else if (gridArray[x, z] == 0) { debugTextArray[x, z].text = ""; }
    }

    public void ClearValue(int x, int z, int value)
    {
        gridArray[x, z] = 0;
        debugTextArray[x, z].text = "";
    }
}
