//This script makes the list of obstacles on the map


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour {
    public List<GameObject> obstacles;
	// Use this for initialization
	void Start () {
        int num = 1;
		while(true)
        {
            string str = "Obstacle_" + fixLength(num.ToString(), 2);
            GameObject obs = GameObject.Find(str);
            if (obs == null)
                break;
            obstacles.Add(obs);
            num++;
        }
	}
    private string fixLength(string num, int len)
    {
        if (num.Length > len)
            return num.Substring(0, len);

        while(num.Length < len)
        {
            num = "0" + num;
        }
        return num;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
