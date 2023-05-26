
">" represents references external to the prefab that must be set when using this prefab in a scene.


Dynisma Integration
-------------------

Dynisma Settings

Reads the input and motion settings (host, ports, etc) from json file. If the json file is present, enables remote input and motion settings and configures them. If not present then disables these features.

> Input Provider: reference to the Dynisma Input Provider component, which handles the input remotely sent to the application from Dynisma via UDP.

> Motion Platform: reference to the Dynisma Motion Platform component in the vehicle, which sends the motion information to Dynisma via UDP.


UniCAVE Network Manager
-----------------------

UC Network Manager

Starts network as Server in the machine of the configured name. Otherwise starts it as CLIENT.

> Head Machine Asset: machine name of the station running as SERVER


Network Monitor

Enables / disables GameObjects based on the network conditions: Server, Client, Client Disconnected, Client Connecting, etc.

> Server Only > Element 0: reserved for SceneTools (logo, text, time scale etc)


PERRINN Cluster Manager
-----------------------

Enables / disables cameras based on the station running as server or client:
	- In server stations enables the Server Camera.
	- In client stations a specific camera is enabled based on the known client machine names.
	- In unknown clients enables the given components and gameobjects.

Also configures the UI elements to be shown properly in the corresponding cameras:
	- UI material in all Graphic elements.
	- Canvas > Render Mode to Screen Space Camera.
	- Canvas > Render Camera to the enabled camera.

Includes a test mode forcing the output of any of the server or client stations.

Also allows to force specific elements enabled on unknown clients, so they could work as spectator.

> Enable Game Objects > Element 0: reserved for SceneTools (logo, text, time scale etc)


PERRINN Render Client
---------------------

Perrinn 424 Render Client

Synchronizes poses and states of the vehicle between client and server. Also configures client elements: physics rate, kinematic rigidbody, disables camera controller.

> Vehicle: vehicle root


UC Network

Currently synchronizes time scale only.


Camera Controller
-----------------

Camera Controller

> Target: vehicle root