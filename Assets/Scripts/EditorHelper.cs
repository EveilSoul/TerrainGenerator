﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class EditorHelper : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var generator = (TerrainGenerator)target;

        if (GUILayout.Button("Create new terrain"))
        {
            generator.InstantiateTerrain();
        }

        if (GUILayout.Button("Generate terrain"))
        {
            generator.Generate();
        }

        if (GUILayout.Button("Generate colors"))
        {
            generator.GenerateColors();
        }

        if (GUILayout.Button("Generate grass"))
        {
            generator.GenerateGrass();
        }

        if (GUILayout.Button("Generate trees"))
        {
            generator.GenerateTrees();
        }

        if (GUILayout.Button("Save current terrain"))
        {
            generator.Save();
        }
    }
}




