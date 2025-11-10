using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================== ENEMIGO BASE ====================
public class EnemigoBase : MonoBehaviour
{
    [Header("Estadísticas")]
    public float vidaMaxima = 50f;
    protected float vidaActual;
    public float velocidadMovimiento = 2f;
    public float dañoContacto = 5f;
    public int experienciaDrop = 10;

    [Header("Configuración de Tipo")]
    public TipoHabilidad[] habilidadesEnemigo; // Vacío, 1, 2, 3 habilidades
    public bool esInverso = false;
    public bool esJefe = false;

    [Header("Combate")]
    public float rangoDeteccion = 10f;
    public float cooldownAtaque = 1f;

    [Header("Drops")]
    public GameObject prefabExperiencia;
    public float probabilidadDropPowerup = 0.1f;
    public GameObject[] powerupsPosibles;

    // Referencias
    protected Transform jugador;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;

    // Control
    protected bool puedeMoverse = true;
    protected float tiempoUltimoAtaque;
    protected Vector2 direccionMovimiento;

    // Efectos de estado
    protected bool estaQuemado = false;
    protected bool estaCongelado = false;
    protected bool estaAturdido = false;
    protected bool estaSangrando = false;

    protected virtual void Start()
    {
        vidaActual = vidaMaxima;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        BuscarJugador();

        // Multiplicadores para tipos especiales
        if (esInverso)
        {
            vidaMaxima *= 3f;
            vidaActual = vidaMaxima;
            dañoContacto *= 2f;
            experienciaDrop *= 5;
        }

        if (esJefe)
        {
            vidaMaxima *= 10f;
            vidaActual = vidaMaxima;
            dañoContacto *= 3f;
            experienciaDrop *= 20;
            transform.localScale *= 1.5f;
        }
    }

    protected virtual void Update()
    {
        if (jugador == null)
        {
            BuscarJugador();
            return;
        }

        if (puedeMoverse && !estaCongelado && !estaAturdido)
        {
            MoverHaciaJugador();
        }
    }

    protected virtual void MoverHaciaJugador()
    {
        direccionMovimiento = (jugador.position - transform.position).normalized;
        float velocidadFinal = estaCongelado ? velocidadMovimiento * 0.5f : velocidadMovimiento;
        rb.velocity = direccionMovimiento * velocidadFinal;
    }

    void BuscarJugador()
    {
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }
    }

    public virtual void RecibirDaño(float cantidad, CombinacionElemental elemento = null)
    {
        vidaActual -= cantidad;

        // Aplicar efectos elementales
        if (elemento != null && elemento.nivelElemento > 0)
        {
            AplicarEfectoElemental(elemento);
        }

        StartCoroutine(EfectoGolpe());

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    void AplicarEfectoElemental(CombinacionElemental elemento)
    {
        float probabilidad = Random.Range(0f, 1f);

        if (probabilidad <= elemento.probabilidadEfecto)
        {
            if (elemento.aplicaQuemadura && !estaQuemado)
            {
                StartCoroutine(EfectoQuemadura(3f, 5f)); // Duración, DPS
            }
            else if (elemento.aplicaCongelamiento && !estaCongelado)
            {
                StartCoroutine(EfectoCongelamiento(2f));
            }
            else if (elemento.aplicaAturdimiento && !estaAturdido)
            {
                StartCoroutine(EfectoAturdimiento(1.5f));
            }
            else if (elemento.aplicaSangrado && !estaSangrando)
            {
                StartCoroutine(EfectoSangrado(4f, 3f));
            }
            else if (elemento.aplicaEmpuje)
            {
                Vector2 direccionEmpuje = (transform.position - jugador.position).normalized;
                rb.AddForce(direccionEmpuje * 300f);
            }
        }
    }

    IEnumerator EfectoQuemadura(float duracion, float dps)
    {
        estaQuemado = true;
        float tiempoTranscurrido = 0;

        while (tiempoTranscurrido < duracion)
        {
            RecibirDaño(dps * Time.deltaTime);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        estaQuemado = false;
    }

    IEnumerator EfectoCongelamiento(float duracion)
    {
        estaCongelado = true;
        Color colorOriginal = spriteRenderer.color;
        spriteRenderer.color = Color.cyan;

        yield return new WaitForSeconds(duracion);

        spriteRenderer.color = colorOriginal;
        estaCongelado = false;
    }

    IEnumerator EfectoAturdimiento(float duracion)
    {
        estaAturdido = true;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(duracion);

        estaAturdido = false;
    }

    IEnumerator EfectoSangrado(float duracion, float dps)
    {
        estaSangrando = true;
        float tiempoTranscurrido = 0;

        while (tiempoTranscurrido < duracion)
        {
            RecibirDaño(dps * Time.deltaTime);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        estaSangrando = false;
    }

    IEnumerator EfectoGolpe()
    {
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = original;
        }
    }

    protected virtual void Morir()
    {
        // Dropear experiencia
        if (prefabExperiencia != null)
        {
            int cantidadExp = experienciaDrop;
            for (int i = 0; i < cantidadExp / 10; i++)
            {
                Vector2 posAleatoria = (Vector2)transform.position + Random.insideUnitCircle * 0.5f;
                Instantiate(prefabExperiencia, posAleatoria, Quaternion.identity);
            }
        }

        // Dropear powerup (probabilidad)
        if (Random.Range(0f, 1f) <= probabilidadDropPowerup && powerupsPosibles.Length > 0)
        {
            GameObject powerup = powerupsPosibles[Random.Range(0, powerupsPosibles.Length)];
            Instantiate(powerup, transform.position, Quaternion.identity);
        }

        // Notificar al spawner
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.EnemigoMuerto(this);
        }

        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time >= tiempoUltimoAtaque + cooldownAtaque)
        {
            PlayerStats jugadorStats = collision.gameObject.GetComponent<PlayerStats>();
            if (jugadorStats != null)
            {
                jugadorStats.RecibirDaño(dañoContacto);
                tiempoUltimoAtaque = Time.time;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}
