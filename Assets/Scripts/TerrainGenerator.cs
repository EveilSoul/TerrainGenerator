using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;

[RequireComponent(typeof(ColorGenerator))]
public class TerrainGenerator : MonoBehaviour
{
    #region BaseFields
    public Vector3 TerrainPosition;
    
    public float Roughness;

    public Terrain CurrentTerrainForGeneration { get; private set; }

    public int Width;
    public int Height;
    [SerializeField] private uint seed;
    [SerializeField] private float perlinIntensity;
    [SerializeField] private float perlinScale;

    public float MinHeight { get; private set; }
    public float MaxHeight { get; private set; }
    public Material DefaultMaterial;
    public Material SovsemDefaultMaterial;
    private ColorGenerator colorGenerator;

    [Range(0, 6)]
    [Tooltip("Сколько раз будет применен какой лмбо метод (e.g smoothing, not perlin)")]
    [SerializeField] private int ApplyingMethodCount;

    public InitedHeightValues InitHeigtValues;

    public static Unity.Mathematics.Random Random;
    private float[,] generatedHeights;

    

    [Serializable]
    public struct InitedHeightValues
    {
        public bool GenerateRandomly;

        [Range(0, 1)]
        [Tooltip("pos [0,0]")]
        public float LeftTopAngle;
        [Range(0, 1)]
        [Tooltip("pos [0,n]")]
        public float RightTopAngle;
        [Range(0, 1)]
        [Tooltip("pos [n,0]")]
        public float LeftBottomAngle;
        [Range(0, 1)]
        [Tooltip("pos [n,n]")]
        public float RightBottomAngle;

        [Tooltip("may be used for initializing values without border, but if not be used values from other side of terrain")]
        public bool IsCustomBorder;
        [Tooltip("from 0 to height")]
        public float DefaultBorderValue;
    }
    #endregion

    private void Start()
    {
        colorGenerator = gameObject.GetComponent<ColorGenerator>();
        generatedHeights = CurrentTerrainForGeneration.terrainData.GetHeights(0, 0, Width, Width);
        UpdateMinMaxHeights(generatedHeights);
        colorGenerator.Generate();
    }

    public void Generate()
    {
        Random.InitState(seed);
        var terrain = CurrentTerrainForGeneration;
        terrain.terrainData = DiamondSquare.GenerateTerrain(terrain.terrainData, this);
        Save();
    }

    public void GenerateColors()
    {
        if (colorGenerator == null)
            colorGenerator = GetComponent<ColorGenerator>();
        colorGenerator.Generate();
    }

    public void GenerateGrass()
    {
        if (colorGenerator == null)
            colorGenerator = GetComponent<ColorGenerator>();
        colorGenerator.GenerateGrass();
    }

    public void GenerateTrees()
    {
        if (colorGenerator == null)
            colorGenerator = GetComponent<ColorGenerator>();
        colorGenerator.GenerateTreesAndStones();
    }


    public void Save()
    {
        generatedHeights = CurrentTerrainForGeneration.terrainData.GetHeights(0, 0, Width, Width);
        UpdateMinMaxHeights(generatedHeights);
    }

    [ContextMenu("Smooth Terrain by Median")]
    void SmoothMedian() => ApplySmoothing(TerrainSmoother.SmoothTerrainMedian);

    [ContextMenu("Terrain by x=sqrt(x)")]
    void SmoothSqrt() => ApplySmoothing(TerrainSmoother.SqrtSmoothing);

    [ContextMenu("Smooth Terrain by x=x^2")]
    void SmoothSquare() => ApplySmoothing(TerrainSmoother.SquareSmoothing);

    [ContextMenu("Apply additive perlin")]
    void ApplyAdditivePerlin() =>
        CurrentTerrainForGeneration.terrainData.SetHeights(0, 0, PerlinNoise.AddPerlin(GetNewHeigtsArray(), Width, perlinIntensity, perlinScale));

    [ContextMenu("Apply substractive perlin")]
    void ApplySubstractivePerlin() =>
        CurrentTerrainForGeneration.terrainData.SetHeights(0, 0, PerlinNoise.AddPerlin(GetNewHeigtsArray(), Width, perlinIntensity,perlinScale, true));

    private void ApplySmoothing(Func<float[,], int, float[,]> smoothMethod)
    {
        float[,] heights = GetNewHeigtsArray();
        for (int i = 0; i < ApplyingMethodCount; i++)
            heights = smoothMethod(heights, Width);

        CurrentTerrainForGeneration.terrainData.SetHeights(0, 0, heights);

        UpdateMinMaxHeights(heights);
    }

    private void UpdateMinMaxHeights(float[,] heights)
    {
        float min = float.MaxValue;
        float max = float.MinValue;

        for(int i = 0; i < Width; i++)
        {
            for(int j = 0; j < Height; j++)
            {
                if (heights[i, j] < min)
                    min = heights[i, j];
                if (heights[i, j] > max)
                    max = heights[i, j];
            }
        }

        MinHeight = min*Height;
        MaxHeight = max*Height;
    }

    private float[,] GetNewHeigtsArray()
    {
        float[,] heights = new float[Width, Width];
        for (int i = 0; i < Width; i++)
            for (int j = 0; j < Width; j++)
                heights[i, j] = generatedHeights[i, j];
        return heights;
    }

    public void InstantiateTerrain()
    {
        colorGenerator = GetComponent<ColorGenerator>();
        colorGenerator.Initialize();
        GameObject terrainObject = new GameObject("Terrain " + DateTime.Now);
        terrainObject.transform.position = TerrainPosition;
        Terrain terrain = terrainObject.AddComponent<Terrain>();
        var collider = terrainObject.AddComponent<TerrainCollider>();

        var terrainData = new TerrainData();

        terrainData.heightmapResolution = Width;
        terrainData.size = new Vector3(Width, Height, Width);

        float[,] heights = new float[Width, Width];
        terrain.materialTemplate = SovsemDefaultMaterial;

        collider.terrainData = terrainData;
        terrain.terrainData = terrainData;

        CurrentTerrainForGeneration = terrain;
    }
}
