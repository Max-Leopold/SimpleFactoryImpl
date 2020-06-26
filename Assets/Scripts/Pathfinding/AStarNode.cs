using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode
{
	private float _distanceToGoal;
	private Vector3 _pos;
	private float _costs;
	private AStarNode predecesor;

	//----------

	public GameObject objectAtThisPosition;

	public AStarNode(Vector3 pos, Vector3 goal)
	{
		_pos = pos;
		_distanceToGoal = Vector3.Distance(pos, goal);
		_costs = _distanceToGoal;
	}

	public float getDistanceToGoal()
	{
		return _distanceToGoal;
	}

	public Vector3 getPos()
	{
		return _pos;
	}

	public float getCosts()
	{
		return _costs;
	}

	public void setPredecesor(AStarNode aStarNode)
	{
		predecesor = aStarNode;
	}

	public void setCosts(float costs)
	{
		_costs = costs;
	}

	public AStarNode getPredecessor()
	{
		return predecesor;
	}

	public void deletePath()
	{
		Debug.Log("AStarNode - Deleting object at position " + _pos);
		UnityEngine.Object.Destroy(objectAtThisPosition);
		if(predecesor != null)
		{
			predecesor.deletePath();
		}
		//Does Garbage Collector delete this object instantly or do we have to call it explicitly?
	}

	public void buildPath(GameObject buildObjectPrefab, Material material)
	{
		Debug.Log("AStarNode - Building new object at position " + _pos);
		objectAtThisPosition = UnityEngine.Object.Instantiate(buildObjectPrefab, _pos, Quaternion.identity);

		objectAtThisPosition.GetComponent<Renderer>().material = material;

		if(predecesor != null)
		{
			predecesor.buildPath(buildObjectPrefab, material);
		}
	}

	public void setPathMaterial(Material material)
	{
		objectAtThisPosition.GetComponent<Renderer>().material = material;

		if (predecesor != null)
		{
			predecesor.setPathMaterial(material);
		}
	}
		 
	public override bool Equals(object obj)
	{
		return obj is AStarNode node &&
			   _distanceToGoal == node._distanceToGoal;
	}

	public override int GetHashCode()
	{
		return 928383959 + _distanceToGoal.GetHashCode();
	}
}
