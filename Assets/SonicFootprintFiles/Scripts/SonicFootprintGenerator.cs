using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Generates the sonic footprint from microphone input and records the audio that went along with it.
/// </summary>
public class SonicFootprintGenerator : MonoBehaviour {
	#region Variables to assign via the unity inspector (SerializeFields).
	[SerializeField]
	private float lengthOfFootprintTime = 4.0f;

	[SerializeField]
	[Range(0.0f, 1.0f)]
	private float minLength = 1.0f;

	[SerializeField]
	private float minMultiplier = 1.0f;

	[SerializeField]
	private float maxMultiplier = 2.0f;

	[SerializeField]
	private float minWidth = 0.1f;

	[SerializeField]
	private GameObject lineRendererPrefab = null;

	[SerializeField]
	private string desiredTone = string.Empty;

	//[SerializeField]
	//[Range(0.0f, 1.0f)]
	//private float tolerance = 0.75f;

	[SerializeField]
	private float toleranceInHz = 100.0f;

	[SerializeField]
	private List<MusicalNotes> musicalNotes = new List<MusicalNotes>();

	[SerializeField]
	private UnityEvent afterReferenceTone = null;

	[SerializeField]
	private GameObject tapToChantText = null;

	[SerializeField]
	private GameObject sendChantButton = null;

	[SerializeField]
	private GameObject messageTextbox = null;

	[SerializeField]
	private GameObject gigScreen = null;

	[SerializeField]
	private GameObject everythingElse = null;

	[SerializeField]
	private GameObject audioPlayer = null;

	[SerializeField]
	private TextMeshProUGUI targetNoteText = null;

	private bool chantOver = true;
	#endregion

	#region Private Variables.
	private float maxWidth = 1.0f;
	private int lineSegments = 90;
	private float increments;
	private float angleIncrements;
	private float currentAngle = 90.0f;
	private int counter = 0;
	private bool coolingDown = false;
	private static MicrophoneInput microphone;
	private MusicalNotes currentNote = null;
	private float closeness = 0.0f;
	private float lineMultiplier = 0.0f;
	private string currentGigName = "DEFAULT_";
	private bool recordingFinished = false;

	//Used for interfacing with other classes.
	private static bool hasStarted = false;
	private static string m_chantFileName = string.Empty;
	private static Action<string, Texture2D> m_callback = null;
	private static float desiredFrequency = 0.0f;
	private static List<GameObject> m_linesList = null;

	//Chant Statis Data.
	private static string chantFilePath = string.Empty;
	private static string sonicFootprintFilePath = string.Empty;
	private static string s_chantMessage = string.Empty;
	#endregion

	#region Private Functions.
	// Start is called before the first frame update
	void Start() {

		//Get the microphone input.
		microphone = this.gameObject.GetComponent<MicrophoneInput>();

		SetUpSonicFootprintLineData();

		//Make sure "hasStarted" is false on application start.
		hasStarted = false;
		recordingFinished = false;
		m_chantFileName = string.Empty;
		desiredFrequency = 0.0f;

		//Make sure the audio player has the correct script.
		if (audioPlayer == null) {
			Debug.LogError("Audio player is not referenced in the sonic footprint generator.");
			return;
		}

		if (audioPlayer.GetComponent<AudioPlayerScript>() == null) {
			Debug.LogError("Audio player does not have an AudioPlayerScript attached to it.");
			return;
		}

		//If there are any previous lines from drawining a sonic footprint, destroy them.
		DestroyAllLines();
	}


	// Update is called once per frame
	void Update() {
		if (chantOver) {
			return;
		}
		if (!coolingDown && hasStarted && counter <= lineSegments && !recordingFinished) {
			HandleRecordingInput();
		} else if (counter > lineSegments) {
			HandleRecordingFinished();
		} else {
			//If the player clicks start the chant recording.
			if (Input.GetMouseButtonDown(0) && !recordingFinished && !coolingDown) {
				coolingDown = true;
				CountdownTimerScript.StartTimer(3, CountdownCallback);

				//Deactivate tap to chant text.
				if (tapToChantText != null) {
					tapToChantText.SetActive(false);
				}
			}
		}
	}

	private float CalculateWidthAtDistance(int a_numLines, float distance) {
		// Numlines = circumference / width
		// width = circumference / numLines
		// circumference = Pi * distance * 2
		// width = (pi * distance * 2) / numLines
		float width = ((float)Mathf.PI * distance * 2.0f) / a_numLines;
		return width;
	}

	private int CalculateLineSegments() {
		//Calculate circumference of the whole circle
		float circumference = (float)Math.PI * minMultiplier * 2.0f;

		//Calculate the number of lines at the minimum width can fit in that circumference.
		int numLines = (int)(circumference / minWidth);
		return numLines;
	}

	private void SetUpSonicFootprintLineData() {
		//Calculate line segment numbers and end of line widths.
		lineSegments = CalculateLineSegments();
		Debug.Log("Sonic footprint line segments before clamp: " + lineSegments);
		lineSegments = Mathf.Clamp(lineSegments, 0, 360);
		Debug.Log("Sonic footprint line segments after clamp: " + lineSegments);
		maxWidth = CalculateWidthAtDistance(lineSegments, maxMultiplier);
		Debug.Log("Max width: " + maxWidth);

		//Figure out the time increments.
		increments = lengthOfFootprintTime / (float)lineSegments;
		angleIncrements = 360.0f / (float)lineSegments;
	}

	/// <summary>
	/// Start recording the sonic footprint and chant audio.
	/// </summary>
	private void CountdownCallback() {
		coolingDown = false;
		SonicFootprintGenerator.StartRecordingFootprint(currentGigName + SendChant.GetUserId(), HandleChantData);
	}

	//Update Functions.

	public void ResetFootprintVariables(bool deactiateGigPanel = true) {
		//Make sure the lines list exists.
		if (m_linesList == null) {
			m_linesList = new List<GameObject>();
		} else {
			//If there are any previous lines from drawining a sonic footprint, destroy them.
			DestroyAllLines();
		}

		//Make sure the recording finished variable is reset.
		recordingFinished = false;

		//Activate the chant text.
		if (tapToChantText != null) {
			tapToChantText.SetActive(true);
		}

		//Make sure chant button is not active.
		if (sendChantButton != null) {
			sendChantButton.SetActive(false);
		}

		//dectivate the text box.
		if (messageTextbox != null) {
			messageTextbox.SetActive(true);
			messageTextbox.GetComponent<TMP_InputField>().text = string.Empty;
			messageTextbox.SetActive(false);
		}

		if (audioPlayer != null) {
			audioPlayer.SetActive(false);
		}

		//Get the gig name.
		if (!deactiateGigPanel) {
			return;
		}
		DeactivateGigPanel();
		//Reset chant/sonic-footprint filepaths.
		chantFilePath = string.Empty;
		sonicFootprintFilePath = string.Empty;
		s_chantMessage = string.Empty;

		//Deactive chant screen.
		if (gigScreen != null) {
			gigScreen.SetActive(false);
		}
	}

	private float GetDesiredFrequency() {
		//Loop through the tones.
		for (int i = 0; i < musicalNotes.Count; i++) {
			if (musicalNotes[i].note == desiredTone) {
				currentNote = musicalNotes[i];
				return musicalNotes[i].frequency;
			}
		}


		//Return 0 and log error.
		Debug.LogError("ERROR: No musical notes supplied.\nError located in SonicFootprintGenerator.cs");
		return 0.0f;
	}

	/// <summary>
	/// Handles recording the frequency input.
	/// </summary>
	private void HandleRecordingInput() {
		if (IsValidVoiceInput(desiredFrequency, toleranceInHz /*desiredFrequency * tolerance*/)) {
			//Get the frequency, multiplier and direction for the lines.
			float fundamentalFrequency = microphone.GetFundamentalFrequency();
			lineMultiplier = Mathf.Lerp(minMultiplier, maxMultiplier, closeness);
			Vector3 direction = ConvertAngleToVector(currentAngle);
			if (lineMultiplier <= 1.0f) {
				Debug.Log("Fundamental Frequency: " + fundamentalFrequency + "\nLine Multiplier: " + lineMultiplier + "\nCloseness: " + closeness);
				Debug.Log("Line Length: " + direction.magnitude);
			}

			//Spawn a line renderer.
			GameObject lineRenderer = Instantiate(lineRendererPrefab, this.transform);
			Vector3 pos = this.gameObject.transform.position + Vector3.forward;
			lineRenderer.transform.position = pos;
			m_linesList.Add(lineRenderer);

			//Set it's positions to the correct values.
			LineRenderer renderer = lineRenderer.GetComponent<LineRenderer>();
			renderer.SetPosition(0, pos + (direction.normalized * minLength));
			renderer.SetPosition(1, pos + direction);

			float currentMult = Mathf.InverseLerp(minMultiplier, maxMultiplier, lineMultiplier);
			renderer.startWidth = minWidth;
			renderer.endWidth = Mathf.Lerp(minWidth, maxWidth, currentMult);

			//Choose a random colour for the renderer.
			if (currentNote == null) {
				//Early out.
				Debug.LogError("ERROR: Current desired note was not assigned in 'SonicFootprintGenerator.cs'.");
				return;
			}

			float volume = Mathf.Clamp(microphone.GetAveragedVolume(), 0.0f, 0.05f);
			volume = Mathf.InverseLerp(0.0f, 0.05f, volume);

			float colourInterpolator = (closeness + volume) * 0.5f;
			Color lineColour = Color.Lerp(currentNote.noteDarkColour, currentNote.noteColour, colourInterpolator);
			renderer.SetColors(lineColour, lineColour);
		}

		counter++;
		currentAngle += angleIncrements;
		StartCoroutine(DrawFootprintCooldown());
	}

	/// <summary>
	/// Handles the recording when it's been finished.
	/// </summary>
	private void HandleRecordingFinished() {
		//Don't allow loop to run again.
		recordingFinished = true;
		hasStarted = false;
		counter = 0;
		currentAngle = 90.0f;
		StartCoroutine(DrawFootprintCooldown());

		//Sonic footprint has finished drawing get relevant files to be sent to firebase.
		if (m_chantFileName == string.Empty) {
			Debug.LogError("ERROR: Invalid Chant File Name\nError Location: SonicFootprintGenerator.cs");
			//Early out.
			return;
		}

		//Take the sonic footprint screenshot.
		Texture2D footprintTexture = TakeScreenshot();

		//Export the recording as a .Wav file.
		string filePath = microphone.ExportCurrentClipToWAV(m_chantFileName);
		if (filePath == string.Empty) {
			Debug.LogError("ERROR: Invalid File Path For Chant Recording\nError Location: SonicFootprintGenerator.cs");
			//Early out.
			return;
		}
		Debug.Log("Exporting Chant as file: " + filePath);

		//Fire off callback function and send relevant files with the function.
		if (m_callback == null) {
			//Early out.
			Debug.LogError("ERROR: Callback function is null.\nError Location: SonicFootprintGenerator.cs");
			return;
		}
		m_callback.Invoke(filePath, footprintTexture);
		//TestFunction(filePath, footprintTexture);

		//Activate the text box.
		if (messageTextbox != null) {
			messageTextbox.SetActive(true);
		}

		//Activate send chant button.
		if (sendChantButton != null) {
			sendChantButton.SetActive(true);
		}

		//Activate the audio player and send the chant to it.
		if (audioPlayer != null) {
			audioPlayer.SetActive(true);
			AudioPlayerScript script = audioPlayer.GetComponent<AudioPlayerScript>();
			if (script == null) {
				Debug.LogError("Audio Player Script attached to audio player was null");
				return;
			}

			AudioClip clip = microphone.GetCurrentClip();
			if (clip == null) {
				Debug.LogError("Audio clip was null");
				return;
			}

			script.AssignAudioClip(clip);
		}

		//Stop chant from being able to record.
		chantOver = true;
	}

	//Screenshot Functions.
	private void HandleChantData(string audiofilepath, Texture2D sonicFootprint) {
		//Export the sonic footprint screenshot as a png.
		byte[] bytes = sonicFootprint.EncodeToPNG();
		string filepath = Application.persistentDataPath + "/screenshots/";
		string filename = m_chantFileName + ".png";
		System.IO.FileInfo file = new System.IO.FileInfo(filepath);
		file.Directory.Create();
		System.IO.File.WriteAllBytes(filepath + filename, bytes);
		Debug.Log("Exporting sonicprint as file: " + filepath + filename);

		//Hold the chant data in statis.
		chantFilePath = audiofilepath;
		sonicFootprintFilePath = filepath + filename;
	}

	private Texture2D TakeScreenshot() {
		everythingElse.SetActive(false);
		int resWidth = 1920;
		int resHeight = 1920;
		RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
		Camera.main.targetTexture = rt;
		Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
		Camera.main.Render();
		RenderTexture.active = rt;
		screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
		Camera.main.targetTexture = null;
		RenderTexture.active = null; // JC: added to avoid errors
		Destroy(rt);
		everythingElse.SetActive(true);
		return screenShot;
	}


	//Voice functions


	/// <summary>
	/// Returns true if the frequency detect by the microphone is within the tolerance range of the input frequency.
	/// </summary>
	/// <param name="a_frequency"></param>
	/// <param name="a_tolerance"></param>
	/// <returns></returns>
	private bool IsValidVoiceInput(float a_frequency, float a_tolerance) {
		//Make sure the tolerance value isn't too small and get the detected frequency value.
		float tol = a_tolerance < 0.0f ? 0.0f : a_tolerance;
		float fundamentalFrequency = microphone.GetFundamentalFrequency();

		//Calculate how close the frequency is and check if the frequency detected is within tolerance.
		float howClose = (new Vector2(a_frequency, 0.0f) - new Vector2(fundamentalFrequency, 0.0f)).magnitude;
		bool withinThreshold = howClose <= tol;

		//Calculate the closness multipler.
		if (withinThreshold) {
			closeness = 1 - Mathf.InverseLerp(0.0f, tol, howClose);
		} else {
			closeness = 0;
		}

		//Return whether or not the fundamental frequency detected is within the tolerance threshold.
		return withinThreshold;
	}

	//Sonic footprint drawing functions.

	/// <summary>
	/// Converts the input angle to a vector.
	/// </summary>
	/// <param name="angle"></param>
	/// <returns></returns>
	private Vector3 ConvertAngleToVector(float angle) {
		if (angle > 360.0f + 90) {
			Debug.LogError("Invalid angle passed into 'ConvertAngleToVector' function in script 'SonicFootprintGenerator.cs'.");
			return Vector3.zero;
		}

		Vector3 angleVec = new Vector3((float)Mathf.Cos(angle * Mathf.Deg2Rad), (float)Mathf.Sin(angle * Mathf.Deg2Rad), 0.0f);
		return angleVec * lineMultiplier;
	}

	private IEnumerator DrawFootprintCooldown() {
		coolingDown = true;
		yield return new WaitForSeconds(increments);
		coolingDown = false;
	}

	//Unity event functions.
	private void OnValidate() {
		lineMultiplier = lineMultiplier <= 0.0f ? 0.01f : lineMultiplier;
		lengthOfFootprintTime = lengthOfFootprintTime <= 0.0f ? 0.0f : lengthOfFootprintTime;
		desiredFrequency = desiredFrequency <= 0.0f ? 0.0f : desiredFrequency;
		toleranceInHz = toleranceInHz <= 0.0f ? 0.0f : toleranceInHz;

		//SetUpSonicFootprintLineData();

		//Ensure desired tone exists.
		bool exists = false;
		for (int i = 0; i < musicalNotes.Count; i++) {
			if (musicalNotes[i].note == desiredTone) {
				exists = true;
				break;
			}
		}
		if (!exists) {
			desiredTone = string.Empty;
			Debug.LogWarning("Desired tone does not yet exist in the musical notes list.");
		}
	}

	private string RemoveUnderscores(string a_sInput) {
		//Parse the string and remove any underscores found.
		string input = string.Empty;
		for (int i = 0; i < a_sInput.Length; i++) {
			if (a_sInput[i] == '_') {
				input = input + ' ';
			} else {
				input = input + a_sInput[i];
			}
		}


		//Return it.
		return input;
	}

	private void DeactivateGigPanel() {
		GameObject[] panelArray = GameObject.FindGameObjectsWithTag("GigPanel");
		if (panelArray.Length > 0) {
			GameObject panel = panelArray[0];
			currentGigName = panel.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text + "_";
			panel.SetActive(false);
			//Debug.Log("'SonicFootprintGenerator.cs': Deactivating gig info panel.");
		}
	}

	private IEnumerator ReferenceToneCooldown(Action<bool> a_callback, AudioClip clip, bool useTimer = true) {
		//Activate target note text.
		if (targetNoteText != null) {
			targetNoteText.gameObject.SetActive(true);
			targetNoteText.text = "Your target note is: " + desiredTone;
		}

		//Deactivate gig stuff.
		DeactivateGigPanel();

		//Deactive chant screen.
		if (gigScreen != null) {
			gigScreen.SetActive(false);
		}

		//Deactivate text stuff.
		Debug.Log("Reference tone playing");
		tapToChantText.SetActive(false);

		//Activate Timer
		if (useTimer) {
			CountdownTimerScript.StartTimer((int)clip.length, null, string.Empty);
		}
		yield return new WaitForSecondsRealtime(clip.length + 1.5f);

		//Reactivate text stuff.
		tapToChantText.SetActive(true);

		//Invoke the callback.
		a_callback?.Invoke(true);

		//Invoke and inspector defined event.
		afterReferenceTone?.Invoke();

		//Allow chant recording to start.
		chantOver = false;

		//Deactivate target note text.
		if (targetNoteText != null) {
			targetNoteText.gameObject.SetActive(false);
		}
	}
	#endregion

	#region Static functions.
	private static void DestroyAllLines() {
		//Make sure any prexisting lines are deleted.
		if (m_linesList != null) {
			for (int i = 0; i < m_linesList.Count; i++) {
				GameObject currentObject = m_linesList[i];
				Destroy(currentObject);
			}
			//Make sure list is clear and reinistialised.
			m_linesList.Clear();
		}
	}

	public static void StartRecordingFootprint(string a_chantName, Action<string, Texture2D> a_callbackFunction) {
		if (microphone != null) {
			microphone.UpdateMicrophone();
		}
		DestroyAllLines();
		hasStarted = true;
		m_chantFileName = a_chantName;
		m_callback = a_callbackFunction;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>string.Empty if file has not been generated.</returns>
	public static string GetChantFilePath() {
		if (chantFilePath == string.Empty) {
			Debug.LogWarning("WARNING: Chant filepath being fetched before it has been generated.");
		}
		return chantFilePath;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>string.Empty if file has not been generated.</returns>
	public static string GetSonicFootprintFilepath() {
		if (sonicFootprintFilePath == string.Empty) {
			Debug.LogWarning("WARNING: Sonic footprint filepath being fetched before it has been generated.");
		}
		return sonicFootprintFilePath;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>string.Empty if no message was supplied.</returns>
	public static string GetChantMessage() {
		if (s_chantMessage == string.Empty) {
			Debug.LogWarning("WARNING: No chant message was supplied to send with the sonic footprint.");
		}
		return s_chantMessage;
	}
	#endregion

	#region Public Access Functions (Getters and Setters).
	public void SetChantMessage(string a_message) {
		Debug.Log("Setting chant message: " + a_message);
		if (a_message == string.Empty) {
			Debug.LogWarning("WARNING: Chant message is empty.");
		}


		s_chantMessage = RemoveUnderscores(a_message);
	}

	public void SetDesiredDone(string a_desiredTone) {
		desiredTone = a_desiredTone;
	}

	public void PlayReferenceTone() {
		//Get the referece tone for the current desired tone.
		desiredFrequency = GetDesiredFrequency();//this initialises the current note variable.
		AudioClip refTone = currentNote.noteReferenceTone;

		//Play it and start a cooldown to load the sonic footprint.
		AudioSource referenceToneSource = GameObject.FindGameObjectsWithTag("ReferenceToneSource")[0].GetComponent<AudioSource>();
		referenceToneSource.clip = refTone;
		referenceToneSource.Play();

		//Start cooldown till sonic footprint.
		StopAllCoroutines();
		StartCoroutine(ReferenceToneCooldown(ResetFootprintVariables, refTone));
	}
	#endregion

	#region Utility Classes/Structs/Enums
	[System.Serializable]
	public class MusicalNotes {
		public string note;
		public float frequency;
		public Color noteColour;
		public Color noteDarkColour;
		public AudioClip noteReferenceTone;

		public MusicalNotes() {

		}
	}
	#endregion
}
