using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComponentTestScript3 : MonoBehaviour
{
	[SerializeField]
	private float rotationSpeed = 1f;

	private void Update()
	{
		transform.RotateAround(transform.position,transform.up,rotationSpeed*Time.deltaTime);
	}
}