using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCheck : MonoBehaviour {
    private GameObject sceneManager;//Scene manager game object
    private SceneManager m;//Scene manager script
    private Vector3 pre, cur;//past position and current position
    private Vector3 displacement;//displacement between original position of this obstacle and position of mouse cursor
    public bool selected;//indicates whether it is clicked

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
			m.replanningFlag = true;
        }
        pre = cur;
        if (!Input.GetButton("Fire1"))
        {
            selected = false;
            m.moving = false;
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
        m.moving = true;
    }
}
