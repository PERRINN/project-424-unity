UniCAVE Network Manager
-----------------------

UC Network Manager

- Head Machine Asset: machine name of the station running as SERVER
- Starts network as Server in that machine. Otherwise starts as CLIENT.

Network Monitor

- Enables / disables GameObjects based on the network conditions: Server, Client, Client Disconnected, Client Connecting, etc.


PERRINN Cluster Manager
-----------------------

- Enables / disables cameras based on the station running as server or client:
	- In server stations enables the Server Camera.
	- In client stations a specific camera is enabled based on the known client machine names.
	- In unkown clients enables the given camera and the list of specific GameObjects.
	
- Also configures the UI elements to be shown properly in the corresponding camera:
	- UI material in all Graphic elements.
	- Canvas > Render Mode to Screen Space Camera.
	- Canvas > Render Camera to the enabled camera.
	
- Does not enable or disable any other objects. Canvases are enabled / disabled by Network Monitor above.


PERRINN Render Client
---------------------

Perrinn 424 Render Client

- Synchronizes poses and states of the vehicle between client and server.
- Configures client elements: physics rate, kinematic rigidbody, disables camera controller.
- REQUIRES REFERENCES OUTSIDE THE DYNISMA HARDWARE INTEGRATION PREFAB: vehicle, visual effects, camera controller.

UC Network

- Currently synchronizes time scale only.

