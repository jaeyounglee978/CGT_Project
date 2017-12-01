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
	float samplingTime;
	float movingTime;

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
		samplingTime = 0.0f;
		movingTime = 0.0f;
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
			samplingTime = 0.0f;
			Debug.Log ("Start : " + Agent.transform.position);
			Debug.Log ("End : " + Target.transform.position);
			path = PathFinding.FindPath (Agent.transform.position, Target.transform.position,
										leftBottom.x, rightTop.x, leftBottom.z, rightTop.z,
										0);
			samplingTime += Time.deltaTime;

			ReplanningFlag = false;
            if (path.Count > 0)
			    currentPath = path.Pop ();
		}
		else
		{
            //ReplanningFlag = true;
			if ((Agent.transform.position - currentPath).magnitude <= 0.01f)
			{
				Debug.Log (path.Count);
				Debug.Log ((Agent.transform.position - Target.transform.position).magnitude);
				if (path.Count == 0 || (Agent.transform.position - Target.transform.position).magnitude <= 0.01f)
				{
					Debug.Log ("moving time : " + movingTime);
					return;
				}
				
				currentPath = path.Pop ();
				Debug.Log (currentPath);
				Debug.Log ("sampling time : " + samplingTime);

			}
			
			Agent.transform.position = Vector3.SmoothDamp (Agent.transform.position, currentPath, ref velocity, 1.0f);
			movingTime += Time.deltaTime;
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
