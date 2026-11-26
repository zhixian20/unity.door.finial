using System;
using System.Collections;
using TMPro;
using UnityEngine;

public sealed class DialogueManager : MonoBehaviour
{
    [Serializable]
    private struct Line
    {
        public string speaker;
        [TextArea] public string text;
    }

    [SerializeField] private Line[] lines = Array.Empty<Line>();
    [SerializeField, Min(0f)] private float typingSpeed = 0.03f;
    [SerializeField] private bool startOnPlay = true;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nextButtonText;
    [SerializeField] private GameObject talkButton;

    private int lineIndex;
    private Coroutine typing;

    private IEnumerator Start()
    {
        // Let the world-space Canvas initialize before reading TMP's character count.
        yield return null;

        if (startOnPlay)
            StartDialogue();
        else
            CloseDialogue();
    }

    public void StartDialogue()
    {
        if (lines.Length == 0)
        {
            CloseDialogue();
            return;
        }

        lineIndex = 0;
        talkButton.SetActive(false);
        dialoguePanel.SetActive(true);
        ShowLine();
    }

    public void Next()
    {
        if (!dialoguePanel.activeSelf)
            return;

        if (typing != null)
        {
            StopTyping();
            dialogueText.maxVisibleCharacters = int.MaxValue;
            return;
        }

        lineIndex++;
        if (lineIndex == lines.Length)
            CloseDialogue();
        else
            ShowLine();
    }

    private void ShowLine()
    {
        StopTyping();
        Line line = lines[lineIndex];
        speakerText.text = line.speaker;
        dialogueText.text = line.text;
        dialogueText.maxVisibleCharacters = int.MaxValue;
        dialogueText.ForceMeshUpdate(true, true);
        nextButtonText.text = lineIndex == lines.Length - 1 ? "Close" : "Next";

        int characterCount = dialogueText.textInfo.characterCount;
        if (typingSpeed > 0f && characterCount > 0)
        {
            dialogueText.maxVisibleCharacters = 0;
            typing = StartCoroutine(TypeLine(characterCount));
        }
    }

    private IEnumerator TypeLine(int characterCount)
    {
        for (int visible = 0; visible < characterCount;)
        {
            yield return new WaitForSecondsRealtime(typingSpeed);
            dialogueText.maxVisibleCharacters = ++visible;
        }

        typing = null;
    }

    private void CloseDialogue()
    {
        StopTyping();
        dialoguePanel.SetActive(false);
        talkButton.SetActive(true);
    }

    private void OnDisable() => StopTyping();

    private void StopTyping()
    {
        if (typing == null)
            return;

        StopCoroutine(typing);
        typing = null;
    }
}
