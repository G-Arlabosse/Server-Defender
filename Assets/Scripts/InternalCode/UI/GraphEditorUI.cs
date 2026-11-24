using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

// Click an object in the scene -> a side panel opens with that object's blueprint.
[RequireComponent(typeof(UIDocument))]
public class GraphEditorUI : MonoBehaviour
{
    public enum Side { Right, Left }

    [Header("Selection")]
    [Tooltip("Camera used to pick objects. Empty = Camera.main")]
    public Camera cam;
    [Tooltip("Clicking an object without a BlueprintGraph gives it one. Off = only objects that already have the component are clickable.")]
    public bool autoAddGraph = true;

    [Header("Saving")]
    [Tooltip("Load blueprints.json when the game starts")]
    public bool autoLoadOnStart = true;
    [Tooltip("Save when the game quits or is paused (mobile)")]
    public bool autoSaveOnQuit = true;
    [Tooltip("Save whenever the side panel is closed")]
    public bool autoSaveOnClose = true;

    [Header("Panel")]
    public Side side = Side.Right;
    [Range(0.25f, 0.9f)] public float width = 0.5f;        // fraction of the screen
    [Range(0.2f, 1f)]    public float opacity = 0.7f;

    [Header("Text (optional)")]
    [Tooltip("Best option: an SDF Font Asset (Assets > Create > Text > Font Asset).")]
    public FontAsset fontAsset;
    [Tooltip("Or a plain .ttf/.otf. An SDF version is generated at startup. Empty = Unity's built-in font.")]
    public Font font;

    VisualElement root, panel, viewHost;
    Label title;
    bool isOpen;
    BlueprintGraph current;
    readonly Dictionary<BlueprintGraph, BlueprintView> views = new();

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.Clear();
        views.Clear();
        isOpen = false;

        var sdf = GetSdfFont();
        if (sdf != null) root.style.unityFontDefinition = FontDefinition.FromSDFFont(sdf);

        // 1) Invisible full-screen layer behind everything: receives clicks that hit the "world"
        var catcher = new VisualElement();
        catcher.StretchToParentSize();
        catcher.RegisterCallback<PointerDownEvent>(OnWorldClick);
        root.Add(catcher);

        // 2) Small top bar
        var bar = new VisualElement();
        bar.style.position = Position.Absolute;
        bar.style.top = 8;
        if (side == Side.Right) bar.style.left = 8; else bar.style.right = 8;
        bar.style.flexDirection = FlexDirection.Row;
        bar.style.alignItems = Align.Center;
        bar.Add(new Button(RunAll) { text = "Run All" });
        bar.Add(new Button(StopAll) { text = "Stop All" });
        bar.Add(new Button(SaveNow) { text = "Save" });
        bar.Add(new Button(LoadNow) { text = "Load" });
        var hint = new Label("Click an object to edit its blueprint");
        hint.style.color = new Color(1, 1, 1, 0.7f);
        hint.style.marginLeft = 8;
        bar.Add(hint);
        root.Add(bar);

        // 3) The side panel (above the catcher, so clicks inside it never reach the world)
        panel = new VisualElement();
        panel.style.position = Position.Absolute;
        panel.style.top = 0;
        panel.style.bottom = 0;
        if (side == Side.Right) panel.style.right = 0; else panel.style.left = 0;
        panel.style.width = Length.Percent(width * 100f);
        panel.style.backgroundColor = new Color(0.05f, 0.05f, 0.07f, opacity);
        UIUtil.Border(panel, 1, new Color(0.35f, 0.35f, 0.40f, 0.8f));
        panel.style.display = DisplayStyle.None;

        var header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.style.justifyContent = Justify.SpaceBetween;
        header.style.alignItems = Align.Center;
        header.style.backgroundColor = new Color(0f, 0f, 0f, 0.35f);
        UIUtil.Pad(header, 10, 6);

        title = new Label();
        title.style.color = Color.white;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.Add(title);
        header.Add(new Button(Close) { text = "X" });

        viewHost = new VisualElement();
        viewHost.style.flexGrow = 1;
        viewHost.style.minHeight = 0;

        panel.Add(header);
        panel.Add(viewHost);
        root.Add(panel);
    }

    void Start()
    {
        if (autoLoadOnStart) LoadNow();
    }

    void OnApplicationQuit()
    {
        if (autoSaveOnQuit) BlueprintSaveSystem.SaveAll();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused && autoSaveOnQuit) BlueprintSaveSystem.SaveAll();
    }

    void Update()
    {
        // the selected object was destroyed while its panel was open
        if (isOpen && current == null) Close();
    }

    // ---------- selection ----------

    void OnWorldClick(PointerDownEvent e)
    {
        if (e.button != 0) return;
        var graph = PickGraph(e.position);
        if (graph != null) Open(graph); else Close();
    }

    BlueprintGraph PickGraph(Vector2 panelPos)
    {
        var c = cam != null ? cam : Camera.main;
        if (c == null) { Debug.LogWarning("GraphEditorUI: no camera (tag one MainCamera or assign Cam)."); return null; }

        // UI Toolkit coordinates (top-left origin, panel units) -> screen pixels (bottom-left origin)
        Vector2 size = root.panel.visualTree.layout.size;
        var screen = new Vector2(panelPos.x * Screen.width / size.x,
                                 Screen.height - panelPos.y * Screen.height / size.y);
        var ray = c.ScreenPointToRay(screen);

        GameObject go = null;
        if (Physics.Raycast(ray, out var hit)) go = hit.collider.gameObject;
        else
        {
            var hit2d = Physics2D.GetRayIntersection(ray);
            if (hit2d.collider != null) go = hit2d.collider.gameObject;
        }
        if (go == null) return null;

        var graph = go.GetComponentInParent<BlueprintGraph>();
        if (graph == null && autoAddGraph) graph = go.AddComponent<BlueprintGraph>();
        return graph;
    }

    // ---------- panel ----------

    void Open(BlueprintGraph g)
    {
        // forget views of objects that no longer exist
        foreach (var dead in views.Keys.Where(k => k == null).ToList())
        {
            views[dead].RemoveFromHierarchy();
            views.Remove(dead);
        }

        // hide the previous object's blueprint
        if (current != null && views.TryGetValue(current, out var old))
            old.style.display = DisplayStyle.None;

        current = g;

        // one cached view per object: its pan/zoom is remembered when you switch back
        if (!views.TryGetValue(g, out var view))
        {
            view = new BlueprintView(g.nodes);
            view.style.flexGrow = 1;
            view.style.backgroundColor = Color.clear;   // the panel provides the see-through background
            view.RunRequested += g.Run;
            view.StopRequested += g.Stop;
            viewHost.Add(view);
            view.Rebuild();
            views[g] = view;
        }
        view.style.display = DisplayStyle.Flex;

        title.text = g.name + "  -  Blueprint";
        panel.style.display = DisplayStyle.Flex;
        isOpen = true;
    }

    void Close()
    {
        if (!isOpen) return;
        if (autoSaveOnClose) BlueprintSaveSystem.SaveAll();
        panel.style.display = DisplayStyle.None;
        isOpen = false;
        current = null;
    }

    void RunAll()  { foreach (var g in BlueprintGraph.All.ToArray()) g.Run(); }
    void StopAll() { foreach (var g in BlueprintGraph.All.ToArray()) g.Stop(); }

    void SaveNow() => BlueprintSaveSystem.SaveAll();

    void LoadNow()
    {
        BlueprintSaveSystem.LoadAll();
        foreach (var v in views.Values) v.Rebuild();   // redraw panels whose graphs were just replaced
    }

    // ---------- crisp text ----------

    FontAsset GetSdfFont()
    {
        if (fontAsset != null) return fontAsset;

        var source = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (source == null) source = Resources.GetBuiltinResource<Font>("Arial.ttf");   // older Unity versions
        if (source == null) return null;

        return FontAsset.CreateFontAsset(source, 64, 8, GlyphRenderMode.SDFAA,
                                         1024, 1024, AtlasPopulationMode.Dynamic, true);
    }
}
