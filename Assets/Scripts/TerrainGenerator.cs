using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;

public class TerrainGenerator : MonoBehaviour
{
    #region BaseFields
    public Vector3 TerrainPosition;
    public Material DefaultMaterial;
    public float Roughness;

    public Terrain CurrentTerrainForGeneration;

    public int Width;
    public int Height;

    [Range(0, 6)]
    [Tooltip("how many times will be applied some method (e.g smoothing)")]
    [SerializeField] private int ApplyingMethodCount;
    [SerializeField] private uint seed;

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

    public void Generate()
    {
        Random.InitState(seed);
        //var terrain = InstantiateTerrain();
        var terrain = CurrentTerrainForGeneration;
        terrain.terrainData = DiamondSquare.GenerateTerrain(terrain.terrainData, this);
        generatedHeights = terrain.terrainData.GetHeights(0, 0, Width, Width);
    }

    public void Save() => generatedHeights = CurrentTerrainForGeneration.terrainData.GetHeights(0, 0, Width, Width);

    [ContextMenu("Smooth Terrain by Median")]
    void SmoothMedian() => ApplySmoothing(TerrainSmoother.SmoothTerrainMedian);

    [ContextMenu("Terrain by x=sqrt(x)")]
    void SmoothSqrt() => ApplySmoothing(TerrainSmoother.SqrtSmoothing);

    [ContextMenu("Smooth Terrain by x=x^2")]
    void SmoothSquare() => ApplySmoothing(TerrainSmoother.SquareSmoothing);

    private void ApplySmoothing(Func<float[,], int, float[,]> smoothMethod)
    {
        float[,] heights = new float[Width, Width];
        for (int i = 0; i < Width; i++)
            for (int j = 0; j < Width; j++)
                heights[i, j] = generatedHeights[i, j];
        for (int i = 0; i < ApplyingMethodCount; i++)
            heights = smoothMethod(heights, Width);

        CurrentTerrainForGeneration.terrainData.SetHeights(0, 0, heights);
    }

    public void InstantiateTerrain()
    {
        GameObject terrainObject = new GameObject("Terrain " + DateTime.Now);
        terrainObject.transform.position = TerrainPosition;
        Terrain terrain = terrainObject.AddComponent<Terrain>();
        var collider = terrainObject.AddComponent<TerrainCollider>();

        var terrainData = new TerrainData();

        terrainData.heightmapResolution = Width;
        terrainData.size = new Vector3(Width, Height, Width);
        Debug.Log(new Vector3(Width, Height, Width));

        float[,] heights = new float[Width, Width];
        terrain.materialTemplate = DefaultMaterial;

        collider.terrainData = terrainData;
        terrain.terrainData = terrainData;

        CurrentTerrainForGeneration = terrain;
    }
}
