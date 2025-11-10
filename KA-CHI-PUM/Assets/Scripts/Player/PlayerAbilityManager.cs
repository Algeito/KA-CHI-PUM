using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================== MANAGER DE HABILIDADES DEL JUGADOR ====================
public class PlayerAbilityManager : MonoBehaviour
{
    [Header("Habilidades Iniciales")]
    public bool iniciarConKA = true;
    public bool iniciarConCHI = false;
    public bool iniciarConPUM = false;

    [Header("Prefabs de Habilidades")]
    public GameObject prefabHabilidadKA;
    public GameObject prefabHabilidadCHI;
    public GameObject prefabHabilidadPUM;

    [Header("Prefabs de Proyectiles/Efectos")]
    public GameObject prefabProyectilKA;
    public GameObject prefabAreaCHI;
    public GameObject prefabOrbitalPUM;

    private List<HabilidadBase> habilidadesActivas = new List<HabilidadBase>();

    void Start()
    {
        if (iniciarConKA) ActivarHabilidad(TipoHabilidad.KA);
        if (iniciarConCHI) ActivarHabilidad(TipoHabilidad.CHI);
        if (iniciarConPUM) ActivarHabilidad(TipoHabilidad.PUM);
    }

    public void ActivarHabilidad(TipoHabilidad tipo)
    {
        // Verificar si ya tiene la habilidad
        if (TieneHabilidad(tipo))
        {
            Debug.Log($"Ya tienes la habilidad {tipo}");
            return;
        }

        GameObject prefab = null;
        GameObject prefabEfecto = null;

        switch (tipo)
        {
            case TipoHabilidad.KA:
                prefab = prefabHabilidadKA;
                prefabEfecto = prefabProyectilKA;
                break;
            case TipoHabilidad.CHI:
                prefab = prefabHabilidadCHI;
                prefabEfecto = prefabAreaCHI;
                break;
            case TipoHabilidad.PUM:
                prefab = prefabHabilidadPUM;
                prefabEfecto = prefabOrbitalPUM;
                break;
        }

        if (prefab != null)
        {
            GameObject habilidadObj = Instantiate(prefab, transform);
            HabilidadBase habilidad = habilidadObj.GetComponent<HabilidadBase>();

            if (habilidad != null)
            {
                // Configurar el prefab de efecto
                if (habilidad is HabilidadKA && prefabEfecto != null)
                {
                    ((HabilidadKA)habilidad).prefabProyectil = prefabEfecto;
                }
                else if (habilidad is HabilidadCHI && prefabEfecto != null)
                {
                    ((HabilidadCHI)habilidad).prefabAreaEfecto = prefabEfecto;
                }
                else if (habilidad is HabilidadPUM && prefabEfecto != null)
                {
                    ((HabilidadPUM)habilidad).prefabOrbital = prefabEfecto;
                }

                habilidadesActivas.Add(habilidad);
                Debug.Log($"¡Habilidad {tipo} activada!");
            }
        }
    }

    public bool TieneHabilidad(TipoHabilidad tipo)
    {
        foreach (HabilidadBase hab in habilidadesActivas)
        {
            if (hab.ObtenerDatos().tipo == tipo)
            {
                return true;
            }
        }
        return false;
    }

    public HabilidadBase ObtenerHabilidad(TipoHabilidad tipo)
    {
        foreach (HabilidadBase hab in habilidadesActivas)
        {
            if (hab.ObtenerDatos().tipo == tipo)
            {
                return hab;
            }
        }
        return null;
    }

    public List<HabilidadBase> ObtenerTodasLasHabilidades()
    {
        return habilidadesActivas;
    }
}
