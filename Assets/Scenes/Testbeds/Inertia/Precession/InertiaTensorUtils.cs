///   Author: Manuel Espino.
///   github: https://github.com/Maesla
///   unity forum: https://forum.unity.com/members/maeslezo.863356/
///   Utilities to get the inertia tensor matrix from a rigidbody 
///   and to get the inertia tensor and inertia tensor rotation from a inertia tensor matrix
///-----------------------------------------------------------------

using UnityEngine;

public static class InertiaTensorUtils
{
    public static Matrix4x4 CalculateInertiaTensorMatrix(Rigidbody rb)
    {
        return CalculateInertiaTensorMatrix(rb.inertiaTensor, rb.inertiaTensorRotation);
    }

    // Inertia Tensor Matrix can be decomposed in M = transpose(R)*D*R
    // M is the original matrix
    // R is a rotation matrix, stored in the rigidbody as a quaternion in inertiaTensorRotation
    // D is a diagonal matrix, stored in the rigidbody as a vector3 in inertiaTensor
    // D are the eigenvalues and R are the eigenvectors
    // Inertia Tensor Matrix is a 3x3 Matrix, so it will appear in the first 3x3 positions of the 4x4 Unity Matrix used here
    public static Matrix4x4 CalculateInertiaTensorMatrix(Vector3 inertiaTensor, Quaternion inertiaTensorRotation)
    {
        Matrix4x4 R = Matrix4x4.Rotate(inertiaTensorRotation); //rotation matrix created
        Matrix4x4 S = Matrix4x4.Scale(inertiaTensor); // diagonal matrix created
        return R * S * R.inverse; // R is orthogonal, so R.transpose == R.inverse
        //return R.transpose * S * R; // R is orthogonal, so R.transpose == R.inverse
    }

    private const float epsilon = 1e-10f;
    private const int maxSweeps = 32;

    /// <summary>
    /// Diagonalization of M
    /// </summary>
    /// <remarks>
    /// M will be decomposed by M = transpose(R)*D*R.
    /// InertiaTensorQuaternion is the quaternion equivalent to R
    /// InertiaTensor is the diagonal values of D
    /// InertiaTensor stores the eigenvalues and R stores the eigenvectors. Since the eigenvectors are the rotation axis, the quaternion representing R is the rotation axis
    /// </remarks>
    /// <param name="m"></param>
    /// <param name="inertiaTensor"></param>
    /// <param name="inertiaTensorRotation"></param>
    public static void DiagonalizeInertiaTensor(Matrix4x4 m, out Vector3 inertiaTensor, out Quaternion inertiaTensorRotation)
    {
        float m11 = m[0, 0];
        float m12 = m[0, 1];
        float m13 = m[0, 2];
        float m22 = m[1, 1];
        float m23 = m[1, 2];
        float m33 = m[2, 2];

        Matrix4x4 r = Matrix4x4.identity;
        for (int a = 0; a < maxSweeps; a++)
        {
            // Exit if off.diagonal entries small enough
            if ((fabs(m12) < epsilon) && (fabs(m13) < epsilon) && (fabs(m23) < epsilon))
                break;

            // Annihilate (1,2) entry.
            if (m12 != 0.0F)
            {
                float u = (m22 - m11) * 0.5F / m12;
                float u2 = u * u;
                float u2p1 = u2 + 1.0F;
                float t = (u2p1 != u2) ?
                ((u < 0.0F) ? -1.0F : 1.0F) * (sqrt(u2p1) - fabs(u))
                : 0.5F / u;
                float c = 1.0F / sqrt(t * t + 1.0F);
                float s = c * t;
                m11 -= t * m12;
                m22 += t * m12;
                m12 = 0.0F;
                float temp = c * m13 - s * m23;
                m23 = s * m13 + c * m23;
                m13 = temp;
                for (int i = 0; i < 3; i++)
                {
                    float tempInner = c * r[i, 0] - s * r[i, 1];
                    r[i, 1] = s * r[i, 0] + c * r[i, 1];
                    r[i, 0] = tempInner;
                }
            }

            // Annihilate (1,3) entry.
            if (m13 != 0.0F)
            {
                float u = (m33 - m11) * 0.5F / m13;
                float u2 = u * u;
                float u2p1 = u2 + 1.0F;
                float t = (u2p1 != u2) ?
                ((u < 0.0F) ? -1.0F : 1.0F) * (sqrt(u2p1) - fabs(u))
                : 0.5F / u;
                float c = 1.0F / sqrt(t * t + 1.0F);
                float s = c * t;
                m11 -= t * m13;
                m33 += t * m13;
                m13 = 0.0F;
                float temp = c * m12 - s * m23;
                m23 = s * m12 + c * m23;
                m12 = temp;
                for (int i = 0; i < 3; i++)
                {
                    float tempInner = c * r[i, 0] - s * r[i, 2];
                    r[i, 2] = s * r[i, 0] + c * r[i, 2];
                    r[i, 0] = tempInner;
                }
            }

            // Annihilate (2,3) entry.
            if (m23 != 0.0F)
            {
                float u = (m33 - m22) * 0.5F / m23;
                float u2 = u * u;
                float u2p1 = u2 + 1.0F;
                float t = (u2p1 != u2) ?
                ((u < 0.0F) ? -1.0F : 1.0F) * (sqrt(u2p1) - fabs(u))
                : 0.5F / u;
                float c = 1.0F / sqrt(t * t + 1.0F);
                float s = c * t;
                m22 -= t * m23;
                m33 += t * m23;
                m23 = 0.0F;
                float temp = c * m12 - s * m13;
                m13 = s * m12 + c * m13;
                m12 = temp;
                for (int i = 0; i < 3; i++)
                {
                    float tempInner = c * r[i, 1] - s * r[i, 2];
                    r[i, 2] = s * r[i, 1] + c * r[i, 2];
                    r[i, 1] = tempInner;
                }
            }
        }

        inertiaTensor.x = m11;
        inertiaTensor.y = m22;
        inertiaTensor.z = m33;

        inertiaTensorRotation = r.rotation;
    }

    private static float fabs(float f)
    {
        return Mathf.Abs(f);
    }

    private static float sqrt(float f)
    {
        return Mathf.Sqrt(f);
    }
}
