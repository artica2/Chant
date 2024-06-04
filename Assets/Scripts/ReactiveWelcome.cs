using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactiveWelcome : MonoBehaviour
{
	[SerializeField]
	private GameObject welcomeScreen = null;

	private void OnDisable() {
		if(welcomeScreen != null) {
			welcomeScreen.SetActive(true);
		}
	}
}
