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
    [AddComponentMenu("")]
    public class UCNetworkManager : NetworkManager
    {
        [Header("UniCAVE Settings")]
        public MachineName headMachineAsset;

        [Tooltip("This can be overriden at runtime with parameter serverAddress, for example \"serverAddress 192.168.0.100\"")]
        public string serverAddress = "localhost";

        [Tooltip("This can be overriden at runtime with parameter serverPort, for example \"serverPort 8421\"")]
        public int serverPort = 7568;

        public string headMachine => MachineName.GetMachineName(null, headMachineAsset);

        static bool m_clientConnected = false;

        /// <summary>
        /// Exposes the client connection flag. Automatically resets it to false when read.
        /// </summary>
        public static bool clientConnected
        {
            get
            {
                bool value = m_clientConnected;
                m_clientConnected = false;
                return value;
            }
        }

        /// <summary>
        /// Resets the client connection flag
        /// </summary>
        public override void OnEnable ()
        {
            m_clientConnected = false;
            base.OnEnable();
        }

        /// <summary>
        /// Starts as client or server.
        /// </summary>
        public override void Start()
        {
            // Don't call base.Start because it only calls StartServer on conditions unrelated to us.
            // base.Start();

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
            if (DebugInfoLevel >= 1) Debug.Log($"serverAddress = {serverAddress}, serverPort = {serverPort}, headMachine = {headMachine}, runningMachine = {runningMachineName}");

            networkAddress = serverAddress;
            (transport as kcp2k.KcpTransport).Port = (ushort)serverPort;

            if ((Util.GetArg("forceClient") == "1") || (Util.GetMachineName() != headMachine))
            {
                StartClient();
            }
            else
            {
                StartServer();
            }
        }

        /// <summary>
        /// Raises the client connected flag when a client connects
        /// </summary>
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            if (DebugInfoLevel >= 2) Debug.Log("UCNetworkManager OnServerAddPlayer");
            base.OnServerReady(conn);
            m_clientConnected = true;
        }

        /// <summary>
        /// Client permanently tries to connect and reconnect to the server
        /// </summary>
        void Update()
        {
            if(Util.GetMachineName() != headMachine)
            {
                if(!NetworkClient.isConnected && !NetworkClient.isConnecting)
                {
                    StartClient();
                }
            }
        }
    }
}
