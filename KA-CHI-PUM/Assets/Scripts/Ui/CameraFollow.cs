using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================== CÁMARA QUE SIGUE AL JUGADOR ====================
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform objetivo;

    [Header("Suavizado")]
    public float suavizado = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Límites (Opcional)")]
    public bool usarLimites = false;
    public float limiteIzquierda = -50f;
    public float limiteDerecha = 50f;
    public float limiteArriba = 50f;
    public float limiteAbajo = -50f;

    void Start()
    {
        if (objetivo == null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
            {
                objetivo = jugador.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        Vector3 posicionDeseada = objetivo.position + offset;

        if (usarLimites)
        {
            posicionDeseada.x = Mathf.Clamp(posicionDeseada.x, limiteIzquierda, limiteDerecha);
            posicionDeseada.y = Mathf.Clamp(posicionDeseada.y, limiteAbajo, limiteArriba);
        }

        Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, suavizado);
        transform.position = posicionSuavizada;
    }
}
