using Firebase;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// A script to initialise firebase in the splash screen so that all dependencies are loaded before the login page is shown to the user.
/// </summary>
public class FirebaseInit : MonoBehaviour {
	[SerializeField]
	private float timeoutSeconds = 60.0f;

	[SerializeField]
	private bool useTimeout = false;

	void Start() {
		//If the initialisation takes too long, display time out message and quit the application.
		if (useTimeout) {
			StartCoroutine(Timeout());
		}
		CheckIfReady();
	}

	private IEnumerator Timeout() {
		yield return new WaitForSeconds(timeoutSeconds);
		MessageManager.QueueMessage("Application timed out", Application.Quit);
	}

	/// <summary>
	/// Ensure the firebase dependencies are ready and then load the next scene when the task succeeds.
	/// </summary>
	public static void CheckIfReady() {

		Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
			Firebase.DependencyStatus dependencyStatus = task.Result;
			if (dependencyStatus == Firebase.DependencyStatus.Available) {

				Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
				Debug.Log("Firebase is ready for use.");
				FirebaseManager.InitializeFirebase();
				SceneManager.LoadScene(1);
			} else if (dependencyStatus != Firebase.DependencyStatus.Available) {
				UnityEngine.Debug.LogError(System.String.Format(
				  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));

				MessageManager.QueueMessage("Application dependencies failed to load", Application.Quit);//THIS IS SO I KNOW WHEN THE APP SPECIFICALLY FAILS TO FETCH DEPENDENCIES.
			}
		});
	}
}