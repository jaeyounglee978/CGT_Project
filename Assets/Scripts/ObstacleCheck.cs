using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCheck : MonoBehaviour {
    private GameObject sceneManager;//Scene manager game object
    private SceneManager m;//Scene manager script
    private Vector3 pre, cur;//past position and current position
    private Vector3 displacement;//displacement between original position of this obstacle and position of mouse cursor
    public bool selected;//indicates whether it is clicked
    public bool fix_x, fix_z;

    private float bound_lower, bound_upper, bound_left, bound_right;

	// Use this for initialization
	void Start () {
        pre = transform.position;
        sceneManager = GameObject.Find("SceneManager");
        m = sceneManager.GetComponent<SceneManager>();
        selected = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (!Input.GetButton("Fire1"))
        {
            selected = false;
            m.moving = false;
        }
        if (selected)
        {
            cur = transform.position;
            if ((cur - pre).magnitude > 0.001)
            {
                m.replanningFlag = true;
            }
            pre = cur;
            Vector3 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(temp.x + displacement.x, 0, temp.z + displacement.z);
            if (fix_x)
            {
                transform.position = new Vector3(pre.x, transform.position.y, transform.position.z);
            }
            if (fix_z)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, pre.z);
            }
            /*
            if (transform.position.z + transform.localScale.z / 2 > bound_upper)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, bound_upper - transform.localScale.z / 2);
            }
            else if (transform.position.z - transform.localScale.z / 2 < bound_lower)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, bound_lower + transform.localScale.z / 2);
            }

            if (transform.position.x + transform.localScale.x / 2 > bound_right)
            {
                transform.position = new Vector3(bound_right - transform.localScale.x / 2, transform.position.y, transform.position.z);
            }
            else if (transform.position.z - transform.localScale.x / 2 < bound_left)
            {
                transform.position = new Vector3(bound_left + transform.localScale.x / 2, transform.position.y, transform.position.z);
            }*/
        }
	}

    private void checkBounds()
    {

    }

    void OnMouseDown()
    {
        if (m.pathFindingC.isRunning)
            return;
        selected = true;
        displacement = - Camera.main.ScreenToWorldPoint(Input.mousePosition) + transform.position;
        m.moving = true;
    }
}
