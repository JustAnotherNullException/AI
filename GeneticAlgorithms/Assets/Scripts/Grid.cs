using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public GameObject Agent;
    public Transform TilePrefab;
    public Vector2 mapSize;

    [Range(0, 1)]
    public float outlinePercent;

    GameObject[,] Tiles;

    void Awake()
    {
        mapSize.x = 11;
        mapSize.y = 11;
        outlinePercent = 0.5f;

        Tiles = new GameObject[(int)mapSize.x, (int)mapSize.y];

        GenerateGrid();
        GenerateWalls();

    }

    public void GenerateGrid()
    {
        string holderName = "GeneratedGrid";

        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform gridHolder = new GameObject(holderName).transform;
        gridHolder.parent = transform;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilePosition = new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -(-mapSize.y / 2 + 0.5f + y));
                Transform newTile = Instantiate(TilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent);
                newTile.parent = gridHolder;

                Tiles[x, y] = newTile.gameObject;
            }
        }

        Tiles[1, 1].SetColor(Color.green);
        Tiles[9, 9].SetColor(Color.red);

        Agent.MoveTo(Tiles[1, 1]);
        Agent.SetColor(Color.black);
    }

    public void GenerateWalls()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            Tiles[x, 0].SetColor(Color.black);
            Tiles[x, (int)mapSize.y - 1].SetColor(Color.black);
        }
        for (int y = 0; y < mapSize.y; y++)
        {
            Tiles[0, y].SetColor(Color.black);
            Tiles[(int)mapSize.x - 1, y].SetColor(Color.black);
        }
    }

    public TileSet[,] GenerateTileSet()
    {
        TileSet[,] tileSet = new TileSet[(int)mapSize.x, (int)mapSize.y];

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Color color = Tiles[x, y].GetColor();
                if(color == Color.white)
                {
                    tileSet[x, y] = TileSet.Empty;
                }
                else if (color == Color.black)
                {
                    tileSet[x, y] = TileSet.Wall;
                }
                else if (color == Color.green)
                {
                    tileSet[x, y] = TileSet.Start;
                }
                else if (color == Color.red)
                {
                    tileSet[x, y] = TileSet.Finish;
                }
            }
        }

        return tileSet;
    }
}

public enum TileSet // Enum that contains each type of tile
{
    Empty,
    Wall,
    Start,
    Finish
}

public static class Extensions
{
    public static void SetColor(this GameObject gameObj, Color color)
    {
        Renderer renderer = gameObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    public static Color GetColor(this GameObject gameObj)
    {
        Renderer renderer = gameObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.material.color;
        }
        else return Color.white; 
    }

    public static void MoveTo(this GameObject gameObject, GameObject target)
    {
        gameObject.transform.position = target.transform.position;
    }
}
