using UnityEngine;

public class GraphTest : MonoBehaviour
{
    void Start()
    {
        var ctx = new Context { target = gameObject };

        // Value nodes (no exec wires, they're pulled from)
        var getPos = new GetPositionNode();
        var greater = new GreaterNode().Set("b", 1.5f).Wire("a", getPos, "x");

        // Action nodes
        var start = new StartEventNode();
        var move = new MoveNode().Set("direction", Vector3.right).Set("duration", 2f);
        var log = new LogNode().Wire("message", getPos, "x");
        var branch = new BranchNode().Wire("condition", greater, "result");
        var red = new SetColorNode().Set("color", Color.red);
        var blue = new SetColorNode().Set("color", Color.blue);

        // Exec wires
        start.Then(move).Then(log).Then(branch);
        branch.nextTrue = red;
        branch.nextFalse = blue;

        // Run: start from the event node's next
        StartCoroutine(GraphRunner.RunChain(start.next, ctx));
    }
}
