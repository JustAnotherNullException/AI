using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{

    public Transform TilePrefab;
    public Vector2 mapSize;

    [Range (0,1)]
    public float outlinePercent;

    void Start()
    {
        mapSize.x = 10;
        mapSize.y = 10;
        outlinePercent = 0.5f;

        GenerateGrid();
        
    }

    public void GenerateGrid() {


        string holderName = "GeneratedGrid";

        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        GameObject[,] Tiles = new GameObject[mapSize.x ,mapSize.y];

        Transform gridHolder = new GameObject(holderName).transform;
        gridHolder.parent = transform;

        for (int x = 0; x < mapSize.x; x++) {

            for (int y = 0; y < mapSize.y; y++) {
                Vector3 tilePosition = new Vector3(-mapSize.x / 2 + 0.5f + x, 0, - mapSize.y/2+ 0.5f + y);
                Transform newTile = Instantiate(TilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent);
                newTile.parent = gridHolder;

                Tiles[x, y] = newTile;
            }   
        }

        Tiles[5, 3].SetActive(false);
    }
}