﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample
{
	public Vector3 pos;
	public Sample parent;
	public List<Sample> childs;
    public float cost; // I added this to make RRT*

    public Sample(Vector3 p) // I added this constructor to make samplinfWOP function
    {
        pos = p;
        parent = null;
    }

	public Sample(Vector3 p, Sample parentSample)
	{
		pos = p;
		parent = parentSample;
        cost = -1;
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

	public static Stack<Vector3> FindPath(Vector3 pos_init, Vector3 pos_quit, float wld_left, float wld_right, float wld_top, float wld_bottom)
	{
		Sample q_init = new Sample (pos_init, null);
		Sample q_quit = new Sample (pos_quit, null);
		List<Sample> samples = new List<Sample> ();

		RRT_PathFinding (samples, q_init, q_quit, wld_left, wld_right, wld_top, wld_bottom, 50000);

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


    //Do RRT* algorithm
    public static void RRT_star(List<Sample> samples, Sample q_init, Sample q_quit, float wld_left, float wld_right, float wld_top, float wld_bottom, int n)
    {
        samples.Add(q_init);
        q_init.cost = 0;
        samplingWOP(samples, wld_left, wld_right, wld_top, wld_bottom, n);

        for (int i = 0; i < samples.Count; i++)
        {
            Sample x_new = samples[i]; // We will find its parent
            List<Sample> X_near = near(samples, i, (wld_left - wld_right) / 10); // I assigned this rad value without thinking. Someday matbe we should change this value
            if (X_near.Count == 0)// In the case that there is no Sample near x_new, unfortunately
            {
                X_near.Add(FindClosestSample(x_new.pos, samples));
            }

            List<KeyValuePair<float, Sample>> L_near = populateSortedList(X_near, x_new);
            Sample x_parent = findBestParent(L_near, x_new);//We found its parent! Yeah!
            if (x_parent == null)//x_new has no parent
            {
                continue;
            }

            samples.Add(x_new);
            addEdge(x_parent, x_new);
            rewireVertices(X_near, x_new);
        }
    }

    //Sampling function without setting parents
    private static void samplingWOP(List<Sample> sampleList, float wld_l, float wld_r, float wld_t, float wld_b, int n)
    {
        for (int i = 0; i < n; i++)
        {
            double x = random.NextDouble() * (wld_r - wld_l) + wld_l;
            double z = random.NextDouble() * (wld_t - wld_b) + wld_b;

            Vector3 samplePos = new Vector3((float)x, 0, (float)z);
            Vector3 s = samplePos + Vector3.up * 10;

            RaycastHit[] hits = Physics.RaycastAll(s, Vector3.down);

            for (int j = 0; j < hits.Length; j++)
            {
                if (hits[j].collider.gameObject.tag == "obstacle")
                    break;

                Sample validSample = new Sample(samplePos); //Only this part is different
                sampleList.Add(validSample);
            }
        }
    }
    
    //Finds Samples near x_new
    //within the distance of rad
    private static List<Sample> near(List<Sample> samples, int x_new_index, float rad)
    {
        Sample x_new = samples[x_new_index];
        List<Sample> ans = new List<Sample>();

        for (int i = 0; i < samples.Count; i++)
        {
            if (i == x_new_index)
            {
                continue;
            }
            Sample x = samples[i];
            float dist = (x.pos - x_new.pos).magnitude;
            if (dist < rad)
            {
                ans.Add(x);
            }
        }

        return ans;
    }

    //Makes an edge from x_par to x_chl
    //Updates x_chl's cost by the sum of x_par's cost and the distance between x_par and x_chl
    private static void addEdge(Sample x_par, Sample x_chl)
    {
        x_par.AddChild(x_chl);
        x_chl.parent = x_par;
        float dist = (x_par.pos - x_chl.pos).magnitude;
        x_chl.cost = x_par.cost + dist;
    }

    //Sort the neighbors of x_new with their costs;
    //Sorted list will be used to find the best parent
    private static List<KeyValuePair<float, Sample>> populateSortedList(List<Sample> X_near, Sample x_new)
    {
        List<KeyValuePair<float, Sample>> L_near = new List<KeyValuePair<float, Sample>>();
        for(int i = 0; i < X_near.Count; i++)
        {
            Sample x_n = X_near[i];
            float dist = (x_n.pos - x_new.pos).magnitude;
            float c = x_n.cost + dist;
            L_near.Add(new KeyValuePair<float, Sample>(c, x_n));
        }
        L_near.Sort();
        return L_near;
    }

    //find the nearest neighbor around x_new, which means it could be the best Sample object to be a parent of x_new;
    private static Sample findBestParent(List<KeyValuePair<float, Sample>> L_near, Sample x_new)
    {
        for (int i = 0; i < L_near.Count; i++)
        {
            Sample x_n = L_near[i].Value;
            if (collisionFree(x_n, x_new))
            {
                return x_n;
            }
        }
        return null;
    }
    
    //checks a direct path from x1 to x2. Simply checks is there any obstacle object between x1 and x;
    //returns true if there is no obstacle between x1 and x2.
    //Now it only returns true. We should modify it
    private static bool collisionFree(Sample x1, Sample x2)
    {
        return true;
    }

    //Optimization
    //If path passes x_new is shorter than the original path, set x_new to be a new parent
    private static void rewireVertices(List<Sample> X_near, Sample x_new)
    {
        for (int i = 0; i< X_near.Count; i++)
        {
            Sample x_n = X_near[i];
            float dist = (x_n.pos - x_new.pos).magnitude;
            if (x_new.cost + dist < x_n.cost)
            {
                if (collisionFree(x_n, x_new))
                {
                    Sample x_old = x_n.parent;
                    x_old.childs.Remove(x_n);
                    addEdge(x_new, x_n);
                }
            }
        }
    }
}
