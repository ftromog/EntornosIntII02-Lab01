using UnityEngine;

public class FollowingCameraController : MonoBehaviour
{
    public GameObject playerReference;

    Vector3 originPosition;
    Vector3 initialOffset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originPosition = transform.position;
        initialOffset = originPosition - playerReference.transform.position;

    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = initialOffset + playerReference.transform.position;
    }
}
