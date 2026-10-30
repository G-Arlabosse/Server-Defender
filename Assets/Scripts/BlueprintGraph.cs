using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Put this on any object that should have its own blueprint.
// (GraphEditorUI can add it automatically the first time you click an object.)
public class BlueprintGraph : MonoBehaviour
{
    // Every active graph, so "Run All" / "Stop All" can reach them
    public static readonly List<BlueprintGraph> All = new();

    // This object's own nodes. The editor panel edits this list directly.
    public readonly List<Node> nodes = new();

    [Tooltip("Stable ID used in the save file. Generated when you add the component in the editor. " +
             "Empty (e.g. added at runtime) = the object's hierarchy path is used instead.")]
    public string saveId;

    public string SaveKey => string.IsNullOrEmpty(saveId) ? ObjectPath.Of(gameObject) : saveId;

    void Reset() => saveId = System.Guid.NewGuid().ToString("N");   // called when added in the editor

    [ContextMenu("Regenerate Save ID")]   // use this after duplicating an object
    void RegenerateSaveId() => saveId = System.Guid.NewGuid().ToString("N");

    // For putting this graph inside your own save system
    public string ToJson() => BlueprintSerializer.ToJson(nodes);
    public bool FromJson(string json) { Stop(); return BlueprintSerializer.FromJson(json, nodes); }

    Vector3 startPos;
    Quaternion startRot;

    void Awake()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        nodes.Add(new StartEventNode { position = new Vector2(40, 80) });   // every new graph starts with an entry point
    }

    void OnEnable()  => All.Add(this);
    void OnDisable() => All.Remove(this);

    public void Run()
    {
        Stop();
        transform.SetPositionAndRotation(startPos, startRot);   // each run starts from the same pose

        // The coroutines run on THIS component, so every object's graph runs independently
        var ctx = new Context { target = gameObject };
        foreach (var start in nodes.OfType<StartEventNode>())
            StartCoroutine(GraphRunner.RunChain(start.next, ctx));
    }

    public void Stop() => StopAllCoroutines();
}
