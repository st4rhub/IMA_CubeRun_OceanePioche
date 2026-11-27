// CMS26_IMA_Game · ships inside the package (Assets/IMA_Game/Editor).
// A unitypackage can't carry project settings, so this adds the package's scenes to the end of
// File → Build Profiles → Scene List when the project loads after the import: the four scenes
// of the template (Game_Title, Game_Level, Game_Win, Game_Lose) and the four of the example
// (Castle_Title, Castle_Level, Castle_Win, Castle_Lose), and the scenes of the tutorials in
// Stations. A scene loads another one by its name,
// and only scenes in that list can be loaded. Your own scenes stay where they are; untick a
// scene to leave it out of your build.
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class IMAGameSetup
{
    const string Root = "Assets/IMA_Game";
    static readonly string[] Scenes =
    {
        "/Template/Game_Title.unity", "/Template/Game_Level.unity", "/Template/Game_Win.unity", "/Template/Game_Lose.unity",
        "/Example/Castle_Title.unity", "/Example/Castle_Level.unity", "/Example/Castle_Win.unity", "/Example/Castle_Lose.unity",
        "/Example/City_Title.unity", "/Example/City_Level.unity", "/Example/City_Win.unity", "/Example/City_Lose.unity",
    };

    static IMAGameSetup()
    {
        // Right away (also in batch mode, where delayCall may never fire) and once more on the
        // next editor tick, in case the editor wasn't ready during the domain reload.
        try { AddScenes(); } catch (System.Exception e) { Debug.LogWarning("IMA_Game: scene list later (" + e.Message + ")"); }
        EditorApplication.delayCall += AddScenes;
    }

    /// Makes sure the package's scenes are in the Scene List (appended at the end). A scene that
    /// is in the list but unticked counts as there.
    public static void AddScenes()
    {
        var wanted = Scenes.Select(s => Root + s).Where(File.Exists).ToList();
        if (Directory.Exists(Root + "/Stations"))
            wanted.AddRange(Directory.GetFiles(Root + "/Stations", "*.unity").Select(p => p.Replace('\\', '/')).OrderBy(p => p, System.StringComparer.Ordinal));
        var list = EditorBuildSettings.scenes.ToList();
        var added = 0;
        foreach (var path in wanted)
        {
            if (list.Any(s => s.path == path)) continue;
            list.Add(new EditorBuildSettingsScene(path, true));
            added++;
        }
        if (added == 0) return;
        EditorBuildSettings.scenes = list.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("IMA_Game: added " + added + " scene(s) to the Scene List");
    }
}
