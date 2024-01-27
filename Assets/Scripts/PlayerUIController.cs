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

    [Header("Producer Elements")]
    [SerializeField] private GameObject producerOverlay;

    [Header("Variables")]
    [SerializeField] private Color inactiveXColor;

    private Sequence[] answerSeqs;
    private Sequence[] xSeqs;

    /***************************************************************************
     **                                                                       **
     **                           LIFECYCLE METHODS                           **
     **                                                                       **
     ***************************************************************************/

    private void Start() {
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
    }

    /***************************************************************************
     **                                                                       **
     **                            PUBLIC METHODS                             **
     **                                                                       **
     ***************************************************************************/

    public void Begin() {
        AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.ThemeMusicWithIntro);
        float introTime = 1.6f;
        DOTween.Sequence().Append(backgroundImage.DOScale(1, introTime).SetEase(Ease.Linear))
            .Join(titleImage.DOScale(1, introTime).SetEase(Ease.Linear))
            .Join(backgroundImage.DOLocalRotate(new Vector3(0, 0, 360 * 4), introTime, RotateMode.LocalAxisAdd).SetEase(Ease.Linear))
            .Join(titleImage.DOLocalRotate(new Vector3(0, 0, 360 * 4), introTime, RotateMode.LocalAxisAdd).SetEase(Ease.Linear))
            .Append(GetTitleSequence());
    }

    public void StartGame(int promptId) {
        DOTween.KillAll();
        AudioController.Instance.StopAllOneShotAudio();
        AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.Bling);
        backgroundImage.localScale = Vector2.one;
        backgroundImage.eulerAngles = Vector2.zero;
        titleImage.localScale = Vector2.one;
        titleImage.eulerAngles = Vector3.zero;

        producerOverlay.SetActive(true);
        AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.Whoosh2, 0.6f);
        Sequence seq = DOTween.Sequence().Append(titleImage.DOScale(0, 1f).SetEase(Ease.InBack))
            .AppendCallback(() => {
                DOTween.Kill(titleImage);
                titleImage.gameObject.SetActive(false);
            })
            .AppendInterval(0.5f)
            .AppendCallback(() => LoadInQuestionUI(promptId, true));
    }

    public void EndGame() {
        DOTween.KillAll();
        AudioController.Instance.StopAllOneShotAudio();
        producerOverlay.SetActive(true);
        questionUI.localScale = Vector2.one;
        AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.Whoosh2, 0.5f);
        DOTween.Sequence().Append(questionUI.DOScale(0, 0.8f).SetEase(Ease.InBack))
            .AppendInterval(0.5f)
            .AppendCallback(() => {
                titleImage.localScale = Vector2.zero;
                titleImage.eulerAngles = Vector3.zero;
                titleImage.gameObject.SetActive(true);
            })
            .Append(titleImage.DOScale(0, 0.5f).SetEase(Ease.Linear))
            .AppendCallback(() => {
                producerOverlay.SetActive(false);
                AudioController.Instance.PlayThemeMusicNoIntro();
            })
            .Append(GetTitleSequence());
    }

    public void StartNextRound(int promptId) {
        DOTween.KillAll();
        AudioController.Instance.StopAllOneShotAudio();
        producerOverlay.SetActive(true);
        questionUI.localScale = Vector2.one;
        AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.Whoosh2, 0.6f);
        DOTween.Sequence().Append(questionUI.DOScale(0, 1f).SetEase(Ease.InBack))
            .AppendInterval(0.5f)
            .AppendCallback(() => LoadInQuestionUI(promptId, false));
    }

    public void SetAnswerVisible(int answerIdx, bool toVisible) {
        SetAnswerVisible(answerIdx, toVisible, true);
        if (toVisible) {
            AudioController.Instance.PlayCorrectSoundEffect();
        }
    }

    public void SetXVisible(int xIdx, bool toVisible) {
        SetXVisible(xIdx, toVisible, true);
        if (toVisible) {
            AudioController.Instance.PlayIncorrectSoundEffect();
        }
    }

    /***************************************************************************
     **                                                                       **
     **                            PRIVATE METHODS                            **
     **                                                                       **
     ***************************************************************************/

    private Sequence GetTitleSequence() {
        return DOTween.Sequence().Append(
                DOTween.Sequence().Append(titleImage.DORotate(new Vector3(0, 0, 15), 0.5f))
                    .Append(titleImage.DORotate(new Vector3(0, 0, -15), 1).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo)))
            .Join(
                DOTween.Sequence().Append(titleImage.DOScale(1.2f, 0.675f))
                    .Append(titleImage.DOScale(0.9f, 1.125f).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo)));
    }

    private void SetupQuestionUIForNextRound(int promptId, bool startingGame) {
        // If starting the game, load in elements one at a time
        if (startingGame) {
            promptText.transform.localScale = Vector2.zero;
        }
        for (int i = 0; i < answers.Length; i++) {
            SetAnswerVisible(i, false, false);
            if (startingGame) {
                answers[i].Item1.GetComponent<Image>().color = Color.clear;
                answers[i].Item1.GetComponentInChildren<TMP_Text>().color = Color.clear;
            }
        }
        for (int i = 0; i < xImages.Count; i++) {
            SetXVisible(i, false, false);
            if (startingGame) {
                xImages[i].GetComponent<Image>().color = Color.clear;
            }
        }
        questionUI.localScale = startingGame ? Vector2.one : Vector2.zero;

        PromptModel prompt = GameController.GetPromptById(promptId);
        promptText.text = prompt.Prompt;
        for (int i = 0; i < answers.Length; i++) {
            answers[i].Item2.GetComponentInChildren<TMP_Text>().text = prompt.Answers[i];
        }

        questionUI.gameObject.SetActive(true);
    }

    private void LoadInQuestionUI(int promptId, bool startingGame) {
        SetupQuestionUIForNextRound(promptId, startingGame);
        if (startingGame) {
            AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.Whoosh1);
            Sequence seq = DOTween.Sequence().Append(promptText.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack))
                .AppendInterval(0.5f);
            for (int i = 0; i < answers.Length; i++) {
                seq.AppendCallback(() =>  AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.Click));
                seq.Append(answers[i].Item1.GetComponent<Image>().DOColor(Color.white, 0.3f));
                seq.Join(answers[i].Item1.GetComponentInChildren<TMP_Text>().DOColor(Color.black, 0.3f));
            }
            seq.AppendInterval(0.1f);
            for (int i = 0; i < xImages.Count; i++) {
                seq.AppendCallback(() =>  AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.BaseBump));
                seq.Append(xImages[i].GetComponent<Image>().DOColor(inactiveXColor, 0.3f));
            }
            seq.AppendCallback(() => producerOverlay.SetActive(false));
        }
        else {
            AudioController.Instance.PlayOneShotAudio(SoundEffectKeys.Whoosh4, 0.1f);
            DOTween.Sequence().Append(questionUI.DOScale(1, 0.6f).SetEase(Ease.OutBack))
                .AppendCallback(() => producerOverlay.SetActive(false));
        }
    }

    private void SetAnswerVisible(int answerIdx, bool toVisible, bool animate) {
        if (answerSeqs[answerIdx] != null) {
            answerSeqs[answerIdx].Kill();
            answerSeqs[answerIdx] = null;
        }
        Transform hiddenAnswer = answers[answerIdx].Item1;
        Transform shownAnswer = answers[answerIdx].Item2;
        if (animate) {
            (toVisible ? shownAnswer : hiddenAnswer).localScale = new Vector3(0, 1, 1);
            (toVisible ? hiddenAnswer : shownAnswer).localScale = Vector2.one;
            hiddenAnswer.gameObject.SetActive(true);
            shownAnswer.gameObject.SetActive(true);
            answerSeqs[answerIdx] = DOTween.Sequence().Append((toVisible ? hiddenAnswer : shownAnswer).DOScaleX(0, 0.1f).SetEase(Ease.Linear))
                .AppendCallback(() => (toVisible ? hiddenAnswer : shownAnswer).gameObject.SetActive(false))
                .Append((toVisible ? shownAnswer : hiddenAnswer).DOScaleX(1, 0.1f).SetEase(Ease.Linear))
                .AppendCallback(() => answerSeqs[answerIdx] = null);
        }
        else {
            hiddenAnswer.localScale = Vector2.one;
            hiddenAnswer.gameObject.SetActive(!toVisible);
            shownAnswer.localScale = Vector2.one;
            shownAnswer.gameObject.SetActive(toVisible);
        }
    }

    public void SetXVisible(int xIdx, bool toVisible, bool animate) {
        if (xSeqs[xIdx] != null) {
            xSeqs[xIdx].Kill();
            xSeqs[xIdx] = null;
        }
        Transform xImage = xImages[xIdx];
        xImage.localScale = Vector3.one;
        xImage.localEulerAngles = Vector3.zero;
        xImage.GetComponent<Image>().color = toVisible ? Color.white : inactiveXColor;
        if (animate && toVisible) {
            xSeqs[xIdx] = DOTween.Sequence().Append(xImage.DOScale(1.3f, 0.1f))
                .Join(xImage.DOShakeRotation(0.3f, new Vector3(0, 0, 15), 20, 90, false, ShakeRandomnessMode.Harmonic))
                .Append(xImage.DOScale(1, 0.1f))
                .AppendCallback(() => xSeqs[xIdx] = null);
        }
    }
}
