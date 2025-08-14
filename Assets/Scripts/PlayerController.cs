using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Componentes
    Rigidbody rb;

    // Variables
    Vector3 movementDirection = new Vector3(0, 0, 0);

    public float speed = 3;
    public float maxSpeed = 15;
    public float dashForce = 300;
    public float jumpForce = 30;
    public float impulsoAbajoForce = 60;

    public bool desbloqueadoImpulsoAbajo = false;
    public bool desbloqueadoSaltar = false;

    int rebotes = 0;

    bool canUseDash = false;
    bool currentlyUsingDash = false;
    bool puedeSaltar = false;
    bool puedeImpulsoAbajo = false;
    bool impulsoAbajo = false;

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
        // Reinicia la escena si la pelotita cae al vacío.
        if (transform.position.y < -100)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Si presiona X y ya desbloqueó o terminó el cooldown del Dash
        if (Keyboard.current.xKey.wasPressedThisFrame && canUseDash)
        {
            StartCoroutine(Dash(1f));
        }

        // Si presiona espacio y ya desbloqueó el salto
        if (Keyboard.current.spaceKey.wasPressedThisFrame && desbloqueadoSaltar)
        {
            // Si puede saltar
            if (puedeSaltar)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                puedeSaltar = false;

                // Habilita el impulso hacia abajo si ya lo desbloqueó
                if (desbloqueadoImpulsoAbajo)
                {
                    puedeImpulsoAbajo = true;
                }
            }
            
            // Si está en el aire y todavía no hace uso del impulso hacia abajo
            else if (puedeImpulsoAbajo)
            {
                rb.AddForce(Vector3.down * impulsoAbajoForce, ForceMode.Impulse);
                impulsoAbajo = true;
                puedeSaltar = false;
                puedeImpulsoAbajo = false;
                rebotes = 0;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            // Rebote contra las paredes
            case "Wall":
                Vector3 normal = collision.GetContact(0).normal;
                Vector3 flattenedNormal = Vector3.ProjectOnPlane(normal, Vector3.up);
                rb.AddForce(flattenedNormal * collision.relativeVelocity.magnitude, ForceMode.VelocityChange);
                break;

            // Rebote contra el suelo
            case "Floor":
                // Si hizo uso del impulso hacia abajo
                if (impulsoAbajo)
                {
                    rebotes++;

                    // Rebota hasta que su velocidad luego de rebotar sea menor a 2.
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
                        puedeSaltar = true;
                    }
                    break;
                }

                // Si toca el suelo sin usar el rebote, entonces puede volver a saltar.
                else
                {
                    puedeSaltar = true;
                }
                break;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Habilita el impulso y deshabilita el salto al dejar el suelo.
        if (collision.gameObject.CompareTag("Floor") && !impulsoAbajo)
        {
            puedeSaltar = false;
            if (desbloqueadoImpulsoAbajo)
            {
                puedeImpulsoAbajo = true;
            }
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
                        // Debug.Log("Moneda");
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
