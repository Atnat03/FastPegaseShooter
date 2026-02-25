using System;
using System.Collections.Generic;
using UnityEngine;

public class Dijkstra : MonoBehaviour
{
    double[,] G =
    {
        { 0, 5, 3, 2, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity },
        { double.PositiveInfinity, 0, double.PositiveInfinity, double.PositiveInfinity, 2,2, double.PositiveInfinity, double.PositiveInfinity },
        { double.PositiveInfinity, 1, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity },
        {double.PositiveInfinity, double.PositiveInfinity,1,0, double.PositiveInfinity, 3, 2, double.PositiveInfinity },
        { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, 0, 4, double.PositiveInfinity,7 },
        { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,4, double.PositiveInfinity },
        {double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, 1, double.PositiveInfinity,0,6},
        {double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, 0}
    };
    
    private double[] L;
    private int[] P;
    private int[] M;
    List<int> visited = new List<int>();
    private int nbSubmit;
    
    private void Start()
    {
        Initialisation(0);
        ProcessAlgo();
    }

    private void Initialisation(int s)
    {
        nbSubmit = G.GetLength(0);
        
        L = new double[nbSubmit];
        P = new int[nbSubmit];
        M = new int[nbSubmit];
        
        for (int i = 0; i < nbSubmit; i++)
        {
            L[i] = double.PositiveInfinity;
            P[i] = -1;
        }

        L[s] = 0;
        P[s] = s;

        for (int i = 0; i < nbSubmit; i++)
        {
            if(!double.IsPositiveInfinity(G[s, i]))
            {
                L[i] = G[s, i];
                P[i] = s;
            }
        }
        
        visited.Add(s);
    }

    public void ProcessAlgo()
    {
        for (int s = 0; s < nbSubmit; s++)
        {
            int x = MiniDistance(s);
            
            for (int y = 0; y < nbSubmit; y++)
            {
                if (!double.IsPositiveInfinity(L[y]))
                {
                    
                    float d = Mathf.Abs((float)(G[s, y] - G[s, x]));
                    
                    Debug.Log(d);
                    
                    if (L[y] > L[x] + d)
                    {
                        L[y] = L[x] + d;
                        P[y] = x;
                    }
                }
            }

            M[x] = x;
        }

        for (int i = 0; i < nbSubmit; i++)
        {
            Debug.Log(i + " / " + P[i]);
        }
    }

    public int MiniDistance(int i)
    {
        double mini = L[i];

        for (int s = 0; s < nbSubmit; s++)
        {
            if(L[s] < mini)
            {
                mini = L[s];
            }
        }
        
        return (int)mini;
    }
}
