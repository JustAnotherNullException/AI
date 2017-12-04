using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    public Grid grid;

    List<Agent> Pop;

    TileSet[,] tileSet;

    private void Start()
    {

        tileSet = grid.GenerateTileSet();
        Pop = new List<Agent>();

        for (int i = 0; i < 100; i++)
        {
            Pop.Add (new Agent());
        }

        Debug.Log(Pop.Max(a => a.CalFitness(tileSet)));

        /*int empties = 0;
        int walls = 0;
        int starts = 0;
        int finishes = 0;
        foreach (var tile in tileSet)
        {
            if (tile == TileSet.Empty) empties++;
            else if (tile == TileSet.Wall) walls++;
            else if (tile == TileSet.Start) starts++;
            else if (tile == TileSet.Finish) finishes++;
        }

        Debug.Log("empties = " + empties);
        Debug.Log("walls = " + walls);
        Debug.Log("starts = " + starts);
        Debug.Log("finishes = " + finishes);*/

    }

    private void Update()
    {
        DrawDebugPath(Pop.OrderByDescending(a => a.CalFitness(tileSet)).First(), Color.magenta);
    }

    private void DrawDebugPath(Agent agent, Color color)
    {
        Vector3 lineStart = transform.position;

        int i = 0;
        foreach (Action gene in agent.CalPath(tileSet))
        {
            Vector3 lineEnd = lineStart;
            if (gene == Action.Up) lineEnd.z += 1;
            else if (gene == Action.Down) lineEnd.z -= 1;
            else if (gene == Action.Left) lineEnd.x -= 1;
            else if (gene == Action.Right) lineEnd.x += 1;

            Color actualColor = color;
            if (i == 0) actualColor = Color.blue;
            if (i == agent.Genes.Length - 1) actualColor =  Color.red;
            
            Debug.DrawLine(lineStart, lineEnd, actualColor);

            lineStart = lineEnd;
            i++;
        }
    }
}


public enum Action
{
    Up, 
    Down, 
    Left,
    Right
}


public class Agent
{
    static int minimumActions = 18; // Mimimum Number of Actions to target (Best Outcome)
    static int maximumActions = 36; // Numbe of actions till the generation is Destroyed (Worst Outcome)

    public Action[] Genes { get; } = new Action[maximumActions]; // List of Actions - with the maximum actions as the length 

    public Agent(Action[] m_Genes) // Constructor takes in an exsisting list of Actions -- Used for :: Mutation :: Crossover 
    {
        Genes = m_Genes;
    }

    public Agent() // Randomlly Generations list of actions
    {
        for (int i = 0; i < maximumActions; i++)
        {
            Action gene = (Action)Random.Range(0,4);
            Genes[i] = gene;
        }
    }

    public int CalFitness(TileSet[,] tileSet)
    {
        int posX = 0;
        int posY = 0;

        int ActionsTaken = 0;

        GetStart(tileSet, out posX, out posY);

        foreach(Action Gene in Genes)
        {
            if(Gene == Action.Up)
            {
                posY--;
            }

            if (Gene == Action.Down)
            {
                posY++;
            }

            if (Gene == Action.Right)
            {
                posX++;
            }

            if (Gene == Action.Left)
            {
                posX--;
            }

            ActionsTaken++;

            if (tileSet[posX,posY] == TileSet.Wall) // If it steps on a wall KILL IT 
            {
                return ActionsTaken;
            }

            if(tileSet[posX, posY] == TileSet.Finish) // If it steps on the finish line Good
            {
                return 1000 + (maximumActions - ActionsTaken);
            }
        }

        return 0; ;
    }

    public List<Action> CalPath(TileSet[,] tileSet)
    {
        List<Action> actionsTaken = new List<Action>();

        int posX = 0;
        int posY = 0;

        GetStart(tileSet, out posX, out posY);

        foreach (Action Gene in Genes)
        {
            if (Gene == Action.Up)
            {
                posY--;
            }

            if (Gene == Action.Down)
            {
                posY++;
            }

            if (Gene == Action.Right)
            {
                posX++;
            }

            if (Gene == Action.Left)
            {
                posX--;
            }

            actionsTaken.Add(Gene);

            if (tileSet[posX, posY] == TileSet.Wall) // If it steps on a wall KILL IT 
            {
                return actionsTaken;
            }

            if (tileSet[posX, posY] == TileSet.Finish) // If it steps on the finish line Good
            {
                return actionsTaken;
            }
        }

        return actionsTaken;
    }

    private static void GetStart(TileSet[,] tileSet, out int posX, out int posY)
    {
        posX = 0;
        posY = 0;

        for (int x = 0; x < tileSet.GetLength(0); x++) // Gets X Co-ord for Start Position
        {
            for (int y = 0; y < tileSet.GetLength(1); y++) // Get Y Co-ord for Start Position
            {
                if (tileSet[x, y] == TileSet.Start)
                {
                    posX = x;
                    posY = y;
                }
            }
        }
    }
}

