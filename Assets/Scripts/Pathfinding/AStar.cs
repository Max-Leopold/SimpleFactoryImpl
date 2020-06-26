using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{
	private readonly LayerMask layerToCheck;

	private static int gridSize = 1;
    private Vector3 _endPoint;

	private MinHeap openList;
	private HashSet<Vector3> closedList;

	public AStar(LayerMask layerToCheck)
	{
		this.layerToCheck = layerToCheck;
	}

	public void buildPath(AStarNode end)
	{
		while (end != null)
		{
			Debug.Log(end.getPos());
			GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = end.getPos();
			end = end.getPredecessor();
		}
	}

	public AStarNode calculatePath(Vector3 startingPoint, Vector3 endPoint)
	{
		openList = new MinHeap(2000); //Size could be changed
		closedList = new HashSet<Vector3>();
		_endPoint = endPoint;

		openList.Add(new AStarNode(startingPoint, endPoint));
		Debug.Log("AStar - Calculate Path from " + startingPoint + " to " + endPoint);

		while(!openList.IsEmpty())
		{
			AStarNode currentNode = openList.Pop();
			Debug.Log("AStar - Checking pos " + currentNode.getPos());

			if (Vector3.Equals(currentNode.getPos(), endPoint))
			{
				Debug.Log("AStar - Found way");
				return currentNode;
			}

			closedList.Add(currentNode.getPos());
			expandNode(currentNode);
		}

		throw new NoPathFoundException("AStar - Could not find path from " + startingPoint + " to " + endPoint);
	}

	private void expandNode(AStarNode currentNode)
	{
		List<Vector3> successors = getNodeSuccessors(currentNode.getPos());

		foreach(Vector3 successor in successors)
		{
			if(closedList.Contains(successor))
			{
				Debug.Log("AStar - Position already in closedList " + successor);
				continue;
			}

			AStarNode successorNode = new AStarNode(successor, _endPoint);
			float tenativeCosts = currentNode.getCosts() + 1 + successorNode.getDistanceToGoal();
			successorNode.setCosts(tenativeCosts);
			Debug.Log("AStar - Cost to " + successor + " are " + tenativeCosts);

			AStarNode containsNode = openList.ContainsAndGet(successorNode);
			if (containsNode != null && tenativeCosts >= containsNode.getCosts())
			{
				Debug.Log("AStar - Cost are bigger");
				continue;
			}

			successorNode.setPredecesor(currentNode);
			if(containsNode != null)
			{
				openList.Remove(containsNode);
			}
			openList.Add(successorNode);
			Debug.Log("AStar - Add new node " + successor);
		}

	}

	private List<Vector3> getNodeSuccessors(Vector3 currentNodePos)
	{
		//Add possible criterias here (e.g. is in ground)
		return validateSuccesors(new List<Vector3>
		{
			new Vector3(currentNodePos.x + gridSize, currentNodePos.y, currentNodePos.z),
			new Vector3(currentNodePos.x - gridSize, currentNodePos.y, currentNodePos.z),
			new Vector3(currentNodePos.x, currentNodePos.y + gridSize, currentNodePos.z),
			new Vector3(currentNodePos.x, currentNodePos.y - gridSize, currentNodePos.z),
			new Vector3(currentNodePos.x, currentNodePos.y, currentNodePos.z + gridSize),
			new Vector3(currentNodePos.x, currentNodePos.y, currentNodePos.z - gridSize)
		});
	}

	private List<Vector3> validateSuccesors(List<Vector3> succesors)
	{
		Debug.Log("AStar - Succesor List size: " + succesors.Count);
		List<Vector3> returnList = new List<Vector3>();
		for(int i = 0; i < succesors.Count; i++)
		{
			if(CustomGridUtil.validatePosition(succesors[i], layerToCheck))
			{
				returnList.Add(succesors[i]);
				Debug.Log("Successor at " + succesors[i] + " did not collide");
			}
		}

		Debug.Log("AStar - Succesor List size after removing: " + returnList.Count);
		return returnList;
	}
}
