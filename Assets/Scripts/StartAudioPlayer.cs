using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartAudioPlayer : MonoBehaviour {
	[SerializeField]
	private AudioSource m_AudioSource = null;

	[SerializeField]
	private GameObject audioPlayer = null;

	[SerializeField]
	private List<GameObject> mixDetails = null;

	[SerializeField]
	private Button button = null;

	[SerializeField]
	private Image buttonImage = null;

	[SerializeField]
	private Color colour;

	private void Update() {
		Debug.LogError("Please do not use this for the audio player, it is deprecated.");
		if (m_AudioSource == null) {
			return;
		}

		if (button == null) {
			return;
		}

		if (buttonImage == null) {
			return;
		}

		if (m_AudioSource.clip == null) {
			button.enabled = false;
			buttonImage.color = colour;
		} else {
			button.enabled = true;
		}

	}

	public void AssignAudioSource() {
		Debug.LogError("Please do not use this for the audio player, it is deprecated.");
		if (AudioPlayerScript.GetInstance() == null) {
			return;
		}
		if (m_AudioSource == null) {
			Debug.LogError("Audio Source is null.");
			return;
		}

		//AudioPlayerScript.AssignAudioClip(m_AudioSource);

	}

	public void ActivateAudioPlayer() {
		Debug.LogError("Please do not use this for the audio player, it is deprecated.");
		if (AudioPlayerScript.GetInstance() == null) {
			
			audioPlayer.SetActive(true);
			//AudioPlayerScript.SetInstance(audioPlayer);
		}
	}
}
