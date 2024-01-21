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

    private int redTeamScore;
    private int blueTeamScore;

    private void Start() {
        Reset();

        // Set up the prompts list
        float contentHeight = 0;
        foreach (PromptModel prompt in GameController.LoadedPrompts) {
            RectTransform promptObj = Instantiate(promptOptionPrefab, promptScrollviewParent);
            promptObj.GetComponentInChildren<TMP_Text>().text = prompt.Prompt;
            promptObj.name = "PromptOption:" + prompt.Id;
            contentHeight += promptObj.sizeDelta.y;
        }
        RectTransform playedPromptsDivider = Instantiate(promptDividerPrefab, promptScrollviewParent);
        playedPromptsDivider.GetComponent<TMP_Text>().text = "Played";
        playedPromptsDivider.name = "PromptDivider:Played";
        contentHeight += playedPromptsDivider.sizeDelta.y;
        promptScrollviewParent.sizeDelta = new Vector2(promptScrollviewParent.sizeDelta.x, contentHeight);
    }

    private void Reset() {
        startGameButton.interactable = true;
        endGameButton.interactable = false;
        SetCurrentPromptText("Game not started");
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
        randomPromptButton.interactable = false;
        nextRoundButton.interactable = false;
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
}
