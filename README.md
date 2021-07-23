# Project 424
Simulation of the PERRINN 424 electric hypercar in Unity using Vehicle Physics Pro

[Hot Lap video](https://www.youtube.com/watch?v=OMoQGtA3gCs)

## Requirements

- Unity 2019.4 LTS (using 2019.4.18f1 at the time of writing this)

## How to run the PERRINN 424 hypercar in autopilot

1. Clone the repository to your computer and open it in Unity.
2. Open the scene "Scenes/424 Nordschleife Scene".
3. Play the scene. The car is at the starting point.
4. Press **Q**. This enables the autopilot.

All other features work normally: telemetry, cameras, time scale...

## How to drive the 424

Requires a steering wheel device: Logitech G27/G29, Thrustmaster, Fanatec Podium.

1. Clone the repository to your computer and open it in Unity.
2. Open one of the scenes at the Scenes folder.
3. In the Hierarchy window select the car (**PERRINN 424**), then select the **Input** GameObject in it.
4. In the **Device Input** component choose your wheel model, select the device number (normally 0), and enter the degrees of rotation currently configured in your wheel.
5. Play the scene.
