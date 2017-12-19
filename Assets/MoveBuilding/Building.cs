using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{

	private Vector2 tilePosOld = new Vector2(0, 0); // remember old tile pos while drag building
	public Vector2 tilePos = new Vector2(0, 0);     // tile position
	public Vector2 tileSize = new Vector2(1, 1);    // building's size (forexample, 1x1, 2x2, 3x3 tiles)
													// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}

