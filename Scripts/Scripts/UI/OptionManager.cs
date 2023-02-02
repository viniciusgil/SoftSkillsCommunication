using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class OptionManager : MonoBehaviour
{
    [SerializeField] private TMP_Text optionDisplay;

    private int current = 0;
    private int optionsCount;

    private void Awake()
    {
        optionsCount = LocalizationSettings.AvailableLocales.Locales.Count;

        optionDisplay.text = LocalizationSettings.SelectedLocale.LocaleName;
    }

    public void OnButtonClicked(int value)
    {
        current = Mathf.Clamp(current + value, 0, optionsCount);

        UpdateOptionDisplay();
    }

    private void UpdateOptionDisplay()
    {
        Locale curLoc = LocalizationSettings.AvailableLocales.Locales[current];

        LocalizationSettings.SelectedLocale = curLoc;

        optionDisplay.text = curLoc.LocaleName;
    }
}
