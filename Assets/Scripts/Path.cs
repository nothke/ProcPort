using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Path : MonoBehaviour
{
    //new public string name;

    public List<Transform> nodes = new List<Transform>();

    public float speedLimit = 50;
    public float validationRange = 40;

    public Color color;

    public List<Path> endNodePaths = new List<Path>();
    public List<int> endNodes = new List<int>();

    public bool loop;

    [HideInInspector]
    public bool activeEdit;

    void OnDrawGizmos()
    {
        if (nodes.Count == 0 && nodes.Count != 0)
        {
            nodes.AddRange(nodes);
        }
        
        //Gizmos.color = new Color32(0, 111, 188, 255);
        Gizmos.color = color;

        for (int i = 0; i < nodes.Count; i++)
        {
            //float dem = (1 / (nodes.Count + 0.1f)) * i;
            //Gizmos.color = new Color(color.r - dem, color.g - dem, color.b - dem);

            if (nodes[i])
            {
                // draw lines between nodes
                if (i != nodes.Count - 1)
                {
                    if (nodes[i + 1])
                        Gizmos.DrawLine(nodes[i].position, nodes[i + 1].position);
                }
                else
                {
                    if (loop)
                        if (nodes[0])
                            Gizmos.DrawLine(nodes[i].position, nodes[0].position);
                }
            }
        }

        if (endNodePaths.Count > 0)
        {
            for (int i = 0; i < endNodes.Count; i++)
            {
                if (endNodePaths[i])
                {

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(nodes[nodes.Count - 1].position, endNodePaths[i].nodes[endNodes[i]].position);
                }
                else
                {
                    endNodePaths.RemoveAt(i);
                    endNodes.RemoveAt(i);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i])
            {
                // draw validation range spheres
                Color lighterColor = new Color(color.r, color.g, color.b, 0.3f);
                Gizmos.color = lighterColor;
                Gizmos.DrawWireSphere(nodes[i].position, validationRange);

                // draw node spherers
                Gizmos.color = color; //new Color32(0, 111, 188, 255);
                Gizmos.DrawSphere(nodes[i].position, 0.1f);
            }
        }
    }


    public void ClearTransformsEditor()
    {
        foreach (Transform t in nodes)
        {
            DestroyImmediate(t.gameObject);
        }

        endNodePaths.Clear();
        endNodes.Clear();
        nodes.Clear();

        Debug.Log("Cleared indeed");
    }

    public void ClearTransforms()
    {
        foreach (Transform t in nodes)
        {
            Destroy(t.gameObject);
        }

        endNodePaths.Clear();
        endNodes.Clear();
        nodes.Clear();

        Debug.Log("Cleared indeed");
    }

    public Transform AddNode(Vector3 position)
    {
        if (nodes.Count == 0)
            transform.position = position;

        GameObject go = new GameObject("PathObject" + nodes.Count);
        go.transform.parent = transform;
        go.transform.position = position;

        nodes.Add(go.transform);

        return go.transform;
    }

    public int NextNode(int n)
    {
        if (n + 1 >= nodes.Count)
        {
            if (!loop)
                return n;
            else
                return 0;
        }
        else
            return n++;
    }
}
