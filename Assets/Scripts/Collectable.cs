using UnityEngine;

public class Collectable : MonoBehaviour
{
    public float rotationSpeed = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(new Vector3(0f, 0f, rotationSpeed));
    }
}
