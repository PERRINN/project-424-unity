# PERRINN 424 Simulation
Simulation of the PERRINN 424 electric hypercar in Unity using Vehicle Physics Pro.

[More information on 424 Simulation](https://discover.perrinn.com/424/424-unity-simulation)

## Videos

A couple of laps in Nordschleife:

[![Vehicle Physics Pro simulating PERRINN 424 in Nordschleife](https://img.youtube.com/vi/mwIE3rJ9SHk/maxresdefault.jpg)](https://youtu.be/mwIE3rJ9SHk)

One of the first tests in Monza:

[First test of new tire model in electric LMP1 racing car](https://www.youtube.com/watch?v=OMoQGtA3gCs)

## Requirements

- Unity 2021.3 LTS (the project uses **Unity 2021.3.45**)

## How to set up and open the project in Unity

1. Clone the repository to your computer.
2. Add the repository folder to Unity Hub: Projects > Open > Add project from disk, select the folder with the repository.
3. Click the newly added project in the list

NOTE: Don't copy the repository folder to an existing Unity project. The simulation won't likely work.

## How to run the PERRINN 424 hypercar in autopilot

1. Open the scene "Scenes/424 Nordschleife Scene".
2. Play the scene. The car is at the starting point.
3. Press **Q** to enable the autopilot.

All other features work normally: telemetry (T), cameras (C), time scale... (see left-bottom section of the UI for the feature list)

## How to drive the 424

1. Open one of the scenes in the Scenes folder and Play it.
2. Press **I** to open the input settings. The first time it shows the default keyboard mappings.
3. Click the inputs and follow the instructions to map the inputs to your device. Currently keyboard and DirectInput devices (with or without force feedback) are supported. Your settings will be saved and remembered.
4. Press the **Gear Up** input to engage the **D** (drive) mode.
5. Drive!

## Triples or multi-monitor settings

After building the project, launch the executable with these command-line options:

    -screen-width 5760 -screen-height 1080 -screen-fullscreen 0 -popupwindow

Change the values according to the combined resolution of your displays.

## Development guidelines

Writing code and components for the Project 424 should follow these rules:

#### Code

Code should follow the conventions of the Unity API:

- Namespace, class, methods, properties, etc.
- Naming and case as in the Unity API.

#### Components

Components must support the same operations supported by built-in Unity components without issues, including:

- Enable / disable in runtime.
- Instance / destroy in runtime.
- Instance / destroy prefabs using the component.
- Modify the public properties in the inspector in runtime.
- Modify the public properties from other scripts, both in editor and runtime (use public fields instead of serializable private fields for the properties available in the inspector)
- Hot script reload (use OnEnable/Ondisable instead of Start/Awake/OnDestroy unless it's specifically justified)

## ML Speed Estimator

### Setup
- Go to 'Assets/Features/Speed Estimator/Trainer'
- Create a python environment ```python3 -m venv .venv```
- Activate the virtual environment
- Install the requirements ```pip install -r requirements.txt```
- You can also use ```setup.ps1```

### Train
- Activate your virtual environment
- Run the training script ```python train_speed_estimator.py```
- The input data is 'data/training_data.csv'
- This input data is in the telemetry format
- The output model will be saved in 'model/speed_estimator.onnx'

### Runtime use
- The simulator uses by default the output model of the training process, 'model/speed_estimator.onnx' as runtime model.
- This can be change in the Main Scene. Go to GameObject '424 Nordschleife Scene/PERRINN 424 Nordschleife/SpeedEstimator '. Change 'ModelAsset' in MLSpeedEstimatorContainer component
- MLSpeedEstimatorContainer is the class that uses the trained model to estimate the speed


