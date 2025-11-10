using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================== ESTADÍSTICAS DEL JUGADOR ====================//
public class PlayerStats : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;
    private float vidaActual;

    [Header("Estadísticas Base")]
    public float velocidadMovimiento = 5f;
    public float dañoBase = 10f;
    public float multiplicadorRecogida = 1f; // Radio de recogida de exp

    [Header("Regeneración")]
    public float regeneracionPorSegundo = 0f;

    [Header("Resistencias")]
    public float armadura = 0f; // Reduce daño recibido
    public float evasion = 0f; // Probabilidad de esquivar (0-1)

    // Modificadores temporales
    private float modificadorVelocidad = 1f;
    private float modificadorDaño = 1f;
    private bool invulnerable = false;

    // Referencias
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Eventos
    public System.Action<float, float> OnVidaCambiada;
    public System.Action OnMuerte;

    void Start()
    {
        vidaActual = vidaMaxima;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (regeneracionPorSegundo > 0)
        {
            StartCoroutine(RegenerarVida());
        }

        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);
    }

    IEnumerator RegenerarVida()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (vidaActual < vidaMaxima)
            {
                Curar(regeneracionPorSegundo);
            }
        }
    }

    public void RecibirDaño(float cantidad)
    {
        if (invulnerable) return;

        // Aplicar evasión
        if (Random.Range(0f, 1f) <= evasion)
        {
            Debug.Log("¡Esquivado!");
            return;
        }

        // Aplicar armadura
        float dañoFinal = cantidad * (100f / (100f + armadura));

        vidaActual -= dañoFinal;
        vidaActual = Mathf.Max(0, vidaActual);

        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);

        StartCoroutine(EfectoGolpe());

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    public void Curar(float cantidad)
    {
        vidaActual += cantidad;
        vidaActual = Mathf.Min(vidaActual, vidaMaxima);

        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);
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

    void Morir()
    {
        Debug.Log("¡Jugador ha muerto!");
        OnMuerte?.Invoke();

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.GameOver();
        }
    }

    // Modificadores
    public void ModificarVidaMaxima(float cantidad)
    {
        vidaMaxima += cantidad;
        vidaActual += cantidad;
        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);
    }

    public void ModificarVelocidad(float cantidad)
    {
        velocidadMovimiento += cantidad;
    }

    public void ModificarDañoBase(float cantidad)
    {
        dañoBase += cantidad;
    }

    public void ModificarArmadura(float cantidad)
    {
        armadura += cantidad;
    }

    public void ModificarEvasion(float cantidad)
    {
        evasion = Mathf.Clamp01(evasion + cantidad);
    }

    public void ActivarInvulnerabilidad(float duracion)
    {
        StartCoroutine(InvulnerabilidadTemporal(duracion));
    }

    IEnumerator InvulnerabilidadTemporal(float duracion)
    {
        invulnerable = true;
        yield return new WaitForSeconds(duracion);
        invulnerable = false;
    }

    // Getters
    public float ObtenerVidaActual() { return vidaActual; }
    public float ObtenerVidaMaxima() { return vidaMaxima; }
    public float ObtenerVelocidadTotal() { return velocidadMovimiento * modificadorVelocidad; }
    public float ObtenerDañoTotal() { return dañoBase * modificadorDaño; }
}
