using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Звуковые эффекты (SFX)")]
    [SerializeField] private AudioClip chopClip;       // Нарезка
    [SerializeField] private AudioClip successClip;    // Успех
    [SerializeField] private AudioClip failClip;       // Провал (тайм-аут)
    [SerializeField] private AudioClip warningClip;    // Сирена подгорания
    [SerializeField] private AudioClip clickClip;      // Клик кнопок
    [SerializeField] private AudioClip interactClip;   // Взять/положить (Е)

    [Header("Музыкальные треки для сцен")]
    [SerializeField] private AudioClip menuMusicClip;      // Музыка в Главном меню и 3D-магазине
    [SerializeField] private AudioClip tutorialMusicClip;  // Музыка в Обучении
    [SerializeField] private List<AudioClip> levelMusicClips; // Музыка для Уровней 1-5

    [Header("Индивидуальная громкость эффектов")]
    [Range(0f, 1f)][SerializeField] private float chopVolume = 1.0f;
    [Range(0f, 1f)][SerializeField] private float successVolume = 0.6f;
    [Range(0f, 1f)][SerializeField] private float failVolume = 0.6f;
    [Range(0f, 1f)][SerializeField] private float warningVolume = 0.8f;
    [Range(0f, 1f)][SerializeField] private float clickVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float interactVolume = 0.7f;

    [Header("Громкость фоновой музыки")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.4f;

    private AudioSource sfxAudioSource;   // Источник для кликов/звуков
    private AudioSource musicAudioSource; // Источник для фоновой музыки

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Создаем два раздельных источника звука на объекте
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
            musicAudioSource = gameObject.AddComponent<AudioSource>();

            musicAudioSource.loop = true; // Музыка всегда играет по кругу
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name, scene.buildIndex);
    }

    private void PlayMusicForScene(string sceneName, int buildIndex)
    {
        if (musicAudioSource == null) return;

        AudioClip targetClip = null;

        if (sceneName == "MainMenuScene" || sceneName == "Shop3DScene")
        {
            targetClip = menuMusicClip;
        }
        else if (sceneName == "TutorialScene")
        {
            targetClip = tutorialMusicClip;
        }
        else if (buildIndex >= 1 && buildIndex <= 5) // Игровые уровни 1-5
        {
            int levelIndex = buildIndex - 1; // Уровень 1 (индекс 1) -> элемент 0 в списке
            if (levelIndex < levelMusicClips.Count)
            {
                targetClip = levelMusicClips[levelIndex];
            }
        }

        // Проигрываем музыку только если она реально сменилась
        if (targetClip != null && musicAudioSource.clip != targetClip)
        {
            musicAudioSource.clip = targetClip;
            musicAudioSource.volume = musicVolume;
            musicAudioSource.Play();
        }
    }

    // Метод остановки музыки (вызывается из LevelManager при конце игры)
    public void StopMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
        }
    }

    // --- ПРОИГРЫВАНИЕ ЗВУКОВЫХ ЭФФЕКТОВ (SFX) ---
    public void PlayChopSound(Vector3 position)
    {
        if (chopClip != null) AudioSource.PlayClipAtPoint(chopClip, position, chopVolume);
    }

    public void PlayWarningSound(Vector3 position)
    {
        if (warningClip != null) AudioSource.PlayClipAtPoint(warningClip, position, warningVolume);
    }

    public void PlayInteractSound(Vector3 position)
    {
        if (interactClip != null) AudioSource.PlayClipAtPoint(interactClip, position, interactVolume);
    }

    public void PlayClickSound()
    {
        if (clickClip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clickClip, clickVolume);
        }
    }

    public void PlaySuccessSound()
    {
        if (successClip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(successClip, successVolume);
        }
    }

    public void PlayFailSound()
    {
        if (failClip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(failClip, failVolume);
        }
    }
}