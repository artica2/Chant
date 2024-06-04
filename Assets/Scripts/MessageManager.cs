///////////////////////////////////////////////////////////////////////////////////////////
/// Filename:         MessageManager.cs
/// Author:           Jack Kellett
/// Date Created:     23/01/2023
/// Purpose:          A central location for any script to quickly pop a message on screen.
///////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MessageManager : MonoBehaviour
{
	#region Variables to assign via the unity inspector (SerializeFields).
	[SerializeField]
	private GameObject messageBox = null;

	[SerializeField]
	private TextMeshProUGUI textMeshText = null;

	[SerializeField]
	[Range(0f, 20f)]
	private float messageSpeed = 1f;
	#endregion

	#region Private Variables.
	private static Queue<MessageInfo> messageQueue = new Queue<MessageInfo>();
	private Coroutine messageCoroutine = null;
	#endregion

	#region Public Access Functions.
	/// <summary>
	/// Queues a message to be displayed. The callback is a function to be ran after the message has been displayed.
	/// </summary>
	/// <param name="a_message"></param>
	/// <param name="a_callback"></param>
	public static void QueueMessage(string a_message, Action a_callback = null) {
		if (messageQueue == null) {
			Debug.LogError("Attempted to show message without adding a message manager to the scene.");
			return;
		}


		//Add the new message to the queue.
		MessageInfo message;
		message.message = a_message;
		message.callback = a_callback;
		messageQueue.Enqueue(message);
	}

	/// <summary>
	/// If an exception needs to be handled, this should be used to display the correct message to the user.
	/// </summary>
	/// <param name="a_message"></param>
	public void QueueException(string a_message) {
		QueueMessage(a_message, null);
	}
	#endregion

	#region Private Functions.
	// Start is called before the first frame update
	private void Start()
    {
		messageCoroutine = null;
		messageQueue = new Queue<MessageInfo>();
		messageBox.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
		//If there is a message being played.
        if(messageCoroutine != null) {
			//Early out.
			return;
		}

		//If the message queue has not been initialised.
		if(messageQueue == null) {
			//Early out.
			return;
		}

		//Show messages where there is atleast one display.
		if(messageQueue.Count > 0) {
			messageCoroutine = StartCoroutine(PlayMessage(messageQueue.Dequeue()));
		}
    }

	private IEnumerator PlayMessage(MessageInfo a_info) {
		messageBox.SetActive(true);
		textMeshText.text = a_info.message;
		yield return new WaitForSecondsRealtime(messageSpeed);
		//Turn off relevant text box info.
		textMeshText.text = "";
		messageBox.SetActive(false);
		messageCoroutine = null;

		//If there's a callback, activate it.
		if(a_info.callback != null) {
			a_info.callback.Invoke();
		}
	}
	#endregion

	#region Structs.
	/// <summary>
	/// This struct is used to bundle the message information with a callback function to be ran after the message has been displayed.
	/// </summary>
	private struct MessageInfo {
		public string message;
		public Action callback;
	}
	#endregion
}
