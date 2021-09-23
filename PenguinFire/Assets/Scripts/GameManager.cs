using TMPro;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject indicatorPrefab;
    public TextMeshProUGUI ammoText;
    public GameObject firemodePrefabManager;
    public bool gameIsPaused = false;
    public GameObject[] pauseMenu;
    public TMP_Dropdown resolutionDropdown, qualitySettingsDropdown, VsyncDropdown;
    public Slider masterVolumeSlider, frameRateSlider;
    public TextMeshProUGUI frameRateText;
    public Toggle fullscreenToggle;
    public TMP_InputField fpsInputField;
    private Resolution[] resolutions;
    public AudioMixer audioMixer;

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    void Start()
    {
        instance = this;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            
            if(resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height){
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        //set resoulution from playerprefs
        int qualityIndex = PlayerPrefs.GetInt("QualityLevel");
        QualitySettings.SetQualityLevel(qualityIndex);
        qualitySettingsDropdown.value = qualityIndex;
        qualitySettingsDropdown.RefreshShownValue();

        //set framerate from playerprefs
        int rate = PlayerPrefs.GetInt("FrameRate");
        frameRateSlider.value = rate;
        Application.targetFrameRate = rate;

        //set resolution from playerprefs
        Resolution resolution = resolutions[PlayerPrefs.GetInt("resolution")];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        resolutionDropdown.value = PlayerPrefs.GetInt("resolution");
        resolutionDropdown.RefreshShownValue();


        //set Vsync from playerprefs
        QualitySettings.vSyncCount = PlayerPrefs.GetInt("Vsync");
        VsyncDropdown.value = PlayerPrefs.GetInt("Vsync");
        VsyncDropdown.RefreshShownValue();

        //set fullscreen from playerPrefs
        bool fllscrn = Convert.ToBoolean(PlayerPrefs.GetInt("fullscrn"));
        Screen.fullScreen = fllscrn;
        fullscreenToggle.isOn = fllscrn;

        //set master volume
        float volume = PlayerPrefs.GetFloat("volume");
        audioMixer.SetFloat("Volume", volume);
        masterVolumeSlider.value = volume;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameIsPaused = !gameIsPaused;
            if (gameIsPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    public void ResumeGame()
    {
        gameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        for (int i = 0; i < pauseMenu.Length; i++)
        {
            pauseMenu[i].SetActive(false);
        }
    }

    void PauseGame()
    {
        gameIsPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        pauseMenu[0].SetActive(true);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
        PlayerPrefs.SetFloat("volume", volume);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
    }

    public void SetFullScreen(bool screen)
    {
        Screen.fullScreen = screen;
        PlayerPrefs.SetInt("fullscrn", Convert.ToInt32(screen));
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];

        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        PlayerPrefs.SetInt("resolution", resolutionIndex);
    }

    public void QuitToWindows()
    {
        Application.Quit();
    }

    public void SetFramRate(float rate)
    {
        Application.targetFrameRate = (int)rate;
        frameRateText.text = rate.ToString("0");
        fpsInputField.text = rate.ToString("0");
        PlayerPrefs.SetInt("FrameRate", (int)rate);
    }

    public void SetVsync(int count)
    {
        QualitySettings.vSyncCount = count;

        PlayerPrefs.SetInt("Vsync", count);
    }

    public void SetFPSInputField(string rate)
    {
        int framerate = Convert.ToInt32(rate);
        if (framerate > 300) return;
        if (framerate < 20) return;
        Application.targetFrameRate = framerate;
        frameRateText.text = framerate.ToString("0");
        frameRateSlider.value = framerate;
        PlayerPrefs.SetInt("FrameRate", framerate);
    }

   
    public void Indicator(string message)
    {
        GameObject indicator = Instantiate(indicatorPrefab, Vector3.zero, Quaternion.identity, GameManager.instance.firemodePrefabManager.transform) as GameObject;
        indicator.GetComponent<TMPro.TextMeshProUGUI>().text = message;
    }
}
