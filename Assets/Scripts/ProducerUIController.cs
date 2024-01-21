using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ProducerUIController : MonoBehaviour {
    [Header("Elements")]
    [Header("Game Control Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button endGameButton;

    [Header("Prompt / Answers Elements")]
    [SerializeField] private TMP_Text currentPromptText;
    [SerializeField] private List<Button> answerButtons;
    [SerializeField] private List<Button> xButtons;

    [Header("Score Elements")]
    [SerializeField] private TMP_Text redTeamScoreText;
    [SerializeField] private Button redTeamMinusButton;
    [SerializeField] private Button redTeamPlusButton;
    [SerializeField] private TMP_Text blueTeamScoreText;
    [SerializeField] private Button blueTeamMinusButton;
    [SerializeField] private Button blueTeamPlusButton;

    [Header("Score Elements")]
    [SerializeField] private TMP_Text nextPromptText;
    [SerializeField] private Button randomPromptButton;
    [SerializeField] private Button nextRoundButton;

    [Header("Prompt Elements")]
    [SerializeField] private RectTransform promptScrollviewParent;
    [SerializeField] private RectTransform promptOptionPrefab;
    [SerializeField] private RectTransform promptDividerPrefab;

    private bool gameRunning;
    private int nextPromptId;
    private List<int> availablePromptIds;

    private int redTeamScore;
    private int blueTeamScore;

    /***************************************************************************
     **                                                                       **
     **                           LIFECYCLE METHODS                           **
     **                                                                       **
     ***************************************************************************/

    private void Start() {
        // Set up the prompts list
        float contentHeight = 0;
        foreach (PromptModel prompt in GameController.LoadedPrompts) {
            RectTransform promptObj = Instantiate(promptOptionPrefab, promptScrollviewParent);
            promptObj.GetComponentInChildren<TMP_Text>().text = prompt.Prompt;
            promptObj.GetComponent<Button>().onClick.AddListener(() => SelectPrompt(prompt.Id));
            promptObj.name = GetPromptOptionName(prompt.Id);
            contentHeight += promptObj.sizeDelta.y;
        }
        RectTransform playedPromptsDivider = Instantiate(promptDividerPrefab, promptScrollviewParent);
        playedPromptsDivider.GetComponent<TMP_Text>().text = "Played";
        playedPromptsDivider.name = "PromptDivider:Played";
        contentHeight += playedPromptsDivider.sizeDelta.y;
        promptScrollviewParent.sizeDelta = new Vector2(promptScrollviewParent.sizeDelta.x, contentHeight);

        // Reset all of the UI components
        Reset();
    }

    /***************************************************************************
     **                                                                       **
     **                            PUBLIC METHODS                             **
     **                                                                       **
     ***************************************************************************/

    public void StartGame() {
        if (nextPromptId < 0) {
            return;
        }
        startGameButton.interactable = false;
        endGameButton.interactable = true;
        foreach (Button answerButton in answerButtons) {
            answerButton.interactable = true;
        }
        foreach (Button xButton in xButtons) {
            xButton.interactable = true;
        }
        redTeamPlusButton.interactable = true;
        blueTeamPlusButton.interactable = true;
        gameRunning = true;
        StartRound();
    }

    public void EndGame() {
        gameRunning = false;
        Reset();
    }

    public void SelectRandomPrompt() {
        if (availablePromptIds.Count > 0) {
            int nextIdIdx = Mathf.FloorToInt(Random.value * availablePromptIds.Count);
            if (nextIdIdx >= availablePromptIds.Count) nextIdIdx--;
            SelectPrompt(availablePromptIds[nextIdIdx]);
        }
    }

    public void StartNextRound() {
        if (nextPromptId >= 0) {
            StartRound();
        }
    }

    public void SelectPrompt(int id) {
        if (id == nextPromptId) return;

        if (nextPromptId >= 0) {
            Color origColor = availablePromptIds.Contains(nextPromptId) ? Color.white : Color.gray;
            GetPromptOptionById(nextPromptId).GetComponent<Image>().color = origColor;
        }

        nextPromptId = id;
        if (!gameRunning) {
            currentPromptText.text = "Press \"Start Game\" to begin the game";
            startGameButton.interactable = true;
        }
        else {
            nextRoundButton.interactable = true;
        }
        SetNextPromptText(GameController.GetPromptById(id).Prompt);
        GetPromptOptionById(id).GetComponent<Image>().color = Color.green;
    }

    /***************************************************************************
     **                                                                       **
     **                            PRIVATE METHODS                            **
     **                                                                       **
     ***************************************************************************/

    private void Reset() {
        nextPromptId = -1;

        startGameButton.interactable = false;
        endGameButton.interactable = false;
        currentPromptText.text = "Select a prompt to begin the game";
        for (int i = 0; i < answerButtons.Count; i++) {
            Button answerButton = answerButtons[i];
            answerButton.interactable = false;
            answerButton.GetComponentInChildren<TMP_Text>().text = "Answer " + (i + 1);
        }
        foreach (Button xButton in xButtons) {
            xButton.interactable = false;
        }
        SetRedTeamScore(0);
        redTeamPlusButton.interactable = false;
        SetBlueTeamScore(0);
        blueTeamPlusButton.interactable = false;
        SetNextPromptText("Not selected");
        randomPromptButton.interactable = true;
        nextRoundButton.interactable = false;

        // Order the prompt options (assumes LoadedPrompts is in order)
        availablePromptIds = new List<int>();
        for (int i = 0; i < GameController.LoadedPrompts.Count; i++) {
            int promptId = GameController.LoadedPrompts[i].Id;
            availablePromptIds.Add(promptId);
            Transform promptOption = GetPromptOptionById(promptId);
            promptOption.GetComponent<Image>().color = Color.white;
            promptOption.SetSiblingIndex(i);
        }
    }

    private void StartRound() {
        if (nextPromptId < 0) {
            return;
        }
        PromptModel prompt = GameController.GetPromptById(nextPromptId);
        SetCurrentPromptText(prompt.Prompt);
        for (int i = 0; i < answerButtons.Count; i++) {
            Button answerButton = answerButtons[i];
            answerButton.GetComponent<Image>().color = Color.white;
            answerButton.GetComponentInChildren<TMP_Text>().text = (i + 1) + ". " + prompt.Answers[i];
        }
        foreach (Button xButton in xButtons) {
            xButton.GetComponent<Image>().color = Color.white;
        }
        nextRoundButton.interactable = false;
        SetNextPromptText("Not selected");

        // Move the selected prompt option to the bottom
        Transform promptOption = GetPromptOptionById(nextPromptId);
        promptOption.SetSiblingIndex(promptOption.parent.childCount);
        promptOption.GetComponent<Image>().color = Color.gray;
        availablePromptIds.Remove(nextPromptId);

        if (availablePromptIds.Count == 0) {
            randomPromptButton.interactable = false;
            nextPromptText.text = "Out of prompts! Either select a played prompt or end the game";
        }

        nextPromptId = -1;
    }

    private void SetRedTeamScore(int score) {
        if (score < 0) score = 0;
        redTeamScore = score;
        redTeamScoreText.text = "" + redTeamScore;
        redTeamMinusButton.interactable = redTeamScore > 0;
    }

    private void SetBlueTeamScore(int score) {
        if (score < 0) score = 0;
        blueTeamScore = score;
        blueTeamScoreText.text = "" + blueTeamScore;
        blueTeamMinusButton.interactable = blueTeamScore > 0;
    }

    private void SetCurrentPromptText(string promptText) {
        currentPromptText.text = "Current Prompt: " + promptText;
    }

    private void SetNextPromptText(string promptText) {
        nextPromptText.text = "Next Prompt: " + promptText;
    }

    private string GetPromptOptionName(int id) {
        return "PromptOption:" + id;
    }

    private Transform GetPromptOptionById(int id) {
        return promptScrollviewParent.Find(GetPromptOptionName(id));
    }
}
