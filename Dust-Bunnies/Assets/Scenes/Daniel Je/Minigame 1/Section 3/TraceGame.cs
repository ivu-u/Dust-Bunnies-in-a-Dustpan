using UnityEngine;
using System.Collections.Generic;

public class TraceGame : MonoBehaviour
{
    public LineRenderer line;
    [SerializeField]
    private float tolerance = 0.5f;
    [SerializeField]
    private float maxTime = 3f;
    [SerializeField]
    private List<TraceNode> revisitable = new List<TraceNode>();
    private TraceNode currentNode;
    private HashSet<TraceNode> visited = new HashSet<TraceNode>();
    private float timeRemaining;
    private bool tracing = false;
    private TraceNode[] allNodes;
    private TraceNode startNode;

    void Start()
    {
        allNodes = FindObjectsByType<TraceNode>(FindObjectsSortMode.None);
    }
    void Update()
    {
        if (!tracing)
            CheckStart();

        if (tracing)
            UpdateTracing();
    }

    void CheckStart()
    {
        Vector3 mouse = GetMouseWorld();
        foreach (var node in allNodes)
        {   
            if (Vector3.Distance(mouse, node.transform.position) < tolerance)
            {
                tracing = true;
                currentNode = node;
                startNode = node;
                visited.Clear();
                visited.Add(node);
                timeRemaining = maxTime;

                line.positionCount = 1;
                line.SetPosition(0, currentNode.transform.position);
            }
        }
    }

    void UpdateTracing()
    {
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            Fail("Time ran out");
            return;
        }

        Vector3 mouse = GetMouseWorld();

        TraceNode bestNeighbor = null;
        float bestDist = float.MaxValue;

        foreach (var neighbor in currentNode.neighbors)
        {
            float d = DistancePointToSegment(
                mouse,
                currentNode.transform.position,
                neighbor.transform.position
            );

            if (d < bestDist)
            {
                bestDist = d;
                bestNeighbor = neighbor;
            }
        }

        if (bestNeighbor == null) return;

        if (bestDist > tolerance)
        {
            Fail("Left corridor");
            return;
        }

        // Check if player is close enough to snap to neighbor
        float snapDist = Vector3.Distance(mouse, bestNeighbor.transform.position);

        if (snapDist < tolerance * 0.5f)
        {
            TraceNode pastNode = currentNode;
            currentNode = bestNeighbor;

            if (!visited.Contains(currentNode))
            {
                visited.Add(currentNode);
                AppendLine(currentNode.transform.position);
                timeRemaining = maxTime;
                if (visited.Contains(startNode) && !startNode.Equals(currentNode))
                {
                    visited.Remove(startNode);
                }
            } else if (revisitable.Contains(currentNode) && !currentNode.Equals(pastNode))
            {
                AppendLine(currentNode.transform.position);
            }
        }
        CheckWin();
    }

    void AppendLine(Vector3 pos)
    {
        line.positionCount++;
        line.SetPosition(line.positionCount - 1, pos);
    }

    void CheckWin()
    {

        foreach (var node in allNodes)
        {
            if (!visited.Contains(node))
                return;
        }

        Success();
    }

    void Fail(string reason)
    {
        Debug.Log("FAIL: " + reason);
        tracing = false;
        line.positionCount = 0;
    }

    void Success()
    {
        Debug.Log("SUCCESS");
    }

    Vector3 GetMouseWorld()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        plane.Raycast(ray, out float enter);
        return ray.GetPoint(enter);
    }
    float DistancePointToSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / Vector3.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        Vector3 projection = a + t * ab;
        return Vector3.Distance(p, projection);
    }
}