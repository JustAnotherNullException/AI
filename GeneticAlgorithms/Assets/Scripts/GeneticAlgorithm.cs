using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


//increase pop size
//crossover doesnt fit project

//----------------------------------------------------------------------------------------------------------------------------//
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

    //----------------------------------------------------------------------------------------------------------------------------//

    int frames = 0;
    private void Update() 
    {
        foreach (Agent agent in Generation.Last().Agents)//For each Agent in the Last Generation -- Draw the paths they took
        {
            DrawDebugPath(agent);
        }

        frames++;
        if(frames > 20) // If frames is greater than 20 then create a new population and reset the list
        {
            for (int i = 0; i < 1; i++)
            {
                Population newPop = new Population(tileSet, Generation.Last());

                Generation.Clear();
                Generation.Add(newPop);
            }
            frames = 0;
        }

    }

    #region Debug

    //----------------------------------------------------------------------------------------------------------------------------//

    private void OnGUI()
    {
        string debugString = "";
        foreach (Agent agent in Generation.Last().Agents.OrderByDescending(a => a.CalFitness(tileSet))) // For each agent in the last generation -- order them from Best to Worst depending on their fitness 
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

    private string ColorToString(Color color) //Enables each color to be drawn to the screen as text for the GUI
    {
        if (color == Color.red) return "Red";
        else if (color == Color.green) return "Green";
        else if (color == Color.blue) return "Blue";
        else if (color == Color.magenta) return "Magenta";
        else if (color == Color.cyan) return "Cyan";
        else if (color == Color.black) return "Black";
        else if (color == Color.white) return "White";
        else if (color == Color.yellow) return "Yellow";
        else return color.ToString();
    }

    private void DrawDebugPath(Agent agent) 
    {
        Vector3 lineStart = transform.position; // Get Position of the agent

        int i = 0;
        foreach (Node node in agent.CalPath(tileSet)) // For each path that an Agent has taken. Draw a line from node to node beginning at (lineStart) and finishing at (lineFinish) using the list of Actions each agent took
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
    #endregion 
}

//--------------------------------------------------------------------------------------------------------------------------//


public class Population // Initial Population
{
    public List<Agent> Agents = new List<Agent>(); //List of Agents that are in each population

    TileSet[,] TileSet; //Gets each type of tile and makes it availible to use by the Population Class

    public Agent Best => Agents.OrderByDescending(a => a.CalFitness(TileSet)).First(); //Gets the best agent from the Population, Ranked / Ordered by Fitness

    static Color[] AllColors = new Color[] // Array of availible colours
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

//--------------------------------------------------------------------------------------------------------------------------//

    public Population(TileSet[,] tileSet, Population Prev) // Selection ------ Mutate :: Crossover
    {
        TileSet = tileSet;

        Agent prev = null;

        foreach(Agent agent in Prev.Agents) // For each Agent in the last Population
        {
            if(agent.CalFitness(tileSet) <= 0.3f) //Mutate all agents that had a fitness level of 0.3 or Less
            {
                Agents.Add(agent.Mutate());
            }

            prev = agent;
        }
    }
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

public enum Action // Enum of Each available action for the Agents
{
    Up, 
    Down, 
    Left,
    Right
}

//--------------------------------------------------------------------------------------------------------------------------//

public class Agent
{
    static int minimumActions = 16; // Mimimum Number of Actions to target (Best Outcome)
    static int maximumActions = 36; // Numbe of actions till the generation is Destroyed (Worst Outcome)

    public Action[] Genes { get; } = new Action[maximumActions]; // List of Actions - with the maximum actions as the length 

    public Color Color; // Gives the agent a color
    
    public Agent(Action[] m_Genes, Color color) // This creates the agent using an existing list of genes
    {
        Genes = m_Genes;
        Color = color;
    }

//--------------------------------------------------------------------------------------------------------------------------//

    public Agent(Color color) // The creates the agent by randomly generating a list of actions
    {
        Color = color;
        
        for (int i = 0; i < maximumActions; i++)
        {
            Action gene = (Action)Random.Range(0, 4); // Gets a random number between 0 and 4 and converts it into an action
            
            // This makes sure that the agent can't double back on itself - it checks if the previous gene is the opposite
            // of the current gene and if it is then it generates a new gene and tries again (a while loop is used to make
            // sure that it keeps trying until it generates a gene which is not the opposite of the previous gene) and the
            // while loop is only run if there is a previous gene (i > 0)
            while (i > 0 && ((gene == Action.Left && Genes[i - 1] == Action.Right) || (gene == Action.Right && Genes[i - 1] == Action.Left) ||
                   (gene == Action.Up && Genes[i - 1] == Action.Down) || (gene == Action.Down && Genes[i - 1] == Action.Up)))
            {
                gene = (Action)Random.Range(0, 4);
            }

            Genes[i] = gene; // Adds the gene to the list of actions
        }
    }

//--------------------------------------------------------------------------------------------------------------------------//

    public Agent Mutate() // Creates a new agent based on the current one except with some random modifications (mutations)
    {
        // Create a new agent using the current list of actions (genes)
        Agent agent = new Agent(Genes, Color);

        for (int i = 0; i < maximumActions; i++)
        {
            // Represents the progess though the for loop - 0 at the first gene (0%), 0.5 at the middle gene (50%) and 1 at the last gene (100%)
            float progress = (float)i / maximumActions;

            // Gets a random number between 0 and 100 and checks if it is less than (progress*100)
            // At the first gene, (progress*100) is 0 and a number between 0 and 100 is impossible to be smaller than 0 (first gene will never change)
            // At the middle gene, (progress*100) is 50 and there is equal chance of a number between 0 and 100 being smaller than 50
            // At the last gene, (progress*100) is 100 and a number between 0 and 100 is almost guaranteed to be smaller than 100
            if (Random.Range(0,100) < (progress * 100))
            {
                // If the random number was smaller than (progress*100), then we want to mutate this gene (ie generate a new random gene)
                // This code is the same as the code above for generating a random gene
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

//--------------------------------------------------------------------------------------------------------------------------//

    //public Agent CrossOver(Agent Other)
    //{
    //    //return Mutate();

    //    Agent agent = new Agent(Genes, Color);

    //    for (int i = 0; i < maximumActions; i++)
    //    {
    //        float progress = (float)i / maximumActions;

    //        if (Random.Range(0, 100) < (100 * progress))
    //        {
    //            Action gene = Other.Genes[i];
    //            Genes[i] = gene;
    //        }

    //    }
    //    return agent;
    //}

//--------------------------------------------------------------------------------------------------------------------------//

    public float CalFitness(TileSet[,] tileSet)
    {
        List<Node> path = CalPath(tileSet);

        Vector2 startPos = new Vector2(0, 0);
        Vector2 finishPos = GetTilePos(TileSet.Finish, tileSet);

        Node agent = path.Last();
        Vector2 agentPos = new Vector2(agent.PosX, agent.PosY);

        Vector2 startToFinish = finishPos - startPos;
        Vector2 agentToFinish = finishPos - agentPos;

        int actionsTaken = path.Count;

        float MutationRange = 0.5f; // Mutation Range
        float CrossoverRange = 1.0f - MutationRange; // CrossOver Range

        if (tileSet[agent.PosX, agent.PosY] == TileSet.Wall) 
        {
            return (1.0f - (agentToFinish.magnitude / startToFinish.magnitude)) * MutationRange;
        }

        if (tileSet[agent.PosX, agent.PosY] == TileSet.Finish) 
        {
            return (((float)minimumActions / actionsTaken) * CrossoverRange) + MutationRange; 
        }

        return 0;
    }

    // Rank each decison as it happens (Manhattan Distance)

//--------------------------------------------------------------------------------------------------------------------------//

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

            if (tileSet[posX, posY] == TileSet.Wall)  
            {
                return actionsTaken;
            }

            if (tileSet[posX, posY] == TileSet.Finish) 
            {
                return actionsTaken;
            }
        }

        return actionsTaken;
    }

//--------------------------------------------------------------------------------------------------------------------------//

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



