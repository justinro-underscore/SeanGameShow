using System;
using UnityEngine;
using CsvHelper;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

public class PromptModel {
    public int Id { get; private set; }
    public string Prompt { get; private set; }
    public string[] Answers { get; private set; }

    public PromptModel(int id, string prompt, string answer1, string answer2, string answer3, string answer4) {
        Id = id;
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
    [SerializeField] private TextAsset promptsCsv;
    public static List<PromptModel> LoadedPrompts { get; private set; }

    private void Awake() {
        LoadedPrompts = new List<PromptModel>();
        using (CsvReader csv = new CsvReader(new StringReader(promptsCsv.text), CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();
            int id = 0;
            while (csv.Read())
            {
                LoadedPrompts.Add(new PromptModel(
                    id++,
                    csv.GetField("Prompt"),
                    csv.GetField("Answer 1"),
                    csv.GetField("Answer 2"),
                    csv.GetField("Answer 3"),
                    csv.GetField("Answer 4")
                ));
            }
        }
    }

    public static PromptModel GetPromptById(int id) {
        if (id >= 0 && id < LoadedPrompts.Count) {
            return LoadedPrompts[id];
        }
        return null;
    }
}
