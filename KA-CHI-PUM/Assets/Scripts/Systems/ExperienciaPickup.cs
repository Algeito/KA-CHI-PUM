using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// ==================== EXPERIENCIA (Pickup) ====================
public class ExperienciaPickup : MonoBehaviour
{
    public int valorExperiencia = 10;
    public float velocidadAtraccion = 5f;
    public float rangoAtraccion = 3f;

    private Transform jugador;
    private bool siendoAtraido = false;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;

        // Pequeño impulso al spawn
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.AddForce(UnityEngine.Random.insideUnitCircle * 100f);

        StartCoroutine(ReducirVelocidad(rb));
    }

    IEnumerator ReducirVelocidad(Rigidbody2D rb)
    {
        yield return new WaitForSeconds(0.5f);
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
    }

    void Update()
    {
        if (jugador == null) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);

        if (distancia <= rangoAtraccion)
        {
            siendoAtraido = true;
        }

        if (siendoAtraido)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                jugador.position,
                velocidadAtraccion * Time.deltaTime
            );
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerExperience expSystem = collision.GetComponent<PlayerExperience>();
            if (expSystem != null)
            {
                expSystem.AñadirExperiencia(valorExperiencia);
                Destroy(gameObject);
            }
        }
    }
}
