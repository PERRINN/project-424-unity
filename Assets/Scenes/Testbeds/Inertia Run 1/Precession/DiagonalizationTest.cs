using EdyCommonTools;
using UnityEngine;

public class DiagonalizationTest : MonoBehaviour
{
    public Matrix4x4 inertiaTensorMatrix;

    public Vector3 inertiaTensorV1;
    public Quaternion inertiaTensorRotationV1;

    public Vector3 inertiaTensorV2;
    public Quaternion inertiaTensorRotationV2;

    public Matrix4x4 inertiaTensorMatrixReconstructed;

    private void Start()
    {
        RigidbodyUtility.DiagonalizeInertiaTensor(inertiaTensorMatrix, out inertiaTensorV1, out inertiaTensorRotationV1);
        InertiaTensorUtils.DiagonalizeInertiaTensor(inertiaTensorMatrix, out inertiaTensorV2, out inertiaTensorRotationV2);

        print($"Inertia Tensor. V1: {inertiaTensorV1} V2: {inertiaTensorV2}");
        print($"Inertia Tensor Rotation. V1: {inertiaTensorRotationV1} V2: {inertiaTensorRotationV2}");

        inertiaTensorMatrixReconstructed = InertiaTensorUtils.CalculateInertiaTensorMatrix(inertiaTensorV1, inertiaTensorRotationV2);
        print($"Original Matrix: \n{inertiaTensorMatrix}. Reconstructed  \n{inertiaTensorMatrixReconstructed}");
    }

    public void Reset()
    {
        inertiaTensorMatrix = Matrix4x4.identity;
        inertiaTensorMatrix[0, 0] = 1200.123f;
        inertiaTensorMatrix[1, 1] = 1338.376f;
        inertiaTensorMatrix[2, 2] = 318.8264f;
        inertiaTensorMatrix[0, 1] = inertiaTensorMatrix[1, 0] = 50;
        inertiaTensorMatrix[0, 2] = inertiaTensorMatrix[2, 0] = 90;
        inertiaTensorMatrix[1, 2] = inertiaTensorMatrix[2, 1] = -43.02782f;
    }
}
