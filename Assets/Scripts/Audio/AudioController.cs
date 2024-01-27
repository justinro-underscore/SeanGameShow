using System;
using System.Collections.Generic;
using UnityEngine;

public static class SoundEffectKeys
{
    // Theme music
    public const string ThemeMusicWithIntro = "GameIntro";
    public const string ThemeMusicNoIntro = "GameIntroNoBeginSting";
    public const string FamilyFeud = "FamilyFeud";

    // Correct answers
    public const string CorrectAnswer = "CorrectAnswer";
    public const string NiceShot = "NiceShot";
    public const string CashRegister = "CashRegister";
    public const string MarioCoin = "MarioCoin";
    public const string TacoBell = "TacoBell";
    public const string HellYeah = "HellYeah";
    public const string LetsaGo = "LetsaGo";
    public const string Nice = "Nice";
    public const string OwenWilson = "OwenWilson";
    public const string ZeldaChest = "ZeldaChest";
    public const string DaBest = "DaBest";
    public static readonly string[] CorrectAnswerKeys = {
        CorrectAnswer,
        NiceShot,
        CashRegister,
        MarioCoin,
        TacoBell,
        HellYeah,
        LetsaGo,
        Nice,
        OwenWilson,
        ZeldaChest,
        DaBest
    };

    // Incorrect answers
    public const string IncorrectAnswer = "Buzzer";
    public const string Bonk = "Bonk";
    public const string CrowdAww = "CrowdAwww";
    public const string Fail = "Fail";
    public const string SadTrombone = "SadTrombone";
    public const string Wilhelm = "Wilhelm";
    public const string WindowsError = "WindowsError";
    public const string Fart = "Fart";
    public const string MarioFall = "MarioFall";
    public const string Oof = "Oof";
    public const string Moo = "Moo";
    public const string TromboneDown = "TromboneDown";
    public const string MarshallHurt = "MarshallHurt";
    public static readonly string[] IncorrectAnswerKeys = {
        IncorrectAnswer,
        Bonk,
        CrowdAww,
        Fail,
        SadTrombone,
        Wilhelm,
        WindowsError,
        Fart,
        MarioFall,
        Oof,
        Moo,
        TromboneDown,
        MarshallHurt
    };

    // Whooshes
    public const string Whoosh1 = "Whoosh1";
    public const string Whoosh2 = "Whoosh2";
    public const string Whoosh3 = "Whoosh3";
    public const string Whoosh4 = "Whoosh4";

    // Misc
    public const string BaseBump = "BaseBump";
    public const string Bling = "Bling";
    public const string Click = "Click";
}

[Serializable]
public class GameAudioClip
{
    public AudioClip audioClip;
    [Range(0, 2)]
    public float volume = 1;
    public string audioName = "";
}

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [SerializeField] private GameAudioSource gameAudioSourcePrefab;

    [SerializeField]
    [Range(0, 1)]
    private float baseVolume;

    [SerializeField] private List<GameAudioClip> audioClipsList;

    [SerializeField] private Dictionary<string, GameAudioClip> audioClips;

    private Dictionary<int, GameAudioSource> oneShotAudioSources = new Dictionary<int, GameAudioSource>();
    private int oneShotAudioSourceNextId = 1;

    public float ChaosSoundModifier = 0f;
    private List<string> chaosCorrectKeys;
    private List<string> chaosIncorrectKeys;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        audioClips = new Dictionary<string, GameAudioClip>(audioClipsList.Count);
        foreach (GameAudioClip gameClip in audioClipsList)
        {
            audioClips[gameClip.audioName != "" ? gameClip.audioName : gameClip.audioClip.name] = gameClip;
        }

        VerifyAudioSources();

        chaosCorrectKeys = new List<string>();
        foreach (string key in SoundEffectKeys.CorrectAnswerKeys)
            chaosCorrectKeys.Add(key);
        chaosIncorrectKeys = new List<string>();
        foreach (string key in SoundEffectKeys.IncorrectAnswerKeys)
            chaosIncorrectKeys.Add(key);
    }

    private void Update()
    {
        List<int> audioClipIds = new List<int>(oneShotAudioSources.Keys);
        foreach (int audioClipId in audioClipIds)
        {
            GameAudioSource audioSource = oneShotAudioSources[audioClipId];
            if (!audioSource.IsPlaying())
            {
                Destroy(audioSource.gameObject);
                oneShotAudioSources.Remove(audioClipId);
            }
        }
    }

    // TODO replace optional params with options struct
    public int PlayOneShotAudio(string key, float delay = 0f, bool looping = false, float volume = -1f)
    {
        if (audioClips == null) return -1;
        if (audioClips.ContainsKey(key))
        {
            GameAudioSource audioSource = Instantiate(gameAudioSourcePrefab, transform);
            audioSource.SetAudioClip(audioClips[key].audioClip, delay);
            audioSource.SetVolume((volume >= 0 ? volume : audioClips[key].volume) * baseVolume);
            audioSource.SetLooping(looping);
            oneShotAudioSources.Add(oneShotAudioSourceNextId, audioSource);
            return oneShotAudioSourceNextId++;
        }
        else
        {
            throw new Exception(String.Format("Audio clip not available {0}", key));
        }
    }

    public bool OneShotAudioPlaying(string key)
    {
        foreach (GameAudioSource audioSource in oneShotAudioSources.Values)
        {
            // This doesn't actually work unless the clip name is the same as the key name
            if (audioSource.GetClipName() == key) return true;
        }
        return false;
    }

    public void StopOneShotAudio(int audioId)
    {
        if (oneShotAudioSources.ContainsKey(audioId))
        {
            oneShotAudioSources[audioId].Stop();
        }
    }

    public bool IsOneShotAudioPlaying(int audioId)
    {
        return oneShotAudioSources.ContainsKey(audioId) && oneShotAudioSources[audioId] && oneShotAudioSources[audioId].IsPlaying();
    }

    public void StopAllOneShotAudio()
    {
        foreach (GameAudioSource audioSource in oneShotAudioSources.Values)
        {
            audioSource.Stop();
            // Audio source will be cleaned up in the next Update
        }
    }

    /**************************************************/

    public void PlayThemeMusicNoIntro()
    {
        float effectVal = UnityEngine.Random.value;
        if (effectVal < ChaosSoundModifier) {
            PlayOneShotAudio(SoundEffectKeys.FamilyFeud);
        }
        else {
            PlayOneShotAudio(SoundEffectKeys.ThemeMusicNoIntro);
        }
    }

    public void PlayCorrectSoundEffect()
    {
        float effectVal = UnityEngine.Random.value;
        if (effectVal < ChaosSoundModifier) {
            int idx = Mathf.FloorToInt(UnityEngine.Random.value * chaosCorrectKeys.Count);
            if (idx == chaosCorrectKeys.Count) idx--;
            PlayOneShotAudio(chaosCorrectKeys[idx]);
            chaosCorrectKeys.RemoveAt(idx);
            if (chaosCorrectKeys.Count == 0) {
                foreach (string key in SoundEffectKeys.CorrectAnswerKeys)
                    chaosCorrectKeys.Add(key);
            }
        }
        else {
            PlayOneShotAudio(SoundEffectKeys.CorrectAnswer);
        }
    }

    public void PlayIncorrectSoundEffect()
    {
        float effectVal = UnityEngine.Random.value;
        if (effectVal < ChaosSoundModifier) {
            int idx = Mathf.FloorToInt(UnityEngine.Random.value * chaosIncorrectKeys.Count);
            if (idx == chaosIncorrectKeys.Count) idx--;
            PlayOneShotAudio(chaosIncorrectKeys[idx]);
            chaosIncorrectKeys.RemoveAt(idx);
            if (chaosIncorrectKeys.Count == 0) {
                foreach (string key in SoundEffectKeys.IncorrectAnswerKeys)
                    chaosIncorrectKeys.Add(key);
            }
        }
        else {
            PlayOneShotAudio(SoundEffectKeys.IncorrectAnswer);
        }
    }

    /**************************************************/

    public void VerifyAudioSources()
    {
        List<string> invalidSoundEffectKeys = new List<string>();
        foreach (System.Reflection.FieldInfo constant in typeof(SoundEffectKeys).GetFields())
        {
            if (constant.GetType() == typeof(string))
            {
                string clipName = (string)constant.GetValue(null);
                if (!audioClips.ContainsKey(clipName))
                {
                    invalidSoundEffectKeys.Add(clipName);
                }
            }
        }

        if (invalidSoundEffectKeys.Count > 0)
        {
            string errorStr = "The following audio clips do not exist!";
            if (invalidSoundEffectKeys.Count > 0)
            {
                errorStr += "\nSound effects:";
                foreach (string clipName in invalidSoundEffectKeys)
                {
                    errorStr += String.Format("\n - {0}", clipName);
                }
            }
            throw new Exception(errorStr);
        }
    }
}
