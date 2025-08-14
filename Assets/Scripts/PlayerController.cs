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

    bool desbloqueadoImpulsoAbajo = true;
    bool desbloqueadoSaltar = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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

        if (Keyboard.current.spaceKey.wasPressedThisFrame && desbloqueadoSaltar)
        {
            if (puedeSaltar)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                puedeSaltar = false;
                if (desbloqueadoImpulsoAbajo)
                {
                    puedeImpulsoAbajo = true;
                }
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
                    // Debug.Log(collision.relativeVelocity.magnitude / rebotes > 1);
                    if (collision.relativeVelocity.magnitude / rebotes > 2.0f)
                    {
                        Vector3 normalRebote = collision.GetContact(0).normal;
                        Vector3 flattenedNormalRebote = Vector3.ProjectOnPlane(normalRebote, Vector3.left);
                        rb.AddForce(flattenedNormalRebote * collision.relativeVelocity.magnitude/rebotes, ForceMode.VelocityChange);
                    }
                    else
                    {
                        impulsoAbajo = false;
                        puedeImpulsoAbajo = true;
                    }
                    break;
                    /*rebotes++;
                    rb.AddForce(Vector3.up * jumpForce/rebotes, ForceMode.Impulse);
                    if (rebotes >= 5)
                    {
                        impulsoAbajo = false;
                        puedeImpulsoAbajo = true;
                    }*/
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
        canUseDash = false;
        currentlyUsingDash = true;

        Vector3 direction = rb.linearVelocity.normalized;

        rb.linearVelocity = rb.linearVelocity.normalized;
        rb.AddForce(direction * dashForce , ForceMode.Impulse);
        
        yield return new WaitForSeconds(duration);

        currentlyUsingDash = false;

        yield return new WaitForSeconds(1f);

        canUseDash = true;
    }


    private void OnTriggerEnter(Collider other)
    {

        switch(other.gameObject.tag)
        {
            case "Collectable":
                switch (other.gameObject.GetComponent<Collectable>().tipo){
                    case 0:
                        Debug.Log("Moneda");
                        break;
                    case 1:
                        Debug.Log("Desbloquea Salto");
                        desbloqueadoSaltar = true;
                        break;
                    case 2:
                        Debug.Log("Desbloquea Impulso Abajo");
                        desbloqueadoImpulsoAbajo = true;
                        break;
                    case 3:
                        Debug.Log("Desbloquea Dash");
                        canUseDash = true;
                        break;
                }
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
