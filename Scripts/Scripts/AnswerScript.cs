using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Rendering;

public class AnswerScript : MonoBehaviour
{
    public bool isCorrect = false;

    public string response;

    public Question question;

    public TextMeshProUGUI textMeshProUGUI;

    public AudioClip audioresponse;

    public AudioSource audio;

 

    public void DelayedChangeAfterAudio()
     {
         audio.Play();
         Invoke("panelswitch", audio.clip.length);

     }
    

     void Start()
    {
        
    }

    IEnumerator PlayAnswerAndChangeText()
     {
        Debug.Log("ChangeText " + Time.time);

        yield return new WaitForSeconds(3f);
        Debug.Log("");

     }
    

    /*float _LastPressTime;
    float _PressDelay = 3f;

    public void OnButtonPress()
    {
        if (_LastPressTime + _PressDelay > Time.deltaTime)
            return;
        _LastPressTime = Time.deltaTime;
        Debug.Log("OnButtonPress");
    }*/
    public void Answer()
    {
        textMeshProUGUI.text= response;

        if(isCorrect)
        {
            //Debug.Log("Good choice");
            question.correct(response);
        }
        else
        {
            //Debug.Log("Try again");
            question.uncorrect(response);
        }
    }
}

