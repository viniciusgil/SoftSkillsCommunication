using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.SceneManagement;
using TMPro;

public class Question : MonoBehaviour
{
    public int count = 0;
    public int score = 0;
    public List<GameObject>panels;
    public int panel = 0;
    public TextMeshProUGUI scoreText;
    public AudioSource audio;

    
    
    // Start is called before the first frame update
    void Start()
    {
       // panelswitch();  
    }

    //Update is called once per frame
    void Update()
    {
        UpdateScore();
    }

    private void UpdateScore()
    {
        scoreText.text = "Final Score = " + score.ToString();
    }
    public void correct(string response)
    {
        if (count == 0)
        {
            score += 25;
        }
        else if (count == 1)
        {
            score += 15;
        }
        else if (count == 2)
        {
            score += 0;
        }
        count = 0;

        if (panel > panels.Count)
        {
            //Questions over.
        }
        else
        {
            panelswitch();
        }




    }

    public void DelayedChangeAfterAudio()
    {
        audio.Play();
        Invoke("panelswitch", audio.clip.length);

    }

    public void uncorrect(string response)
    {
        count++;

    }
    public void panelswitch()
    {
        panel++;
        foreach (GameObject GO in panels) 
        {
            GO.SetActive(false);
        }
        panels[panel].SetActive(true);
    }
   
}
