using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    public Grid grid;

    public List<Population> Generation = new List<Population>();

    TileSet[,] tileSet;

    private void Start()
    {
        tileSet = grid.GenerateTileSet();

        Generation.Add(new Population(tileSet));

       // Debug.Log(Pop.Best.CalFitness(tileSet));

    }

    private void Update() // comment
    {
        foreach (Agent agent in Generation.Last().Agents)
        {
            DrawDebugPath(agent);
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            for(int i = 0; i < 1000; i++)
            {
                Generation.Add(new Population(tileSet, Generation.Last()));
            }
        }
    }

    private void OnGUI()
    {
        string debugString = "";
        foreach (Agent agent in Generation.Last().Agents.OrderByDescending(a => a.CalFitness(tileSet)))
        {
            string agentString = string.Format("{0} ({1}) = ", ColorToString(agent.Color), agent.CalFitness(tileSet));
            foreach (Node node in agent.CalPath(tileSet))
            {
                agentString += node.Action.ToString() + ",";
            }
            debugString += agentString + "\n";
        }

        GUI.Label(new Rect(20, 20, 1280, 720), debugString);
    }

    private string ColorToString(Color color)
    {
        if (color == Color.red) return "red";
        else if (color == Color.green) return "green";
        else if (color == Color.blue) return "blue";
        else if (color == Color.magenta) return "magenta";
        else if (color == Color.cyan) return "cyan";
        else if (color == Color.black) return "black";
        else if (color == Color.white) return "white";
        else if (color == Color.yellow) return "yellow";
        else return color.ToString();
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

//--------------------------------------------------------------------------------------------------------------------------//


public class Population // Initial Population
{
    public List<Agent> Agents = new List<Agent>(); 

    TileSet[,] TileSet;

    public Agent Best => Agents.OrderByDescending(a => a.CalFitness(TileSet)).First();

    static Color[] AllColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.magenta,
        Color.cyan,
        Color.black,
        Color.white,
        Color.yellow
    };

    public Population(TileSet[,] tileSet)
    {
        TileSet = tileSet;
        for (int i = 0; i < AllColors.Length; i++)
        {
            Agents.Add(new Agent(AllColors[i]));
        }
    }

    public Population(TileSet[,] tileSet, Population Prev) // Selection ------ Mutate :: Crossover
    {
        TileSet = tileSet;

        Agent temp = null;

        foreach(Agent agent in Prev.Agents)
        {
            if(agent.CalFitness(tileSet) < 0.5f) // Mutation
            {
                Agents.Add(agent.Mutate());
            }
            else if(temp != null) //Crossover
            {
                Agents.Add(agent.CrossOver(temp));
            }

            temp = agent;
        }
    }
}


//  Initial
//
//  Selection
//
//  Mutate -- Crossover
//
//  

//--------------------------------------------------------------------------------------------------------------------------//

public enum Action
{
    Up, 
    Down, 
    Left,
    Right
}

//--------------------------------------------------------------------------------------------------------------------------//

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

//--------------------------------------------------------------------------------------------------------------------------//

public class Agent
{
    static int minimumActions = 16; // Mimimum Number of Actions to target (Best Outcome)
    static int maximumActions = 36; // Numbe of actions till the generation is Destroyed (Worst Outcome)

    public Action[] Genes { get; } = new Action[maximumActions]; // List of Actions - with the maximum actions as the length 

    public Color Color;
    
    public Agent(Action[] m_Genes, Color color)
    {
        Genes = m_Genes;
        Color = color;
    }



    public Agent(Color color) // Randomlly Generations list of actions
    {
        Color = color;
        for (int i = 0; i < maximumActions; i++)
        {
            Action gene = gene = (Action)Random.Range(0, 4);
            while (i > 0 && ((gene == Action.Left && Genes[i - 1] == Action.Right) || (gene == Action.Right && Genes[i - 1] == Action.Left) ||
                   (gene == Action.Up && Genes[i - 1] == Action.Down) || (gene == Action.Down && Genes[i - 1] == Action.Up)))
            {
                gene = (Action)Random.Range(0, 4);
            }

            Genes[i] = gene;
        }
    }

    public Agent Mutate()
    {
        Agent agent = new Agent(Genes, Color);

        for (int i = 0; i < maximumActions; i++)
        {
            float progress = (float)i / maximumActions;

            if (Random.Range(0,100) < (100 * progress))
            {
                Action gene = gene = (Action)Random.Range(0, 4);
                while (i > 0 && ((gene == Action.Left && Genes[i - 1] == Action.Right) || (gene == Action.Right && Genes[i - 1] == Action.Left) ||
                       (gene == Action.Up && Genes[i - 1] == Action.Down) || (gene == Action.Down && Genes[i - 1] == Action.Up)))
                {
                    gene = (Action)Random.Range(0, 4);
                }
                Genes[i] = gene;
            }
        }

        return agent;
    }

    public Agent CrossOver(Agent Other)
    {
        return Mutate();

        /*Agent agent = new Agent(Genes, Color);

        for (int i = 0; i < maximumActions; i++)
        {
            float progress = (float)i / maximumActions;

            if (Random.Range(0, 100) < (100 * progress))
            {
                Action gene = Other.Genes[i];
                Genes[i] = gene;
            }

        }
        return agent;*/
    }

    //--------------------------------------------------------------------------------------------------------------------------//

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

        float range1 = 0.8f;
        float range2 = 1.0f - range1;

        if (tileSet[agent.PosX, agent.PosY] == TileSet.Wall) // If it steps on a wall KILL IT 
        {
            return (1.0f - (agentToFinish.magnitude / startToFinish.magnitude)) * range1;
        }

        if (tileSet[agent.PosX, agent.PosY] == TileSet.Finish) // If it steps on the finish line Good
        {
            return (((float)minimumActions / actionsTaken) * range2) + range1;
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

//--------------------------------------------------------------------------------------------------------------------------//



