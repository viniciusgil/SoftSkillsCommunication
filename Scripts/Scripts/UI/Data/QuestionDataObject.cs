using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName="Aramco/Questions/Entry")]
public class QuestionDataObject : ScriptableObject
{
    /// <summary>
    /// Title of the quiz (localized table data)
    /// </summary>
    public LocalizedString QuizTitle;
    
    /// <summary>
    /// End message (localized table data)
    /// </summary>
    public LocalizedString EndMessage;
    
    /// <summary>
    /// List of questions that this quiz has
    /// </summary>
    public List<QuestionEntry> Questions;
    
    /// <summary>
    /// The audio that is played for the correct or incorrect result
    /// </summary>
    public LocalizedAudioClip CorrectAudio, IncorrectAudio;

    public int RewardScore;
    public int BaseReduction;
}

[Serializable]
public class QuestionEntry
{
    /// <summary>
    /// Question of the quiz, from the looks of it, it may not be used?
    /// </summary>
    public LocalizedString QuizQuestion;
    
    /// <summary>
    /// The dialogue to go along with the question
    /// </summary>
    public LocalizedAudioClip Dialogue;
    
    /// <summary>
    /// List of options
    /// </summary>
    public List<AudibleOption> Options;
}

[Serializable]
public class AudibleOption
{
    /// <summary>
    /// The display for this option
    /// </summary>
    public LocalizedString OptionAnswer;
    
    /// <summary>
    /// The response shown when clicking this option
    /// </summary>
    public LocalizedString OptionResponse;
    
    /// <summary>
    /// The answer as dialogue
    /// </summary>
    public LocalizedAudioClip Dialogue;
    
    /// <summary>
    /// Whether this option is correct
    /// </summary>
    public bool IsCorrect;
}
