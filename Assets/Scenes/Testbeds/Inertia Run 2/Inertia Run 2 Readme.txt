
Inertia Test 01 - 424_DIAG_5000_X_1000_Z

    - Results with a diagonal inertia match perfectly the SimDrive results.

Inertia Test 02 - 424_5000_X_1000_Z

    - Results with non-diagonal inertia and precession are close, but there are significant discrepancies:
        · X velocity decreases. It increases in SimDrive.
        · Expected change of sign in Z after removing the force is not observed.

Inertia Test 03 - 424nd10_1000_X_1000_Z 6x

    - Using an exaggerated non-diagonal matrix should provide the results in the video.
    - Results in Unity are totally different and mostly nonsense.
    - The video is assumed to be accelerated 6x, as supposedly it's 30 seconds of animation.
    - Description from Lucasz:

        "Thats a visualization on what impact the inertia tensor has on rotation and nicely shows
        the staggering that can occur. [Uses the] full 424 tensor with ten times higher I_YZ/I_ZY.

        The simulation is simple - the gyro is initially in rest and standing vertically straight.
        Directly after the start I apply 1000 [Nm] of torque on the tip around the vertical axis
        for 0.1 [s] and let it spin for 30 [s]. No friction, no gravity."

Inertia Test 03a - Inertia Test 03a - 424_1000_X_10000_Z 6x

    - Figured out some settings that somewhat match the video:
        · Standard 424 inertia (video uses nd10 inertia)
        · Applying 10000 Nm instead of 1000 (maybe a mistake from Lucasz?)
    - Even with precession the result is very different from the video:
        · Seems more like an oscillation of the axis, instead of a staggering.
        · Oscillation occurs in a single plane, instead of varying as with the staggering.


----------------------------------------------------------------------------------------------------


Scene and results naming:

    [Inertia Tensor]_[Distance of point of impulse from CoM in mm]_[Direction point of impulse to CoM]_[Force in N]_[Direction of force]

    Ej. 424_5000_X_1000_Z means: 424 inertia tensor, 5000 mm between CoG and point of impulse in X direction, 1000 N in Z direction.

    Point of impulse and force vector are perpendicular.


Inertia Tensors:

424
    1182      1     -1
       1   1280    -12
      -1    -12    131

424DIAG

    1182      0      0
       0   1280      0
       0      0    131

DIAG

       1      0      0
       0      1      0
       0      0      1

424nd10

    1182     10    -10
      10   1280   -120
     -10   -120    131
