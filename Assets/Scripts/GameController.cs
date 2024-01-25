using System;
using UnityEngine;
using CsvHelper;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

[Serializable]
public class PromptListDataRaw {
    public string Name;
    public TextAsset PromptsCsv;
}

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

public class PromptListData {
    public string Name { get; private set; }
    public List<PromptModel> Prompts { get; private set; }

    public PromptListData(string name, List<PromptModel> prompts) {
        Name = name;
        Prompts = prompts;
    }
}

public class GameController : MonoBehaviour {
    [SerializeField] private List<PromptListDataRaw> promptsDatas;
    public static List<PromptListData> LoadedPrompts { get; private set; }

    private void Awake() {
        LoadedPrompts = new List<PromptListData>();
        int promptId = 0;
        foreach (PromptListDataRaw promptListDataRaw in promptsDatas) {
            List<PromptModel> prompts = new List<PromptModel>();
            using (CsvReader csv = new CsvReader(new StringReader(promptListDataRaw.PromptsCsv.text), CultureInfo.InvariantCulture)) {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    prompts.Add(new PromptModel(
                        promptId++,
                        csv.GetField("Prompt"),
                        csv.GetField("Answer 1"),
                        csv.GetField("Answer 2"),
                        csv.GetField("Answer 3"),
                        csv.GetField("Answer 4")
                    ));
                }
            }
            LoadedPrompts.Add(new PromptListData(promptListDataRaw.Name, prompts));
        }
    }

    public static PromptModel GetPromptById(int id) {
        foreach (PromptListData promptListData in LoadedPrompts) {
            PromptModel prompt = promptListData.Prompts.Find((PromptModel promptModel) => promptModel.Id == id);
            if (prompt != null) return prompt;
        }
        return null;
    }
}
