using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================== MOVIMIENTO DEL JUGADOR ====================
public class PlayerMovement : MonoBehaviour
{
    private PlayerStats stats;
    private Rigidbody2D rb;
    private Vector2 movimiento;
    private Animator animator;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        // Input de movimiento
        movimiento.x = Input.GetAxisRaw("Horizontal");
        movimiento.y = Input.GetAxisRaw("Vertical");
        movimiento.Normalize();

        // Actualizar animaciones
        if (animator != null)
        {
            animator.SetFloat("VelocidadX", movimiento.x);
            animator.SetFloat("VelocidadY", movimiento.y);
            animator.SetBool("EnMovimiento", movimiento.magnitude > 0);
        }
    }

    void FixedUpdate()
    {
        if (stats != null)
        {
            rb.velocity = movimiento * stats.ObtenerVelocidadTotal();
        }
    }
}
