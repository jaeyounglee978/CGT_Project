using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFindingC
{
	System.Random random;
	public bool isRunning;
	public bool isFinished;
    public bool pathExists;

	float radius = 0.5f * Mathf.Sqrt (3) / 2;

	public PathFindingC()
	{
		random = new System.Random();
		isRunning = false;
		isFinished = false;
        pathExists = true;
	}

	public bool CheckSampleValidity(Vector3 pos)
	{
		Vector3 s = pos + Vector3.up * 10;

		RaycastHit[] hits = Physics.RaycastAll (s, Vector3.down, 15f);

		for(int i = 0; i < hits.Length; i++)
		{
			if (hits [i].collider.gameObject.tag == "obstacle")
				return false;
		}

		Collider[] colliders = Physics.OverlapSphere (pos, radius);

		if (colliders.Length == 0)
			return true;

		for(int j = 0; j < colliders.Length; j++)
		{
			if (colliders [j].gameObject.tag == "obstacle")
				return false;
		}

		return true;
	}

	// Finds the closest sample.
	public Sample FindClosestSample (Vector3 pos, List<Sample> prevSamples)
	{
		Sample closestSample = null;
		float distance = 9999999999999f;

		for (int i = 0; i < prevSamples.Count; i++)
		{
			if ((prevSamples [i].pos - pos).magnitude < distance)
			{
				closestSample = prevSamples [i];
				distance = (closestSample.pos - pos).magnitude;
			}
		}

		return closestSample;
	}

	//Finds Samples near x_new
	//within the distance of rad
	private List<Sample> Near(List<Sample> samples, Sample x_new)
	{
		List<Sample> ans = new List<Sample>();

		float rad = Mathf.Pow(Mathf.Log(samples.Count) / samples.Count, 1/3);

		for (int i = 0; i < samples.Count; i++)
		{
			Sample x = samples[i];
			float dist = (x.pos - x_new.pos).magnitude;
			if (dist < rad)
			{
				ans.Add(x);
			}
		}

		if (ans.Count == 0)
			ans.Add (FindClosestSample (x_new.pos, samples));

		return ans;
	}


	//Sort the neighbors of x_new with their costs;
	//Sorted list will be used to find the best parent
	private List<KeyValuePair<float, Sample>> PopulateSortedList(List<Sample> X_near, Sample x_new)
	{
		List<KeyValuePair<float, Sample>> L_near = new List<KeyValuePair<float, Sample>>();
		for(int i = 0; i < X_near.Count; i++)
		{
			Sample x_n = X_near[i];
			float dist = (x_n.pos - x_new.pos).magnitude;
			float c = x_n.cost + dist;
			L_near.Add(new KeyValuePair<float, Sample>(c, x_n));
		}
		L_near = L_near.OrderBy(o => o.Key).ToList();
		return L_near;
	}

	//find the nearest neighbor around x_new, which means it could be the best Sample object to be a parent of x_new;
	private Sample FindBestParent(List<KeyValuePair<float, Sample>> L_near, Sample x_new)
	{
		for (int i = 0; i < L_near.Count; i++)
		{
			Sample x_n = L_near[i].Value;
			if (CheckValidPath(x_n, x_new))
			{
				return x_n;
			}
		}
		return null;
	}

	//Makes an edge from x_par to x_chl
	//Updates x_chl's cost by the sum of x_par's cost and the distance between x_par and x_chl
	private void AddEdge(Sample x_par, Sample x_chl)
	{
		x_par.AddChild(x_chl);
		x_chl.parent = x_par;
		float dist = (x_par.pos - x_chl.pos).magnitude;
		x_chl.cost = x_par.cost + dist;
	}

	//checks a direct path from x1 to x2. Simply checks is there any obstacle object between x1 and x;
	//returns true if there is no obstacle between x1 and x2.
	//Now it only returns true. We should modify it
	private bool CheckValidPath(Sample x1, Sample x2)
	{
		RaycastHit[] hits = Physics.RaycastAll(x1.pos + Vector3.up/2, x2.pos - x1.pos);
		for (int i = 0; i < hits.Length; i++)
		{
			if ((hits[i].point - x1.pos).magnitude > (x2.pos - x1.pos).magnitude)
				continue;
			if (hits[i].collider.gameObject.tag == "obstacle")
				return false;
		}
		return true;
	}
	private bool CheckValidPath(Sample x1, Vector3 x2)
	{
		RaycastHit[] hits = Physics.RaycastAll(x1.pos, x2 - x1.pos);
		for (int i = 0; i < hits.Length; i++)
		{
			if ((hits[i].point - x1.pos).magnitude > (x2 - x1.pos).magnitude)
				continue;
			if (hits[i].collider.gameObject.tag == "obstacle")
				return false;
		}
		return true;
	}

	//Optimization
	//If path passes x_new is shorter than the original path, set x_new to be a new parent
	private void RewireVertices(List<Sample> X_near, Sample x_new)
	{
		for (int i = 0; i< X_near.Count; i++)
		{
			Sample x_n = X_near[i];
			float dist = (x_n.pos - x_new.pos).magnitude;
			if (x_new.cost + dist < x_n.cost)
			{
				if (CheckValidPath(x_n, x_new))
				{
					Sample x_old = x_n.parent;
					x_old.childs.Remove(x_n);
					AddEdge(x_new, x_n);
				}
			}
		}
	}

	public IEnumerator FindPathByRRT(Vector3 pos_init, Vector3 pos_quit, 
								 	 float wld_left, float wld_right, float wld_top, float wld_bottom,
									 Stack<Vector3> pathStack, int numberOfSample)
	{
		Debug.Log ("Start RRT");
		Sample q_init = new Sample (pos_init, null);
		Sample q_quit = new Sample (pos_quit, null);
		List<Sample> samples = new List<Sample> ();
		//RRT (samples, q_init, q_quit, wld_left, wld_right, wld_top, wld_bottom, 10);
		samples.Add(q_init);

		//Sampling(samples, wld_left, wld_right, wld_top, wld_bottom, n);
		for (int i = 0; i < numberOfSample; i++)
		{
			double x = random.NextDouble () * (wld_right - wld_left) + wld_left;
			double z = random.NextDouble () * (wld_top - wld_bottom) + wld_bottom;

			Vector3 samplePos = new Vector3 ((float)x, 0, (float)z);

			/*Vector3 s = samplePos + Vector3.up * 10;

			RaycastHit[] hits = Physics.RaycastAll (s, Vector3.down, 15f);

			for(int j = 0; j < hits.Length; j++)
			{
				if (hits [j].collider.gameObject.tag == "obstacle")
					goto END;
			}*/

			if (!CheckSampleValidity (samplePos))
				goto END;

			samplePos.y += 1;

			// We need to modify steps below for more effective algorithm.
			Sample closestSample = FindClosestSample (samplePos, samples);

			if(!CheckValidPath(closestSample, samplePos))
				continue;

			Sample validSample = new Sample(samplePos, closestSample);
			samples.Add (validSample);

			END: {}

			if (i % 500 == 0)
				yield return new WaitForEndOfFrame ();
		}

		Sample closest = FindClosestSample (q_quit.pos, samples);
		q_quit.parent = closest;
		q_quit.ReCalculateCose ();

		Debug.Log ("Total vallid samples : " + (samples.Count - 1));
		samples.Add (q_quit);

		Sample d = q_quit;

		// climb up tree
		while (d.parent != null)
		{
			pathStack.Push (d.pos);
			d = d.parent;
		}
		Debug.Log ("pathStack number : " + pathStack.Count);

		isFinished = true;
        //isRunning = false;
	}

	public IEnumerator FindPathByRRTstar(Vector3 pos_init, Vector3 pos_quit, 
										  float wld_left, float wld_right, float wld_top, float wld_bottom,
										  Stack<Vector3> pathStack, int numberOfSample)
	{
		Debug.Log ("Start RRT*");
		Sample q_init = new Sample (pos_init, null);
		Sample q_quit = new Sample (pos_quit, null);
		List<Sample> samples = new List<Sample> ();
		List<Sample> visited = new List<Sample> ();

		//RRT_star (samples, q_init, q_quit, wld_left, wld_right, wld_top, wld_bottom, 10);
		samples.Add(q_init);
		visited.Add (q_init);
		q_init.cost = 0;
		//SamplingWOP(samples, wld_left, wld_right, wld_top, wld_bottom, n);
		for (int i = 0; i < numberOfSample; i++)
		{
			double x = random.NextDouble() * (wld_right - wld_left) + wld_left;
			double z = random.NextDouble() * (wld_top - wld_bottom) + wld_bottom;

			Vector3 samplePos = new Vector3((float)x, 0, (float)z);
			Vector3 s = samplePos + Vector3.up * 10;

			RaycastHit[] hits = Physics.RaycastAll(s, Vector3.down);
			Sample validSample;

			for (int j = 0; j < hits.Length; j++)
			{
				if (hits[j].collider.gameObject.tag == "obstacle")
					goto END;
			}

			samplePos.y += 1;
			validSample = new Sample(samplePos); //Only this part is different
			samples.Add(validSample);
			List<Sample> X_near = Near (visited, validSample);//, (wld_left - wld_right) / 5); // I assigned this rad value without thinking. Someday matbe we should change this value
			List<KeyValuePair<float, Sample>> L_near = PopulateSortedList(X_near, validSample);
			Sample x_parent = FindBestParent(L_near, validSample);//We found its parent! Yeah!
			if (x_parent != null)//x_new has no parent
			{
				visited.Add(validSample);
				AddEdge(x_parent, validSample);
				RewireVertices(X_near, validSample);
			}

			END:
			if (i % 100 == 0)
				yield return new WaitForEndOfFrame ();
		}

		Debug.Log ("Total vallid samples : " + (samples.Count - 1));

		// add last sample
		samples.Add(q_quit);
		List<Sample> Xq_near = Near(visited, q_quit);//, (wld_left - wld_right) / 5); // I assigned this rad value without thinking. Someday matbe we should change this value

		List<KeyValuePair<float, Sample>> Lq_near = PopulateSortedList(Xq_near, q_quit);
		Sample xq_parent = FindBestParent(Lq_near, q_quit);//We found its parent! Yeah!
		if (xq_parent != null)//x_new has no parent
		{
			visited.Add(q_quit);
			AddEdge(xq_parent, q_quit);
			RewireVertices(Xq_near, q_quit);
		}

		Sample d = q_quit;

		// climb up tree
		while (d.parent != null)
		{
			pathStack.Push (d.pos);
			d = d.parent;
		}
        if (CheckValidPath(d, q_init))
            pathExists = true;
        else
            pathExists = false;
		Debug.Log ("End");
		isFinished = true;
        //isRunning = false;
	}
}
