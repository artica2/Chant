using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// A script that will countdown to 0 from a starting time.
/// It will animate itself using size and colour as the time counts down.
/// Should be attached to a textMeshPro text object within a canvas.
/// </summary>
public class CountdownTimerScript : MonoBehaviour {
	#region Variables to assign via the unity inspector (SerializeFields).
	[SerializeField]
	private float startSize = 24;

	[SerializeField]
	private float endSize = 96;

	[SerializeField]
	private Color startColor;

	[SerializeField]
	private Color endColor;
	#endregion

	#region Private Variables.
	//Text object for class.
	TextMeshProUGUI text = null;


	//Used for public static functions.
	private static string message = string.Empty;
	private static Action timerCallback = null;
	private static int timerId = 0;
	#endregion

	#region Private Functions.
	// Start is called before the first frame update
	void Start() {
		text = this.gameObject.GetComponent<TextMeshProUGUI>();
		if (text == null) {
			Debug.LogError("TextMeshProUGUI component was not attached to countdown timer object.");
			return;
		}

		message = string.Empty;
		ResetText();
	}

	// Update is called once per frame
	void Update() {
		if (timerId > 0) {
			//Stop any previous timers.
			text.text = timerId.ToString();
			ResetText();
			StopAllCoroutines();

			//Start the new timer.
			StartCoroutine(IntTimer(timerId));
			timerId = 0;
		}
	}

	private IEnumerator IntTimer(int a_time) {
		//Initiliase timer variables.
		float startTime = Time.time;
		float currentTime = 1.0f;
		int lastFrameTime = 1;



		Debug.Log("Countdown Timer starting");
		while ((Time.time - startTime) < (float)a_time + 1) {
			//Cache the last frame time.
			lastFrameTime = (int)currentTime;

			//Wait till the next frame.
			yield return null;

			//Update the frame time.
			currentTime += Time.deltaTime;
			if ((Time.time - startTime) > (float)a_time) {
				//This is the last second, update the text to show the start message.
				text.text = message;
			} else if ((int)currentTime > lastFrameTime) {
				//update the text if the next second has ellapsed.
				text.text = (a_time - (int)currentTime + 1).ToString();
			}

			//Animate the text.
			float frameTime = (currentTime - ((float)lastFrameTime));
			float sizeT = Mathf.InverseLerp(0.0f, 1.0f, frameTime);
			float colourT;
			if (frameTime < 0.5f) {
				colourT = Mathf.InverseLerp(0.0f, 0.5f, frameTime);
			} else {
				colourT = Mathf.InverseLerp(1.0f, 0.5f, frameTime);
			}
			HandleTextAnimation(colourT, sizeT);
		}
		Debug.Log("Countdown Timer Finished");

		//Reset the timer text.
		ResetText();
		text.text = a_time.ToString();

		//Invoke the callback and make sure it can't be called again.
		if (timerCallback != null) {
			timerCallback.Invoke();
			timerCallback = null;
		}


	}

	private void HandleTextAnimation(float a_colort, float a_sizet) {
		text.color = Color.Lerp(startColor, endColor, a_colort);
		text.fontSize = Mathf.Lerp(startSize, endSize, a_sizet);
	}

	private void ResetText() {
		if (text == null) {
			Debug.LogError("TextMeshProUGUI component was not attached to countdown timer.");
			return;
		}

		text.color = startColor;
		text.fontSize = startSize;
	}
	#endregion

	#region Public Access Functions.
	/// <summary>
	/// Starts a timer that runs for "int a_time" seconds.
	/// </summary>
	/// <param name="a_time"></param>
	public static void StartTimer(int a_time, Action a_callback = null, string a_message = "Chant!!") {
		timerCallback = a_callback;
		timerId = a_time;
		message = a_message;
	}
	#endregion
}
