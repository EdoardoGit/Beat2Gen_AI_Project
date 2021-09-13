using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OrientedPoint
{
    public Vector3 pos;
    public Quaternion rot;

    public OrientedPoint( Vector3 pos, Quaternion rot)
    {
        this.pos = pos;
        this.rot = rot;
    }
    public OrientedPoint(Vector3 pos, Vector3 foward)
    {
        this.pos = pos;
        this.rot = Quaternion.LookRotation(foward);
    }

    public Vector3 LocalToWorld(Vector3 localSpacePos)
    {
        return pos+rot* localSpacePos;
    }

    public Vector3 LocaltoWorldVect (Vector3 localSpacePos)
    {
        return rot * localSpacePos;
    }
}
