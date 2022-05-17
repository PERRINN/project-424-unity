# Project 424
Simulation of the PERRINN 424 electric hypercar in Unity using Vehicle Physics Pro

[Hot Lap video in Monza](https://www.youtube.com/watch?v=OMoQGtA3gCs)

## Requirements

- Unity 2019.4 LTS (using 2019.4.18f1 at the time of writing this)

## How to run the PERRINN 424 hypercar in autopilot

1. Clone the repository to your computer and open it in Unity.
2. Open the scene "Scenes/424 Nordschleife Scene".
3. Play the scene. The car is at the starting point.
4. Press **Q** to enable the autopilot.

All other features work normally: telemetry, cameras, time scale...

## How to drive the 424

1. Clone the repository to your computer and open it in Unity.
2. Open one of the scenes in the Scenes folder and Play it.
3. Press **I** to open the input settings. The first time it shows the default keyboard mappings.
4. Click the inputs and follow the instructions to map the inputs to your device. Currently keyboard and DirectInput devices (with or without force feedback) are supported. Your settings will be saved and remembered.
5. Press the **Gear Up** input to engage the **D** (drive) mode.
6. Drive!

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
