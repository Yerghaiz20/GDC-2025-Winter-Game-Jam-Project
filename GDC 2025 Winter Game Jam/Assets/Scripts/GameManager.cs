using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Ships / Gameplay")]
    public int shipsRemaining = 5;
    public int currentShips;

    [Header("Score / Stats")]
    public float score;

    [Header("Music")]
    public AudioSource bgmSource;
    public AudioClip[] musicTracks; // 0: Start/Story, 1: Gameplay, 2: Ending, 3: GameOver

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Ship Registration
    public void RegisterShip()
    {
        currentShips++;
    }

    public void UnregisterShip()
    {
        currentShips--;
    }
    #endregion

    #region Scene Music
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Determine which track to play based on scene name
        AudioClip desiredClip = null;

        switch (scene.name)
        {
            case "Start Screen":
            case "OpeningStory":
                desiredClip = musicTracks.Length > 0 ? musicTracks[0] : null;
                break;

            case "Stage1":
            case "Stage2":
            case "Stage3":
                desiredClip = musicTracks.Length > 1 ? musicTracks[1] : null;
                break;

            case "Ending":
                desiredClip = musicTracks.Length > 2 ? musicTracks[2] : null;
                break;

            case "GameOver":
                desiredClip = musicTracks.Length > 3 ? musicTracks[3] : null;
                break;

            default:
                desiredClip = null;
                break;
        }

        if (desiredClip != null && bgmSource.clip != desiredClip)
        {
            bgmSource.clip = desiredClip;
            bgmSource.Play();
        }
    }
    #endregion

    #region Scene Results
    public void ApplySceneResults(int remainingShips)
    {
        shipsRemaining = remainingShips;
    }
    #endregion
}
