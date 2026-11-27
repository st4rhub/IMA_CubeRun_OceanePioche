// CMS26_IMA_Game · a list of the best results, kept on this computer.
// Sits on HUD_Leaderboard. The game hands its result over through PlayerPrefs: an FSM stores a
// number with the action Player Prefs Set Float (or Player Prefs Set Int) under the key
// "result", then switches HUD_Leaderboard on. The list reads the number, asks for a name if the
// result is among the best, and shows the places. It stays saved when the game is closed.
//
//   Board Name   one list per name: two games, two names
//   Ranking      Lowest Time (a stopwatch) or Highest Score (coins, points)
//   Result Key   the key the FSM stored the number under
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("IMA/Leaderboard")]
public class Leaderboard : MonoBehaviour
{
    public enum RankingKind { LowestTime, HighestScore }

    public string boardName = "My game";
    public RankingKind ranking = RankingKind.LowestTime;
    public string resultKey = "result";

    [Header("Parts of HUD_Leaderboard")]
    public TMP_Text resultText;
    public GameObject entry;
    public TMP_InputField nameField;
    public Button okButton;
    public TMP_Text[] rows;

    struct Place { public string name; public float value; }
    readonly List<Place> _places = new List<Place>();
    float _result;
    bool _hasResult;

    string StoreKey => "leaderboard." + boardName;

    void OnEnable()
    {
        Load();
        _hasResult = PlayerPrefs.HasKey(resultKey);
        if (_hasResult)
        {
            _result = PlayerPrefs.GetFloat(resultKey, float.NaN);
            if (float.IsNaN(_result)) _result = PlayerPrefs.GetInt(resultKey, 0);
            PlayerPrefs.DeleteKey(resultKey);   // one result is entered once
        }
        if (resultText != null) resultText.text = _hasResult ? "YOUR RESULT  " + Format(_result) : "";
        var asks = _hasResult && Qualifies(_result);
        if (entry != null) entry.SetActive(asks);
        if (okButton != null)
        {
            okButton.onClick.RemoveListener(Enter);
            okButton.onClick.AddListener(Enter);
        }
        if (asks && nameField != null)
        {
            nameField.text = "";
            nameField.Select();
            nameField.ActivateInputField();
        }
        Show(-1);
    }

    /// The OK button: the name and the result go into the list.
    public void Enter()
    {
        if (!_hasResult) return;
        var who = nameField != null ? nameField.text.Trim() : "";
        if (who.Length == 0) who = "???";
        if (who.Length > 12) who = who.Substring(0, 12);
        var mine = new Place { name = who.Replace("|", " ").Replace("\n", " "), value = _result };
        var at = 0;
        while (at < _places.Count && !Better(mine.value, _places[at].value)) at++;
        _places.Insert(at, mine);
        while (_places.Count > rows.Length) _places.RemoveAt(_places.Count - 1);
        Save();
        _hasResult = false;
        if (entry != null) entry.SetActive(false);
        Show(at);
    }

    /// Empties this list. For a button of your own.
    public void Clear()
    {
        _places.Clear();
        PlayerPrefs.DeleteKey(StoreKey);
        Show(-1);
    }

    bool Better(float a, float b) => ranking == RankingKind.LowestTime ? a < b : a > b;

    bool Qualifies(float value) => _places.Count < rows.Length || Better(value, _places[_places.Count - 1].value);

    string Format(float value) => ranking == RankingKind.LowestTime
        ? value.ToString("0.0", CultureInfo.InvariantCulture) + " s"
        : Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);

    void Show(int mine)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null) continue;
            rows[i].text = i < _places.Count ? (i + 1) + "   " + _places[i].name + "   " + Format(_places[i].value) : (i + 1) + "   —";
            rows[i].fontStyle = i == mine ? FontStyles.Bold : FontStyles.Normal;
            rows[i].color = i == mine ? new Color(1f, 0.82f, 0.3f) : Color.white;
        }
    }

    void Load()
    {
        _places.Clear();
        foreach (var line in PlayerPrefs.GetString(StoreKey, "").Split('\n'))
        {
            var parts = line.Split('|');
            if (parts.Length == 2 && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                _places.Add(new Place { name = parts[0], value = v });
        }
    }

    void Save()
    {
        var lines = new List<string>();
        foreach (var p in _places) lines.Add(p.name + "|" + p.value.ToString("R", CultureInfo.InvariantCulture));
        PlayerPrefs.SetString(StoreKey, string.Join("\n", lines));
        PlayerPrefs.Save();
    }
}
