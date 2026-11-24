using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum PinKind { ExecIn, ExecOut, DataIn, DataOut }

public class PinView
{
    public NodeView owner;
    public string name;
    public PinKind kind;
    public Type type;                 // null for exec pins
    public VisualElement element;

    public bool IsExec => kind == PinKind.ExecIn || kind == PinKind.ExecOut;
    public bool IsOutput => kind == PinKind.ExecOut || kind == PinKind.DataOut;
}

// ------------------------------------------------------------------
// Small styling / field helpers
// ------------------------------------------------------------------
public static class UIUtil
{
    public static Color TypeColor(Type t)
    {
        if (t == typeof(float))   return new Color(0.40f, 0.90f, 0.40f);
        if (t == typeof(int))     return new Color(0.30f, 0.85f, 0.85f);
        if (t == typeof(bool))    return new Color(0.90f, 0.30f, 0.30f);
        if (t == typeof(string))  return new Color(0.90f, 0.40f, 0.90f);
        if (t == typeof(Vector3)) return new Color(1.00f, 0.85f, 0.30f);
        if (t == typeof(Color))   return new Color(1.00f, 0.60f, 0.20f);
        return Color.gray;
    }

    public static void Border(VisualElement e, float width, Color color, float radius = 0)
    {
        e.style.borderTopWidth = e.style.borderBottomWidth =
            e.style.borderLeftWidth = e.style.borderRightWidth = width;
        e.style.borderTopColor = e.style.borderBottomColor =
            e.style.borderLeftColor = e.style.borderRightColor = color;
        e.style.borderTopLeftRadius = e.style.borderTopRightRadius =
            e.style.borderBottomLeftRadius = e.style.borderBottomRightRadius = radius;
    }

    public static void Pad(VisualElement e, float h, float v)
    {
        e.style.paddingLeft = e.style.paddingRight = h;
        e.style.paddingTop = e.style.paddingBottom = v;
    }

    // Dark background + white text for the typing area, so it is readable whatever the UI theme is
    static void StyleInput(VisualElement field)
    {
        var input = field.Q(className: "unity-base-text-field__input");
        if (input == null) return;   // e.g. Toggle has no text input
        input.style.backgroundColor = new Color(0.20f, 0.20f, 0.24f);
        input.style.color = Color.white;
        input.style.unityTextAlign = TextAnchor.MiddleLeft;
        input.style.paddingLeft = input.style.paddingRight = 3;
        input.style.minHeight = 18;
    }

    // Unity's Vector3Field collapses into a tall, cramped layout when it is narrow,
    // so this builds the same thing by hand: X [..] Y [..] Z [..] on one row.
    static VisualElement MakeVector3Field(Pin pin, Vector3 start)
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;

        string[] axes = { "X", "Y", "Z" };
        Color[] axisColors =
        {
            new Color(1.0f, 0.45f, 0.45f),
            new Color(0.5f, 1.0f, 0.5f),
            new Color(0.5f, 0.7f, 1.0f)
        };

        for (int i = 0; i < 3; i++)
        {
            int axis = i;   // captured by the callback below

            var label = new Label(axes[i]);
            label.style.color = axisColors[i];
            label.style.marginLeft = 3;
            label.style.marginRight = 1;
            row.Add(label);

            var field = new FloatField { value = start[i] };
            field.style.width = 52;
            field.style.marginLeft = field.style.marginRight = 0;
            field.RegisterValueChangedCallback(e =>
            {
                var current = pin.defaultValue is Vector3 vec ? vec : Vector3.zero;
                current[axis] = e.newValue;
                pin.defaultValue = current;
            });
            StyleInput(field);
            row.Add(field);
        }
        return row;
    }

    // Creates an editable field for a pin's default value (null if the type isn't supported)
    public static VisualElement MakeField(Pin pin)
    {
        switch (pin.defaultValue)
        {
            case float f:
            {
                var field = new FloatField { value = f };
                field.style.width = 60;
                field.RegisterValueChangedCallback(e => pin.defaultValue = e.newValue);
                StyleInput(field);
                return field;
            }
            case int i:
            {
                var field = new IntegerField { value = i };
                field.style.width = 60;
                field.RegisterValueChangedCallback(e => pin.defaultValue = e.newValue);
                StyleInput(field);
                return field;
            }
            case bool b:
            {
                var field = new Toggle { value = b };
                field.RegisterValueChangedCallback(e => pin.defaultValue = e.newValue);
                StyleInput(field);
                return field;
            }
            case string s:
            {
                var field = new TextField { value = s };
                field.style.width = 90;
                field.RegisterValueChangedCallback(e => pin.defaultValue = e.newValue);
                StyleInput(field);
                return field;
            }
            case Vector3 v:
                return MakeVector3Field(pin, v);
            case Color c:
            {
                // ColorField is editor-only, so colors are edited as hex text
                var field = new TextField { value = "#" + ColorUtility.ToHtmlStringRGB(c) };
                field.style.width = 80;
                field.RegisterValueChangedCallback(e =>
                {
                    if (ColorUtility.TryParseHtmlString(e.newValue, out var col)) pin.defaultValue = col;
                });
                StyleInput(field);
                return field;
            }
        }
        return null;
    }
}

// ------------------------------------------------------------------
// One node on the canvas
// ------------------------------------------------------------------
public class NodeView : VisualElement
{
    public readonly Node node;
    public PinView execIn;
    public readonly Dictionary<string, PinView> execOuts = new();
    public readonly Dictionary<string, PinView> dataIns = new();
    public readonly Dictionary<string, PinView> dataOuts = new();

    readonly BlueprintView graph;
    readonly Dictionary<string, VisualElement> fields = new();

    public NodeView(BlueprintView graph, Node node)
    {
        this.graph = graph;
        this.node = node;

        style.position = Position.Absolute;
        style.left = node.position.x;
        style.top = node.position.y;
        style.minWidth = 180;
        style.backgroundColor = new Color(0.14f, 0.14f, 0.17f, 0.97f);
        UIUtil.Border(this, 1, new Color(0.35f, 0.35f, 0.40f), 6);

        // --- title bar (drag handle) ---
        Color headerColor =
            node is StartEventNode ? new Color(0.65f, 0.15f, 0.15f) :
            node is ValueNode      ? new Color(0.15f, 0.45f, 0.20f) :
                                     new Color(0.15f, 0.30f, 0.60f);
        var title = new Label(node.Title);
        title.style.backgroundColor = headerColor;
        title.style.color = Color.white;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.borderTopLeftRadius = title.style.borderTopRightRadius = 5;
        UIUtil.Pad(title, 8, 4);
        title.RegisterCallback<PointerDownEvent>(e => graph.OnNodeDown(this, e));
        Add(title);

        // clicking anywhere else on a node must not pan the canvas
        RegisterCallback<PointerDownEvent>(e => e.StopPropagation());

        // --- body: left column = inputs, right column = outputs ---
        var body = new VisualElement();
        body.style.flexDirection = FlexDirection.Row;
        body.style.paddingTop = body.style.paddingBottom = 4;
        Add(body);

        var left = new VisualElement();
        left.style.flexGrow = 1;
        var right = new VisualElement();
        right.style.alignItems = Align.FlexEnd;
        right.style.marginLeft = 16;
        body.Add(left);
        body.Add(right);

        if (node.HasExecIn)
        {
            var row = Row(false);
            execIn = MakePin(PinKind.ExecIn, "in", null);
            row.Add(execIn.element);
            left.Add(row);
        }

        foreach (var kv in node.inputs)
        {
            var row = Row(false);
            var pv = MakePin(PinKind.DataIn, kv.Key, kv.Value.type);
            dataIns[kv.Key] = pv;
            row.Add(pv.element);
            row.Add(MakeLabel(kv.Key));
            var field = UIUtil.MakeField(kv.Value);
            if (field != null) { row.Add(field); fields[kv.Key] = field; }
            left.Add(row);
        }

        foreach (var name in node.ExecOutputs)
        {
            var row = Row(true);
            var pv = MakePin(PinKind.ExecOut, name, null);
            execOuts[name] = pv;
            row.Add(MakeLabel(name));
            row.Add(pv.element);
            right.Add(row);
        }

        foreach (var o in node.DataOutputs)
        {
            var row = Row(true);
            var pv = MakePin(PinKind.DataOut, o.name, o.type);
            dataOuts[o.name] = pv;
            row.Add(MakeLabel(o.name));
            row.Add(pv.element);
            right.Add(row);
        }

        RefreshFields();
    }

    // Hide the default-value field while the pin has a wire plugged in
    public void RefreshFields()
    {
        foreach (var kv in fields)
            kv.Value.style.display =
                node.inputs[kv.Key].sourceNode == null ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public IEnumerable<PinView> AllPins()
    {
        if (execIn != null) yield return execIn;
        foreach (var p in execOuts.Values) yield return p;
        foreach (var p in dataIns.Values) yield return p;
        foreach (var p in dataOuts.Values) yield return p;
    }

    static VisualElement Row(bool alignRight)
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.minHeight = 22;
        row.style.justifyContent = alignRight ? Justify.FlexEnd : Justify.FlexStart;
        return row;
    }

    static Label MakeLabel(string text)
    {
        var l = new Label(text);
        l.style.color = new Color(0.85f, 0.85f, 0.85f);
        l.style.marginLeft = l.style.marginRight = 3;
        return l;
    }

    PinView MakePin(PinKind kind, string name, Type type)
    {
        bool exec = kind == PinKind.ExecIn || kind == PinKind.ExecOut;
        var el = new VisualElement();
        el.style.width = el.style.height = 12;
        el.style.marginLeft = el.style.marginRight = 4;
        el.style.backgroundColor = exec ? Color.white : UIUtil.TypeColor(type);
        el.style.borderTopLeftRadius = el.style.borderTopRightRadius =
            el.style.borderBottomLeftRadius = el.style.borderBottomRightRadius = exec ? 2 : 6;

        var pv = new PinView { owner = this, name = name, kind = kind, type = type, element = el };
        el.RegisterCallback<PointerDownEvent>(e => graph.OnPinDown(pv, e));
        return pv;
    }
}

// ------------------------------------------------------------------
// The canvas: pan / zoom, wires, menus, connecting pins
// ------------------------------------------------------------------
public class BlueprintView : VisualElement
{
    public event Action RunRequested, StopRequested;

    readonly List<Node> nodes;
    readonly VisualElement content = new();     // everything that pans/zooms lives in here
    readonly VisualElement wireLayer = new();
    readonly Dictionary<Node, NodeView> views = new();

    enum Mode { None, Pan, DragNode, DragWire }
    Mode mode;
    NodeView dragNode;
    Vector2 grabOffset;
    PinView wireStart;
    Vector2 wireEnd;          // in content space
    Vector2 lastPointer;      // in panel space
    float zoom = 1f;
    VisualElement menu;

    // All concrete Node types, found by reflection, used to fill the "add node" menu
    static readonly List<(string category, string title, Type type)> Catalog = BuildCatalog();

    static List<(string category, string title, Type type)> BuildCatalog()
    {
        var list = new List<(string category, string title, Type type)>();
        foreach (var t in typeof(Node).Assembly.GetTypes())
        {
            if (!typeof(Node).IsAssignableFrom(t) || t.IsAbstract) continue;
            if (t.GetConstructor(Type.EmptyTypes) == null) continue;
            var n = (Node)Activator.CreateInstance(t);
            list.Add((n.Category, n.Title, t));
        }
        list.Sort((a, b) =>
        {
            int c = string.Compare(a.category, b.category, StringComparison.Ordinal);
            return c != 0 ? c : string.Compare(a.title, b.title, StringComparison.Ordinal);
        });
        return list;
    }

    public BlueprintView(List<Node> nodes)
    {
        this.nodes = nodes;

        style.overflow = Overflow.Hidden;
        style.backgroundColor = new Color(0.07f, 0.07f, 0.09f, 0.8f);   // see-through so you can watch the scene

        content.style.position = Position.Absolute;
        content.style.left = 0;
        content.style.top = 0;
        content.style.width = Length.Percent(100);
        content.style.height = Length.Percent(100);
        content.style.transformOrigin = new TransformOrigin(Length.Percent(0), Length.Percent(0));
        Add(content);

        wireLayer.style.position = Position.Absolute;
        wireLayer.style.left = wireLayer.style.top = wireLayer.style.right = wireLayer.style.bottom = 0;
        wireLayer.pickingMode = PickingMode.Ignore;
        wireLayer.generateVisualContent += DrawWires;
        wireLayer.schedule.Execute(wireLayer.MarkDirtyRepaint).Every(16);   // simple: repaint constantly
        content.Add(wireLayer);

        BuildToolbar();

        RegisterCallback<PointerDownEvent>(OnPointerDown);
        RegisterCallback<PointerMoveEvent>(OnPointerMove);
        RegisterCallback<PointerUpEvent>(OnPointerUp);
        RegisterCallback<WheelEvent>(OnWheel);
    }

    void BuildToolbar()
    {
        var bar = new VisualElement();
        bar.style.position = Position.Absolute;
        bar.style.left = 8;
        bar.style.top = 8;
        bar.style.flexDirection = FlexDirection.Row;
        bar.Add(new Button(() => RunRequested?.Invoke()) { text = "Run" });
        bar.Add(new Button(() => StopRequested?.Invoke()) { text = "Stop" });
        Add(bar);

        var hint = new Label("Right-click: add node / title menu / disconnect pin   |   Drag empty space: pan   |   Wheel: zoom");
        hint.style.position = Position.Absolute;
        hint.style.left = 8;
        hint.style.bottom = 6;
        hint.style.color = new Color(1, 1, 1, 0.5f);
        hint.pickingMode = PickingMode.Ignore;
        Add(hint);
    }

    // ---------- building views ----------

    public void Rebuild()
    {
        foreach (var v in views.Values) v.RemoveFromHierarchy();
        views.Clear();
        foreach (var n in nodes) AddView(n);
        RefreshAll();
    }

    void AddView(Node n)
    {
        var v = new NodeView(this, n);
        views[n] = v;
        content.Add(v);
    }

    void RefreshAll()
    {
        foreach (var v in views.Values) v.RefreshFields();
        wireLayer.MarkDirtyRepaint();
    }

    // ---------- wire drawing ----------

    Vector2 PinPos(PinView pv) => content.WorldToLocal(pv.element.worldBound.center);

    void DrawWires(MeshGenerationContext mgc)
    {
        var p = mgc.painter2D;
        p.lineWidth = 3;

        foreach (var v in views.Values)
        {
            foreach (var kv in v.execOuts)
            {
                var target = v.node.GetExec(kv.Key);
                if (target != null && views.TryGetValue(target, out var tv) && tv.execIn != null)
                    DrawWire(p, PinPos(kv.Value), PinPos(tv.execIn), Color.white);
            }

            foreach (var kv in v.dataIns)
            {
                var pin = v.node.inputs[kv.Key];
                if (pin.sourceNode != null
                    && views.TryGetValue(pin.sourceNode, out var sv)
                    && sv.dataOuts.TryGetValue(pin.sourceOutput, out var outPin))
                    DrawWire(p, PinPos(outPin), PinPos(kv.Value), UIUtil.TypeColor(outPin.type));
            }
        }

        // the wire currently being dragged
        if (mode == Mode.DragWire && wireStart != null)
        {
            var s = PinPos(wireStart);
            if (wireStart.IsOutput) DrawWire(p, s, wireEnd, Color.gray);
            else                    DrawWire(p, wireEnd, s, Color.gray);
        }
    }

    static void DrawWire(Painter2D p, Vector2 a, Vector2 b, Color color)
    {
        if (float.IsNaN(a.x) || float.IsNaN(a.y) || float.IsNaN(b.x) || float.IsNaN(b.y)) return;  // layout not ready yet
        float dx = Mathf.Max(50f, Mathf.Abs(b.x - a.x) * 0.5f);
        p.strokeColor = color;
        p.BeginPath();
        p.MoveTo(a);
        p.BezierCurveTo(a + Vector2.right * dx, b + Vector2.left * dx, b);
        p.Stroke();
    }

    // ---------- pointer handling ----------

    void OnPointerDown(PointerDownEvent e)
    {
        CloseMenu();
        if (e.target != this && e.target != content) return;   // only the empty background pans

        if (e.button == 1) { ShowAddMenu(e.position); return; }
        if (e.button == 0 || e.button == 2)
        {
            mode = Mode.Pan;
            lastPointer = e.position;
            this.CapturePointer(e.pointerId);
        }
    }

    public void OnNodeDown(NodeView v, PointerDownEvent e)
    {
        CloseMenu();
        e.StopPropagation();

        if (e.button == 0)
        {
            mode = Mode.DragNode;
            dragNode = v;
            grabOffset = content.WorldToLocal(e.position) - v.node.position;
            this.CapturePointer(e.pointerId);
            v.BringToFront();
        }
        else if (e.button == 1)
        {
            ShowMenu(e.position, new List<(string label, Action action)>
            {
                ("Delete node", () => DeleteNode(v))
            });
        }
    }

    public void OnPinDown(PinView pv, PointerDownEvent e)
    {
        CloseMenu();
        e.StopPropagation();

        if (e.button == 1) { ClearPin(pv); return; }        // right-click = disconnect
        if (e.button != 0) return;

        mode = Mode.DragWire;
        wireStart = pv;
        wireEnd = PinPos(pv);
        this.CapturePointer(e.pointerId);
    }

    void OnPointerMove(PointerMoveEvent e)
    {
        switch (mode)
        {
            case Mode.Pan:
            {
                Vector2 now = e.position;
                content.transform.position += (Vector3)(now - lastPointer);
                lastPointer = now;
                break;
            }
            case Mode.DragNode:
            {
                Vector2 p = content.WorldToLocal(e.position) - grabOffset;
                dragNode.node.position = p;
                dragNode.style.left = p.x;
                dragNode.style.top = p.y;
                break;
            }
            case Mode.DragWire:
                wireEnd = content.WorldToLocal(e.position);
                break;
        }
    }

    void OnPointerUp(PointerUpEvent e)
    {
        if (mode == Mode.DragWire)
        {
            var target = PinAt(e.position);
            if (target != null) TryConnect(wireStart, target);
        }

        mode = Mode.None;
        wireStart = null;
        dragNode = null;
        if (this.HasPointerCapture(e.pointerId)) this.ReleasePointer(e.pointerId);
        wireLayer.MarkDirtyRepaint();
    }

    void OnWheel(WheelEvent e)
    {
        float newZoom = Mathf.Clamp(zoom * (e.delta.y > 0 ? 0.9f : 1.1f), 0.3f, 2f);
        Vector2 local = content.WorldToLocal(e.mousePosition);   // point under the cursor, in content space
        Vector2 screen = this.WorldToLocal(e.mousePosition);

        zoom = newZoom;
        content.transform.scale = new Vector3(zoom, zoom, 1);
        content.transform.position = screen - zoom * local;       // keep that point under the cursor
        e.StopPropagation();
    }

    // Finds the pin under a panel-space point (with a little slack so it's easy to hit)
    PinView PinAt(Vector2 world)
    {
        foreach (var v in views.Values)
            foreach (var pv in v.AllPins())
            {
                var r = pv.element.worldBound;
                r.xMin -= 8; r.xMax += 8; r.yMin -= 8; r.yMax += 8;
                if (r.Contains(world)) return pv;
            }
        return null;
    }

    // ---------- connecting ----------

    void TryConnect(PinView a, PinView b)
    {
        // must be one output + one input, same family (exec/data), different nodes
        if (a.IsOutput == b.IsOutput || a.IsExec != b.IsExec || a.owner == b.owner) return;

        var from = a.IsOutput ? a : b;
        var to = a.IsOutput ? b : a;

        if (from.IsExec)
        {
            if (ExecReaches(to.owner.node, from.owner.node, null))
            {
                Debug.LogWarning("That exec connection would create a loop.");
                return;
            }
            from.owner.node.SetExec(from.name, to.owner.node);   // an exec output has one target: replaces the old one
        }
        else
        {
            if (!CanConvert(from.type, to.type)) return;
            if (DataDependsOn(from.owner.node, to.owner.node))
            {
                Debug.LogWarning("That data connection would create a loop.");
                return;
            }
            var pin = to.owner.node.inputs[to.name];             // a data input has one source: replaces the old one
            pin.sourceNode = from.owner.node;
            pin.sourceOutput = from.name;
        }
        RefreshAll();
    }

    static bool CanConvert(Type from, Type to)
    {
        if (from == to || to == typeof(string) || to == typeof(object)) return true;
        return from.IsPrimitive && to.IsPrimitive;   // float/int/bool convert between each other
    }

    // Would an exec edge into 'n' ... reach 'goal'? (cycle check)
    static bool ExecReaches(Node n, Node goal, HashSet<Node> seen)
    {
        if (n == goal) return true;
        seen ??= new HashSet<Node>();
        if (!seen.Add(n)) return false;
        foreach (var o in n.ExecOutputs)
        {
            var t = n.GetExec(o);
            if (t != null && ExecReaches(t, goal, seen)) return true;
        }
        return false;
    }

    static bool DataDependsOn(Node n, Node goal)
    {
        if (n == goal) return true;
        foreach (var p in n.inputs.Values)
            if (p.sourceNode != null && DataDependsOn(p.sourceNode, goal)) return true;
        return false;
    }

    // ---------- disconnecting / deleting ----------

    void ClearPin(PinView pv)
    {
        var n = pv.owner.node;
        switch (pv.kind)
        {
            case PinKind.ExecOut:
                n.SetExec(pv.name, null);
                break;
            case PinKind.DataIn:
            {
                var p = n.inputs[pv.name];
                p.sourceNode = null;
                p.sourceOutput = null;
                break;
            }
            case PinKind.ExecIn:
                ClearExecTo(n);
                break;
            case PinKind.DataOut:
                ClearDataFrom(n, pv.name);
                break;
        }
        RefreshAll();
    }

    void ClearExecTo(Node target)
    {
        foreach (var o in nodes)
            foreach (var name in o.ExecOutputs)
                if (o.GetExec(name) == target) o.SetExec(name, null);
    }

    void ClearDataFrom(Node source, string output)   // output == null: all outputs
    {
        foreach (var o in nodes)
            foreach (var p in o.inputs.Values)
                if (p.sourceNode == source && (output == null || p.sourceOutput == output))
                {
                    p.sourceNode = null;
                    p.sourceOutput = null;
                }
    }

    void DeleteNode(NodeView v)
    {
        ClearExecTo(v.node);
        ClearDataFrom(v.node, null);
        nodes.Remove(v.node);
        views.Remove(v.node);
        v.RemoveFromHierarchy();
        RefreshAll();
    }

    // ---------- popup menus ----------

    // Empty popup frame at a panel-space position. It swallows clicks and wheel events
    // so interacting with it never pans, zooms or closes it.
    VisualElement CreateMenuShell(Vector2 world, out Vector2 localPos)
    {
        CloseMenu();
        menu = new VisualElement();
        menu.style.position = Position.Absolute;
        localPos = this.WorldToLocal(world);
        menu.style.left = localPos.x;
        menu.style.top = localPos.y;
        menu.style.minWidth = 170;
        menu.style.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 0.98f);
        UIUtil.Border(menu, 1, new Color(0.4f, 0.4f, 0.45f), 4);
        menu.RegisterCallback<PointerDownEvent>(e => e.StopPropagation());
        menu.RegisterCallback<WheelEvent>(e => e.StopPropagation());
        Add(menu);
        return menu;
    }

    static void StyleMenuButton(Button b)
    {
        b.style.unityTextAlign = TextAnchor.MiddleLeft;
        b.style.marginLeft = b.style.marginRight = b.style.marginTop = b.style.marginBottom = 0;
        b.style.borderTopLeftRadius = b.style.borderTopRightRadius =
            b.style.borderBottomLeftRadius = b.style.borderBottomRightRadius = 0;
    }

    // Simple flat menu (used for "Delete node")
    void ShowMenu(Vector2 world, List<(string label, Action action)> items)
    {
        var shell = CreateMenuShell(world, out _);
        foreach (var item in items)
        {
            var action = item.action;
            var b = new Button(() => { CloseMenu(); action(); }) { text = item.label };
            StyleMenuButton(b);
            shell.Add(b);
        }
    }

    // The "add node" menu: scrollable, with collapsible categories
    void ShowAddMenu(Vector2 world)
    {
        Vector2 spawn = content.WorldToLocal(world);
        var shell = CreateMenuShell(world, out var local);

        // keep it on screen: nudge up if near the bottom, cap the height
        float top = Mathf.Clamp(local.y, 0, Mathf.Max(0, resolvedStyle.height - 220));
        shell.style.top = top;

        var scroll = new ScrollView();
        scroll.style.maxHeight = Mathf.Max(120, Mathf.Min(380, resolvedStyle.height - top - 12));
        shell.Add(scroll);

        VisualElement items = null;
        string current = null;
        foreach (var entry in Catalog)
        {
            if (entry.category != current)           // Catalog is sorted, so categories come in runs
            {
                current = entry.category;
                items = AddCategory(scroll, current);
            }

            var type = entry.type;
            var b = new Button(() => { CloseMenu(); SpawnNode(type, spawn); }) { text = entry.title };
            StyleMenuButton(b);
            b.style.marginLeft = 14;
            items.Add(b);
        }
    }

    // A clickable header plus a container that it shows / hides. Starts collapsed.
    static VisualElement AddCategory(VisualElement parent, string name)
    {
        var items = new VisualElement();
        items.style.display = DisplayStyle.None;

        bool open = false;
        var header = new Button { text = "+  " + name };
        StyleMenuButton(header);
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.style.backgroundColor = new Color(0.20f, 0.20f, 0.26f);
        header.clicked += () =>
        {
            open = !open;
            items.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            header.text = (open ? "-  " : "+  ") + name;
        };

        parent.Add(header);
        parent.Add(items);
        return items;
    }

    void SpawnNode(Type type, Vector2 position)
    {
        var n = (Node)Activator.CreateInstance(type);
        n.position = position;
        nodes.Add(n);
        AddView(n);
    }

    void CloseMenu()
    {
        menu?.RemoveFromHierarchy();
        menu = null;
    }
}
