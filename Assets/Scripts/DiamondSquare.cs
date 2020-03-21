using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondSquare
{
    public static float Roughness => terrainGenerator.Roughness;

    public static int Width => terrainGenerator.Width;
    public static int Height => terrainGenerator.Height;
    public static int Lenght => Width;

    public static TerrainGenerator.InitedHeightValues InitHeigtValues => terrainGenerator.InitHeigtValues;

    private static TerrainGenerator terrainGenerator;

    public static TerrainData GenerateTerrain(TerrainData terrainData, TerrainGenerator generator)
    {
        terrainGenerator = generator;
        terrainData.size = new Vector3(Width, Height, Width);
        terrainData.heightmapResolution = Lenght;
        var generatedHeights = GenerateMap();

        terrainData.SetHeights(0, 0, generatedHeights);
        return terrainData;
    }

    private static float[,] GenerateMap()
    {
        float[,] heights = new float[Lenght, Width];
        GenerateBorder(heights);
        int len = Lenght - Lenght % 2;
        while (len > 1)
        {
            PreformSquare(len, heights);
            PerformDiamond(len, heights);
            len /= 2;
        }
        return heights;
    }

    private static void GenerateBorder(float[,] heights)
    {
        heights[0, 0] = InitHeigtValues.GenerateRandomly ? TerrainGenerator.Random.NextFloat(0, 1) : InitHeigtValues.LeftTopAngle;
        heights[0, Lenght - 1] = InitHeigtValues.GenerateRandomly ? TerrainGenerator.Random.NextFloat(0, 1) : InitHeigtValues.RightTopAngle;
        heights[Lenght - 1, 0] = InitHeigtValues.GenerateRandomly ? TerrainGenerator.Random.NextFloat(0, 1) : InitHeigtValues.LeftBottomAngle;
        heights[Lenght - 1, Lenght - 1] = InitHeigtValues.GenerateRandomly ? TerrainGenerator.Random.NextFloat(0, 1) : InitHeigtValues.RightBottomAngle;
    }

    // Выполнение шага Diamond алгоритма для всей карты
    private static void PerformDiamond(int len, float[,] heights)
    {
        for (int x = 0; x < Lenght - 1; x += len)
        {
            for (int y = 0; y < Width - 1; y += len)
            {
                DiamondStep(x, y + len / 2, len / 2, heights);
                DiamondStep(x + len / 2, y, len / 2, heights);
                DiamondStep(x + len, y + len / 2, len / 2, heights);
                DiamondStep(x + len / 2, y + len, len / 2, heights);
            }
        }
    }

    // Выполнение шага Square алгоритма для всей карты
    private static void PreformSquare(int len, float[,] heights)
    {
        for (int x = 0; x < Lenght - 1; x += len)
            for (int y = 0; y < Width - 1; y += len)
                SquareStep(x, y, x + len, y + len, heights);
    }

    // Определение высоты средней точки для квадрата, заданного двумя противоположными точками
    private static void SquareStep(int leftX, int bottomY, int rightX, int topY, float[,] heights)
    {
        // Берем значения высоты во всех вершинах квадрата и суммируем
        var leftTop = heights[leftX, topY];
        var leftBottom = heights[leftX, bottomY];
        var rightTop = heights[rightX, topY];
        var rightBottom = heights[rightX, bottomY];
        float sum = leftTop + leftBottom + rightTop + rightBottom;

        var length = (rightX - leftX) / 2;
        var centerX = leftX + length;
        var centerY = bottomY + length;

        // Определяем высоту средней точки
        SetHeight(sum, length, centerX, centerY, heights);
    }

    // Шаг Diamond для конкретной точки.
    // Определение высоты средней точки в получившихся
    // на шаге Square ромбах
    public static void DiamondStep(int centerX, int centerY, int length, float[,] heights)
    {
        float left, right, top, bottom;

        // Если точки не выходят за границы массива, берем их высоту из карты
        if (centerX - length >= 0)
            left = heights[centerX - length, centerY];
        else left = InitHeigtValues.IsCustomBorder ? InitHeigtValues.DefaultBorderValue / Height : heights[centerX + length, centerY];

        if (centerX + length < Lenght)
            right = heights[centerX + length, centerY];
        else right = InitHeigtValues.IsCustomBorder ? InitHeigtValues.DefaultBorderValue / Height : heights[centerX - length, centerY];

        if (centerY - length >= 0)
            bottom = heights[centerX, centerY - length];
        else bottom = InitHeigtValues.IsCustomBorder ? InitHeigtValues.DefaultBorderValue / Height : heights[centerX, centerY + length];

        if (centerY + length < Lenght)
            top = heights[centerX, centerY + length];
        else top = InitHeigtValues.IsCustomBorder ? InitHeigtValues.DefaultBorderValue / Height : heights[centerX, centerY - length];

        // Определяем высоту средней точки
        var sum = left + right + top + bottom;
        SetHeight(sum, length, centerX, centerY, heights);
    }

    // Возвращает высоту определенной точки, отталкиваясь от суммы соседних и длины текущего шага
    private static void SetHeight(float sum, int len, int posX, int posY, float[,] heights)
    {
        var result = sum / 4 + TerrainGenerator.Random.NextDouble(-Roughness * len, Roughness * len);
        heights[posX, posY] = (float)result;
    }
}
