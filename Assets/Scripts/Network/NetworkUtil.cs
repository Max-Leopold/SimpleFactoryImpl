using SimpleFactoryServerLib.Network.Utils;
using UnityEngine;

public class NetworkUtil
{
    public static Position convertVector3ToPosition(Vector3 vector3)
    {
        return new Position(
            (int) vector3.x,
            (int) vector3.y,
            (int) vector3.z
        );
    }

    public static Rotation convertQuaternionToPosition(Quaternion quaternion)
    {
        return new Rotation(
            quaternion.eulerAngles.x,
            quaternion.eulerAngles.y,
            quaternion.eulerAngles.z
        );
    }
}