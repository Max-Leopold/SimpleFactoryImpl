using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SimpleFactoryServerLib.Objects;

public class AssemblyLinePrefabMap : MonoBehaviour
{

    public GameObject AssemblyLine_X_Z_Straight;
    public GameObject AssemblyLine_X_Z_Curve;

    public GameObject AssemblyLine_X_Y;
    public GameObject AssemblyLine_Y_Y;
    public GameObject AssemblyLine_Y_X;

    private Dictionary<Vector3Pair, PrefabRotation> assemblyLinePrefabMap = new Dictionary<Vector3Pair, PrefabRotation>();

    public class Vector3Pair : IEquatable<Vector3Pair>
    {
        public Vector3 first;
        public Vector3 second;
        public Vector3Pair(Vector3 first, Vector3 second) {
            this.first = first;
            this.second = second;
        }

        public bool Equals(Vector3Pair other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return first.Equals(other.first) && second.Equals(other.second);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Vector3Pair) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (first.GetHashCode() * 397) ^ second.GetHashCode();
            }
        }
    }

    public struct PrefabRotation
    {
        public Quaternion rotation;
        public GameObject prefab;
        public AssemblyLineEnum assemblyLineEnum;

        public PrefabRotation(GameObject prefab, Quaternion rotation, AssemblyLineEnum assemblyLineEnum)
        {
            this.rotation = rotation;
            this.prefab = prefab;
            this.assemblyLineEnum = assemblyLineEnum;
        }
    }

    public void Start()
    {
        //X Z Straight
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,0,-1), new Vector3(0,0,1)), new PrefabRotation(AssemblyLine_X_Z_Straight, Quaternion.Euler(0,0,0), AssemblyLineEnum.AssemblyLine_X_Z_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,0,1), new Vector3(0,0,-1)), new PrefabRotation(AssemblyLine_X_Z_Straight, Quaternion.Euler(0,0,0), AssemblyLineEnum.AssemblyLine_X_Z_Straight));
        
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(-1,0,0), new Vector3(1,0,0)), new PrefabRotation(AssemblyLine_X_Z_Straight, Quaternion.Euler(0,90,0), AssemblyLineEnum.AssemblyLine_X_Z_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(1,0,0), new Vector3(-1,0,0)), new PrefabRotation(AssemblyLine_X_Z_Straight, Quaternion.Euler(0,90,0), AssemblyLineEnum.AssemblyLine_X_Z_Straight));
        
        //X Z Curve
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,0,1), new Vector3(1,0,0)), new PrefabRotation(AssemblyLine_X_Z_Curve, Quaternion.Euler(0,180,0), AssemblyLineEnum.AssemblyLine_X_Z_Curve));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(1,0,0), new Vector3(0,0,1)), new PrefabRotation(AssemblyLine_X_Z_Curve, Quaternion.Euler(0,180,0), AssemblyLineEnum.AssemblyLine_X_Z_Curve));
        
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,0,-1), new Vector3(1,0,0)), new PrefabRotation(AssemblyLine_X_Z_Curve, Quaternion.Euler(0,270,0), AssemblyLineEnum.AssemblyLine_X_Z_Curve));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(1,0,0), new Vector3(0,0,-1)), new PrefabRotation(AssemblyLine_X_Z_Curve, Quaternion.Euler(0,270,0), AssemblyLineEnum.AssemblyLine_X_Z_Curve));
        
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(-1,0,0), new Vector3(0,0,-1)), new PrefabRotation(AssemblyLine_X_Z_Curve, Quaternion.Euler(0,0,0), AssemblyLineEnum.AssemblyLine_X_Z_Curve));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,0,-1), new Vector3(-1,0,0)), new PrefabRotation(AssemblyLine_X_Z_Curve, Quaternion.Euler(0,0,0), AssemblyLineEnum.AssemblyLine_X_Z_Curve));
        
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(-1,0,0), new Vector3(0,0,1)), new PrefabRotation(AssemblyLine_X_Z_Curve, Quaternion.Euler(0,90,0), AssemblyLineEnum.AssemblyLine_X_Z_Curve));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,0,1), new Vector3(-1,0,0)), new PrefabRotation(AssemblyLine_X_Z_Curve, Quaternion.Euler(0,90,0), AssemblyLineEnum.AssemblyLine_X_Z_Curve));
        
        //X Y
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(1,0,0), new Vector3(0,-1,0)), new PrefabRotation(AssemblyLine_X_Y, Quaternion.Euler(0,270,0), AssemblyLineEnum.AssemblyLine_X_Y_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(-1,0,0), new Vector3(0,-1,0)), new PrefabRotation(AssemblyLine_X_Y, Quaternion.Euler(0,90,0), AssemblyLineEnum.AssemblyLine_X_Y_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,0,1), new Vector3(0,-1,0)), new PrefabRotation(AssemblyLine_X_Y, Quaternion.Euler(0,180,0), AssemblyLineEnum.AssemblyLine_X_Y_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,0,-1), new Vector3(0,-1,0)), new PrefabRotation(AssemblyLine_X_Y, Quaternion.Euler(0,0,0), AssemblyLineEnum.AssemblyLine_X_Y_Straight));
        
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,-1,0), new Vector3(1,0,0)), new PrefabRotation(AssemblyLine_X_Y, Quaternion.Euler(0,270,0), AssemblyLineEnum.AssemblyLine_X_Y_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,-1,0), new Vector3(-1,0,0)), new PrefabRotation(AssemblyLine_X_Y, Quaternion.Euler(0,90,0), AssemblyLineEnum.AssemblyLine_X_Y_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,-1,0), new Vector3(0,0,1)), new PrefabRotation(AssemblyLine_X_Y, Quaternion.Euler(0,180,0), AssemblyLineEnum.AssemblyLine_X_Y_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,-1,0), new Vector3(0,0,-1)), new PrefabRotation(AssemblyLine_X_Y, Quaternion.Euler(0,0,0), AssemblyLineEnum.AssemblyLine_X_Y_Straight));
        
        //Y Y
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,-1,0), new Vector3(0,1,0)), new PrefabRotation(AssemblyLine_Y_Y, Quaternion.Euler(0,0,0), AssemblyLineEnum.AssemblyLine_Y_Y));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,1,0), new Vector3(0,-1,0)), new PrefabRotation(AssemblyLine_Y_Y, Quaternion.Euler(0,0,0), AssemblyLineEnum.AssemblyLine_Y_Y));
        
        //X Y
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,1,0), new Vector3(1,0,0)), new PrefabRotation(AssemblyLine_Y_X, Quaternion.Euler(0,270,0), AssemblyLineEnum.AssemblyLine_Y_X_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,1,0), new Vector3(-1,0,0)), new PrefabRotation(AssemblyLine_Y_X, Quaternion.Euler(0,90,0), AssemblyLineEnum.AssemblyLine_Y_X_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,1,0), new Vector3(0,0,1)), new PrefabRotation(AssemblyLine_Y_X, Quaternion.Euler(0,180,0), AssemblyLineEnum.AssemblyLine_Y_X_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,1,0), new Vector3(0,0,-1)), new PrefabRotation(AssemblyLine_Y_X, Quaternion.Euler(0,0,0), AssemblyLineEnum.AssemblyLine_Y_X_Straight));
        
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(1,0,0), new Vector3(0,1,0)), new PrefabRotation(AssemblyLine_Y_X, Quaternion.Euler(0,270,0), AssemblyLineEnum.AssemblyLine_Y_X_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(-1,0,0), new Vector3(0,1,0)), new PrefabRotation(AssemblyLine_Y_X, Quaternion.Euler(0,90,0), AssemblyLineEnum.AssemblyLine_Y_X_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,0,1), new Vector3(0,1,0)), new PrefabRotation(AssemblyLine_Y_X, Quaternion.Euler(0,180,0), AssemblyLineEnum.AssemblyLine_Y_X_Straight));
        assemblyLinePrefabMap.Add(new Vector3Pair(new Vector3(0,0,-1), new Vector3(0,1,0)), new PrefabRotation(AssemblyLine_Y_X, Quaternion.Euler(0,0,0), AssemblyLineEnum.AssemblyLine_Y_X_Straight));
    }

    public PrefabRotation getAssemblyLinePrefab(Vector3 from, Vector3 to)
    {
        Debug.Log("AssemblyLinePrefabMap - Try to get prefab and rotation from " + from + " to " + to);
        return assemblyLinePrefabMap[new Vector3Pair(from, to)];
    }
}