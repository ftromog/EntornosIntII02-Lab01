using System.Collections;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    // Variables
    public float rotationSpeed = 0;
    public float verticalSpeed = 0;
    public int tipo;

    private void Start()
    {
        // Para que se vea bonito y diferenciar los power ups de las monedas, estos además de rotar van a "flotar".
        if (verticalSpeed > 0)
        {
            StartCoroutine(cambiar_vertical());
        }
    }

    void Update()
    {
        gameObject.transform.Rotate(new Vector3(0f, 0f, rotationSpeed));
        gameObject.transform.position += new Vector3(0, verticalSpeed * Time.deltaTime, 0);
    }

    IEnumerator cambiar_vertical()
    {
        yield return new WaitForSeconds(0.5f);
        verticalSpeed = -verticalSpeed;
        StartCoroutine(cambiar_vertical());
    }
}
