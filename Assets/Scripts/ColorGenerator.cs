using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor;
using System.Linq;

[RequireComponent(typeof(TerrainGenerator))]
public class ColorGenerator : MonoBehaviour
{
    #region BaseFields
    private TerrainGenerator terrainGenerator;
    public TextureData selectedTextureData { get; private set; }
    private Dictionary<BiomeType, TextureData> textureData;

    private const int MaxGrassIntensity = 20;
    private const int MinGrassIntensity = 0;

    public BiomeType CurrentBiome;
    public bool CustomBiome;
    public Layer[] BiomeLayers;
    [Range(MinGrassIntensity + 1, MaxGrassIntensity)]
    public int GrassIntensity = 7;
    public Grass[] Grass;
    public int TreeAndStoneIntensity = 3;
    public Tree[] TreesAndStones;
    #endregion

    public void Generate()
    {
        if (terrainGenerator == null)
        {
            terrainGenerator = GetComponent<TerrainGenerator>();
        }
        // Чертова юнити
        // Это вот работает
        var min = terrainGenerator.MinHeight + UnityEngine.Random.Range(-0.001f, 0.001f);
        selectedTextureData.UpdateMeshHeights(terrainGenerator.DefaultMaterial, min, terrainGenerator.MaxHeight);
        // А это нет
        //selectedTextureData.UpdateMeshHeights(terrainGenerator.DefaultMaterial, terrainGenerator.MinHeight, terrainGenerator.MaxHeight);

        
        if (!CustomBiome)
        {
            selectedTextureData = textureData[CurrentBiome];
            selectedTextureData.layers.CopyTo(BiomeLayers, 0);
        }
        else
        {
            BiomeLayers.CopyTo(selectedTextureData.layers, 0);
        }

        selectedTextureData.ApplyToMaterial(terrainGenerator.DefaultMaterial);
        terrainGenerator.CurrentTerrainForGeneration.materialTemplate = terrainGenerator.DefaultMaterial;
    }

    public void GenerateTreesAndStones()
    {
        var maxRandom = TreeAndStoneIntensity;

        var trees = new TreePrototype[TreesAndStones.Length];
        for (int i = 0; i < TreesAndStones.Length; i++)
        {
            trees[i] = new TreePrototype();
            trees[i].prefab = TreesAndStones[i].prefab;
            maxRandom += TreesAndStones[i].intensity;
        }
        terrainGenerator.CurrentTerrainForGeneration.terrainData.treePrototypes = trees;
        terrainGenerator.CurrentTerrainForGeneration.terrainData.treeInstances = new TreeInstance[0];

        float width = terrainGenerator.Width;

        for (int i = 0; i < width; i += TreeAndStoneIntensity)
        {
            for (int j = 0; j < width; j += TreeAndStoneIntensity)
            {
                var height = terrainGenerator.CurrentTerrainForGeneration.terrainData.GetHeight(i, j);
                int randomRes = UnityEngine.Random.Range(0, maxRandom);

                int counter = TreeAndStoneIntensity;
                if (randomRes <= counter)
                    continue;

                for (int treeType = 0; treeType < TreesAndStones.Length; treeType++)
                {
                    counter += TreesAndStones[treeType].intensity;
                    if (randomRes <= counter)
                    {
                        if (height >= TreesAndStones[treeType].minHeight && height < TreesAndStones[treeType].maxHeight)
                        {
                            var newTree = new TreeInstance();
                            newTree.prototypeIndex = treeType;
                            newTree.position = new Vector3(i / width, height, j / width);
                            newTree.heightScale = TreesAndStones[treeType].scale;
                            newTree.widthScale = TreesAndStones[treeType].scale;
                            terrainGenerator.CurrentTerrainForGeneration.AddTreeInstance(newTree);
                        }
                    }
                }
            }
        }
    }

    public void GenerateGrass()
    {
        var details = new DetailPrototype[Grass.Length];
        for (int i = 0; i < Grass.Length; i++)
        {
            details[i] = new DetailPrototype();
            details[i].renderMode = DetailRenderMode.GrassBillboard;
            details[i].prototypeTexture = Grass[i].texture;
            details[i].minHeight = Grass[i].minSizeHeight;
            details[i].maxHeight = Grass[i].maxSizeHeight;
            details[i].minWidth = Grass[i].minSizeWidth;
            details[i].maxWidth = Grass[i].maxSizeWidth;
        }
        terrainGenerator.CurrentTerrainForGeneration.terrainData.detailPrototypes = details;
        terrainGenerator.CurrentTerrainForGeneration.detailObjectDistance = 250;

        var k = 2;
        int width = terrainGenerator.Width * k;
        terrainGenerator.CurrentTerrainForGeneration.terrainData.SetDetailResolution(width, 32);

        for (int g = 0; g < Grass.Length; g++)
        {
            int[,] newMap = new int[width, width];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                    float height = terrainGenerator.CurrentTerrainForGeneration.terrainData.GetHeight(j / k, i / k);
                    if (height >= Grass[g].minPlaceHeight && height < Grass[g].maxPlaceHeight)
                    {
                        if (UnityEngine.Random.Range((float)MinGrassIntensity, MaxGrassIntensity) <= (GrassIntensity * 0.2f))
                            newMap[i, j] = Grass[g].intensity;
                    }
                    else
                    {
                        newMap[i, j] = 0;
                    }
                }
            }

            terrainGenerator.CurrentTerrainForGeneration.terrainData.SetDetailLayer(0, 0, g, newMap);
        }
    }

    internal void Initialize()
    {
        var res = Resources.LoadAll<TextureData>("Biomes");
        textureData = res.ToDictionary(x => x.Type);
        selectedTextureData = textureData[CurrentBiome];
        BiomeLayers = new Layer[selectedTextureData.layers.Length];
        Array.Copy(selectedTextureData.layers, BiomeLayers, BiomeLayers.Length);
    }
}

public enum BiomeType
{
    Forest,
    Ocean,
    Snow
}

[System.Serializable]
public struct Grass
{
    public Texture2D texture;
    [Range(0, 1)]
    public float minPlaceHeight;
    [Range(0, 1)]
    public float maxPlaceHeight;
    [Range(1, 10)]
    public int intensity;
    public float minSizeHeight;
    public float maxSizeHeight;
    public float minSizeWidth;
    public float maxSizeWidth;
}

[System.Serializable]
public struct Tree
{
    public GameObject prefab;
    [Range(0, 1)]
    public float minHeight;
    [Range(0, 1)]
    public float maxHeight;
    [Range(1, 10)]
    public int intensity;
    [Range(0, 10)]
    public float scale;
}