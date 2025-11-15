using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemigo especializado en combate a distancia
/// Características: Mantiene distancia, dispara proyectiles, huye si el jugador se acerca
/// </summary>
public class EnemigoRanged : MonoBehaviour
{
    [Header("Estadisticas")]
    public int vidaMaxima = 40;
    private int vidaActual;
    public int danioProyectil = 12;

    [Header("Movimiento")]
    public float velocidadMovimiento = 2f;
    public float velocidadHuida = 3.5f;

    [Header("Deteccion y Combate")]
    public float rangoDeteccion = 8f;
    public float rangoAtaqueMinimo = 3f; // Distancia mínima que mantiene
    public float rangoAtaqueMaximo = 7f; // Distancia máxima de disparo
    public float tiempoEntreDisparos = 1.5f;
    public LayerMask capaJugador;

    [Header("Proyectiles")]
    public GameObject proyectilPrefab;
    public Transform puntoDisparo; // Punto desde donde sale el proyectil
    public float velocidadProyectil = 8f;
    public int proyectilesRafaga = 1; // Cantidad de proyectiles por ráfaga
    public float tiempoEntreProyectilesRafaga = 0.2f;

    [Header("Habilidades Especiales")]
    public bool puedeDisparoCircular = true;
    public int proyectilesCirculares = 8; // Proyectiles en patrón circular
    public float cooldownDisparoCircular = 8f;

    [Header("Patrullaje")]
    public bool patrulla = true;
    public float rangoPatrullaje = 5f;
    public float tiempoEsperaPatrulla = 2f;

    // Referencias privadas
    private Transform jugador;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Control de combate
    private bool puedeDisparar = true;
    private bool estaDisparando = false;
    private bool puedeDisparoCircularNuevamente = true;

    // Control de estados
    public enum EstadoEnemigo
    {
        Idle,
        Patrullando,
        Persiguiendo,
        Atacando,
        Huyendo,
        Muerto
    }

    private EstadoEnemigo estadoActual = EstadoEnemigo.Idle;

    // Variables de patrullaje
    private Vector2 puntoInicial;
    private Vector2 destinoPatrulla;
    private float tiempoEsperaActual;

    // Variables de animación
    private Vector2 ultimaDireccion = Vector2.down;

    void Start()
    {
        // Inicializar componentes
        vidaActual = vidaMaxima;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Configurar Rigidbody2D
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Si no hay punto de disparo, usar la posición del enemigo
        if (puntoDisparo == null)
        {
            puntoDisparo = transform;
        }

        // Guardar posición inicial
        puntoInicial = transform.position;

        // Buscar al jugador
        BuscarJugador();

        // Iniciar patrullaje si está activado
        if (patrulla)
        {
            GenerarPuntoPatrulla();
            estadoActual = EstadoEnemigo.Patrullando;
        }
        else
        {
            estadoActual = EstadoEnemigo.Idle;
        }
    }

    void Update()
    {
        if (estadoActual == EstadoEnemigo.Muerto)
            return;

        // Si no hay jugador, intentar buscarlo
        if (jugador == null)
        {
            BuscarJugador();
            if (jugador == null)
            {
                estadoActual = EstadoEnemigo.Idle;
                return;
            }
        }

        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

        // Verificar si puede hacer disparo circular especial
        if (puedeDisparoCircular && puedeDisparoCircularNuevamente &&
            distanciaAlJugador <= rangoAtaqueMaximo && distanciaAlJugador >= rangoAtaqueMinimo &&
            estadoActual == EstadoEnemigo.Atacando)
        {
            // 20% de probabilidad de hacer disparo circular
            if (Random.Range(0, 100) < 20)
            {
                StartCoroutine(DisparoCircular());
            }
        }

        // Máquina de estados
        switch (estadoActual)
        {
            case EstadoEnemigo.Idle:
                ModoIdle();
                if (distanciaAlJugador <= rangoDeteccion)
                {
                    estadoActual = EstadoEnemigo.Persiguiendo;
                }
                else if (patrulla)
                {
                    estadoActual = EstadoEnemigo.Patrullando;
                }
                break;

            case EstadoEnemigo.Patrullando:
                ModoPatrulla();
                if (distanciaAlJugador <= rangoDeteccion)
                {
                    estadoActual = EstadoEnemigo.Persiguiendo;
                }
                break;

            case EstadoEnemigo.Persiguiendo:
                // Si el jugador está muy cerca, huir
                if (distanciaAlJugador < rangoAtaqueMinimo)
                {
                    estadoActual = EstadoEnemigo.Huyendo;
                }
                // Si está en rango de ataque, atacar
                else if (distanciaAlJugador >= rangoAtaqueMinimo && distanciaAlJugador <= rangoAtaqueMaximo)
                {
                    estadoActual = EstadoEnemigo.Atacando;
                }
                // Si perdió al jugador, volver a patrullar
                else if (distanciaAlJugador > rangoDeteccion * 1.5f)
                {
                    estadoActual = patrulla ? EstadoEnemigo.Patrullando : EstadoEnemigo.Idle;
                    GenerarPuntoPatrulla();
                }
                else
                {
                    AcercarseAlJugador();
                }
                break;

            case EstadoEnemigo.Huyendo:
                // Si ya está a distancia segura, volver a atacar
                if (distanciaAlJugador >= rangoAtaqueMinimo * 1.2f)
                {
                    estadoActual = EstadoEnemigo.Atacando;
                }
                else
                {
                    HuirDelJugador();
                }
                break;

            case EstadoEnemigo.Atacando:
                // Si el jugador se acerca mucho, huir
                if (distanciaAlJugador < rangoAtaqueMinimo)
                {
                    estadoActual = EstadoEnemigo.Huyendo;
                    estaDisparando = false;
                }
                // Si el jugador se aleja mucho, perseguir
                else if (distanciaAlJugador > rangoAtaqueMaximo)
                {
                    estadoActual = EstadoEnemigo.Persiguiendo;
                    estaDisparando = false;
                }
                else
                {
                    Disparar();
                }
                break;
        }

        // Actualizar animaciones
        ActualizarAnimaciones();
    }

    void ModoIdle()
    {
        rb.velocity = Vector2.zero;
    }

    void ModoPatrulla()
    {
        // Si está esperando
        if (tiempoEsperaActual > 0)
        {
            tiempoEsperaActual -= Time.deltaTime;
            rb.velocity = Vector2.zero;
            return;
        }

        // Moverse hacia el destino
        Vector2 direccion = (destinoPatrulla - (Vector2)transform.position).normalized;
        rb.velocity = direccion * velocidadMovimiento;
        ultimaDireccion = direccion;

        // Si llegó al destino, generar nuevo punto
        if (Vector2.Distance(transform.position, destinoPatrulla) < 0.5f)
        {
            tiempoEsperaActual = tiempoEsperaPatrulla;
            GenerarPuntoPatrulla();
        }
    }

    void AcercarseAlJugador()
    {
        if (jugador == null) return;

        Vector2 direccion = (jugador.position - transform.position).normalized;
        rb.velocity = direccion * velocidadMovimiento;
        ultimaDireccion = direccion;
    }

    void HuirDelJugador()
    {
        if (jugador == null) return;

        // Moverse en dirección opuesta al jugador
        Vector2 direccion = (transform.position - jugador.position).normalized;
        rb.velocity = direccion * velocidadHuida;
        ultimaDireccion = direccion;
    }

    void Disparar()
    {
        rb.velocity = Vector2.zero;

        // Mirar hacia el jugador
        if (jugador != null)
        {
            ultimaDireccion = (jugador.position - transform.position).normalized;
        }

        if (puedeDisparar && !estaDisparando)
        {
            StartCoroutine(EjecutarDisparo());
        }
    }

    IEnumerator EjecutarDisparo()
    {
        estaDisparando = true;
        puedeDisparar = false;

        // Activar animación de ataque
        if (animator != null)
        {
            animator.SetTrigger("Atacando");
        }

        // Disparar ráfaga de proyectiles
        for (int i = 0; i < proyectilesRafaga; i++)
        {
            CrearProyectil(jugador.position);

            if (i < proyectilesRafaga - 1)
            {
                yield return new WaitForSeconds(tiempoEntreProyectilesRafaga);
            }
        }

        // Esperar antes de poder disparar de nuevo
        yield return new WaitForSeconds(tiempoEntreDisparos);
        puedeDisparar = true;
        estaDisparando = false;
    }

    IEnumerator DisparoCircular()
    {
        puedeDisparoCircularNuevamente = false;
        puedeDisparar = false;
        estaDisparando = true;

        Debug.Log("Enemigo Ranged ejecuta DISPARO CIRCULAR");

        // Activar animación especial si existe
        if (animator != null)
        {
            animator.SetTrigger("AtaqueEspecial");
        }

        // Pequeña pausa dramática
        yield return new WaitForSeconds(0.5f);

        // Crear proyectiles en todas direcciones
        float anguloIncremento = 360f / proyectilesCirculares;

        for (int i = 0; i < proyectilesCirculares; i++)
        {
            float angulo = anguloIncremento * i;
            float radianes = angulo * Mathf.Deg2Rad;

            Vector2 direccion = new Vector2(Mathf.Cos(radianes), Mathf.Sin(radianes));
            CrearProyectilConDireccion(direccion);
        }

        yield return new WaitForSeconds(0.5f);
        estaDisparando = false;
        puedeDisparar = true;

        // Cooldown para el siguiente disparo circular
        yield return new WaitForSeconds(cooldownDisparoCircular);
        puedeDisparoCircularNuevamente = true;
    }

    void CrearProyectil(Vector3 objetivo)
    {
        if (proyectilPrefab == null)
        {
            Debug.LogWarning("No hay prefab de proyectil asignado al enemigo ranged");
            return;
        }

        // Calcular dirección hacia el objetivo
        Vector2 direccion = (objetivo - puntoDisparo.position).normalized;

        // Instanciar proyectil
        GameObject proyectil = Instantiate(proyectilPrefab, puntoDisparo.position, Quaternion.identity);

        // Configurar el proyectil
        ProyectilEnemigo scriptProyectil = proyectil.GetComponent<ProyectilEnemigo>();
        if (scriptProyectil != null)
        {
            scriptProyectil.EstablecerDireccion(direccion);
            scriptProyectil.EstablecerDanio(danioProyectil);
            scriptProyectil.EstablecerVelocidad(velocidadProyectil);
        }
    }

    void CrearProyectilConDireccion(Vector2 direccion)
    {
        if (proyectilPrefab == null) return;

        // Instanciar proyectil
        GameObject proyectil = Instantiate(proyectilPrefab, puntoDisparo.position, Quaternion.identity);

        // Configurar el proyectil
        ProyectilEnemigo scriptProyectil = proyectil.GetComponent<ProyectilEnemigo>();
        if (scriptProyectil != null)
        {
            scriptProyectil.EstablecerDireccion(direccion);
            scriptProyectil.EstablecerDanio(danioProyectil);
            scriptProyectil.EstablecerVelocidad(velocidadProyectil);
        }
    }

    void GenerarPuntoPatrulla()
    {
        // Generar un punto aleatorio dentro del rango de patrullaje
        Vector2 puntoAleatorio = Random.insideUnitCircle * rangoPatrullaje;
        destinoPatrulla = puntoInicial + puntoAleatorio;
    }

    void BuscarJugador()
    {
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }
    }

    void ActualizarAnimaciones()
    {
        if (animator == null) return;

        // Actualizar parámetros de velocidad para animaciones de movimiento
        animator.SetFloat("Velocidad_X", ultimaDireccion.x);
        animator.SetFloat("Velocidad_Y", ultimaDireccion.y);
        animator.SetBool("En_Movimiento", rb.velocity.magnitude > 0.1f);
        animator.SetBool("Huyendo", estadoActual == EstadoEnemigo.Huyendo);
    }

    public void RecibirDanio(int cantidad)
    {
        if (estadoActual == EstadoEnemigo.Muerto)
            return;

        vidaActual -= cantidad;
        Debug.Log("Enemigo Ranged recibió " + cantidad + " de daño. Vida restante: " + vidaActual);

        // Efecto visual de daño
        StartCoroutine(EfectoGolpe());

        // Si recibe daño, huir inmediatamente
        if (jugador != null)
        {
            float distancia = Vector2.Distance(transform.position, jugador.position);
            if (distancia < rangoAtaqueMinimo * 1.5f)
            {
                estadoActual = EstadoEnemigo.Huyendo;
            }
            else
            {
                estadoActual = EstadoEnemigo.Atacando;
            }
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    IEnumerator EfectoGolpe()
    {
        if (spriteRenderer != null)
        {
            Color colorOriginal = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = colorOriginal;
        }
    }

    void Morir()
    {
        estadoActual = EstadoEnemigo.Muerto;
        Debug.Log("Enemigo Ranged eliminado");

        // Detener movimiento
        rb.velocity = Vector2.zero;

        // Desactivar colisiones
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Animación de muerte si existe
        if (animator != null)
        {
            animator.SetTrigger("Morir");
        }

        // Destruir después de un tiempo
        StartCoroutine(DestruirDespuesDeTiempo(1f));
    }

    IEnumerator DestruirDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);

        // Aquí puedes agregar efectos adicionales:
        // - Spawn de items
        // - Efectos de partículas
        // - Sonidos

        Destroy(gameObject);
    }

    // Visualizar rangos en el editor
    void OnDrawGizmosSelected()
    {
        // Rango de detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Rango de ataque mínimo (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangoAtaqueMinimo);

        // Rango de ataque máximo (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaqueMaximo);

        // Rango de patrullaje (azul)
        if (patrulla)
        {
            Vector3 puntoInicio = Application.isPlaying ? puntoInicial : transform.position;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(puntoInicio, rangoPatrullaje);
        }
    }

    // Getters públicos
    public int ObtenerVidaActual() => vidaActual;
    public int ObtenerVidaMaxima() => vidaMaxima;
    public EstadoEnemigo ObtenerEstadoActual() => estadoActual;
}
