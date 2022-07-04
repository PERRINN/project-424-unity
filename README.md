# Project 424
Simulation of the PERRINN 424 electric hypercar in Unity using Vehicle Physics Pro

[Hot Lap video in Monza](https://www.youtube.com/watch?v=OMoQGtA3gCs)

## Requirements

- Unity 2019.4 LTS (using 2019.4.18f1 at the time of writing this)

## How to set up and open the project in Unity

1. Clone the repository to your computer.
2. Add the repository folder to Unity Hub: Projects > Open > Add project from disk, select the folder with the repository. 
3. Click the newly added project in the list

NOTE: Don't copy the repository folder to an existing Unity project. The simulation won't likely work.

## How to run the PERRINN 424 hypercar in autopilot

1. Open the scene "Scenes/424 Nordschleife Scene".
2. Play the scene. The car is at the starting point.
3. Press **Q** to enable the autopilot.

All other features work normally: telemetry, cameras, time scale...

## How to drive the 424

1. Open one of the scenes in the Scenes folder and Play it.
2. Press **I** to open the input settings. The first time it shows the default keyboard mappings.
3. Click the inputs and follow the instructions to map the inputs to your device. Currently keyboard and DirectInput devices (with or without force feedback) are supported. Your settings will be saved and remembered.
4. Press the **Gear Up** input to engage the **D** (drive) mode.
5. Drive!

## Development guidelines

Writing code and components for the Project 424 should follow these rules:

#### Code

Code should follow the conventions of the Unity API:

- Namespace, class, methods, properties, etc.
- Naming and case as in the Unity API.

#### Components

Components must support the same operations supported by built-in Unity components without errors, including:

- Enable / disable in runtime.
- Instance / destroy in runtime.
- Instance / destroy prefabs using the component.
- Modify the public properties in the inspector in runtime.
- Modify the public properties from other scripts, both in editor and runtime.
- Hot script reload.
