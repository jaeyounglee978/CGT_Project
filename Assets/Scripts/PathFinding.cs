using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample
{
	public Vector3 pos;
	public Sample parent;
	public List<Sample> childs;

	public Sample(Vector3 p, Sample parentSample)
	{
		pos = p;
		parent = parentSample;
	}

	public void AddChild(Sample s)
	{
		childs.Add (s);
	}
}

public static class PathFinding
{
	static System.Random random = new System.Random();

	// Get intial Sample list(== initial position), then make sample tree.
	private static void Sampling(List<Sample> sampleList, float wld_l, float wld_r, float wld_t, float wld_b, int n)
	{
		for (int i = 0; i < n; i++)
		{
			double x = random.NextDouble () * (wld_r - wld_l) + wld_l;
			double z = random.NextDouble () * (wld_t - wld_b) + wld_b;

			Vector3 samplePos = new Vector3 ((float)x, 0, (float)z);
			Vector3 s = samplePos + Vector3.up * 10;

			RaycastHit[] hits = Physics.RaycastAll (s, Vector3.down);

			for(int j = 0; j < hits.Length; j++)
			{
				if (hits [j].collider.gameObject.tag == "obstacle")
					break;

				// We need to modify steps below for more effective algorithm.
				Sample closestSample = FindClosestSample (samplePos, sampleList);

				Sample validSample = new Sample(samplePos, closestSample);
				sampleList.Add (validSample);
			}
		}
	}

	// Reassemble Tree for algorithms like RRT*
	public static void ReassembleTree()
	{
	}

	// Finds the closest sample.
	public static Sample FindClosestSample (Vector3 pos, List<Sample> prevSamples)
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

	// Get list, initial positon, area that we do sampling. Do sampling by RRT algorithm.
	public static void RRT_PathFinding(List<Sample> samples, Sample q_init, Sample q_quit, float wld_left, float wld_right, float wld_top, float wld_bottom, int n)
	{
		samples.Add(q_init);
		Sampling(samples, wld_left, wld_right, wld_top, wld_bottom, n);
		Sample closest = FindClosestSample (q_quit.pos, samples);
		q_quit.parent = closest;
		samples.Add (q_quit);
	}

	public static Stack<Vector3> FindPath(Vector3 pos_init, Vector3 pos_quit)
	{
		Sample q_init = new Sample (pos_init, null);
		Sample q_quit = new Sample (pos_quit, null);
		List<Sample> samples = new List<Sample> ();

		RRT_PathFinding (samples, q_init, q_quit, 0, 0, 0, 0, 5000);

		Sample d = q_quit;
		Stack<Vector3> positionStack = new Stack<Vector3> ();

		// climb up tree
		while (d.parent != null)
		{
			positionStack.Push (d.pos);
			d = d.parent;
		}

		return positionStack;
	}
}
