using System;
using UnityEngine;
using CsvHelper;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

public class PromptModel {
    public string Prompt { get; private set; }
    public string[] Answers { get; private set; }

    public PromptModel(string prompt, string answer1, string answer2, string answer3, string answer4) {
        Prompt = prompt;
        Answers = new string[4] {
            answer1,
            answer2,
            answer3,
            answer4
        };
    }
}

public class GameController : MonoBehaviour {
    public static GameController instance = null;

    [SerializeField] private TextAsset promptsCsv;
    private static List<PromptModel> availablePrompts;

    private int round;
    private int score;
    private PromptModel nextPrompt;

    private void Awake() {
        if ( null == instance ) {
            instance = this;
            DontDestroyOnLoad( this.gameObject );
        } else {
            Destroy( this.gameObject );
        }
    }

    private void Start() {
        availablePrompts = new List<PromptModel>();
        using (CsvReader csv = new CsvReader(new StringReader(promptsCsv.text), CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                availablePrompts.Add(new PromptModel(
                    csv.GetField("Prompt"),
                    csv.GetField("Answer 1"),
                    csv.GetField("Answer 2"),
                    csv.GetField("Answer 3"),
                    csv.GetField("Answer 4")
                ));
            }
        }
    }

    public void StartNewGame() {
        round = 0;
        score = 0;
        nextPrompt = null;
    }


}
