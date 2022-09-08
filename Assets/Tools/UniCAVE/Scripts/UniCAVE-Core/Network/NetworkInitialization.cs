//MIT License
//Copyright 2016-Present
//Ross Tredinnick
//Benny Wysong-Grass
//University of Wisconsin - Madison Virtual Environments Group
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute,
//sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniCAVE
{
    /// <summary>
    /// Starts the program as either client or server depending on machine
    /// </summary>
    public class NetworkInitialization : MonoBehaviour
    {
        public int TimeoutWaitTime = 20; //make this changeable from command line?

        public NetworkManager networkManager;

        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("headMachine")]
        string oldHeadMachine = "C6_V1_HEAD";

        [SerializeField]
        MachineName headMachineAsset;

        public string headMachine => MachineName.GetMachineName(oldHeadMachine, headMachineAsset);

        [Tooltip("This can be overriden at runtime with parameter serverAddress, for example \"serverAddress 192.168.0.100\"")]
        public string serverAddress = "192.168.4.140";

        [Tooltip("This can be overriden at runtime with parameter serverPort, for example \"serverPort 8421\"")]
        public int serverPort = 7568;

        /// <summary>
        /// Starts as client or server.
        /// </summary>
        void Start()
        {
            string serverArg = Util.GetArg("serverAddress");
            if(serverArg != null)
            {
                serverAddress = serverArg;
            }

            string portArg = Util.GetArg("serverPort");
            if(portArg != null)
            {
                int.TryParse(portArg, out serverPort);
            }

            string runningMachineName = Util.GetMachineName();
            Debug.Log($"serverAddress = {serverAddress}, serverPort = {serverPort}, headMachine = {headMachine}, runningMachine = {runningMachineName}");

            networkManager.networkAddress = serverAddress;
            (networkManager.transport as kcp2k.KcpTransport).Port = (ushort)serverPort;

            if ((Util.GetArg("forceClient") == "1") || (Util.GetMachineName() != headMachine))
            {
                networkManager.StartClient();
            }
            else
            {
                networkManager.StartServer();
            }
        }

        /// <summary>
        /// Quits after 20 seconds if no connection is made to server.
        /// </summary>
        void Update()
        {
            if(Util.GetMachineName() != oldHeadMachine)
            {
                // if(!NetworkClient.active)
                // {
                    // networkManager.StartClient();
                // }

                if(TimeoutWaitTime > 0 && Time.time > TimeoutWaitTime && !NetworkClient.isConnected)
                {
                    Application.Quit();
                }
            }
        }

#if UNITY_EDITOR
        [CanEditMultipleObjects]
        [CustomEditor(typeof(NetworkInitialization))]
        class Editor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(networkManager)));

                //special handling for old machine names:
                SerializedProperty oldMachineName = serializedObject.FindProperty(nameof(oldHeadMachine));
                SerializedProperty machineName = serializedObject.FindProperty(nameof(headMachineAsset));
                MachineName.DrawDeprecatedMachineName(oldMachineName, machineName, "HeadMachine");

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(serverAddress)));

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(serverPort)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TimeoutWaitTime)));

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}