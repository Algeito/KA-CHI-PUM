using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemigo Jefe con múltiples fases y ataques especiales
/// Características: Alta vida, múltiples fases, ataques variados, invoca enemigos
/// </summary>
public class EnemigoJefe : MonoBehaviour
{
    [Header("Estadisticas")]
    public int vidaMaxima = 300;
    private int vidaActual;
    public int danioAtaqueMelee = 20;
    public int danioProyectil = 15;

    [Header("Movimiento")]
    public float velocidadMovimiento = 1.5f;
    public float velocidadCarga = 6f;

    [Header("Deteccion y Combate")]
    public float rangoDeteccion = 12f;
    public float rangoAtaqueMelee = 2.5f;
    public float rangoAtaqueRanged = 8f;
    public LayerMask capaJugador;

    [Header("Proyectiles")]
    public GameObject proyectilPrefab;
    public Transform puntoDisparo;
    public float velocidadProyectil = 10f;

    [Header("Invocacion de Enemigos")]
    public GameObject[] enemigosParaInvocar; // Prefabs de enemigos a invocar
    public int maxEnemigosPorInvocacion = 3;
    public float radioInvocacion = 5f;
    public float cooldownInvocacion = 15f;

    [Header("Fases del Jefe")]
    public bool usarSistemaFases = true;
    [Range(0, 100)] public int porcentajeFase2 = 66; // Cambio a fase 2 al 66% de vida
    [Range(0, 100)] public int porcentajeFase3 = 33; // Cambio a fase 3 al 33% de vida

    [Header("Efectos Visuales")]
    public GameObject efectoInvocacion;
    public GameObject efectoCambioFase;

    // Referencias privadas
    private Transform jugador;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Control de combate
    private bool puedeAtacar = true;
    private bool estaAtacando = false;
    private bool puedeInvocar = true;

    // Sistema de fases
    public enum FaseJefe
    {
        Fase1, // Comportamiento básico
        Fase2, // Más agresivo, ataques más rápidos
        Fase3  // Modo berserk, todos los ataques desbloqueados
    }

    private FaseJefe faseActual = FaseJefe.Fase1;
    private bool yaEntroFase2 = false;
    private bool yaEntroFase3 = false;

    // Control de estados
    public enum EstadoJefe
    {
        Idle,
        Persiguiendo,
        AtacandoMelee,
        AtacandoRanged,
        Cargando,
        Invocando,
        CambiandoFase,
        Muerto
    }

    private EstadoJefe estadoActual = EstadoJefe.Idle;

    // Variables de animación
    private Vector2 ultimaDireccion = Vector2.down;

    // Contador de enemigos invocados vivos
    private int enemigosInvocadosVivos = 0;

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

        // Si no hay punto de disparo, usar la posición del jefe
        if (puntoDisparo == null)
        {
            puntoDisparo = transform;
        }

        // Buscar al jugador
        BuscarJugador();

        Debug.Log("¡JEFE APARECE! Vida: " + vidaMaxima);
    }

    void Update()
    {
        if (estadoActual == EstadoJefe.Muerto || estadoActual == EstadoJefe.CambiandoFase)
            return;

        // Verificar cambios de fase
        if (usarSistemaFases)
        {
            VerificarCambioFase();
        }

        // Si no hay jugador, intentar buscarlo
        if (jugador == null)
        {
            BuscarJugador();
            if (jugador == null)
            {
                estadoActual = EstadoJefe.Idle;
                return;
            }
        }

        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

        // Verificar si debe invocar enemigos (solo en fase 2 y 3)
        if (faseActual != FaseJefe.Fase1 && puedeInvocar && enemigosInvocadosVivos < maxEnemigosPorInvocacion)
        {
            // 15% de probabilidad de invocar cada frame cuando puede
            if (Random.Range(0, 1000) < 5)
            {
                estadoActual = EstadoJefe.Invocando;
                StartCoroutine(InvocarEnemigos());
                return;
            }
        }

        // Máquina de estados
        switch (estadoActual)
        {
            case EstadoJefe.Idle:
                ModoIdle();
                if (distanciaAlJugador <= rangoDeteccion)
                {
                    estadoActual = EstadoJefe.Persiguiendo;
                }
                break;

            case EstadoJefe.Persiguiendo:
                if (distanciaAlJugador <= rangoAtaqueMelee)
                {
                    // Ataque melee
                    estadoActual = EstadoJefe.AtacandoMelee;
                }
                else if (distanciaAlJugador <= rangoAtaqueRanged && faseActual != FaseJefe.Fase1)
                {
                    // Ataque ranged (solo en fase 2 y 3)
                    if (Random.Range(0, 100) < 60) // 60% de probabilidad de ataque ranged
                    {
                        estadoActual = EstadoJefe.AtacandoRanged;
                    }
                    else
                    {
                        PerseguirJugador();
                    }
                }
                else if (distanciaAlJugador > rangoDeteccion * 2f)
                {
                    estadoActual = EstadoJefe.Idle;
                }
                else
                {
                    PerseguirJugador();
                }
                break;

            case EstadoJefe.AtacandoMelee:
                if (distanciaAlJugador > rangoAtaqueMelee * 1.5f)
                {
                    estadoActual = EstadoJefe.Persiguiendo;
                    estaAtacando = false;
                }
                else
                {
                    AtaqueMelee();
                }
                break;

            case EstadoJefe.AtacandoRanged:
                if (distanciaAlJugador <= rangoAtaqueMelee)
                {
                    estadoActual = EstadoJefe.AtacandoMelee;
                    estaAtacando = false;
                }
                else if (distanciaAlJugador > rangoAtaqueRanged)
                {
                    estadoActual = EstadoJefe.Persiguiendo;
                    estaAtacando = false;
                }
                else
                {
                    AtaqueRanged();
                }
                break;

            case EstadoJefe.Invocando:
                // La invocación se maneja en la corrutina
                break;
        }

        // Actualizar animaciones
        ActualizarAnimaciones();
    }

    void VerificarCambioFase()
    {
        float porcentajeVida = (float)vidaActual / vidaMaxima * 100f;

        // Cambio a Fase 2
        if (!yaEntroFase2 && porcentajeVida <= porcentajeFase2)
        {
            yaEntroFase2 = true;
            StartCoroutine(CambiarFase(FaseJefe.Fase2));
        }
        // Cambio a Fase 3
        else if (!yaEntroFase3 && porcentajeVida <= porcentajeFase3)
        {
            yaEntroFase3 = true;
            StartCoroutine(CambiarFase(FaseJefe.Fase3));
        }
    }

    IEnumerator CambiarFase(FaseJefe nuevaFase)
    {
        EstadoJefe estadoAnterior = estadoActual;
        estadoActual = EstadoJefe.CambiandoFase;
        rb.velocity = Vector2.zero;

        Debug.Log($"¡JEFE CAMBIA A {nuevaFase}!");

        // Efecto visual de cambio de fase
        if (efectoCambioFase != null)
        {
            Instantiate(efectoCambioFase, transform.position, Quaternion.identity);
        }

        // Animación de cambio de fase
        if (animator != null)
        {
            animator.SetTrigger("CambioFase");
        }

        // Cambiar color temporalmente
        if (spriteRenderer != null)
        {
            Color colorOriginal = spriteRenderer.color;
            spriteRenderer.color = Color.magenta;
            yield return new WaitForSeconds(0.5f);
            spriteRenderer.color = Color.yellow;
            yield return new WaitForSeconds(0.5f);
            spriteRenderer.color = colorOriginal;
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // Aplicar cambios de fase
        faseActual = nuevaFase;

        switch (nuevaFase)
        {
            case FaseJefe.Fase2:
                velocidadMovimiento *= 1.3f;
                danioAtaqueMelee = Mathf.RoundToInt(danioAtaqueMelee * 1.2f);
                Debug.Log("Fase 2: Más rápido y fuerte. Desbloquea ataques a distancia.");
                break;

            case FaseJefe.Fase3:
                velocidadMovimiento *= 1.4f;
                danioAtaqueMelee = Mathf.RoundToInt(danioAtaqueMelee * 1.5f);
                danioProyectil = Mathf.RoundToInt(danioProyectil * 1.3f);
                Debug.Log("Fase 3: MODO BERSERK - Poder máximo desbloqueado!");
                break;
        }

        // Pequeña curación al cambiar de fase
        vidaActual = Mathf.Min(vidaActual + 30, vidaMaxima);

        // Volver al estado anterior o perseguir
        estadoActual = jugador != null ? EstadoJefe.Persiguiendo : EstadoJefe.Idle;
    }

    void ModoIdle()
    {
        rb.velocity = Vector2.zero;
    }

    void PerseguirJugador()
    {
        if (jugador == null) return;

        Vector2 direccion = (jugador.position - transform.position).normalized;
        rb.velocity = direccion * velocidadMovimiento;
        ultimaDireccion = direccion;
    }

    void AtaqueMelee()
    {
        rb.velocity = Vector2.zero;

        // Mirar hacia el jugador
        if (jugador != null)
        {
            ultimaDireccion = (jugador.position - transform.position).normalized;
        }

        if (puedeAtacar && !estaAtacando)
        {
            // En fase 3, puede hacer un combo de 3 ataques
            if (faseActual == FaseJefe.Fase3 && Random.Range(0, 100) < 40)
            {
                StartCoroutine(ComboMelee());
            }
            else
            {
                StartCoroutine(EjecutarAtaqueMelee());
            }
        }
    }

    IEnumerator EjecutarAtaqueMelee()
    {
        estaAtacando = true;
        puedeAtacar = false;

        // Activar animación de ataque
        if (animator != null)
        {
            animator.SetTrigger("AtaqueMelee");
        }

        yield return new WaitForSeconds(0.3f);

        // Detectar y dañar al jugador
        Collider2D[] objetosGolpeados = Physics2D.OverlapCircleAll(transform.position, rangoAtaqueMelee, capaJugador);
        foreach (Collider2D obj in objetosGolpeados)
        {
            PlayerController jugadorScript = obj.GetComponent<PlayerController>();
            if (jugadorScript != null)
            {
                jugadorScript.RecibirDanio(danioAtaqueMelee);
                Debug.Log($"JEFE golpea al jugador por {danioAtaqueMelee} de daño");
            }
        }

        // Cooldown basado en la fase
        float cooldown = faseActual == FaseJefe.Fase3 ? 1f : (faseActual == FaseJefe.Fase2 ? 1.5f : 2f);
        yield return new WaitForSeconds(cooldown);

        puedeAtacar = true;
        estaAtacando = false;
    }

    IEnumerator ComboMelee()
    {
        estaAtacando = true;
        puedeAtacar = false;

        Debug.Log("¡JEFE EJECUTA COMBO!");

        // 3 ataques rápidos
        for (int i = 0; i < 3; i++)
        {
            if (animator != null)
            {
                animator.SetTrigger("AtaqueMelee");
            }

            yield return new WaitForSeconds(0.2f);

            Collider2D[] objetosGolpeados = Physics2D.OverlapCircleAll(transform.position, rangoAtaqueMelee, capaJugador);
            foreach (Collider2D obj in objetosGolpeados)
            {
                PlayerController jugadorScript = obj.GetComponent<PlayerController>();
                if (jugadorScript != null)
                {
                    int danioCombo = Mathf.RoundToInt(danioAtaqueMelee * 0.7f);
                    jugadorScript.RecibirDanio(danioCombo);
                    Debug.Log($"JEFE combo {i + 1}/3 - {danioCombo} daño");
                }
            }

            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(1.5f);
        puedeAtacar = true;
        estaAtacando = false;
    }

    void AtaqueRanged()
    {
        rb.velocity = Vector2.zero;

        // Mirar hacia el jugador
        if (jugador != null)
        {
            ultimaDireccion = (jugador.position - transform.position).normalized;
        }

        if (puedeAtacar && !estaAtacando)
        {
            // En fase 3, puede hacer disparo circular
            if (faseActual == FaseJefe.Fase3 && Random.Range(0, 100) < 30)
            {
                StartCoroutine(DisparoCircular());
            }
            else
            {
                StartCoroutine(EjecutarAtaqueRanged());
            }
        }
    }

    IEnumerator EjecutarAtaqueRanged()
    {
        estaAtacando = true;
        puedeAtacar = false;

        // Activar animación de ataque
        if (animator != null)
        {
            animator.SetTrigger("AtaqueRanged");
        }

        yield return new WaitForSeconds(0.3f);

        // Disparar proyectiles
        int cantidadProyectiles = faseActual == FaseJefe.Fase3 ? 3 : 1;

        for (int i = 0; i < cantidadProyectiles; i++)
        {
            if (jugador != null)
            {
                CrearProyectil(jugador.position);
            }

            if (i < cantidadProyectiles - 1)
            {
                yield return new WaitForSeconds(0.2f);
            }
        }

        float cooldown = faseActual == FaseJefe.Fase3 ? 1.5f : 2f;
        yield return new WaitForSeconds(cooldown);

        puedeAtacar = true;
        estaAtacando = false;
    }

    IEnumerator DisparoCircular()
    {
        estaAtacando = true;
        puedeAtacar = false;

        Debug.Log("¡JEFE EJECUTA DISPARO CIRCULAR!");

        if (animator != null)
        {
            animator.SetTrigger("AtaqueEspecial");
        }

        yield return new WaitForSeconds(0.5f);

        // Crear 12 proyectiles en círculo
        int cantidadProyectiles = 12;
        float anguloIncremento = 360f / cantidadProyectiles;

        for (int i = 0; i < cantidadProyectiles; i++)
        {
            float angulo = anguloIncremento * i;
            float radianes = angulo * Mathf.Deg2Rad;
            Vector2 direccion = new Vector2(Mathf.Cos(radianes), Mathf.Sin(radianes));
            CrearProyectilConDireccion(direccion);
        }

        yield return new WaitForSeconds(2.5f);
        puedeAtacar = true;
        estaAtacando = false;
    }

    IEnumerator InvocarEnemigos()
    {
        puedeInvocar = false;

        Debug.Log("¡JEFE INVOCA REFUERZOS!");

        rb.velocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetTrigger("Invocando");
        }

        // Efecto visual
        if (efectoInvocacion != null)
        {
            Instantiate(efectoInvocacion, transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(1f);

        // Invocar enemigos
        if (enemigosParaInvocar != null && enemigosParaInvocar.Length > 0)
        {
            int cantidadInvocar = Mathf.Min(maxEnemigosPorInvocacion - enemigosInvocadosVivos,
                                            faseActual == FaseJefe.Fase3 ? 3 : 2);

            for (int i = 0; i < cantidadInvocar; i++)
            {
                // Posición aleatoria alrededor del jefe
                Vector2 offset = Random.insideUnitCircle * radioInvocacion;
                Vector3 posicionInvocacion = transform.position + new Vector3(offset.x, offset.y, 0);

                // Elegir enemigo aleatorio para invocar
                GameObject prefabEnemigo = enemigosParaInvocar[Random.Range(0, enemigosParaInvocar.Length)];
                GameObject enemigo = Instantiate(prefabEnemigo, posicionInvocacion, Quaternion.identity);

                // Efecto de invocación
                if (efectoInvocacion != null)
                {
                    Instantiate(efectoInvocacion, posicionInvocacion, Quaternion.identity);
                }

                enemigosInvocadosVivos++;
                StartCoroutine(ContarEnemigoInvocado(enemigo));
            }

            Debug.Log($"Invocados {cantidadInvocar} enemigos. Total vivos: {enemigosInvocadosVivos}");
        }

        estadoActual = EstadoJefe.Persiguiendo;

        yield return new WaitForSeconds(cooldownInvocacion);
        puedeInvocar = true;
    }

    IEnumerator ContarEnemigoInvocado(GameObject enemigo)
    {
        // Esperar hasta que el enemigo sea destruido
        while (enemigo != null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        enemigosInvocadosVivos--;
        Debug.Log($"Enemigo invocado eliminado. Restantes: {enemigosInvocadosVivos}");
    }

    void CrearProyectil(Vector3 objetivo)
    {
        if (proyectilPrefab == null) return;

        Vector2 direccion = (objetivo - puntoDisparo.position).normalized;
        GameObject proyectil = Instantiate(proyectilPrefab, puntoDisparo.position, Quaternion.identity);

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

        GameObject proyectil = Instantiate(proyectilPrefab, puntoDisparo.position, Quaternion.identity);

        ProyectilEnemigo scriptProyectil = proyectil.GetComponent<ProyectilEnemigo>();
        if (scriptProyectil != null)
        {
            scriptProyectil.EstablecerDireccion(direccion);
            scriptProyectil.EstablecerDanio(danioProyectil);
            scriptProyectil.EstablecerVelocidad(velocidadProyectil);
        }
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

        animator.SetFloat("Velocidad_X", ultimaDireccion.x);
        animator.SetFloat("Velocidad_Y", ultimaDireccion.y);
        animator.SetBool("En_Movimiento", rb.velocity.magnitude > 0.1f);
        animator.SetInteger("Fase", (int)faseActual + 1);
    }

    public void RecibirDanio(int cantidad)
    {
        if (estadoActual == EstadoJefe.Muerto)
            return;

        vidaActual -= cantidad;
        Debug.Log($"¡JEFE recibió {cantidad} de daño! Vida restante: {vidaActual}/{vidaMaxima}");

        // Efecto visual de daño
        StartCoroutine(EfectoGolpe());

        // Si está en idle, empezar a pelear
        if (estadoActual == EstadoJefe.Idle)
        {
            estadoActual = EstadoJefe.Persiguiendo;
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
        estadoActual = EstadoJefe.Muerto;
        Debug.Log("¡¡¡JEFE DERROTADO!!!");

        // Detener movimiento
        rb.velocity = Vector2.zero;

        // Desactivar colisiones
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Animación de muerte
        if (animator != null)
        {
            animator.SetTrigger("Morir");
        }

        // Destruir después de un tiempo
        StartCoroutine(DestruirDespuesDeTiempo(3f));
    }

    IEnumerator DestruirDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);

        // Aquí puedes agregar:
        // - Recompensas especiales
        // - Cinemática de victoria
        // - Desbloquear siguiente nivel

        Destroy(gameObject);
    }

    // Visualizar rangos en el editor
    void OnDrawGizmosSelected()
    {
        // Rango de detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Rango de ataque melee (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaqueMelee);

        // Rango de ataque ranged (naranja)
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, rangoAtaqueRanged);

        // Radio de invocación (magenta)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radioInvocacion);
    }

    // Getters públicos
    public int ObtenerVidaActual() => vidaActual;
    public int ObtenerVidaMaxima() => vidaMaxima;
    public EstadoJefe ObtenerEstadoActual() => estadoActual;
    public FaseJefe ObtenerFaseActual() => faseActual;
    public float ObtenerPorcentajeVida() => (float)vidaActual / vidaMaxima * 100f;
}
