using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class Crosshair : MonoBehaviour
{
    public float thickness = 4f;
    public float length = 15f;
    public RectTransform[] verticalReticles, horizontalReticles;
    public Image[] reticles;
    public Slider thicknessSlider, lengthSlider, gapSlider, centreDotSize, R, G, B;
    public static Crosshair instance;
    public float spreadValue;
    public RectTransform crosshair, centreDot;
    public Toggle centreDotToggle;
    bool isDynamic = true;
    public TMP_Dropdown crosshairType;
    private void Start()
    {
        instance = this;
    }

    private void OnEnable()
    {
        float val = PlayerPrefs.GetInt("isDynamic");
        if (val == 0)
        {
            isDynamic = true;
            crosshairType.value = 0;
        }
        if (val == 1)
        {
            isDynamic = false;
            crosshairType.value = 1;
        }

        thicknessSlider.value = PlayerPrefs.GetFloat("crosshairThickness");
        lengthSlider.value = PlayerPrefs.GetFloat("crosshairLength");
        gapSlider.value = PlayerPrefs.GetFloat("crosshairGap");
        centreDotSize.value = PlayerPrefs.GetFloat("crosshairDot");

        R.value = PlayerPrefs.GetFloat("crosshairColorR");
        G.value = PlayerPrefs.GetFloat("crosshairColorG");
        B.value = PlayerPrefs.GetFloat("crosshairColorB");

        if (PlayerPrefs.GetInt("crosshairColorB") == 0)
            centreDotToggle.isOn = true;
        else
            centreDotToggle.isOn = false;
    }
    void Update()
    {
        thickness = thicknessSlider.value;
        length = lengthSlider.value;
        for (int i = 0; i < reticles.Length; i++)
        {
            reticles[i].color = new Color(R.value, G.value, B.value);
        }
        centreDot.sizeDelta = new Vector2(centreDotSize.value, centreDotSize.value);
        if (isDynamic)
            crosshair.sizeDelta = Vector2.Lerp(crosshair.sizeDelta, new Vector2(36f + spreadValue + gapSlider.value, 36f + spreadValue + gapSlider.value), 5f * Time.smoothDeltaTime);
        else
            crosshair.sizeDelta = Vector2.Lerp(crosshair.sizeDelta, new Vector2(36f + gapSlider.value, 36f + gapSlider.value), 5f * Time.smoothDeltaTime);
        
        
        for (int i = 0; i < verticalReticles.Length; i++)
        {
            verticalReticles[i].localScale = new Vector3(thickness, length, 0f);
        }

        for (int i = 0; i < horizontalReticles.Length; i++)
        {
            horizontalReticles[i].localScale = new Vector3(length, thickness, 0f);
        }
    }
    private void OnDisable()
    {
        PlayerPrefs.SetFloat("crosshairThickness", thicknessSlider.value);
        PlayerPrefs.SetFloat("crosshairLength", lengthSlider.value);
        PlayerPrefs.SetFloat("crosshairGap", gapSlider.value);
        PlayerPrefs.SetFloat("crosshairDot", centreDotSize.value);

        PlayerPrefs.SetFloat("crosshairColorR", R.value);
        PlayerPrefs.SetFloat("crosshairColorG", G.value);
        PlayerPrefs.SetFloat("crosshairColorB", B.value);

        if (centreDotToggle.isOn)
            PlayerPrefs.SetInt("crosshairColorB", 0);
        else
            PlayerPrefs.SetInt("crosshairColorB", 1);


    }

    public void ChooseCrossHairType(int val)
    {
        if(val == 0)
        {
            isDynamic = true;
            PlayerPrefs.SetInt("isDynamic", 0);
        }
        if(val == 1)
        {
            isDynamic = false;
            PlayerPrefs.SetInt("isDynamic", 1);
        }
    }
}
