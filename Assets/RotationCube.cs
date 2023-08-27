using UnityEngine;

public class RotationCube : MonoBehaviour
{
    public float rotateSpeed = 40f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}
