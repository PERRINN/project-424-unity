
using UnityEngine;
using UnityEditor;
using VehiclePhysics;


[RequireComponent(typeof(VPReplayController))]
public class SaveAndLoadReplayAsset : MonoBehaviour
    {
	VPReplay m_replay;
    VPReplayController m_controller;


    void OnEnable ()
        {
		m_replay = GetComponent<VPReplay>();
        m_controller = GetComponent<VPReplayController>();
        }


	void Update ()
		{
		if (Input.GetKeyDown(m_controller.saveReplayKey))
			{
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				LoadReplayFromAssetFile(m_controller.saveReplayFileName);
			else
				SaveReplayToAssetFile(m_controller.saveReplayFileName);
			}
		}


	void SaveReplayToAssetFile (string assetFileName)
		{
		#if UNITY_EDITOR
		if (string.IsNullOrEmpty(assetFileName)) return;

		VPReplayAsset replayAsset = m_replay.SaveReplayToAsset();
		UnityEditor.AssetDatabase.CreateAsset(replayAsset, assetFileName + ".asset");
		UnityEditor.AssetDatabase.SaveAssets();
		#endif
		}


	void LoadReplayFromAssetFile (string assetFileName)
		{
		#if UNITY_EDITOR
		if (string.IsNullOrEmpty(assetFileName)) return;

		VPReplayAsset replayAsset = (VPReplayAsset)UnityEditor.AssetDatabase.LoadAssetAtPath(assetFileName + ".asset", typeof(VPReplayAsset));
		m_replay.LoadReplayFromAsset(replayAsset);
		#endif
		}
    }
