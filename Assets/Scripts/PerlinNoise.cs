using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise
{
    public static float[,] AddPerlin(float[,] heights, int lenght, float intensity, float scale, bool isSubstraction = false)
    {
        var multiplier = isSubstraction ? -1 : 1;
        for (int i = 0; i < lenght; i++)
        {
            for (int j = 0; j < lenght; j++)
            {
                heights[i, j] += Mathf.PerlinNoise(i / scale, j / scale) * intensity * multiplier;
            }
        }
        return heights;
    }
}
