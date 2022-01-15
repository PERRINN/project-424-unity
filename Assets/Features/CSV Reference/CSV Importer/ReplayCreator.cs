using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VehiclePhysics;

public class ReplayCreator : MonoBehaviour
{

    public SerializedDenseArray what;
    private void Awake()
    {
        DenseArray<List<float>, float> xxx = new DenseArray<List<float>, float>(10, 10);
        DenseArray<float [], float> xxx2 = new DenseArray<float [], float>(10,10);
    }


    [ContextMenu("test")]
    private void Create()
    {
        VPReplayAsset asset = ScriptableObject.CreateInstance<VPReplayAsset>();
        asset.notes = " dummy";
        asset.recordedData = new List<VPReplay.Frame>();

        AssetDatabase.CreateAsset(asset, "Assets/NewScripableObject.asset");
        //AssetDatabase.SaveAssets();

    }
}
