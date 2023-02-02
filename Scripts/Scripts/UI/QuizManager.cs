using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Random = System.Random;

public class QuizManager : MonoBehaviour
{
    private enum UIState
    {
        Intro,
        Quiz,
        End
    }
    
    [HideInInspector] public int Score;
    
    //TODO: Tidy up this awful mess
    /// <summary>
    /// option UI element prefab
    /// </summary>
    [Header("UI Elements")]
    [SerializeField] private GameObject optionPrefab;
    
    /// <summary>
    /// Option container (in game UI to hold the option boxes)
    /// </summary>
    [SerializeField] private GameObject optionContainer;
    
    /// <summary>
    /// Canvas group that controls the intro 
    /// </summary>
    [SerializeField] private GameObject introCanvas;
    
    /// <summary>
    /// Canvas group that controls the quiz
    /// </summary>
    [SerializeField] private GameObject quizCanvas;
    
    /// <summary>
    /// Canvas group that controls the end
    /// </summary>
    [SerializeField] private GameObject endCanvas;
    
    /// <summary>
    /// The main scriptable object that is responsible for the quiz data.
    /// This means you can switch it out and it will still work fine.
    /// </summary>
    [SerializeField] private QuestionDataObject activeQuiz;

    [SerializeField] private Button startButton;

    /// <summary>
    /// The restart button
    /// </summary>
    [SerializeField] private Button restartButton;
    
    /// <summary>
    /// The exit button
    /// </summary>
    [SerializeField] private Button exitButton;
    
    /// <summary>
    /// Localized string event for the Intro text mesh pro
    /// </summary>
    [Header("Localization")]
    [SerializeField] private LocalizeStringEvent introEvent;
    
    /// <summary>
    /// Localized string event for the question text mesh pro (used but not needed?)
    /// </summary>
    [SerializeField] private LocalizeStringEvent questionEvent;
    
    /// <summary>
    /// Localized string event for the end text
    /// </summary>
    [SerializeField] private LocalizeStringEvent endEvent;
    
    /// <summary>
    /// Localized string event for the score text
    /// </summary>
    [SerializeField] private LocalizeStringEvent scoreEvent;
    [SerializeField] private LocalizedString localizedScore;

    /// <summary>
    /// The audio source that plays the audio clips
    /// </summary>
    [Header("Audio")] 
    [SerializeField] private AudioSource targetAudioSource;
    [SerializeField] private LocalizeAudioClipEvent audioClipEvent;

    [SerializeField] private LocalizedAudioClip instructionMessage;
    [SerializeField] private LocalizedAudioClip[] introSequence;
    
    /// <summary>
    /// The message played at the end of the quiz/conversation/discussion
    /// </summary>
    [SerializeField] private LocalizedAudioClip endOfQuizMessage;
    
    /// <summary>
    /// End of the application message
    /// </summary>
    [SerializeField] private LocalizedAudioClip endOfAppMessage;
    
    /// <summary>
    /// Time between playing the next clip
    /// </summary>
    [SerializeField] private float timeBetweenClips = 0.5f;
    
    private QuestionEntry currentQuestion;
    private List<OptionElement> constructedOptions;

    private UIState State
    {
        set
        {
            switch (value)
            {
                case UIState.Quiz:
                    UpdateQuizUI();
                    break;
                case UIState.End:
                    UpdateEndUI();
                    break;
            }
            
            state = value;
            SwitchCanvases();
        }
    }
    
    private UIState state;
    private int incorrectCount;
    private int maxScore;

    private void Awake()
    {
        if (targetAudioSource.isPlaying)
        {
            targetAudioSource.Stop();
        }
        
        State = UIState.Intro;
        introEvent.StringReference = activeQuiz.QuizTitle;
        currentQuestion = activeQuiz.Questions[0];

        maxScore = activeQuiz.RewardScore * activeQuiz.Questions.Count;

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void Start()
    {
        StartCoroutine(PlaySequenceWithCallback(() =>
        {
            StartCoroutine(PlaySequenceWithCallback(() =>
            {
                startButton.interactable = true;
            }, introSequence));
        }, instructionMessage));
    }

    private void OnDestroy() => LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;

    private void OnLocaleChanged(Locale locale)
    {
        //When the system recognizes a different locale
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        foreach (var t in texts)
        {
            t.isRightToLeftText = locale.LocaleName == "Arabic (ar)";
        }
    }

    /// <summary>
    /// Triggered from the Intro Screen
    /// </summary>
    public void OnStartClicked() => State = UIState.Quiz;

    /// <summary>
    /// Triggered from the End screen
    /// </summary>
    public void OnResetClicked()
    {
        restartButton.interactable = false;
        exitButton.interactable = false;
        Score = 0;
        currentQuestion = activeQuiz.Questions[0];
        ClearOptions();
        State = UIState.Intro;
    }

    public void OnExitClicked()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// When an Option is clicked
    /// </summary>
    /// <param name="option"></param>
    public void OnOptionClicked(OptionElement option)
    {
        AudibleOption answer = option.Option;
        LocalizedAudioClip chosenClip = answer.IsCorrect ? activeQuiz.CorrectAudio : activeQuiz.IncorrectAudio;

        ToggleOptions(false);
        option.UpdateOptionText();
        ClearOptions(option);
        
        StartCoroutine(PlaySequenceWithCallback(() =>
        {
            if (answer.IsCorrect)
            {
                ClearOptions(option);
                Score += Mathf.Clamp(activeQuiz.RewardScore - (activeQuiz.BaseReduction * incorrectCount), 0, maxScore);
                StartCoroutine(TryNextQuestion());
            }
            else
            {
                RevertOptions();
                incorrectCount++;
                ToggleOptions(true);
            }

            if (incorrectCount >= 2)
            {
                ClearOptions();
                StartCoroutine(TryNextQuestion());
            }
        }, chosenClip, answer.Dialogue));
    }

    /// <summary>
    /// Switches the values of the UI based on the current state
    /// </summary>
    private void SwitchCanvases()
    {
        introCanvas.SetActive(state == UIState.Intro);
        quizCanvas.SetActive(state == UIState.Quiz);
        endCanvas.SetActive(state == UIState.End);
    }

    /// <summary>
    /// Updates the UI of the Quiz screen with the values of the current question.
    /// It will reconstruct options if there are more or less answers than previous
    /// </summary>
    private void UpdateQuizUI()
    {
        if (currentQuestion == null) return;

        if (constructedOptions == null || constructedOptions.Count != currentQuestion.Options.Count)
        {
            constructedOptions = new List<OptionElement>();
            
            //Removing current options from the transform
            for (int i = optionContainer.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(optionContainer.transform.GetChild(i).gameObject);
            }
            
            RectTransform optionContainerTransform = optionContainer.transform as RectTransform;
            for (int i = 0; i < currentQuestion.Options.Count; i++)
            {
                GameObject prefab = Instantiate(optionPrefab);
                prefab.transform.SetParent(optionContainerTransform);

                OptionElement oe = prefab.GetComponent<OptionElement>();
                oe.manager = this;
                constructedOptions.Add(oe);

                prefab.transform.localPosition = Vector3.zero;
                prefab.transform.localScale = optionPrefab.transform.localScale;
                prefab.transform.localRotation = Quaternion.identity;
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(optionContainer.transform as RectTransform);
        }
        
        ToggleOptions(false);

        questionEvent.StringReference = currentQuestion.QuizQuestion;

        // Will play the question dialogue and then it will run 
        // A callback to display the answers
        StartCoroutine(PlaySequenceWithCallback(() =>
        {
            List<AudibleOption> randomizedOptions = ShuffleOptions();

            for (int i = 0; i < constructedOptions.Count; i++)
            {
                constructedOptions[i].Option = randomizedOptions[i];
            }
        
            ToggleOptions(true);
            
        }, currentQuestion.Dialogue));
    }

    /// <summary>
    /// Shuffles the answers
    /// </summary>
    /// <returns></returns>
    private List<AudibleOption> ShuffleOptions()
    {
        Random rand = new Random();
        return currentQuestion.Options.OrderBy(_ => rand.Next()).ToList();
    }

    /// <summary>
    /// Players a sequence of audio clips, callback can be null.
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="clips"></param>
    /// <returns></returns>
    private IEnumerator PlaySequenceWithCallback(Action callback, params LocalizedAudioClip[] clips)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            audioClipEvent.AssetReference = clips[i];

            //Necessary, the audioClipEvent appears to take longer than 1 frame.
            yield return new WaitForSeconds(1.0f);
            
            while (targetAudioSource.isPlaying) yield return null;

            yield return new WaitForSeconds(timeBetweenClips);
        }

        callback?.Invoke();
    }

    /// <summary>
    /// Tries to assume the next quiz question, if the next question is invalid (end of the array)
    /// then it assumes the end of quiz.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TryNextQuestion()
    {
        yield return new WaitForSeconds(1f);
        
        int current = activeQuiz.Questions.IndexOf(currentQuestion);
        if (current + 1 == activeQuiz.Questions.Count)
        {
            //End of quiz
            //Play end of quiz message and switch UI state
            yield return StartCoroutine(PlaySequenceWithCallback(() => State = UIState.End, endOfQuizMessage));
            
            State = UIState.End;
            yield break;
        }

        incorrectCount = 0;
        current++;
        currentQuestion = activeQuiz.Questions[current];
        UpdateQuizUI();
    }

    /// <summary>
    /// Toggles the interactable state of the buttons
    /// </summary>
    /// <param name="interactable"></param>
    /// <param name="ignore"></param>
    private void ToggleOptions(bool interactable, OptionElement ignore = null)
    {
        foreach (var option in constructedOptions)
        {
            if (ignore != null)
            {
                //TODO: Improve
                if (option.Option.OptionAnswer == ignore.Option.OptionAnswer) continue;
            }

            option.GetComponent<Button>().interactable = interactable;
        }
    }

    /// <summary>
    /// Updates the End screen UI
    /// </summary>
    private void UpdateEndUI()
    {
        restartButton.interactable = false;
        exitButton.interactable = false;
        scoreEvent.StringReference = localizedScore;
        endEvent.StringReference = activeQuiz.EndMessage;
        
        //Will play the message before letting you press restart.
        //Can remove that callback and just leave it null if you want to press restart whenever.
        StartCoroutine(PlaySequenceWithCallback(() =>
        {
            restartButton.interactable = true;
            exitButton.interactable = true;
        }, endOfAppMessage));
    }

    /// <summary>
    /// Makes the option text blank.
    /// Note when switching language in the editor this tends to get overwritten.
    /// But because it happens in editor, it isn't behaviour you would see in the build.
    /// </summary>
    /// <param name="ignore"></param>
    private void ClearOptions(OptionElement ignore = null)
    {
        foreach (var option in constructedOptions)
        {
            if (ignore != null)
            {
                if (option.Option.OptionAnswer == ignore.Option.OptionAnswer) continue;
            }

            option.ClearText();
        }
    }

    /// <summary>
    /// Makes the options revert to their answer originally.
    /// </summary>
    private void RevertOptions()
    {
        foreach (var option in constructedOptions)
        {
            option.RevertOptionText();
        }
    }
}