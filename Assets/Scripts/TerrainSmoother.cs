using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSmoother
{
    public static float[,] SmoothTerrainMedian(float[,] heights, int lenght)
    {
        for (int i = 0; i < lenght; i++)
        {
            for (int j = 0; j < lenght; j++)
            {
                if (i * j != 0 && i != lenght - 1 && j != lenght - 1)
                {
                    heights[i, j] = Median(heights[i, j], heights[i + 1, j], heights[i, j + 1], heights[i - 1, j], heights[i, j - 1]);
                }
                else if (i == 0 && j == 0)
                {
                    heights[i, j] = Median(heights[i, j], heights[i + 1, j], heights[i, j + 1]);
                }
                else if (i == 0 && j != lenght - 1)
                {
                    heights[i, j] = Median(heights[i, j], heights[i + 1, j], heights[i, j + 1], heights[i, j - 1]);
                }
                else if (i == 0 && j == lenght - 1)
                {
                    heights[i, j] = Median(heights[i, j], heights[i + 1, j], heights[i, j - 1]);
                }
                else if (j == 0 && i != lenght - 1)
                {
                    heights[i, j] = Median(heights[i, j], heights[i + 1, j], heights[i, j + 1], heights[i - 1, j]);
                }
                else if (j == 0 && i == lenght - 1)
                {
                    heights[i, j] = Median(heights[i, j], heights[i, j + 1], heights[i - 1, j]);
                }
                else if (i == lenght - 1 && j == lenght - 1)
                {
                    heights[i, j] = Median(heights[i, j], heights[i - 1, j], heights[i, j - 1]);
                }
                else if (i == lenght - 1)
                {
                    heights[i, j] = Median(heights[i, j], heights[i, j + 1], heights[i - 1, j], heights[i, j - 1]);
                }
                else if (j == lenght - 1)
                {
                    heights[i, j] = Median(heights[i, j], heights[i + 1, j], heights[i - 1, j], heights[i, j - 1]);
                }
            }
        }
        return heights;
    }

    private static float Median(params float[] points)
    {
        points = BubleSort(points);
        if (points.Length % 2 != 0)
            return points[points.Length / 2];
        else return (points[points.Length / 2] + points[points.Length / 2 - 1]) / 2;
    }

    private static float[] BubleSort(float[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            for (int j = i; j < array.Length; j++)
            {
                if (array[i] > array[j])
                {
                    var t = array[i];
                    array[i] = array[j];
                    array[j] = t;
                }
            }
        }
        return array;
    }

    public static float[,] SqrtSmoothing(float[,] heights, int lenght)
    {
        for (int i = 0; i < lenght; i++)
            for (int j = 0; j < lenght; j++)
                heights[i, j] = Mathf.Sqrt(heights[i, j]);
        return heights;
    }

    public static float[,] SquareSmoothing(float[,] heights, int lenght)
    {
        for (int i = 0; i < lenght; i++)
            for (int j = 0; j < lenght; j++)
                heights[i, j] *= heights[i, j];
        return heights;
    }
}
