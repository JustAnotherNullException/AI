using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public GameObject Agent; // 
    public Transform TilePrefab;
    public Vector2 mapSize;

    [Range(0, 1)]
    public float outlinePercent;

    GameObject[,] Tiles;

    //Dictate number of squares in the X and Y axis 
    //Dictate outine Percentage
    //Create tiles based on number on map Size X and map Size Y
    //Generate the grid
    //Generate the Walls
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

        Transform gridHolder = new GameObject(holderName).transform; // Temp holder for the grid at any given time 
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

        Tiles[1, 1].SetColor(Color.green); // Sets Tile 1,1 to green 
        Tiles[9, 9].SetColor(Color.red); // Sets tile 9,9 to red

        Agent.MoveTo(Tiles[1, 1]);
        Agent.SetColor(Color.black);
    }

    public void GenerateWalls() // Generates walls around the outside of the grid 
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            Tiles[x, 0].SetColor(Color.black);
            Tiles[x, (int)mapSize.y - 1].SetColor(Color.black); // Sets the walls Color to black
        }
        for (int y = 0; y < mapSize.y; y++)
        {
            Tiles[0, y].SetColor(Color.black);
            Tiles[(int)mapSize.x - 1, y].SetColor(Color.black); // Sets the walls color to black
        }
    }


    public TileSet[,] GenerateTileSet() // Generates Tiles and colors them using Color extention // Sets each tile depending on its color
    {
        TileSet[,] tileSet = new TileSet[(int)mapSize.x, (int)mapSize.y];

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Color color = Tiles[x, y].GetColor();
                if(color == Color.white)
                {
                    tileSet[x, y] = TileSet.Empty; // if color is white -- Tile is empty
                }
                else if (color == Color.black)
                {
                    tileSet[x, y] = TileSet.Wall; // if color is black -- Tile is a wall
                }
                else if (color == Color.green)
                {
                    tileSet[x, y] = TileSet.Start; // if color is green -- Tile is the Start tile
                }
                else if (color == Color.red)
                {
                    tileSet[x, y] = TileSet.Finish; // if color is red -- Tile is the Finish tile
                }
            }
        }

        return tileSet;
    }
}

public enum TileSet // Enum that contains each type of tile
{
    Empty, // is a standard grid tile
    Wall, // is a wall
    Start, //  is the start point
    Finish // is the finish point
}
