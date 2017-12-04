using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    Agent A;
    Agent B;
    Agent C;

    private void Start()
    {
        A = new Agent();
        B = new Agent();
        C = new Agent();
    }

    private void Update()
    {
        DrawDebugPath(A, Color.green);
        DrawDebugPath(B, Color.black);
        DrawDebugPath(C, Color.white);
    }

    private void DrawDebugPath(Agent agent, Color color)
    {
        Vector3 lineStart = transform.position;
        for (int i = 0; i < agent.Genes.Length; i++)
        {
            Action gene = agent.Genes[i];

            Vector3 lineEnd = lineStart ;
            if (gene == Action.Up) lineEnd.z += 1;
            else if (gene == Action.Down) lineEnd.z -= 1;
            else if (gene == Action.Left) lineEnd.x -= 1;
            else if (gene == Action.Right) lineEnd.x += 1;

            Color actualColor = color;
            if (i == 0) actualColor = Color.blue;
            if (i == agent.Genes.Length - 1) actualColor =  Color.red;


            Debug.DrawLine(lineStart, lineEnd, actualColor);

            lineStart = lineEnd;
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

    public int CalFitness()
    {
        return 0;
    }

}