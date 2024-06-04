using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Messaging;
using UnityEngine;

/// <summary>
/// This script should be used to enable notifications to be sent to a user's device whenever one is pushed by the firebase console.
/// </summary>
public class Notifications : MonoBehaviour {

    void Start() {

    }

	private void OnEnable() {
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    private void OnDisable() {
        Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
    }

	public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
        Debug.Log("Received Reg Token:" + token.Token);
    }
    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
        Debug.Log("Received a new Message from:" + e.Message.From);
    }

}