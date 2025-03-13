using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class StartGame : MonoBehaviour
{
	[SerializeField]
	private GameObject player;

	public void StartGamePlay()
	{
		var pScript = player.GetComponent<Player>();
		pScript.SetCanMove(true);
		Destroy(gameObject);
	}
}