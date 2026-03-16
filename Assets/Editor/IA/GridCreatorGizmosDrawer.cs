using System.Collections.Generic;
using UnityEngine;

static public class GridCreatorGizmosDrawer
{
    private static List<Matrix4x4> matrices = new List<Matrix4x4>();
    private static List<Vector3> linePoints = new List<Vector3>();
    
    static public void GenerateNodeMatrix(List<PathfindingNode> nodes)
    {
        matrices.Clear();
        foreach (PathfindingNode node in nodes)
        {
            matrices.Add(Matrix4x4.TRS(
                node.position,
                Quaternion.identity,
                Vector3.one));
        }
    }

    static public void GenerateConnectionMatrix(List<PathfindingNode> nodes)
    {
        linePoints.Clear();
        foreach(var node in nodes)
        {
            foreach(int n in node.neighborsIndex)
            {
                linePoints.Add(node.position);
                linePoints.Add(nodes[n].position);
            }
        }
    }

    static public void DrawNodes(Material nodeMaterial, float nodeSize)
    {
        if(matrices.Count == 0) return;
        
        nodeMaterial.SetPass(0);
        
        GL.PushMatrix();
        GL.Begin(GL.QUADS);

        foreach(var mat in matrices)
        {
            Vector3 pos = mat.GetPosition();
            float s = nodeSize * 0.5f;

            GL.Vertex(pos + new Vector3(-s, 0.05f, -s));
            GL.Vertex(pos + new Vector3(-s, 0.05f, s));
            GL.Vertex(pos + new Vector3(s, 0.05f, s));
            GL.Vertex(pos + new Vector3(s, 0.05f, -s));
        }

        GL.End();
        GL.PopMatrix();
    }

    static public void DrawConnections(Material lineMaterial)
    {
        if(linePoints.Count == 0) return;

        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.Begin(GL.LINES);

        for(int i = 0; i < linePoints.Count; i += 2)
        {
            GL.Vertex(linePoints[i]);
            GL.Vertex(linePoints[i+1]);
        }

        GL.End();
        GL.PopMatrix();
    }

    static public void ClearLists()
    {
        matrices.Clear();
        linePoints.Clear();
    }
}
