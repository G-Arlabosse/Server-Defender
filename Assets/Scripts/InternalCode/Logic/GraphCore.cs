using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Context
{
    public GameObject target;   // the object this graph controls
}

public class Pin
{
    public string name;
    public Type type;             // used by the UI for colors, fields and connection checks
    public object defaultValue;   // used when nothing is wired in
    public Node sourceNode;       // data wire: which node feeds this pin
    public string sourceOutput;   // ...and which of its outputs

    public T Get<T>(Context c)
    {
        object v = sourceNode != null
            ? sourceNode.Evaluate(sourceOutput, c)
            : defaultValue;

        if (v is T t) return t;
        return (T)Convert.ChangeType(v, typeof(T));
    }
}

public abstract class Node
{
    public string id = Guid.NewGuid().ToString();
    public Vector2 position;                          // where the UI draws this node
    public Node next;                                 // default exec wire
    public Dictionary<string, Pin> inputs = new();    // data input pins

    // ---- What the UI needs to know to draw this node ----
    public virtual string Title =>
        Regex.Replace(GetType().Name.Replace("Node", ""), "(?<!^)([A-Z])", " $1");

    // Which folder this node appears under in the "add node" menu
    public virtual string Category => "Misc";

    public virtual bool HasExecIn => true;

    // Names of the exec output pins. Most nodes have just "next".
    public virtual string[] ExecOutputs => new[] { "next" };

    // Data outputs (value nodes). The name is what Evaluate() receives.
    public virtual (string name, Type type)[] DataOutputs
        => Array.Empty<(string name, Type type)>();

    // Read / write an exec output by name. Nodes with several exec outputs override these.
    public virtual Node GetExec(string output) => next;
    public virtual void SetExec(string output, Node target) => next = target;

    protected void In(string name, object def = null)
        => inputs[name] = new Pin
        {
            name = name,
            defaultValue = def,
            type = def?.GetType() ?? typeof(object)
        };

    public virtual IEnumerator Execute(Context c) { yield break; }
    public virtual object Evaluate(string outputName, Context c) => null;

    public Node Then(Node n) { next = n; return n; }
}

public static class NodeExt
{
    public static T Set<T>(this T n, string pin, object value) where T : Node
    {
        n.inputs[pin].defaultValue = value;
        return n;
    }

    public static T Wire<T>(this T n, string pin, Node source, string output = "value") where T : Node
    {
        n.inputs[pin].sourceNode = source;
        n.inputs[pin].sourceOutput = output;
        return n;
    }
}

public static class GraphRunner
{
    public static IEnumerator RunChain(Node start, Context c)
    {
        for (var n = start; n != null; n = n.next)
            yield return n.Execute(c);
    }
}
