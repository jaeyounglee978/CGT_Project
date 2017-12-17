using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public bool paused;

	public bool replanningFlag;
    public bool moving;//do nothing when an ostacle is moving
	GameObject Floor;
	GameObject Agent;
	GameObject Target;
	Stack<Vector3> pathStack;
	Vector3 currentPath;
	Vector3 velocity;
	Vector3 leftBottom;
	Vector3 rightTop;
	float samplingTime;
	float movingTime;
	float speed = 2.0f;
	public PathFindingC pathFindingC;
	Coroutine c;
	public GameObject pathIndicator;
	public List<GameObject> obstacles = new List<GameObject> ();

	// Use this for initialization
	void Start ()
	{
        paused = false;
		for (int i = 0; i < obstacles.Count; i++)
			obstacles [i].GetComponent<Renderer> ().material.SetColor ("_Color", Color.black);
		pathStack = new Stack<Vector3> ();
		replanningFlag = true;
        moving = false;
		Floor = GameObject.Find ("Floor");
		Agent = GameObject.Find ("Agent");
		Target = GameObject.Find ("Target");
		Bounds floorBound = Floor.GetComponent<Renderer> ().bounds;
		leftBottom = new Vector3 (floorBound.center.x - floorBound.size.x / 2, 0, floorBound.center.z - floorBound.size.z / 2);
		rightTop = new Vector3 (floorBound.center.x + floorBound.size.x / 2, 0, floorBound.center.z + floorBound.size.z / 2);
		samplingTime = 0.0f;
		movingTime = 0.0f;
		pathFindingC = new PathFindingC ();

		Agent.GetComponent<Renderer> ().material.SetColor ("_Color", Color.blue);
		Target.GetComponent<Renderer> ().material.SetColor ("_Color", Color.green);
	}
	
	// Update is called once per frame
	void Update ()
	{
        if (paused)
            return;
        if (Input.GetButton("Fire1") && (Agent.transform.position - Target.transform.position).magnitude <= 0.01f)
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        if (moving)//obstacle is now moving
        {
            return;
        }
		if (replanningFlag)
		{
			//Debug.Log ("Replanning Start");
			//path = PathFinding.FindPath (Agent.transform.position, Target.transform.position,
			//							leftBottom.x, rightTop.x, leftBottom.z, rightTop.z,
			//							1);
			// aaa
			if (!pathFindingC.isRunning)
			{
				pathFindingC.isRunning = true;
                pathFindingC.isFinished = false;

                /*
				StartCoroutine (pathFindingC.FindPathByRRT (Agent.transform.position, Target.transform.position,
								leftBottom.x, rightTop.x, leftBottom.z, rightTop.z,
								pathStack, 10000));*/

				
				StartCoroutine (pathFindingC.FindPathByRRTstar (Agent.transform.position, Target.transform.position,
								leftBottom.x, rightTop.x, leftBottom.z, rightTop.z,
								pathStack, 10000));
			}
			
			samplingTime += Time.deltaTime;

			if (pathFindingC.isFinished)
			{
                pathFindingC.isRunning = false;
				replanningFlag = false;
                //ShowPath (pathStack);
                if (pathStack.Count == 0)
                    return;
				currentPath = pathStack.Pop();
				Debug.Log ("sampling Time : " + samplingTime);
			}
		}
		else
		{
            if (pathFindingC.isRunning /*|| !pathFindingC.pathExists*/)
                return;
			if ((Agent.transform.position - currentPath).magnitude <= 0.01f)
			{
				if (pathStack.Count == 0 || (Agent.transform.position - Target.transform.position).magnitude <= 0.01f)
				{
					Debug.Log ("moving time : " + movingTime);
					return;
				}
				
				currentPath = pathStack.Pop ();
			}
			
			Agent.transform.position = Vector3.MoveTowards (Agent.transform.position, currentPath, speed * Time.deltaTime);
			movingTime += Time.deltaTime;
		}

	}

	IEnumerator PathFindingCouroutine(Vector3 aPos, Vector3 tPos, float lBx, float rTx, float lBz, float rTz, int flag)
	{
		yield return new WaitUntil(() => (pathStack = PathFinding.FindPath(aPos, tPos, lBx, rTx, lBz, rTz, flag)) != null);
		replanningFlag = false;
		samplingTime += Time.deltaTime;

		if (pathStack.Count > 0)
			currentPath = pathStack.Pop ();

		Debug.Log ("sampling time : " + samplingTime);
	}

	public void ObjectPlaced()
	{
		replanningFlag = true;
		pathFindingC.isRunning = true;
		pathStack = new Stack<Vector3> ();
	}

	private bool CheckClearState()
	{
		return true;
	}

	private void ShowPath(Stack<Vector3> path)
	{
		Vector3[] p = path.ToArray ();

		Stack<Vector3> s = new Stack<Vector3> (path);

		//for (int i = 0; i < p.Length; i++)
		while(s.Count != 0)
		{
			GameObject g = Instantiate (pathIndicator);
			g.GetComponent<Renderer> ().material.SetColor ("_Color", Color.red);
			g.transform.position = s.Pop();
		}
	}
}
