using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//--------------------------------------------------------------------------------------------------------------------------------//
public class GeneticAlgorithm : MonoBehaviour
{
    public Grid grid; 

    public List<Population> Generation = new List<Population>();

    bool finishStop = false; // Checks to see if an agent has made it to the finish line or not

    TileSet[,] tileSet; //Gets each type of tile and makes it availible to use by the Population Class

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
            agent.CalFitness(tileSet, ref finishStop);
        }

        frames++;
        if(frames > 20) // If frames is greater than 20 then create a new population and reset the list
        {
            if(finishStop == false) // If an agent has NOT reached the finish line then create new populations until one does
            {
                for (int i = 0; i < 20; i++)
                {
                    Population newPop = new Population(tileSet, Generation.Last());

                    Generation.Clear();
                    Generation.Add(newPop);
                }
                frames = 0;
            }
            
        }

    }

    #region Debug

    //----------------------------------------------------------------------------------------------------------------------------//

    private void OnGUI()
    {
        // For each agent in the last generation -- order them from Best to Worst depending on their fitness 
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
        // For each path that an Agent has taken. Draw a line from node to node beginning at (lineStart) 
        // and finishing at (lineFinish) using the list of Actions each agent took
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

            else if(prev != null) //CrossOver Requires Previous 
            {
                Agents.Add(agent.CrossOver(prev));
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
                while 
                    (i > 0 && 
                    ((gene == Action.Left && Genes[i - 1] == Action.Right) 
                    || (gene == Action.Right && Genes[i - 1] == Action.Left) 
                    || (gene == Action.Up && Genes[i - 1] == Action.Down) 
                    || (gene == Action.Down && Genes[i - 1] == Action.Up)))
                {
                    gene = (Action)Random.Range(0, 4);
                }
                Genes[i] = gene;
            }
        }

        return agent;
    }

//--------------------------------------------------------------------------------------------------------------------------//

    public Agent CrossOver(Agent Other)
    {
        //return Mutate();

        Agent agent = new Agent(Genes, Color);

        for (int i = 0; i < maximumActions; i++)
        {
            float progress = (float)i / maximumActions;

            if (Random.Range(0, 100) < (100 * progress))
            {
                Action gene = Other.Genes[i];
                Genes[i] = gene;
            }

        }
        return agent;
    }

//--------------------------------------------------------------------------------------------------------------------------//

    public float CalFitness(TileSet[,] tileSet)
    {
        bool discard = false;
        return CalFitness(tileSet, ref discard);
    }

    public float CalFitness(TileSet[,] tileSet,ref bool finishStop) // Calculate how good (fit) the agent is
    {
        Vector2 startPos = new Vector2(0, 0);
        Vector2 finishPos = GetTilePos(TileSet.Finish, tileSet);
        
        // Calculate the path that the agent would take
        List<Node> path = CalPath(tileSet);

        // The last node in the path is where the agent ended up
        Node agent = path.Last();
        
        Vector2 agentPos = new Vector2(agent.PosX, agent.PosY);

        Vector2 startToFinish = finishPos - startPos; // Calculate distance from start to finish
        Vector2 agentToFinish = finishPos - agentPos; // Calculate distance from agent to finish

        int actionsTaken = path.Count; // Get the number of action taken before reaching the goal (or dying)

        
        float MutationRange = 0.5f; // If the agent 
        float CrossoverRange = 1.0f - MutationRange; // CrossOver Range

        // If the agent died (hit a wall)
        if (tileSet[agent.PosX, agent.PosY] == TileSet.Wall) 
        {
            // Work out how close the agent was to the finish, as a fraction of the total distance from start to finish
            float percentUnfinished = agentToFinish.magnitude / startToFinish.magnitude;
            
            // Flip percent unfinished so instead of 0..1 it is 1..0
            float percentFinished = 1.0f - percentUnfinished;
            
            float fitness = percentFinished * MutationRange;
            
            return fitness;
        }

        // If the agent reached the goal
        else if (tileSet[agent.PosX, agent.PosY] == TileSet.Finish) 
        {
            finishStop = true;

            // Get the minimum number of possible steps as a fraction of the number of steps actually taken
            float percentActionsTaken = (float)minimumActions / actionsTaken;
            
            float fitness = (percentActionsTaken * CrossoverRange) + MutationRange;
            
            return fitness;
        }

        else return 0.5f; // what is the fitness if it avoids dying but doesn't reach the target?
    }

//--------------------------------------------------------------------------------------------------------------------------//

    public List<Node> CalPath(TileSet[,] tileSet) // Calculate the path that the agent takes
    {
        List<Node> actionsTaken = new List<Node>();

        // Set posX and posY to the position of the start tile
        int posX, posY;
        GetTilePos(TileSet.Start, tileSet, out posX, out posY);

        // For every action...
        foreach (Action Gene in Genes)
        {
            // Check what action it is...
            switch (Gene)
            {
                // If the action is up, decrease posY
                case Action.Up:
                    posY--;
                    break;

                // If the action is down, increase posY
                case Action.Down:
                    posY++;
                    break;

                // If the action is left, decrease posX
                case Action.Left:
                    posX--;
                    break;

                // If the action is right, increase posX
                case Action.Right:
                    posX++;
                    break;
            }

            // Add a node containing the action and the posX and posY of the action to the list
            actionsTaken.Add(new Node(Gene, posX, posY));

            // If the tile at (posX, posY) is a wall, stop and return the actions taken up to this point
            if (tileSet[posX, posY] == TileSet.Wall)  
            {
                return actionsTaken;
            }

            // If the tile at (posX, posY) is the finish, stop and return the actions taken up to this point
            else if (tileSet[posX, posY] == TileSet.Finish) 
            {
                return actionsTaken;
            }
        }

        // If we have gone through all the actions without hitting a wall or the finish, return the actions taken
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



/* Reasons the Algorithm Didnt Work
 * 
 * First :  
 * The population size should have been much larger to greatly reduce the law of diminishing returns 
 * 
 * eg. after a few runs the population would become stagnant
 * and all have similar behaviour.
 * 
 * Second :
 * The Rules implemented into the algorithm should have told more to the agent. Such as using a heuristic to calculate the number of actions left to finish 
 * and mutating on a more micro and detailed level. 
 * 
 * eg. each action could have been valued seperatly and scored depending on the action said agent took after each action was taken. this way
 * the agent would have more to work with in its learning progress and theoretically would be able to have a greater chance at acheiving its goal more efficiently.
 * Also using this system above should have allowed the agent to improve on how it reached the finish rather than just randomally finding it as it does in its 
 * 
 * Third :
 * Because each gene is just the action it had taken followed by and proceded by another action the crossover funcition was rather usless in producing and improved
 * results. The algorithm would have been in a more efficient state had time not been wasted on the crossover function and if that time was focused optimising and implementing
 * the points above the algorithm would have most likely been funcitonal as intended or something close to it.
 * 
 *  
 * 
 * 
 */