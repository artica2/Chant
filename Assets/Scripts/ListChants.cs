using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Storage;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class ListChants : MonoBehaviour {
	public GameObject chantDetailsPrefab;
	public Transform Gigs_List;
	public int numOfListItems;

	FirebaseStorage storage;
	StorageReference storageReference;
	Texture2D downloadedTexture;

	FirebaseUser user;
	private List<GameObject> chantHolder = new List<GameObject>();

	public GameObject panel;

	string gigNameStorage;
	string artMainNameStorage;
	string artThumbNameStorage;

	public void GetChants() {
		List<string> listOfStrings = new List<string>();
		// clear the previous gigs (to avoid duplicates)
		foreach (GameObject currentGig in chantHolder) {
			Destroy(currentGig);
		}
		Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
		user = auth.CurrentUser;
		// Retrieve the gig data
		FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
		Query allGigsQuery = db.Collection("gigs");
		storage = FirebaseStorage.DefaultInstance;
		storageReference = storage.GetReferenceFromUrl("gs://chant-app-fcc98.appspot.com/");
		DocumentReference userInfo = db.Collection("users").Document(user.UserId);
		userInfo.GetSnapshotAsync().ContinueWithOnMainThread(task => {
			DocumentSnapshot docSnap = task.Result;
			Dictionary<string, object> dict = docSnap.ToDictionary();
			// retrieve the data about the gigs the user has sent chants for
			if (dict.ContainsKey("Sent Chants")) {
				List<System.Object> sentChants = (List<System.Object>)dict["Sent Chants"];
				foreach (System.Object sentChant in sentChants) {
					listOfStrings.Add(sentChant.ToString());
					Debug.Log("Mark Red");
				}
			}
			allGigsQuery.GetSnapshotAsync().ContinueWithOnMainThread(task => {
				QuerySnapshot allGigsQuerySnapshot = task.Result;
				// for each gig
				foreach (DocumentSnapshot documentSnapshot in allGigsQuerySnapshot.Documents) {
					Debug.Log(System.String.Format("Document data for {0} document:", documentSnapshot.Id));
					Dictionary<string, object> gigs = documentSnapshot.ToDictionary();
					// set up the gig info button
					var newGig = Instantiate(chantDetailsPrefab, Gigs_List);
					chantHolder.Add(newGig);
					GigInfoTemplate GigInfo = newGig.GetComponent<GigInfoTemplate>();
					GigInfoButton GigButton = newGig.GetComponent<GigInfoButton>();
					panel.SetActive(true);
					// initialize the gig info button and panel,
					GigInfo.Initialize();
					GigButton.Initialize();

					var gigTitle = newGig.transform.GetChild(0).gameObject;
					var gigDate = newGig.transform.GetChild(1).gameObject;
					var gigPic = newGig.transform.GetChild(2).gameObject;
					var gigDesc = newGig.transform.GetChild(3).gameObject;
					var gigArt = newGig.transform.GetChild(4).gameObject;

					foreach (KeyValuePair<string, object> pair in gigs) {
						Debug.Log("Looped");

						if (pair.Key == "name") {
							if (pair.Value is string) {
								// test the names of the gigs against the names of the gigs the user has sent chants for
								// if there is a match, then set up the button
								bool test = false;
								foreach (string name in listOfStrings) {
									if (Utility.instance.SplitStringAtChar(name, '_', true) == (string)pair.Value) {
										test = true;
										GigInfo.gigDesc.text = Utility.instance.SplitStringAtChar(name, '_', false);
									}
								}
								// if there is no match, the user has not sent a chant to that gig, and thus the button should be destroyed
								if (test != true) {
									Destroy(newGig);
									continue;
								}

								GigInfo.gigTitle.text = (string)pair.Value;
								gigNameStorage = (string)pair.Value;
							}
						}
						// parse the relevant gig information
						if (pair.Key == "artMain") {
							artMainNameStorage = pair.Value.ToString();
						}
						if (pair.Key == "artThumb") {
							artThumbNameStorage = pair.Value.ToString();
						}
						if (pair.Key == "startDate") {
							if (pair.Value is Firebase.Firestore.Timestamp) {
								var timestamp = (Firebase.Firestore.Timestamp)pair.Value;
								var myDateTime = timestamp.ToDateTime();
								GigInfo.gigDate.text = myDateTime.ToString();
								//gigDate.GetComponent<TMP_Text>().text = myDateTime.ToString();
							} else {
								Debug.Log("Retrieved date is not The correct Firestore.Timestamp class!");
							}
						}
						if (pair.Key == "description") {
							GigInfo.gigDesc.text = (string)pair.Value + "\n\n\nPress the 'record chant' button to re-record this chant.";
						}
					}
					// parse the names of the pictures and get the correct storage reference for them
						storage = FirebaseStorage.DefaultInstance;
						storageReference = storage.GetReferenceFromUrl("gs://chant-app-fcc98.appspot.com/");
						string artMainString = "gigs/" + gigNameStorage + "/Footprints/" + user.UserId + ".png";
						string artThumbString = "gigs/" + gigNameStorage + "/" + artThumbNameStorage + ".jpg";
						StorageReference artMainImage = storageReference.Child(artMainString);
						// download the main image
						artMainImage.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
						{
							if (!task.IsFaulted && !task.IsCanceled) {
								RawImage rawImage = gigPic.GetComponent<RawImage>();
								StartCoroutine(GetTexture(Convert.ToString(task.Result), GigInfo.gigArt, false));
							} else {
								Debug.Log(task.Exception);
							}
						});
						// download the thumbnail image
						StorageReference artThumbImage = storageReference.Child(artThumbString);
						artThumbImage.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
							if (!task.IsFaulted && !task.IsCanceled) {
								RawImage rawImage = gigPic.GetComponent<RawImage>();
								StartCoroutine(GetTexture(Convert.ToString(task.Result), GigInfo.gigThumb, false));
							} else {
								Debug.Log(task.Exception);
							}
						});
					
				}
			});

		});
	}

	IEnumerator GetTexture(string URLTexture, RawImage image, bool useWideCrop) {
		// parse the string into a URL
		string newString = URLTexture.Replace(" ", "%20");
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(newString);
		yield return www.SendWebRequest();
		// download the texture
		if (www.result != UnityWebRequest.Result.Success) {
			Debug.Log(www.error);
		} else {
			downloadedTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
			if (useWideCrop) {
				int width = downloadedTexture.width / 1;
				downloadedTexture = CropTexture(downloadedTexture, width, (int)(width * 0.711f));
			} else {
				downloadedTexture = CropTexture(downloadedTexture);
			}
			image.texture = downloadedTexture;

		}
	}

	// crop functions to make images fit better
	private Texture2D CropTexture(Texture2D tex, int a_width, int a_height) {
		RenderTexture renderTexture = new RenderTexture(a_width, a_height, 32);
		RenderTexture.active = renderTexture;
		Graphics.Blit(tex, renderTexture);
		Texture2D resizedTexture = new Texture2D(a_width, a_height);
		resizedTexture.ReadPixels(new Rect(0, 0, a_width, a_height), 0, 0);
		resizedTexture.Apply();
		return resizedTexture;
	}

	private Texture2D CropTexture(Texture2D tex) {
		int height = tex.height;
		int width = tex.width;
		if (height == width) {
			return tex;
		}
		int min = Mathf.Min(height, width);
		RenderTexture renderTexture = new RenderTexture(min, min, 32);
		RenderTexture.active = renderTexture;
		Graphics.Blit(tex, renderTexture);
		Texture2D resizedTexture = new Texture2D(min, min);
		resizedTexture.ReadPixels(new Rect(0, 0, min, min), 0, 0);
		resizedTexture.Apply();
		return resizedTexture;
	}
}