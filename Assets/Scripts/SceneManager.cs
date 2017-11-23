using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
	public bool ReplanningFlag;
    public bool moving;//do nothing when an ostacle is moving
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
        moving = false;
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
        if (moving)//obstacle is now moving
        {
            return;
        }
		if (ReplanningFlag)
		{
			//Debug.Log ("Replanning Start");
			path = PathFinding.FindPath (Agent.transform.position, Target.transform.position, leftBottom.x, rightTop.x, leftBottom.z, rightTop.z);
			ReplanningFlag = false;
            if (path.Count > 0)
			    currentPath = path.Pop ();
		}
		else
		{
            //ReplanningFlag = true;
			if ((Agent.transform.position - currentPath).magnitude <= 0.01f && path.Count > 0)
			{
				currentPath = path.Pop ();
				currentPath.y += Agent.GetComponent<Renderer> ().bounds.center.y;
			}
			
			Agent.transform.position = Vector3.SmoothDamp (Agent.transform.position, currentPath, ref velocity, 1.0f);
		}

		// If agent arrived target, check clear condition.
        /*
		if (Agent.transform.position == Target.transform.position)
		{
			if (CheckClearState ())
				Debug.Log ("Clear!");
			else
				Debug.Log ("Failed");
		}*/
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
