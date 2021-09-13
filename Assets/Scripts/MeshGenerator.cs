using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    [SerializeField]
    Mesh2D shape2D;

    //Number of cuts on the bezier curve, higher = more vertex and triangles 
    [Range(2, 30)]
    [SerializeField]
    int edgeRingCount = 10;

    [SerializeField]
    BezierSpline spine;

    Mesh mesh;

    int lastPointIndex;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "Segment";
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    public void GenerateMesh(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {

        //Generate the vertices
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        for (int r = 0; r < edgeRingCount; r++)
        {
            float t = r / (edgeRingCount - 1f);

            OrientedPoint op = GetBezierOP(p0, p1, p2, p3, t);

            for (int i = 0; i < shape2D.VertexCount; i++)
            {
                verts.Add(op.LocalToWorld(shape2D.vertices[i].point));
                normals.Add(op.LocaltoWorldVect(shape2D.vertices[i].normal));
            }
        }

        //Generate the triangles
        List<int> triIndices = new List<int>();
        for (int r = 0; r < edgeRingCount - 1; r++)
        {
            int rootIndex = r * shape2D.VertexCount;
            int rootIndexNext = (r + 1) * shape2D.VertexCount;

            for (int line = 0; line < shape2D.lineCount; line += 2)
            {
                int lineIndexA = shape2D.lineIndices[line];
                int lineIndexB = shape2D.lineIndices[line + 1];

                int currentA = rootIndex + lineIndexA;
                int currentB = rootIndex + lineIndexB;
                int nextA = rootIndexNext + lineIndexA;
                int nextB = rootIndexNext + lineIndexB;

                triIndices.Add(currentA);
                triIndices.Add(nextA);
                triIndices.Add(nextB);
                triIndices.Add(currentA);
                triIndices.Add(nextB);
                triIndices.Add(currentB);
            }
        }

        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triIndices, 0);
    }

    public void Start()
    {
        mesh.Clear();
        GenerateMesh(spine.getPointList()[0], spine.getPointList()[1], spine.getPointList()[2], spine.getPointList()[3]);
        lastPointIndex = 3;
    }

    private void Update()
    {
        if (!checkCurveMesh())
        {
            combineMesh();
        }
    }

    OrientedPoint GetBezierOP(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 pos = Bezier.GetPoint(p0, p1, p2, p3, t);
        Vector3 tangent = Bezier.GetFirstDerivative(p0, p1, p2, p3, t);

        return new OrientedPoint(pos, tangent);
    }

    public bool checkCurveMesh()
    {
        return lastPointIndex == spine.getPointList().Length - 1;
    }

    public void combineMesh()
    {
        Quaternion oldRot = transform.rotation;
        Vector3 OldPos = transform.position;

        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;

        CombineInstance[] combine = new CombineInstance[2];
        combine[0].mesh = GetComponent<MeshFilter>().sharedMesh;
        combine[0].transform = GetComponent<MeshFilter>().transform.localToWorldMatrix;
        GenerateMesh(spine.getPointList()[lastPointIndex], spine.getPointList()[lastPointIndex + 1], spine.getPointList()[lastPointIndex + 2], spine.getPointList()[lastPointIndex + 3]);
        GetComponent<MeshFilter>().sharedMesh = mesh;
        combine[1].mesh = GetComponent<MeshFilter>().sharedMesh;
        combine[1].transform = GetComponent<MeshFilter>().transform.localToWorldMatrix;

        Mesh newMash = new Mesh();
        newMash.CombineMeshes(combine);
        GetComponent<MeshFilter>().sharedMesh = newMash;

        transform.rotation = oldRot;
        transform.position = OldPos;
        lastPointIndex += 3;
    }
}
