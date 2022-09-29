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
using Mirror.Discovery;
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

        [Tooltip("If client, automatically search and find server (requires Network Discovery component). If client is forced (forceClient = 1) then connect to Server Address.")]
        public bool clientAutoConnect = true;

        public string headMachine => MachineName.GetMachineName(null, headMachineAsset);

        static bool m_newClientConnected = false;

        NetworkDiscovery m_networkDiscovery;
        bool m_asClient;
        bool m_asForcedClient;

        //EDY: Start is not called when disabling/enabling the component or on hot script reload.
        bool m_startCalled;

        /// <summary>
        /// Exposes the client connection flag. Automatically resets it to false when read.
        /// </summary>
        public static bool newClientConnected
        {
            get
            {
                bool value = m_newClientConnected;
                m_newClientConnected = false;
                return value;
            }
        }

        /// <summary>
        /// Resets the client connection flag and find if we have a Network Discovery component
        /// </summary>
        public override void OnEnable ()
        {
            m_startCalled = false;
            m_newClientConnected = false;
            m_asForcedClient = Util.GetArg("forceClient") == "1";
            m_asClient = m_asForcedClient || Util.GetMachineName() != headMachine;
            m_networkDiscovery = GetComponent<NetworkDiscovery>();

            // Server re-connection only in client mode (not in host)
            if (m_asClient && !m_asForcedClient && m_networkDiscovery != null)
                m_networkDiscovery.OnServerFound.AddListener(OnDiscoveredServer);

            base.OnEnable();
        }

        /// <summary>
        /// Starts as client or server.
        /// </summary>
        public override void Start()
        {
            // Don't call base.Start because it only calls StartServer on conditions unrelated to us.
            // base.Start();

            m_startCalled = true;

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

            if (m_asClient)
            {
                if (clientAutoConnect)
                {
                    if (m_asForcedClient)
                        StartClient();
                    else
                    if (m_networkDiscovery != null)
                        m_networkDiscovery.StartDiscovery();
                }
            }
            else
            {
                StartServer();
            }
        }

        /// <summary>
        /// Stop server and/or client connections (base). Stop server discovery.
        /// </summary>
        public override void OnDisable ()
            {
            if (m_networkDiscovery != null)
            {
                m_networkDiscovery.StopDiscovery();
                if (m_asClient && !m_asForcedClient)
                    m_networkDiscovery.OnServerFound.RemoveListener(OnDiscoveredServer);
            }
            base.OnDisable();
            }

        /// <summary>
        /// Server: server ready. Start advertising as server.
        /// </summary>
        public override void OnStartServer()
        {
            if (DebugInfoLevel >= 2) Debug.Log("UCNetworkManager OnStartServer");
            base.OnStartServer();
            if (m_networkDiscovery != null)
                m_networkDiscovery.AdvertiseServer();
        }

        /// <summary>
        /// Server: client connected and ready. Raises the client connected flag.
        /// </summary>
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            if (DebugInfoLevel >= 2) Debug.Log("UCNetworkManager OnServerReady");
            base.OnServerReady(conn);
            m_newClientConnected = true;
        }

        /// <summary>
        /// Client: client disconnected. Restart server discovery.
        /// Server discovery is automatically stopped when the client connects.
        /// </summary>
        public override void OnClientDisconnect()
        {
            if (DebugInfoLevel >= 2) Debug.Log("UCNetworkManager OnClientDisconnect");

            base.OnClientDisconnect();
            if (clientAutoConnect && !m_asForcedClient && m_networkDiscovery != null)
                m_networkDiscovery.StartDiscovery();
        }

        /// <summary>
        /// Forced client permanently tries to connect and reconnect to the server address
        /// </summary>
        void Update()
        {
            //EDY: Start is not called when disabling/enabling the component or on hot script reload.
            if (!m_startCalled)
                Start();

            if(clientAutoConnect && m_asClient && m_asForcedClient)
            {
                if(!NetworkClient.isConnected && !NetworkClient.isConnecting)
                {
                    StartClient();
                }
            }
        }

        /// <summary>
        /// Client: Server responded to client's server discovery. Connect to it immediately.
        /// </summary>
        void OnDiscoveredServer(ServerResponse info)
        {
            if (DebugInfoLevel >= 2) Debug.Log($"UCNetworkManager OnDiscoveredServer: {info.uri} isConnected: {NetworkClient.isConnected} isConnecting: {NetworkClient.isConnecting}");

            if (clientAutoConnect && !NetworkClient.isConnected && !!NetworkClient.isConnecting)
            {
                StartClient(info.uri);
            }
        }
    }
}
