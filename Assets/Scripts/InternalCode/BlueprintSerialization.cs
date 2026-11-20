using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// ------------------------------------------------------------------
// Plain data classes that become JSON. Nodes reference each other by id.
// ------------------------------------------------------------------
[Serializable] public class PinValueData  { public string name, value; }
[Serializable] public class ExecLinkData  { public string from, output, to; }
[Serializable] public class DataLinkData  { public string toNode, toPin, fromNode, fromOutput; }

[Serializable]
public class NodeData
{
    public string id, type;
    public float x, y;
    public List<PinValueData> pins = new();   // default values of input pins
}

[Serializable]
public class GraphData
{
    public int version = 1;                   // bump if the format ever changes
    public List<NodeData> nodes = new();
    public List<ExecLinkData> execLinks = new();
    public List<DataLinkData> dataLinks = new();
}

// ------------------------------------------------------------------
// Node list <-> GraphData / JSON. Use this directly if you want to put a
// graph inside your own save system.
// ------------------------------------------------------------------
public static class BlueprintSerializer
{
    // ---------- public API ----------

    public static string ToJson(List<Node> nodes, bool pretty = false)
        => JsonUtility.ToJson(ToData(nodes), pretty);

    // Replaces the contents of 'into' on success; leaves it untouched on failure.
    public static bool FromJson(string json, List<Node> into)
    {
        try { return Load(JsonUtility.FromJson<GraphData>(json), into); }
        catch (Exception e) { Debug.LogError("Blueprint: could not read graph JSON. " + e.Message); return false; }
    }

    public static GraphData ToData(List<Node> nodes)
    {
        var data = new GraphData();
        foreach (var n in nodes)
        {
            var nd = new NodeData { id = n.id, type = n.GetType().FullName, x = n.position.x, y = n.position.y };

            foreach (var pin in n.inputs.Values)
            {
                string s = FormatValue(pin.defaultValue);
                if (s != null) nd.pins.Add(new PinValueData { name = pin.name, value = s });

                if (pin.sourceNode != null)
                    data.dataLinks.Add(new DataLinkData
                    {
                        toNode = n.id, toPin = pin.name,
                        fromNode = pin.sourceNode.id, fromOutput = pin.sourceOutput
                    });
            }

            foreach (var output in n.ExecOutputs)
            {
                var target = n.GetExec(output);
                if (target != null)
                    data.execLinks.Add(new ExecLinkData { from = n.id, output = output, to = target.id });
            }

            data.nodes.Add(nd);
        }
        return data;
    }

    public static bool Load(GraphData data, List<Node> into)
    {
        if (data == null || data.nodes == null) return false;

        // 1) create every node
        var byId = new Dictionary<string, Node>();
        var created = new List<Node>();
        foreach (var nd in data.nodes)
        {
            if (!NodeTypes.TryGetValue(nd.type ?? "", out var type))
            {
                Debug.LogWarning($"Blueprint: unknown node type '{nd.type}' - skipped.");
                continue;
            }

            var node = (Node)Activator.CreateInstance(type);
            node.id = nd.id;
            node.position = new Vector2(nd.x, nd.y);

            foreach (var pv in nd.pins)
                if (node.inputs.TryGetValue(pv.name, out var pin) && TryParse(pv.value, pin.type, out var val))
                    pin.defaultValue = val;

            byId[nd.id] = node;
            created.Add(node);
        }

        // 2) restore exec wires
        foreach (var l in data.execLinks)
            if (byId.TryGetValue(l.from, out var src) && byId.TryGetValue(l.to, out var dst)
                && Array.IndexOf(src.ExecOutputs, l.output) >= 0)
                src.SetExec(l.output, dst);

        // 3) restore data wires
        foreach (var l in data.dataLinks)
            if (byId.TryGetValue(l.toNode, out var consumer) && byId.TryGetValue(l.fromNode, out var producer)
                && consumer.inputs.TryGetValue(l.toPin, out var inPin))
            {
                inPin.sourceNode = producer;
                inPin.sourceOutput = l.fromOutput;
            }

        into.Clear();
        into.AddRange(created);
        return true;
    }

    // ---------- node type lookup (by name, so renaming files or moving assemblies is fine) ----------

    static Dictionary<string, Type> nodeTypes;
    static Dictionary<string, Type> NodeTypes
    {
        get
        {
            if (nodeTypes != null) return nodeTypes;
            nodeTypes = new Dictionary<string, Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (System.Reflection.ReflectionTypeLoadException e) { types = e.Types; }
                catch (Exception) { continue; }

                foreach (var t in types)
                    if (t != null && typeof(Node).IsAssignableFrom(t) && !t.IsAbstract
                        && t.GetConstructor(Type.EmptyTypes) != null)
                        nodeTypes[t.FullName] = t;
            }
            return nodeTypes;
        }
    }

    // ---------- values <-> strings (culture independent) ----------

    static string FormatValue(object v)
    {
        switch (v)
        {
            case null:       return null;
            case float f:    return f.ToString("R", CultureInfo.InvariantCulture);
            case double d:   return d.ToString("R", CultureInfo.InvariantCulture);
            case string s:   return s;
            case Vector3 p:  return string.Join(";", F(p.x), F(p.y), F(p.z));
            case Color c:    return "#" + ColorUtility.ToHtmlStringRGBA(c);
            case IConvertible conv: return conv.ToString(CultureInfo.InvariantCulture);   // int, bool, ...
        }
        return null;   // unsupported type: not saved, the node keeps its default
    }

    static string F(float f) => f.ToString("R", CultureInfo.InvariantCulture);
    static float P(string s) => float.Parse(s, CultureInfo.InvariantCulture);

    static bool TryParse(string s, Type t, out object result)
    {
        result = null;
        try
        {
            if (t == typeof(string)) { result = s; return true; }
            if (t == typeof(Vector3))
            {
                var p = s.Split(';');
                result = new Vector3(P(p[0]), P(p[1]), P(p[2]));
                return true;
            }
            if (t == typeof(Color))
            {
                if (!ColorUtility.TryParseHtmlString(s, out var c)) return false;
                result = c;
                return true;
            }
            if (t.IsPrimitive)
            {
                result = Convert.ChangeType(s, t, CultureInfo.InvariantCulture);
                return true;
            }
        }
        catch (Exception) { }
        return false;
    }
}
