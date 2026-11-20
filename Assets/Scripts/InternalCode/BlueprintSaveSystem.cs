using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable] public class GraphEntry { public string key; public GraphData graph; }
[Serializable] public class SaveFile   { public int version = 1; public List<GraphEntry> graphs = new(); }

// Saves / loads every BlueprintGraph in the scene to one JSON file.
public static class BlueprintSaveSystem
{
    public static string FilePath => Path.Combine(Application.persistentDataPath, "blueprints.json");

    // Writes all active graphs. Graphs of objects that aren't in the scene right now are kept as they were.
    public static void SaveAll()
    {
        var file = ReadFile() ?? new SaveFile();

        int saved = 0;
        foreach (var g in BlueprintGraph.All)
        {
            string key = g.SaveKey;
            file.graphs.RemoveAll(e => e.key == key);
            if (IsTrivial(g.nodes)) continue;       // nothing worth saving

            file.graphs.Add(new GraphEntry { key = key, graph = BlueprintSerializer.ToData(g.nodes) });
            saved++;
        }

        Write(file);
        Debug.Log($"Blueprints: saved {saved} graph(s) to {FilePath}");
    }

    // Restores graphs onto their objects. Objects that don't have a BlueprintGraph yet get one.
    public static int LoadAll()
    {
        var file = ReadFile();
        if (file == null) { Debug.Log("Blueprints: no save file yet."); return 0; }

        var existing = new Dictionary<string, BlueprintGraph>();
        foreach (var g in BlueprintGraph.All) existing[g.SaveKey] = g;

        int loaded = 0;
        foreach (var entry in file.graphs)
        {
            if (!existing.TryGetValue(entry.key, out var graph))
            {
                var go = ObjectPath.Find(entry.key);
                if (go == null)
                {
                    Debug.LogWarning($"Blueprints: couldn't find an object for '{entry.key}' - skipped.");
                    continue;
                }
                graph = go.GetComponent<BlueprintGraph>();
                if (graph == null) graph = go.AddComponent<BlueprintGraph>();
            }

            graph.Stop();   // running coroutines still point at the old nodes
            if (BlueprintSerializer.Load(entry.graph, graph.nodes)) loaded++;
        }

        Debug.Log($"Blueprints: loaded {loaded} graph(s) from {FilePath}");
        return loaded;
    }

    // A graph that is empty or just an unconnected Start node isn't worth a save entry
    static bool IsTrivial(List<Node> nodes)
        => nodes.Count == 0 || (nodes.Count == 1 && nodes[0] is StartEventNode s && s.next == null);

    static SaveFile ReadFile()
    {
        string path = FilePath;
        if (!File.Exists(path)) return null;
        try { return JsonUtility.FromJson<SaveFile>(File.ReadAllText(path)); }
        catch (Exception e)
        {
            Debug.LogError("Blueprints: save file is unreadable, keeping a copy as .corrupt. " + e.Message);
            try { File.Copy(path, path + ".corrupt", true); } catch (Exception) { }
            return null;
        }
    }

    static void Write(SaveFile file)
    {
        string path = FilePath, tmp = path + ".tmp";
        try
        {
            File.WriteAllText(tmp, JsonUtility.ToJson(file, true));   // write the temp file first so a crash can't leave a half-written save
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }
        catch (Exception e) { Debug.LogError("Blueprints: save failed. " + e.Message); }
    }
}

// Identifies a scene object by where it is in the hierarchy: "Scene|Parent[0]/Cube[2]"
public static class ObjectPath
{
    public static string Of(GameObject go)
    {
        var parts = new List<string>();
        for (var t = go.transform; t != null; t = t.parent)
            parts.Add(t.name + "[" + t.GetSiblingIndex() + "]");
        parts.Reverse();
        return go.scene.name + "|" + string.Join("/", parts);
    }

    public static GameObject Find(string path)
    {
        int bar = path.IndexOf('|');
        if (bar < 0) return null;
        string sceneName = path.Substring(0, bar);
        var segments = path.Substring(bar + 1).Split('/');

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded || scene.name != sceneName) continue;

            Transform current = null;
            for (int s = 0; s < segments.Length; s++)
            {
                if (!Parse(segments[s], out var name, out var index)) return null;

                Transform next = null;
                if (s == 0)
                {
                    foreach (var root in scene.GetRootGameObjects())
                        if (root.name == name && root.transform.GetSiblingIndex() == index)
                        { next = root.transform; break; }
                }
                else if (index < current.childCount)
                {
                    var child = current.GetChild(index);
                    if (child.name == name) next = child;
                }

                if (next == null) return null;
                current = next;
            }
            return current.gameObject;
        }
        return null;
    }

    static bool Parse(string segment, out string name, out int index)
    {
        name = null;
        index = 0;
        int b = segment.LastIndexOf('[');
        if (b < 0 || !segment.EndsWith("]")) return false;
        name = segment.Substring(0, b);
        return int.TryParse(segment.Substring(b + 1, segment.Length - b - 2), out index);
    }
}
