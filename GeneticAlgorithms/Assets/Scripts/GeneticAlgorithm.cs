using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    public Grid grid;

    Population Pop;

    TileSet[,] tileSet;

    private void Start()
    {
        tileSet = grid.GenerateTileSet();

        Pop = new Population(tileSet);
        

        Debug.Log(Pop.Best.CalFitness(tileSet));

    }

    private void Update() // comment
    {
        foreach (Agent agent in Pop.Agents) 
        {
            DrawDebugPath(agent);
        }
    }

    private void DrawDebugPath(Agent agent) // comment
    {
        Vector3 lineStart = transform.position;

        int i = 0;
        foreach (Node node in agent.CalPath(tileSet))
        {
            Vector3 lineEnd = lineStart;
            if (node.Action == Action.Up) lineEnd.z += 1;
            else if (node.Action == Action.Down) lineEnd.z -= 1;
            else if (node.Action == Action.Left) lineEnd.x -= 1;
            else if (node.Action == Action.Right) lineEnd.x += 1; 

            Color actualColor = agent.Color;
            if (i == 0) actualColor = Color.blue;
            if (i == agent.Genes.Length - 1) actualColor =  Color.red;
            
            Debug.DrawLine(lineStart, lineEnd, actualColor);

            lineStart = lineEnd;
            i++;
        }
    }
}


public class Population // comment
{
    public List<Agent> Agents = new List<Agent>(); 

    TileSet[,] TileSet;

    public Agent Best => Agents.OrderByDescending(a => a.CalFitness(TileSet)).First();

    public Population(TileSet[,] tileSet)
    {
        TileSet = tileSet;
        for (int i = 0; i < 100; i++)
        {
            Agents.Add(new Agent());
        }
    }

    public Population(TileSet[,] tileSet, Population Prev)
    {
        TileSet = tileSet;
        
    }
}


public enum Action
{
    Up, 
    Down, 
    Left,
    Right
}

public class Node
{
    public Action Action;
    public int PosX;
    public int PosY;

    public Node(Action action, int posX, int posY)
    {
        Action = action;
        PosX = posX;
        PosY = posY;
    }
}

public class Agent
{
    static int minimumActions = 16; // Mimimum Number of Actions to target (Best Outcome)
    static int maximumActions = 36; // Numbe of actions till the generation is Destroyed (Worst Outcome)

    public Action[] Genes { get; } = new Action[maximumActions]; // List of Actions - with the maximum actions as the length 

    public Color Color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1);

    public Agent(Action[] m_Genes) // Constructor takes in an exsisting list of Actions -- Used for :: Mutation :: Crossover 
    {
        Genes = m_Genes;
    }

    public Agent() // Randomlly Generations list of actions
    {
        for (int i = 0; i < maximumActions; i++)
        {
            Action gene = (Action)Random.Range(0, 4);
            Genes[i] = gene;
        }
    }

    public float CalFitness(TileSet[,] tileSet)
    {
        List<Node> path = CalPath(tileSet);

        Vector2 startPos = GetTilePos(TileSet.Start, tileSet);
        Vector2 finishPos = GetTilePos(TileSet.Finish, tileSet);
        
        Node agent = path.Last();
        Vector2 agentPos = new Vector2(agent.PosX, agent.PosY);

        Vector2 startToFinish = finishPos - startPos;
        Vector2 agentToFinish = finishPos - agentPos;

        int actionsTaken = path.Count;

        if (tileSet[agent.PosX, agent.PosY] == TileSet.Wall) // If it steps on a wall KILL IT 
        {
            return (1 - (agentToFinish.magnitude / startToFinish.magnitude)) * 0.5f;
        }

        if (tileSet[agent.PosX, agent.PosY] == TileSet.Finish) // If it steps on the finish line Good
        {
            return (((float)minimumActions / actionsTaken) * 0.5f) + 0.5f;
        }

        return 0;
    }

    public List<Node> CalPath(TileSet[,] tileSet)
    {
        List<Node> actionsTaken = new List<Node>();

        int posX, posY;
        GetTilePos(TileSet.Start, tileSet, out posX, out posY);

        foreach (Action Gene in Genes)
        {
            switch (Gene)
            {
                case Action.Up:
                    posY--;
                    break;

                case Action.Down:
                    posY++;
                    break;

                case Action.Left:
                    posX--;
                    break;

                case Action.Right:
                    posX++;
                    break;
            }

            actionsTaken.Add(new Node(Gene, posX, posY));

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

    private static void GetTilePos(TileSet desiredTile, TileSet[,] tileSet, out int posX, out int posY)
    {
        posX = 0;
        posY = 0;

        for (int x = 0; x < tileSet.GetLength(0); x++) // Gets X Co-ord for Start Position
        {
            for (int y = 0; y < tileSet.GetLength(1); y++) // Get Y Co-ord for Start Position
            {
                if (tileSet[x, y] == desiredTile)
                {
                    posX = x;
                    posY = y;
                }
            }
        }
    }

    private static Vector2 GetTilePos(TileSet desiredTile, TileSet[,] tileSet)
    {
        int x, y;
        GetTilePos(desiredTile, tileSet, out x, out y);
        return new Vector2(x, y);
    }
}

