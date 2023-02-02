using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class OptionElement : MonoBehaviour
{
    /// <summary>
    /// Text mesh pro reference of the option display text, only used to clear
    /// </summary>
    [SerializeField] private TMP_Text optionText;

    /// <summary>
    /// Localized string event of the option text
    /// </summary>
    [SerializeField] private LocalizeStringEvent optionEvent;

    /// <summary>
    /// Locally injected manager for the reference to send off OnOptionClicked
    /// </summary>
    [HideInInspector] public QuizManager manager;

    public AudibleOption Option
    {
        get => _option;
        set
        {
            optionEvent.StringReference = value.OptionAnswer;
            _option = value;
        }
    }

    private AudibleOption _option;

    public void OnClick()
    {
        GetComponent<Button>().interactable = false;
        manager.OnOptionClicked(this);
    }

    public void UpdateOptionText() => optionEvent.StringReference = Option.OptionResponse;

    public void RevertOptionText() => optionEvent.StringReference = Option.OptionAnswer;

    public void ClearText() => optionText.text = string.Empty;
}