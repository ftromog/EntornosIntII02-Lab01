using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;

    Vector3 movementDirection = new Vector3(0, 0, 0);

    public float speed = 3;
    public float maxSpeed = 15;
    public float dashForce = 300;
    public float jumpForce = 30;
    public float impulsoAbajoForce = 60;

    int rebotes = 0;

    bool canUseDash = false;
    bool currentlyUsingDash = false;
    bool puedeSaltar = false;
    bool puedeImpulsoAbajo = false;
    bool impulsoAbajo = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        canUseDash = true;
        currentlyUsingDash = false;
    }

    void OnMove(InputValue movementValue)
    {
        movementDirection = movementValue.Get<Vector2>();
        movementDirection = new Vector3(movementValue.Get<Vector2>().x, 0f, movementValue.Get<Vector2>().y);

    }

    private void Update()
    {
        if (transform.position.y < -100)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Keyboard.current.xKey.wasPressedThisFrame && canUseDash)
        {
            StartCoroutine(Dash(1f));
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (puedeSaltar)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                puedeSaltar = false;
                puedeImpulsoAbajo = true;
            }
            else if (puedeImpulsoAbajo)
            {
                rb.AddForce(Vector3.down * impulsoAbajoForce, ForceMode.Impulse);
                impulsoAbajo = true;
                rebotes = 0;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Wall":
                // Debug.Log(collision.relativeVelocity);
                Vector3 normal = collision.GetContact(0).normal;
                Vector3 flattenedNormal = Vector3.ProjectOnPlane(normal, Vector3.up);
                rb.AddForce(flattenedNormal * collision.relativeVelocity.magnitude, ForceMode.VelocityChange);
                break;
            case "Floor":
                if (impulsoAbajo)
                {
                    rebotes++;
                    rb.AddForce(Vector3.up * jumpForce/rebotes, ForceMode.Impulse);
                    if (rebotes >= 5)
                    {
                        impulsoAbajo = false;
                        puedeImpulsoAbajo = true;
                    }
                }
                else
                {
                    puedeSaltar = true;
                    
                }
                break;
        }
    }

    IEnumerator Dash(float duration)
    {
        // Debug.Log("Comienza DASH");
        canUseDash = false;
        currentlyUsingDash = true;

        Vector3 direction = rb.linearVelocity.normalized;

        rb.linearVelocity = rb.linearVelocity.normalized;
        rb.AddForce(direction * dashForce , ForceMode.Impulse);
        
        yield return new WaitForSeconds(duration);

        // Debug.Log("Termina DASH");
        currentlyUsingDash = false;

        yield return new WaitForSeconds(1f);

        canUseDash = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        /*if(other.gameObject.CompareTag("Collectable"))
        {
            Debug.Log("Collectable obtained!");
        }
        else if(other.gameObject.CompareTag("Finish"))
        {
            Application.Quit();
        }
        */

        switch(other.gameObject.tag)
        {
            case "Collectable":
                Debug.Log("Collectable obtained!");
                Destroy(other.gameObject);
                break;

            case "Finish":
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
        }

    }


    private void FixedUpdate()
    {
        Vector3 force = movementDirection * speed;
        rb.AddForce(force);

        if(rb.linearVelocity.magnitude > maxSpeed && !currentlyUsingDash)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

    }
}
