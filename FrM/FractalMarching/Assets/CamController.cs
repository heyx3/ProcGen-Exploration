using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CamController : MonoBehaviour
{
	public float MoveSpeed = 7.5f,
				 TurnSpeed = 30.0f;
	private Transform tr;

	private void Awake()
	{
		tr = transform;
	}
	public void Update()
	{
		tr.position += (tr.forward * MoveSpeed * Time.deltaTime *
					        GetAxis(KeyCode.W, KeyCode.S)) +
					   (tr.right * MoveSpeed * Time.deltaTime *
					        GetAxis(KeyCode.D, KeyCode.A)) +
					   (Vector3.up * MoveSpeed * Time.deltaTime *
					        GetAxis(KeyCode.Space, KeyCode.LeftControl));

		var eulerAngs = tr.eulerAngles;
		tr.eulerAngles = new Vector3(eulerAngs.x +
										 (TurnSpeed * Time.deltaTime * -Input.GetAxis("Mouse Y")),
									 eulerAngs.y +
										 (TurnSpeed * Time.deltaTime * Input.GetAxis("Mouse X")),
									 0.0f);
	}

	private float GetAxis(KeyCode forward, KeyCode backward)
	{
		return (Input.GetKey(forward) ? 1.0f : 0.0f) +
			   (Input.GetKey(backward) ? -1.0f : 0.0f);
	}
}