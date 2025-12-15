using UnityEngine;
using UnityEngine.UI;

public class BarraVidaEnemigo : MonoBehaviour
{
    [Header("Referencias")]
    public Image barraVidaFill;
    public Canvas canvas;

    [Header("Configuración")]
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Offset sobre el enemigo
    public float tiempoVisible = 3f; // Tiempo que permanece visible después de recibir daño
    public bool siempreVisible = false;

    [Header("Colores")]
    public Color colorVidaAlta = Color.green;
    public Color colorVidaMedia = Color.yellow;
    public Color colorVidaBaja = Color.red;

    private Enemigo enemigo;
    private float tiempoRestanteVisible;
    private Camera camaraMain;
    private RectTransform rectTransform;

    void Start()
    {
        enemigo = GetComponentInParent<Enemigo>();
        if (enemigo == null)
        {
            Debug.LogError("BarraVidaEnemigo: No se encontró el componente Enemigo en el padre");
            return;
        }

        camaraMain = Camera.main;

        if (canvas != null)
        {
            canvas.worldCamera = camaraMain;
            rectTransform = canvas.GetComponent<RectTransform>();
        }

        // Ocultar al inicio si no es siempre visible
        if (!siempreVisible && canvas != null)
        {
            canvas.enabled = false;
        }

        ActualizarBarra();
    }

    void Update()
    {
        if (enemigo == null) return;

        // Actualizar barra
        ActualizarBarra();

        // Controlar visibilidad
        if (!siempreVisible)
        {
            if (tiempoRestanteVisible > 0)
            {
                tiempoRestanteVisible -= Time.deltaTime;
                if (canvas != null && !canvas.enabled)
                {
                    canvas.enabled = true;
                }
            }
            else
            {
                if (canvas != null && canvas.enabled)
                {
                    canvas.enabled = false;
                }
            }
        }

        // Hacer que la barra siempre mire a la cámara
        if (canvas != null && camaraMain != null)
        {
            canvas.transform.LookAt(canvas.transform.position + camaraMain.transform.rotation * Vector3.forward,
                camaraMain.transform.rotation * Vector3.up);
        }
    }

    void ActualizarBarra()
    {
        if (barraVidaFill == null || enemigo == null) return;

        float vidaActual = enemigo.ObtenerVidaActual();
        float vidaMaxima = enemigo.ObtenerVidaMaxima();
        float porcentaje = vidaActual / vidaMaxima;

        // Actualizar fill
        barraVidaFill.fillAmount = porcentaje;

        // Cambiar color según porcentaje
        if (porcentaje > 0.6f)
            barraVidaFill.color = colorVidaAlta;
        else if (porcentaje > 0.3f)
            barraVidaFill.color = colorVidaMedia;
        else
            barraVidaFill.color = colorVidaBaja;
    }

    public void MostrarBarra()
    {
        tiempoRestanteVisible = tiempoVisible;
        if (canvas != null && !siempreVisible)
        {
            canvas.enabled = true;
        }
    }

    public void OcultarBarra()
    {
        if (canvas != null && !siempreVisible)
        {
            canvas.enabled = false;
        }
    }
}
