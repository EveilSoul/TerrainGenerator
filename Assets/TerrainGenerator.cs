using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class TerrainGenerator : MonoBehaviour
{
    public Vector3 TerrainPosition;
    public float Roughness;

    public Terrain OldTerrain;

    //[SerializeField] private int lenght;
    [SerializeField] private int width;
    [SerializeField] private int height;

    private int lenght;

    [SerializeField] private int smoothSteps;
    [SerializeField] private uint seed;

    [SerializeField] private InitedHeightValues InitHeigtValues;

    public static Unity.Mathematics.Random Random;
    private float[,] generatedHeights;

    [Serializable]
    struct InitedHeightValues
    {
        public bool GenerateRandomly;

        [Range(0, 1)] [Tooltip("pos [0,0]")]
        public float LeftTopAngle;
        [Range(0, 1)] [Tooltip("pos [0,n]")]
        public float RightTopAngle;
        [Range(0, 1)] [Tooltip("pos [n,0]")]
        public float LeftBottomAngle;
        [Range(0, 1)] [Tooltip("pos [n,n]")]
        public float RightBottomAngle;

        [Tooltip("may be used for initializing values without border, but if not be used values from other side of terrain")]
        public bool IsCustomBorder;
        [Tooltip("from 0 to height")]
        public float DefaultBorderValue;
    }


    [ContextMenu("Generate Terrain")]
    void Genarate()
    {
        Random.InitState(seed);
        lenght = width;
        //var terrain = InstantiateTerrain();
        var terrain = OldTerrain;
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }


    [ContextMenu("Save terrain for processing")]
    void Save()
    {
        generatedHeights = OldTerrain.terrainData.GetHeights(0, 0, width, lenght);
    }

    [ContextMenu("Smooth Terrain by Median")]
    void SmoothMedian()
    {
        float[,] heights = new float[lenght, width];
        for (int i = 0; i < lenght; i++)
            for (int j = 0; j < width; j++)
                heights[i, j] = generatedHeights[i, j];

        for (int i = 0; i < smoothSteps; i++)
        {
            heights = TerrainSmoother.SmoothTerrainMedian(heights, lenght);
        }
        OldTerrain.terrainData.SetHeights(0, 0, heights);
    }

    [ContextMenu("Terrain by x=sqrt(x)")]
    void SmoothSqrt()
    {
        float[,] heights = new float[lenght, width];
        for (int i = 0; i < lenght; i++)
            for (int j = 0; j < width; j++)
                heights[i, j] = generatedHeights[i, j];

        for (int i = 0; i < smoothSteps; i++)
        {
            heights = TerrainSmoother.SqrtSmoothing(heights, lenght);
        }
        OldTerrain.terrainData.SetHeights(0, 0, heights);
    }

    [ContextMenu("Smooth Terrain by x=x^2")]
    void SmoothSquare()
    {
        float[,] heights = new float[lenght, width];
        for (int i = 0; i < lenght; i++)
            for (int j = 0; j < width; j++)
                heights[i, j] = generatedHeights[i, j];

        for (int i = 0; i < smoothSteps; i++)
        {
            heights = TerrainSmoother.SquareSmoothing(heights, lenght);
        }
        OldTerrain.terrainData.SetHeights(0, 0, heights);
    }

    Terrain InstantiateTerrain()
    {
        GameObject terrainObject = new GameObject();
        terrainObject.transform.position = TerrainPosition;
        Terrain terrain = terrainObject.AddComponent<Terrain>();
        terrain.terrainData = new TerrainData();
        return terrain;
    }

    private TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.size = new Vector3(lenght, height, width);
        terrainData.heightmapResolution = lenght;
        generatedHeights = GenerateMap();

        terrainData.SetHeights(0, 0, generatedHeights);
        return terrainData;
    }

    private float[,] GenerateMap()
    {
        float[,] heights = new float[lenght, width];
        GenerateBorder(heights);
        int len = lenght - lenght % 2;
        while (len > 1)
        {
            PreformSquare(len, heights);
            PerformDiamond(len, heights);
            len /= 2;
        }
        return heights;
    }

    private void GenerateBorder(float[,] heights)
    {
        heights[0, 0] = InitHeigtValues.GenerateRandomly ? Random.NextFloat(0, 1) : InitHeigtValues.LeftTopAngle;
        heights[0, lenght - 1] = InitHeigtValues.GenerateRandomly ? Random.NextFloat(0, 1) : InitHeigtValues.RightTopAngle;
        heights[lenght - 1, 0] = InitHeigtValues.GenerateRandomly ? Random.NextFloat(0, 1) : InitHeigtValues.LeftBottomAngle;
        heights[lenght - 1, lenght - 1] = InitHeigtValues.GenerateRandomly ? Random.NextFloat(0, 1) : InitHeigtValues.RightBottomAngle;
    }

    // Выполнение шага Diamond алгоритма для всей карты
    private void PerformDiamond(int len, float[,] heights)
    {
        for (int x = 0; x < lenght - 1; x += len)
        {
            for (int y = 0; y < width - 1; y += len)
            {
                DiamondStep(x, y + len / 2, len / 2, heights);
                DiamondStep(x + len / 2, y, len / 2, heights);
                DiamondStep(x + len, y + len / 2, len / 2, heights);
                DiamondStep(x + len / 2, y + len, len / 2, heights);
            }
        }
    }

    // Выполнение шага Square алгоритма для всей карты
    private void PreformSquare(int len, float[,] heights)
    {
        for (int x = 0; x < lenght - 1; x += len)
            for (int y = 0; y < width - 1; y += len)
                SquareStep(x, y, x + len, y + len, heights);
    }

    // Определение высоты средней точки для квадрата, заданного двумя противоположными точками
    private void SquareStep(int leftX, int bottomY, int rightX, int topY, float[,] heights)
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
    public void DiamondStep(int centerX, int centerY, int length, float[,] heights)
    {
        float left = 0, right = 0, top = 0, bottom = 0;
        //// Получаем начальные значения высоты на граничных точках
        //var left = GetBorderValue();
        //var right = GetBorderValue();
        //var top = GetBorderValue();
        //var bottom = GetBorderValue();

        // Если точки не выходят за границы массива, берем их высоту из карты
        if (centerX - length >= 0)
            left = heights[centerX - length, centerY];
        else left = InitHeigtValues.IsCustomBorder? InitHeigtValues.DefaultBorderValue / height : heights[centerX + length, centerY];

        if (centerX + length < lenght)
            right = heights[centerX + length, centerY];
        else right = InitHeigtValues.IsCustomBorder ? InitHeigtValues.DefaultBorderValue / height : heights[centerX - length, centerY];

        if (centerY - length >= 0)
            bottom = heights[centerX, centerY - length];
        else bottom = InitHeigtValues.IsCustomBorder ? InitHeigtValues.DefaultBorderValue / height : heights[centerX, centerY + length];

        if (centerY + length < lenght)
            top = heights[centerX, centerY + length];
        else top = InitHeigtValues.IsCustomBorder ? InitHeigtValues.DefaultBorderValue / height : heights[centerX, centerY - length];

        // Определяем высоту средней точки
        var sum = left + right + top + bottom;
        SetHeight(sum, length, centerX, centerY, heights);
    }

    // Возвращает высоту определенной точки, отталкиваясь от суммы соседних и длины текущего шага
    private void SetHeight(float sum, int len, int posX, int posY, float[,] heights)
    {
        var result = sum / 4 + Random.NextDouble(-Roughness * len, Roughness * len);
        Debug.Log($"sum: {sum} {result}");
        heights[posX, posY] = (float)result;
    }
}
