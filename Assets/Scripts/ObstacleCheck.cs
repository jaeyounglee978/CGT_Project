using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCheck : MonoBehaviour {
    private GameObject sceneManager;
    private SceneManager m;
    private Vector3 pre, cur;
	// Use this for initialization
	void Start () {
        pre = transform.position;
        sceneManager = GameObject.Find("SceneManager");
        m = sceneManager.GetComponent<SceneManager>();
	}
	
	// Update is called once per frame
	void Update () {
        cur = transform.position;
        if ((cur - pre).magnitude > 0.001)
        {
            m.ReplanningFlag = true;
        }
        pre = cur;
	}
}
