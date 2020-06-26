using System;
using UnityEngine;

[ExecuteInEditMode]
public class CustomGrid: MonoBehaviour
{
	public float xAngleOffset = 0;
	public float yAngleOffset = 0;
	public float zAngleOffset = 0;

	public float xOffset = 0;
	public float yOffset = 0;
	public float zOffset = 0;

	public float forwardOffSetValue = 0;

	//Private
	private Vector3 realPos;
	private Quaternion realRot;

	private bool finalPosition = false;

	// Update is called once per frame
	void LateUpdate()
	{
		Transform parent = transform.parent;
		if (parent == null)
		{
			if (!finalPosition)
			{
				transform.position = realPos;

				transform.rotation = realRot;
			}
			finalPosition = true;
		}
		else
		{
			transform.position = CustomGridUtil.getTruePos(parent.transform, StaticGameVariables.GRIDSIZE, yOffset, forwardOffSetValue);

			transform.rotation = CustomGridUtil.getTrueRot(parent.transform, xAngleOffset, yAngleOffset, zAngleOffset);

			realPos = transform.position;
			realRot = transform.rotation;
		}
	}
}
