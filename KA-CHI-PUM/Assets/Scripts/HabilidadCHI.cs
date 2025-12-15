using System.Collections;
using UnityEngine;

public class HabilidadCHI : MonoBehaviour
{
    [Header("Configuración de CHI")]
    public GameObject prefabEfectoCHI; // Prefab del efecto visual
    public Transform puntoOrigen; // Desde donde se origina el CHI
    public float radioArea = 3f; // Radio del área de efecto
    public int danioBase = 25;
    public float duracionEfecto = 0.5f; // Duración de la animación/efecto
    public float tiempoRecarga = 3f; // Cooldown de la habilidad
    public LayerMask capaEnemigos;
    public KeyCode teclaActivacion = KeyCode.Q;

    [Header("Efectos Visuales")]
    public Color colorDanio = Color.cyan;
    public float intensidadShake = 0.1f;
    public float duracionShake = 0.2f;

    private bool puedeUsar = true;
    private float tiempoProximoUso = 0f;
    private Animator animator;
    private PlayerController playerController;

    // Hashes para optimización
    private int chiHash;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();

        // Crear hash para la animación
        chiHash = Animator.StringToHash("UsandoCHI");

        // Si no se asignó punto de origen, usar la posición del jugador
        if (puntoOrigen == null)
        {
            puntoOrigen = transform;
        }
    }

    void Update()
    {
        // Verificar si se puede usar la habilidad
        if (Time.time >= tiempoProximoUso)
        {
            puedeUsar = true;
        }

        // Input para activar CHI
        if (Input.GetKeyDown(teclaActivacion) && puedeUsar)
        {
            StartCoroutine(EjecutarCHI());
        }
    }

    IEnumerator EjecutarCHI()
    {
        puedeUsar = false;
        tiempoProximoUso = Time.time + tiempoRecarga;

        Debug.Log("¡CHI activado! Daño en área de radio: " + radioArea);

        // Activar animación si existe
        if (animator != null)
        {
            animator.SetTrigger(chiHash);
        }

        // Crear efecto visual
        GameObject efectoVisual = null;
        if (prefabEfectoCHI != null)
        {
            efectoVisual = Instantiate(prefabEfectoCHI, puntoOrigen.position, Quaternion.identity);

            // Escalar el efecto según el radio
            efectoVisual.transform.localScale = Vector3.one * (radioArea / 1.5f);

            // Destruir el efecto después de la duración
            Destroy(efectoVisual, duracionEfecto);
        }

        // Pequeña espera para sincronizar con la animación
        yield return new WaitForSeconds(0.15f);

        // Detectar todos los enemigos en el área
        Collider2D[] enemigosEnArea = Physics2D.OverlapCircleAll(
            puntoOrigen.position,
            radioArea,
            capaEnemigos
        );

        // Aplicar daño a todos los enemigos detectados
        int enemigosGolpeados = 0;
        foreach (Collider2D enemigo in enemigosEnArea)
        {
            Enemigo enemigoScript = enemigo.GetComponent<Enemigo>();
            if (enemigoScript != null)
            {
                // Calcular daño (puedes añadir variaciones aquí)
                int danioFinal = danioBase;

                // Aplicar el daño
                enemigoScript.RecibirDanio(danioFinal);
                enemigosGolpeados++;

                // Efecto visual en el enemigo
                StartCoroutine(EfectoGolpeEnemigo(enemigo.gameObject));

                Debug.Log($"CHI golpeó a {enemigo.name} por {danioFinal} de daño");
            }
        }

        Debug.Log($"CHI impactó a {enemigosGolpeados} enemigos");

        // Efecto de screen shake si hay enemigos golpeados
        if (enemigosGolpeados > 0)
        {
            StartCoroutine(ScreenShake());
        }
    }

    IEnumerator EfectoGolpeEnemigo(GameObject enemigo)
    {
        SpriteRenderer sprite = enemigo.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color colorOriginal = sprite.color;
            sprite.color = colorDanio;
            yield return new WaitForSeconds(0.15f);
            sprite.color = colorOriginal;
        }
    }

    IEnumerator ScreenShake()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        Vector3 posicionOriginal = mainCam.transform.localPosition;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracionShake)
        {
            float x = Random.Range(-1f, 1f) * intensidadShake;
            float y = Random.Range(-1f, 1f) * intensidadShake;

            mainCam.transform.localPosition = posicionOriginal + new Vector3(x, y, 0);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.localPosition = posicionOriginal;
    }

    // Visualizar el área de efecto en el editor
    void OnDrawGizmosSelected()
    {
        Vector3 centro = puntoOrigen != null ? puntoOrigen.position : transform.position;

        // Dibujar el radio del área de efecto
        Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan semi-transparente
        Gizmos.DrawSphere(centro, radioArea);

        // Dibujar el borde del área
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(centro, radioArea);

        // Información del cooldown
        if (Application.isPlaying && !puedeUsar)
        {
            Gizmos.color = Color.red;
            float progreso = 1f - ((tiempoProximoUso - Time.time) / tiempoRecarga);
            Gizmos.DrawWireSphere(centro, radioArea * progreso);
        }
    }

    // Métodos públicos para acceder al estado de la habilidad
    public bool PuedeUsarCHI()
    {
        return puedeUsar;
    }

    public float ObtenerTiempoRestanteCooldown()
    {
        return Mathf.Max(0, tiempoProximoUso - Time.time);
    }

    public float ObtenerProgresoCooldown()
    {
        if (puedeUsar) return 1f;
        return 1f - (ObtenerTiempoRestanteCooldown() / tiempoRecarga);
    }
}