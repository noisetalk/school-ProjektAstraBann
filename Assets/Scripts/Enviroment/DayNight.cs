using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DayNight : MonoBehaviour
{
    // Zyklus-Geschwindigkeit
    public float timeScale = 10f;
    public TMP_Text DisplayTimeText;
    public Camera mainCamera;
    public Vector3 camPo; 

    // 0 = Nacht, 1 = Day
    private float _timeOfDay;
    
    void Start()
    {
        StartDay();
        camPo = mainCamera.transform.localPosition;
    }
    void Update()
    {
        // 24-Stunden-Zyklus
        _timeOfDay += Time.deltaTime / timeScale / 24f; 
        _timeOfDay %= 1;     // sorgt dafür dass die Variable im Bereich 0 bis 1 bleibt
        transform.localRotation = Quaternion.Euler((_timeOfDay * 360f) - 90, -30, 0);
        // Add minor movement to camera to simulate breathing
        mainCamera.transform.localPosition = new Vector3(camPo.x, ((camPo.y) + (Mathf.Sin(_timeOfDay * 16 * Mathf.PI) * 0.1f)), camPo.z);
        
        // damit _timeOfDay nicht negativ wird
        if(_timeOfDay < 0)
        {
            _timeOfDay += 1;
        }
        DisplayTime();
    }
    public void StartDay()
    {
        // Wert für den Beginn des Tages (24 Stunden * 0.25 = 6:00 Uhr)
        _timeOfDay = 0.25f;
    }

    public void StartNight()
    {
        // Wert für Tagesende (24h * 0.75 = 18:00 Uhr)
        _timeOfDay = 0.75f;
    }
    public void DisplayTime()
    {   
        // Konvertierung von _timeOfDay zu Stunden und Minuten
        float timeConvHours = 24 * _timeOfDay;
        int hours = Mathf.FloorToInt(timeConvHours);
        int minutes = Mathf.FloorToInt((timeConvHours * 60) % 60);
        
	    // Output in designiertes Text mit Formatierung
        DisplayTimeText.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }
}