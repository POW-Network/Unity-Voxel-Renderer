using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VoxelRenderer))]
public class VoxelRendererEditor : Editor
{
    static Vector3 randPosMin, randPosMax;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        VoxelRenderer voxelRenderer = (VoxelRenderer)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Random Perlin Position", EditorStyles.boldLabel);
        randPosMin = EditorGUILayout.Vector3Field("Random Position Min", randPosMin);
        randPosMax = EditorGUILayout.Vector3Field("Random Position Max", randPosMax);
        if (GUILayout.Button("Randomize Perlin Position"))
        {
            voxelRenderer.perlinPosition = new Vector3(
                Random.Range(randPosMin.x, randPosMax.x),
                Random.Range(randPosMin.y, randPosMax.y),
                Random.Range(randPosMin.z, randPosMax.z)
            );
            voxelRenderer.GenerateMap(voxelRenderer.perlinPosition);
        }
    }
}
