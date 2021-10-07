using UnityEngine;

public class PrecesionTest : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    private PrecessionEffect preccesion;
    
    private void OnEnable()
    {
        preccesion = new PrecessionEffect(rb);
    }

    private void FixedUpdate()
    {
        preccesion.Apply();
    }
}
