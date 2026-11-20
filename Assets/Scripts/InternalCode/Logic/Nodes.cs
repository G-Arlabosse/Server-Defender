using System;
using System.Collections;
using UnityEngine;

// Entry point: no exec input, the runner starts from its .next
public class StartEventNode : Node
{
    public override bool HasExecIn => false;
    public override string Title => "On Start";
    public override string Category => "Events";
}

// Base class for pure value nodes: no exec pins at all
public abstract class ValueNode : Node
{
    public override string Category => "Math";
    public override bool HasExecIn => false;
    public override string[] ExecOutputs => Array.Empty<string>();
}

// ---------- ACTION NODES ----------

public class MoveNode : Node
{
    public override string Category => "Actions";
    public MoveNode() { In("direction", Vector3.forward); In("duration", 1f); }

    public override IEnumerator Execute(Context c)
    {
        var dir = inputs["direction"].Get<Vector3>(c);
        var dur = inputs["duration"].Get<float>(c);
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            c.target.transform.Translate(dir * Time.deltaTime, Space.World);
            yield return null;
        }
    }
}

public class WaitNode : Node
{
    public override string Category => "Flow";
    public WaitNode() { In("seconds", 1f); }
    public override IEnumerator Execute(Context c)
    {
        yield return new WaitForSeconds(inputs["seconds"].Get<float>(c));
    }
}

public class LogNode : Node
{
    public override string Category => "Debug";
    public LogNode() { In("message", "Hello"); }
    public override IEnumerator Execute(Context c)
    {
        Debug.Log(inputs["message"].Get<string>(c));
        yield break;
    }
}

public class SetColorNode : Node
{
    public override string Category => "Actions";
    public SetColorNode() { In("color", Color.white); }
    public override IEnumerator Execute(Context c)
    {
        c.target.GetComponent<Renderer>().material.color = inputs["color"].Get<Color>(c);
        yield break;
    }
}

public class BranchNode : Node
{
    public override string Category => "Flow";
    public Node nextTrue, nextFalse;
    public BranchNode() { In("condition", false); }

    public override string[] ExecOutputs => new[] { "true", "false" };
    public override Node GetExec(string output) => output == "true" ? nextTrue : nextFalse;
    public override void SetExec(string output, Node target)
    {
        if (output == "true") nextTrue = target; else nextFalse = target;
    }

    public override IEnumerator Execute(Context c)
    {
        var chosen = inputs["condition"].Get<bool>(c) ? nextTrue : nextFalse;
        yield return GraphRunner.RunChain(chosen, c);
    }
}

public class RepeatNode : Node
{
    public override string Category => "Flow";
    public Node body;
    public RepeatNode() { In("count", 3); }

    public override string[] ExecOutputs => new[] { "body", "next" };
    public override Node GetExec(string output) => output == "body" ? body : next;
    public override void SetExec(string output, Node target)
    {
        if (output == "body") body = target; else next = target;
    }

    public override IEnumerator Execute(Context c)
    {
        int n = inputs["count"].Get<int>(c);
        for (int i = 0; i < n; i++)
            yield return GraphRunner.RunChain(body, c);
    }
}

// ---------- VALUE NODES ----------

public class AddNode : ValueNode
{
    public AddNode() { In("a", 0f); In("b", 0f); }
    public override (string name, Type type)[] DataOutputs => new[] { ("result", typeof(float)) };
    public override object Evaluate(string output, Context c)
        => inputs["a"].Get<float>(c) + inputs["b"].Get<float>(c);
}

public class GreaterNode : ValueNode
{
    public GreaterNode() { In("a", 0f); In("b", 0f); }
    public override (string name, Type type)[] DataOutputs => new[] { ("result", typeof(bool)) };
    public override object Evaluate(string output, Context c)
        => inputs["a"].Get<float>(c) > inputs["b"].Get<float>(c);
}

public class GetPositionNode : ValueNode
{
    public override string Category => "Object";
    public override (string name, Type type)[] DataOutputs => new[]
    {
        ("position", typeof(Vector3)),
        ("x", typeof(float)),
        ("y", typeof(float)),
        ("z", typeof(float)),
    };

    public override object Evaluate(string output, Context c)
    {
        var p = c.target.transform.position;
        switch (output)
        {
            case "x": return p.x;
            case "y": return p.y;
            case "z": return p.z;
            default:  return p;
        }
    }
}
