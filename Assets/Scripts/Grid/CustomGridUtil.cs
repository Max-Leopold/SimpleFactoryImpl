using System;
using UnityEngine;

public class CustomGridUtil
{
    public static Vector3 getTruePos(
        Transform parent,
        float gridSize,
        float yOffset,
        float forwardOffsetValue
    )
    {
        // Debug.Log("CustomGridUtil - Convert pos " + parent.position + " to grid pos");
        return new Vector3(
            Mathf.RoundToInt(parent.position.x / gridSize) * gridSize,
            Mathf.RoundToInt(parent.position.y / gridSize) * gridSize + yOffset,
            Mathf.RoundToInt(parent.position.z / gridSize) * gridSize
        ) + getOffSet(parent.transform, forwardOffsetValue);
    }

    public static Vector3 getTruePos(
        Vector3 position,
        float gridSize
    )
    {
        // Debug.Log("CustomGridUtil - Convert pos " + position + " to grid pos");
        return new Vector3(
            Mathf.RoundToInt(position.x / gridSize) * gridSize,
            Mathf.RoundToInt(position.y / gridSize) * gridSize,
            Mathf.RoundToInt(position.z / gridSize) * gridSize
        );
    }

    public static Quaternion getTrueRot(
        Transform parent,
        float xAngleOffset,
        float yAngleOffset,
        float zAngleOffset
    )
    {
        return Quaternion.Euler(
            xAngleOffset,
            Mathf.RoundToInt(parent.transform.rotation.eulerAngles.y / 90) * 90 + yAngleOffset,
            zAngleOffset);
    }

    public static Quaternion getTrueRot(
        Quaternion rotation,
        float xAngleOffset,
        float yAngleOffset,
        float zAngleOffset
    )
    {
        return Quaternion.Euler(
            xAngleOffset,
            Mathf.RoundToInt(rotation.eulerAngles.y / 90) * 90 + yAngleOffset,
            zAngleOffset);
    }

    public static bool validatePosition(
        Vector3 pos,
        LayerMask layer
    )
    {
        Vector3 truePos = getTruePos(pos, StaticGameVariables.GRIDSIZE);
        // Debug.Log("CustomGridUtils - Check if pos " + truePos + " is valid");
        if (Physics.CheckBox(
            truePos,
            new Vector3(StaticGameVariables.ASSEMBLY_LINE_SIZE / 3, StaticGameVariables.ASSEMBLY_LINE_SIZE / 3,
                StaticGameVariables.ASSEMBLY_LINE_SIZE / 3),
            Quaternion.identity,
            layer)
        )
        {
            return false;
        }

        return true;
    }

    private static Vector3 getMiddlePointPos(
        Transform thisTransform,
        float gridSize
    )
    {
        Transform parent = thisTransform.parent;
        if (parent == null)
        {
            return thisTransform.position;
        }
        else
        {
            return new Vector3(
                Mathf.RoundToInt(parent.position.x / gridSize) * gridSize,
                Mathf.RoundToInt(parent.position.y / gridSize) * gridSize,
                Mathf.RoundToInt(parent.position.z / gridSize) * gridSize
            );
        }
    }

    public static Vector3 getRealPos(
        Transform thisTransform,
        float gridSize,
        float yOffset,
        float forwardOffsetValue
    )
    {
        if (thisTransform.parent.parent == null)
        {
            return thisTransform.position;
        }
        else
        {
            return getTruePos(
                thisTransform.parent.parent,
                gridSize,
                yOffset,
                forwardOffsetValue
            );
        }
    }

    public static Vector3 getOffSet(Transform transform, float forwardOffsetValue)
    {
        Vector3 forwardVector = transform.forward;

        if (Math.Abs(forwardVector.x) > Math.Abs(forwardVector.z))
        {
            if (forwardVector.x > 0)
            {
                return new Vector3(forwardOffsetValue, 0, 0);
            }
            else
            {
                return new Vector3(-forwardOffsetValue, 0, 0);
            }
        }
        else
        {
            if (forwardVector.z > 0)
            {
                return new Vector3(0, 0, forwardOffsetValue);
            }
            else
            {
                return new Vector3(0, 0, -forwardOffsetValue);
            }
        }
    }
}