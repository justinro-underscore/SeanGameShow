using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using DG.Tweening;

public class PlayerUIController : MonoBehaviour {
    [Header("Elements")]
    [Header("Generic Elements")]
    [SerializeField] private Transform backgroundImage;
    [SerializeField] private Transform titleImage;

    [Header("Question UI Elements")]
    [SerializeField] private Transform questionUI;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Transform answerImageParent;
    private Tuple<Transform, Transform>[] answers;
    [SerializeField] private List<Transform> xImages;

    [Header("Variables")]
    [SerializeField] private Color inactiveXColor;

    private bool inGame;
    private Sequence[] answerSeqs;
    private Sequence[] xSeqs;

    /***************************************************************************
     **                                                                       **
     **                           LIFECYCLE METHODS                           **
     **                                                                       **
     ***************************************************************************/

    private void Start() {
        inGame = false;
        answers = new Tuple<Transform, Transform>[answerImageParent.childCount / 2];
        for (int i = 0; i < (answerImageParent.childCount / 2); i++) {
            answers[i] = new Tuple<Transform, Transform>(
                answerImageParent.GetChild(2 * i),
                answerImageParent.GetChild((2 * i) + 1));
        }
        answerSeqs = new Sequence[answers.Length];
        xSeqs = new Sequence[xImages.Count];

        backgroundImage.localScale = Vector2.zero;
        titleImage.localScale = Vector2.zero;
        DOTween.Sequence().Append(backgroundImage.DOScale(1, 2).SetEase(Ease.Linear))
            .Join(titleImage.DOScale(1, 2).SetEase(Ease.Linear))
            .Join(backgroundImage.DOLocalRotate(new Vector3(0, 0, 360 * 5), 2, RotateMode.LocalAxisAdd).SetEase(Ease.Linear))
            .Join(titleImage.DOLocalRotate(new Vector3(0, 0, 360 * 5), 2, RotateMode.LocalAxisAdd).SetEase(Ease.Linear))
            .Append(GetTitleSequence());
            
    }

    /***************************************************************************
     **                                                                       **
     **                            PUBLIC METHODS                             **
     **                                                                       **
     ***************************************************************************/

    public void StartGame(int promptId) {
        DOTween.KillAll();
        backgroundImage.localScale = Vector2.one;
        backgroundImage.eulerAngles = Vector2.zero;
        titleImage.localScale = Vector2.one;
        titleImage.eulerAngles = Vector2.zero;

        Sequence seq = DOTween.Sequence().Append(titleImage.DOScale(0, 1f).SetEase(Ease.InBack))
            .AppendCallback(() => {
                DOTween.Kill(titleImage);
                titleImage.gameObject.SetActive(false);
            });

        seq.AppendCallback(() => {
                StartNextRoundInternal(promptId);
                questionUI.localScale = Vector2.zero;
                questionUI.gameObject.SetActive(true);
            }).Append(questionUI.DOScale(1, 0.3f).SetEase(Ease.OutBack));

        inGame = true;
    }

    public void EndGame() {
    }

    public void StartNextRound(int promptId, Sequence seq=null) {
    }

    public void SetAnswerVisible(int answerIdx, bool toVisible) {
        if (answerSeqs[answerIdx] != null) answerSeqs[answerIdx].Kill();
        Transform hiddenAnswer = answers[answerIdx].Item1;
        Transform shownAnswer = answers[answerIdx].Item2;
        (toVisible ? shownAnswer : hiddenAnswer).localScale = new Vector3(0, 1, 1);
        (toVisible ? hiddenAnswer : shownAnswer).localScale = Vector2.one;
        hiddenAnswer.gameObject.SetActive(true);
        shownAnswer.gameObject.SetActive(true);
        answerSeqs[answerIdx] = DOTween.Sequence().Append((toVisible ? hiddenAnswer : shownAnswer).DOScaleX(0, 0.1f).SetEase(Ease.Linear))
            .AppendCallback(() => (toVisible ? hiddenAnswer : shownAnswer).gameObject.SetActive(false))
            .Append((toVisible ? shownAnswer : hiddenAnswer).DOScaleX(1, 0.1f).SetEase(Ease.Linear))
            .AppendCallback(() => answerSeqs[answerIdx] = null);
    }

    public void SetXVisible(int xIdx, bool toVisible) {
        if (xSeqs[xIdx] != null) xSeqs[xIdx].Kill();
        Transform xImage = xImages[xIdx];
        xImage.localScale = Vector3.one;
        xImage.localEulerAngles = Vector3.zero;
        xImage.GetComponent<Image>().color = toVisible ? Color.white : inactiveXColor;
        if (toVisible) {
            xSeqs[xIdx] = DOTween.Sequence().Append(xImage.DOScale(1.3f, 0.1f))
                .Join(xImage.DOShakeRotation(0.3f, new Vector3(0, 0, 15), 20, 90, false, ShakeRandomnessMode.Harmonic))
                .Append(xImage.DOScale(1, 0.1f))
                .AppendCallback(() => xSeqs[xIdx] = null);
        }
    }

    /***************************************************************************
     **                                                                       **
     **                            PRIVATE METHODS                            **
     **                                                                       **
     ***************************************************************************/

    private void StartNextRoundInternal(int promptId) {
        PromptModel prompt = GameController.GetPromptById(promptId);
        promptText.text = prompt.Prompt;
        for (int i = 0; i < answers.Length; i++) {
            answers[i].Item2.GetComponentInChildren<TMP_Text>().text = prompt.Answers[i];
        }
    }

    private Sequence GetTitleSequence() {
        return DOTween.Sequence().Append(
                DOTween.Sequence().Append(titleImage.DORotate(new Vector3(0, 0, 15), 0.5f))
                    .Append(titleImage.DORotate(new Vector3(0, 0, -15), 1).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo)))
            .Join(
                DOTween.Sequence().Append(titleImage.DOScale(1.2f, 0.675f))
                    .Append(titleImage.DOScale(0.9f, 1.125f).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo)));
    }
}
