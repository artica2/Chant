using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// To play any audio that might be required. Whether it is listening to a recording or playing a mix that has been pulled down.
/// </summary>
public class AudioPlayerScript : MonoBehaviour {
	#region Variables to assign via the unity inspector (SerializeFields).
	//[SerializeField]
	//private AudioClip testClip = null;

	[SerializeField]
	private Slider m_Slider = null;

	[SerializeField]
	private TextMeshProUGUI m_text = null;

	[SerializeField]
	private TextMeshProUGUI m_buttonText = null;

	[SerializeField]
	private GameObject thingsToDisable = null;
	#endregion

	#region Private Variable Declarations.
	private AudioSource m_AudioSource = null;
	private static GameObject s_activeInstance = null;

	private bool shouldPlay = false;
	private float currentTime = 0.0f;
	#endregion

	#region Private Functions.
	// Start is called before the first frame update
	void Start() {
		//Get the audio source.
		m_AudioSource = this.gameObject.GetComponent<AudioSource>();
		if (m_AudioSource == null) {
			m_AudioSource = this.gameObject.AddComponent<AudioSource>();
		}

		//Set the instance of the audio player singleton to this.
		s_activeInstance = this.gameObject;

		//Make sure the timer starts correctly.
		UpdateTimerText();
	}

	// Update is called once per frame
	void Update() {
		HandleAudio();
	}

	private void OnEnable() {
		if (thingsToDisable != null) {
			thingsToDisable.SetActive(false);
		}
	}

	private void HandleAudio() {
		if (m_AudioSource == null) {
			Debug.LogError("Audio source was not attached to audio player.");
			return;
		}

		if (m_AudioSource.isPlaying) {
			UpdateTimer();
		}
	}

	private IEnumerator ClipCooldown(float time) {
		if (m_AudioSource != null) {
			//Update the timer max and min values.
			m_Slider.minValue = 0.0f;
			m_Slider.maxValue = m_AudioSource.clip.length;
		}
		yield return new WaitForSecondsRealtime(time);
		if (m_AudioSource != null && m_buttonText != null && m_Slider != null) {
			m_AudioSource.Stop();
			m_buttonText.text = "Play";
			currentTime = 0.0f;
			m_Slider.value = currentTime;
			m_Slider.onValueChanged.Invoke(currentTime);
		}
	}

	private void UpdateTimer() {
		if (m_Slider == null) {
			Debug.LogError("Slider was not assigned to audio player.");
			return;
		}

		if (m_AudioSource == null) {
			Debug.LogError("Audio source was not attached to audio player.");
			return;
		}

		//Get the current time and then update the timer's value.
		currentTime += Time.deltaTime;
		m_Slider.value = currentTime;
		m_Slider.onValueChanged.Invoke(currentTime);
	}
	#endregion

	#region Public Access Functions (Getters and Setters).
	/// <summary>
	/// Switch between play and pause mode.
	/// </summary>
	public void PlayPause() {
		if (m_AudioSource == null) {
			Debug.LogError("Audio source was not attached to audio player.");
			return;
		}

		if (m_AudioSource.clip == null) {
			//early out as there is no clip to play.
			return;
		}

		if (m_buttonText == null) {
			Debug.LogError("Play Button text was not assigned to audio player.");
			return;
		}

		if (m_Slider == null) {
			Debug.LogError("Slider was not assigned to audio player.");
			return;
		}

		//If the audiosource is not playing.
		if (!m_AudioSource.isPlaying) {
			//Play the music and set the button text to stop.
			m_buttonText.text = "Stop";
			m_AudioSource.Play();
			StartCoroutine(ClipCooldown(m_AudioSource.clip.length));
		} else {
			//Stop the music and set the button text to play.
			StopAllCoroutines();
			m_AudioSource.Stop();
			m_buttonText.text = "Play";
			currentTime = 0.0f;
			m_Slider.value = currentTime;
			m_Slider.onValueChanged.Invoke(currentTime);
		}
	}

	public void UpdateTimerText() {
		if (m_Slider == null) {
			Debug.LogError("Slider was not assigned to audio player.");
			return;
		}

		if (m_text == null) {
			Debug.LogError("Text was not assigned to audio player.");
			return;
		}

		//Get the current value of the timer slider and update the text to match.
		float time = Mathf.Round(m_Slider.value);
		m_text.text = "Time: " + time + "s";
	}

	public void AssignAudioClip(AudioClip a_clip) {
		if (a_clip == null) {
			Debug.LogError("Audio clip being sent to audio player is null.");
			return;
		}

		AudioSource audioSource = this.gameObject.GetComponent<AudioSource>();
		m_AudioSource = audioSource;
		if (audioSource == null) {
			Debug.LogError("Audio source on audio player was null.");
			return;
		}

		//Set the audio source reference to the correct one.
		audioSource.clip = a_clip;
	}

	public static GameObject GetInstance() {
		return s_activeInstance;
	}
	#endregion
}
