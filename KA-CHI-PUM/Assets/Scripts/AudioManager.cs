using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    // Singleton
    public static AudioManager instance;

    [Header("═══ MÚSICA ═══")]
    [SerializeField] private AudioClip musicaLobby;
    [SerializeField] private AudioClip musicaNivel1;
    [SerializeField] private AudioClip musicaGameOver; // Opcional
    
    [Header("═══ CONFIGURACIÓN ═══")]
    [Range(0f, 1f)]
    [SerializeField] private float volumenMusica = 0.5f;
    [SerializeField] private bool fadeEntreCanciones = true;
    [SerializeField] private float tiempoFade = 1f;

    [Header("═══ EFECTOS DE SONIDO (Opcional) ═══")]
    [SerializeField] private AudioClip sonidoBoton;
    [SerializeField] private AudioClip sonidoInicio;
    [Range(0f, 1f)]
    [SerializeField] private float volumenEfectos = 0.7f;

    private AudioSource audioSourceMusica;
    private AudioSource audioSourceEfectos;
    private string escenaActual;

    private void Awake()
    {
        // Implementar Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // NO destruir entre escenas
            InicializarAudioSources();
        }
        else
        {
            Destroy(gameObject); // Si ya existe otro AudioManager, destruir este
            return;
        }
    }

    private void Start()
    {
        // Subscribirse al evento de cambio de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Reproducir música de la escena inicial
        escenaActual = SceneManager.GetActiveScene().name;
        ReproducirMusicaDeEscena(escenaActual);
    }

    private void OnDestroy()
    {
        // Desubscribirse del evento
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InicializarAudioSources()
    {
        // AudioSource para música
        audioSourceMusica = gameObject.AddComponent<AudioSource>();
        audioSourceMusica.loop = true;
        audioSourceMusica.playOnAwake = false;
        audioSourceMusica.volume = volumenMusica;

        // AudioSource para efectos
        audioSourceEfectos = gameObject.AddComponent<AudioSource>();
        audioSourceEfectos.loop = false;
        audioSourceEfectos.playOnAwake = false;
        audioSourceEfectos.volume = volumenEfectos;

        Debug.Log("AudioManager inicializado correctamente");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string nombreEscena = scene.name;
        
        // Solo cambiar música si es una escena diferente
        if (nombreEscena != escenaActual)
        {
            escenaActual = nombreEscena;
            ReproducirMusicaDeEscena(nombreEscena);
        }
    }

    private void ReproducirMusicaDeEscena(string nombreEscena)
    {
        AudioClip musicaNueva = null;

        // Determinar qué música reproducir según la escena
        switch (nombreEscena)
        {
            case "MenuPrincipal":
            case "Lobby":
            case "Menu":
                musicaNueva = musicaLobby;
                break;

            case "Nivel_1":
            case "Nivel1":
            case "Game":
                musicaNueva = musicaNivel1;
                break;

            case "GameOver":
                musicaNueva = musicaGameOver;
                break;

            default:
                Debug.LogWarning($"No hay música asignada para la escena: {nombreEscena}");
                return;
        }

        // Reproducir la música nueva
        if (musicaNueva != null)
        {
            if (fadeEntreCanciones)
            {
                StartCoroutine(CambiarMusicaConFade(musicaNueva));
            }
            else
            {
                CambiarMusicaDirecta(musicaNueva);
            }
        }
    }

    private void CambiarMusicaDirecta(AudioClip nuevaMusica)
    {
        if (audioSourceMusica.clip == nuevaMusica && audioSourceMusica.isPlaying)
        {
            // Ya está sonando esta música
            return;
        }

        audioSourceMusica.Stop();
        audioSourceMusica.clip = nuevaMusica;
        audioSourceMusica.Play();

        Debug.Log($"Reproduciendo: {nuevaMusica.name}");
    }

    private IEnumerator CambiarMusicaConFade(AudioClip nuevaMusica)
    {
        // Si ya está sonando esta música, no hacer nada
        if (audioSourceMusica.clip == nuevaMusica && audioSourceMusica.isPlaying)
        {
            yield break;
        }

        // Fade Out (bajar volumen)
        float volumenInicial = audioSourceMusica.volume;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoFade)
        {
            tiempoTranscurrido += Time.deltaTime;
            audioSourceMusica.volume = Mathf.Lerp(volumenInicial, 0f, tiempoTranscurrido / tiempoFade);
            yield return null;
        }

        // Cambiar la canción
        audioSourceMusica.Stop();
        audioSourceMusica.clip = nuevaMusica;
        audioSourceMusica.Play();

        Debug.Log($"Reproduciendo con fade: {nuevaMusica.name}");

        // Fade In (subir volumen)
        tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoFade)
        {
            tiempoTranscurrido += Time.deltaTime;
            audioSourceMusica.volume = Mathf.Lerp(0f, volumenMusica, tiempoTranscurrido / tiempoFade);
            yield return null;
        }

        audioSourceMusica.volume = volumenMusica;
    }

    // ═══ MÉTODOS PÚBLICOS ═══

    public void ReproducirEfecto(AudioClip efecto)
    {
        if (efecto != null && audioSourceEfectos != null)
        {
            audioSourceEfectos.PlayOneShot(efecto, volumenEfectos);
        }
    }

    public void ReproducirSonidoBoton()
    {
        ReproducirEfecto(sonidoBoton);
    }

    public void ReproducirSonidoInicio()
    {
        ReproducirEfecto(sonidoInicio);
    }

    public void CambiarVolumenMusica(float nuevoVolumen)
    {
        volumenMusica = Mathf.Clamp01(nuevoVolumen);
        audioSourceMusica.volume = volumenMusica;
    }

    public void CambiarVolumenEfectos(float nuevoVolumen)
    {
        volumenEfectos = Mathf.Clamp01(nuevoVolumen);
        audioSourceEfectos.volume = volumenEfectos;
    }

    public void MutearMusica(bool mutear)
    {
        audioSourceMusica.mute = mutear;
    }

    public void MutearEfectos(bool mutear)
    {
        audioSourceEfectos.mute = mutear;
    }

    public void PausarMusica()
    {
        audioSourceMusica.Pause();
    }

    public void ReanudarMusica()
    {
        audioSourceMusica.UnPause();
    }

    public void DetenerMusica()
    {
        audioSourceMusica.Stop();
    }
}
