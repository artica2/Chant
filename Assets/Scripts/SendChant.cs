using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using Firebase.Extensions;

public class SendChant : MonoBehaviour {
	public FirebaseFirestore db;
	public static FirebaseUser user;

	FirebaseStorage storage;
	StorageReference storageReference;

	public TMP_Text gigTitleText;
	public GameObject panel;
	// Start is called before the first frame update
	void Start() {
		// get user data
		user = null;
		db = FirebaseFirestore.DefaultInstance;
		FirebaseAuth auth = FirebaseAuth.DefaultInstance;
		user = auth.CurrentUser;
	}

	public void OnSendChant() {
		// retrieve the message
		string chantMessage = SonicFootprintGenerator.GetChantMessage();
		DocumentReference userInfo = db.Collection("users").Document(user.UserId);
		string gigTitle = gigTitleText.text;
		string messageToSend = gigTitle + '_' + chantMessage;
		// The data is stored via a 'Sent Chants' array in the firebase user data
		// The following code will check for an existing array, and if one exists, will either add or overwrite depending on if an entry exists for the gig in question
		// if no array exists, the code will instead create one
		userInfo.GetSnapshotAsync().ContinueWithOnMainThread(task => {
			DocumentSnapshot docSnap = task.Result;
			Dictionary<string, object> dict = docSnap.ToDictionary();
			// if sent chants doesnt exist
			if (!dict.ContainsKey("Sent Chants")) {
				List<string> listOfStrings = new List<string>();
				listOfStrings.Add(messageToSend);
				Dictionary<string, List<string>> path = new Dictionary<string, List<string>>() {
					{ "Sent Chants", listOfStrings }
				};
				userInfo.SetAsync(path, SetOptions.MergeAll).ContinueWithOnMainThread(task => {

				});
				// if sent chants does exist
			} else {
				List<System.Object> sentChants = (List<System.Object>)dict["Sent Chants"];
				List<string> listOfStrings = new List<string>();

				foreach (System.Object sentChant in sentChants) {
					listOfStrings.Add(sentChant.ToString());
				}
				// if the gig doesn't exist
				List<string> gigsList = new List<string>();
				foreach (string gig in listOfStrings) {
					gigsList.Add(Utility.instance.SplitStringAtChar(messageToSend, '_', true));

                }
				
				if (!gigsList.Contains(gigTitle)) {
					listOfStrings.Add(messageToSend);
					Dictionary<string, List<string>> path = new Dictionary<string, List<string>>() {
					{ "Sent Chants", listOfStrings }
				};
					userInfo.SetAsync(path, SetOptions.MergeAll).ContinueWithOnMainThread(task => {

					});
					// gig already exists
				} else {
					int length = listOfStrings.Count;
					for (int i = 0; i < length; i++) {
						if (Utility.instance.SplitStringAtChar(listOfStrings[i]) == gigTitle) {
							Debug.Log("String To Remove: " + listOfStrings[i]);
							listOfStrings.Remove(listOfStrings[i]);
                        }
                    }

					listOfStrings.Add(messageToSend);
					Dictionary<string, List<string>> path = new Dictionary<string, List<string>>() {
					{ "Sent Chants", listOfStrings }
				};
					userInfo.SetAsync(path, SetOptions.MergeAll).ContinueWithOnMainThread(task => {

					});

				}
				//Get the sonic footprint .png path and chant .wav recording filepath.
				string sonicFootprintFilepath = SonicFootprintGenerator.GetSonicFootprintFilepath();
				string chantFilepath = SonicFootprintGenerator.GetChantFilePath();
				// retrieve the location of where the files are supposed to go
				storage = FirebaseStorage.DefaultInstance;
				storageReference = storage.GetReferenceFromUrl("gs://chant-app-fcc98.appspot.com/");
				StorageReference chantRef = storageReference.Child("gigs/" + gigTitle + "/Chants/" + user.UserId + ".wav");
				StorageReference footprintRef = storageReference.Child("gigs/" + gigTitle + "/Footprints/" + user.UserId + ".png");
				MessageManager.QueueMessage("Thank you for your donation!!");
				MessageManager.QueueMessage("Your chant mix will be ready in 2 weeks.");
				// Upload the files
				chantRef.PutFileAsync(chantFilepath).ContinueWithOnMainThread(task => {
					Debug.Log("Chant sent");
				});
				footprintRef.PutFileAsync(sonicFootprintFilepath).ContinueWithOnMainThread(task => {
					Debug.Log("Footprint Sent");
				});

				//If store window is not getting destroyed on first click, uncomment the code below which destroys it.
				//This code is by no means ideal but for demonstrating the flow of the project it's here to make it seem less messy
				//whilst the real reason for the bug is being found.
				GameObject window = GameObject.Find("UIFakeStoreWindow");
				Destroy(window);
			}
		});
	}

	// Update is called once per frame
	public void OnClickOff() {
		panel.SetActive(false);
	}

	public static string GetUserId() {
		if(user == null) {
			//early out.
			return string.Empty;
		}
		return user.UserId;
	}
}
