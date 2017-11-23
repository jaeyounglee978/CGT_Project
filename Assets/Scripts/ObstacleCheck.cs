using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCheck : MonoBehaviour {
    private GameObject sceneManager;
    private SceneManager m;
    private Vector3 pre, cur;
    private Vector3 displacement;
    public bool selected;

	// Use this for initialization
	void Start () {
        pre = transform.position;
        sceneManager = GameObject.Find("SceneManager");
        m = sceneManager.GetComponent<SceneManager>();
        selected = false;
	}
	
	// Update is called once per frame
	void Update () {
        cur = transform.position;
        if ((cur - pre).magnitude > 0.001)
        {
            m.ReplanningFlag = true;
        }
        pre = cur;
        if (!Input.GetButton("Fire1"))
        {
            selected = false;
        }
        if (selected)
        {
            Vector3 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(temp.x + displacement.x, 0, temp.z + displacement.z);
        }
	}

    void OnMouseDown()
    {
        selected = true;
        displacement = - Camera.main.ScreenToWorldPoint(Input.mousePosition) + transform.position;
    }
}
