using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pause : MonoBehaviour {
    GameObject sceneManager;
    SceneManager m;
    public void Start()
    {
        sceneManager = GameObject.Find("SceneManager");
        m = sceneManager.GetComponent<SceneManager>();
    }
	public void click()
    {
        m.paused = !m.paused;
        //UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
