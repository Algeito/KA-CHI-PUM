using UnityEngine;

public class EfectoCHIVisual : MonoBehaviour
{
    [Header("Configuración de Animación")]
    public float velocidadRotacion = 180f;
    public AnimationCurve curvaEscala = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve curvaAlpha = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float duracion = 0.5f;

    private SpriteRenderer spriteRenderer;
    private float tiempoTranscurrido = 0f;
    private Vector3 escalaInicial;
    private Color colorInicial;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            colorInicial = spriteRenderer.color;
        }

        escalaInicial = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        tiempoTranscurrido += Time.deltaTime;
        float progreso = tiempoTranscurrido / duracion;

        // Rotación constante
        transform.Rotate(Vector3.forward, velocidadRotacion * Time.deltaTime);

        // Escala animada
        float escalaMultiplicador = curvaEscala.Evaluate(progreso);
        transform.localScale = escalaInicial * escalaMultiplicador;

        // Alpha animado
        if (spriteRenderer != null)
        {
            Color nuevoColor = colorInicial;
            nuevoColor.a = curvaAlpha.Evaluate(progreso);
            spriteRenderer.color = nuevoColor;
        }

        // Auto-destruirse al finalizar (por seguridad, aunque ya se destruye en HabilidadCHI)
        if (progreso >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
