using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
	bool ReplanningFlag;
	GameObject Floor;
	GameObject Agent;
	GameObject Target;
	Stack<Vector3> path;
	Vector3 currentPath;
	Vector3 velocity;
	Vector3 leftBottom;
	Vector3 rightTop;

	// Use this for initialization
	void Start ()
	{
		path = null;
		ReplanningFlag = true;
		Floor = GameObject.Find ("Floor");
		Agent = GameObject.Find ("Agent");
		Target = GameObject.Find ("Target");
		Bounds floorBound = Floor.GetComponent<Renderer> ().bounds;
		leftBottom = new Vector3 (floorBound.center.x - floorBound.size.x / 2, 0, floorBound.center.z - floorBound.size.z / 2);
		rightTop = new Vector3 (floorBound.center.x + floorBound.size.x / 2, 0, floorBound.center.z + floorBound.size.z / 2);
		Debug.Log (Target.transform.position);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (ReplanningFlag)
		{
			Debug.Log ("Replanning Start");
			path = PathFinding.FindPath (Agent.transform.position, Target.transform.position, leftBottom.x, rightTop.x, leftBottom.z, rightTop.z);
			ReplanningFlag = false;
			currentPath = path.Pop ();
		}
		else
		{
			if (Agent.transform.position == currentPath)
			{
				currentPath = path.Pop ();
				currentPath.y += Agent.GetComponent<Bounds> ().center.y;
			}
			
			Agent.transform.position = Vector3.SmoothDamp (Agent.transform.position, currentPath, ref velocity, 1.0f);
		}

		// If agent arrived target, check clear condition.
		if (Agent.transform.position == Target.transform.position)
		{
			if (CheckClearState ())
				Debug.Log ("Clear!");
			else
				Debug.Log ("Failed");
		}
	}

	public void ObjectPlaced()
	{
		ReplanningFlag = true;
	}

	private bool CheckClearState()
	{
		return true;
	}
}
