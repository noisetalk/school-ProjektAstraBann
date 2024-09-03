using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEditor;

public class SettingsMenu : MonoBehaviour
{
    private InputActionAsset input;
    private bool settingsMenuOpen = false;
   
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private Slider brightnessBar;
    [SerializeField] private Slider volumeBar;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Volume postProcessProfile;
    [SerializeField] private ColorAdjustments colorAdjustments;
    
    void Start()
    {
        postProcessProfile.profile.TryGet(out colorAdjustments);
        input = new InputActionAsset();
        input.Enable();
        // input.Settings.performed += Settings_performed;
    }

    private void Settings_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Cancel(!settingsMenuOpen);
    }
    
    public void AdjustBrightness()
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = (brightnessBar.value);
        }
    }

    public void AdjustVolume()
    {
        audioMixer.SetFloat("Master", volumeBar.value);
    }
    
    public void Cancel(bool setMenuOpen)
    {
        settingsMenuOpen = setMenuOpen;
        settingsMenu.SetActive(settingsMenuOpen);
        Time.timeScale = settingsMenuOpen ? 0f : 1f;
    }
    
    public void ExitGame()
    {
        Application.Quit();
    }
    
}
