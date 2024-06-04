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

public class RecieveMixes : MonoBehaviour
{
    public GameObject MixDetailsPrefab;
    public Transform Gigs_List;
    public int numOfListItems;
    FirebaseStorage storage;
    StorageReference storageReference;
    Texture2D downloadedTexture;
    FirebaseUser user;
    private List<GameObject> mixHolder = new List<GameObject>();

    public void GetMixes() {
        // clear the list of mixes
        List<string> listOfStrings = new List<string>();
        foreach (GameObject currentMix in mixHolder) {
            Destroy(currentMix);
        }
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        storage = FirebaseStorage.DefaultInstance;
        storageReference = storage.GetReferenceFromUrl("gs://chant-app-fcc98.appspot.com/");
        // retrieve user data
        DocumentReference userInfo = db.Collection("users").Document(user.UserId);
        userInfo.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            DocumentSnapshot docSnap = task.Result;
            Dictionary<string, object> dict = docSnap.ToDictionary();
            // retrieve a list of all the gigs the user has submitted a chant for
            if(dict.ContainsKey("Sent Chants")) {
                List<System.Object> sentChants = (List<System.Object>)dict["Sent Chants"];                
                foreach (System.Object sentChant in sentChants) {
                    string stringToAdd = Utility.instance.SplitStringAtChar(sentChant.ToString(), '_', true);
                    listOfStrings.Add(stringToAdd);
                }
            }

            foreach (string chantString in listOfStrings) {
                string mixStringStorage = string.Empty;
                // retrieve gig data
                DocumentReference chantRef = db.Collection("gigs").Document(chantString);
                chantRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {

                    Dictionary<string, object> chant = task.Result.ToDictionary();
                    // check to see if the gig has a mix associated and if so, store its name
                    if (chant.ContainsKey("mixName")) {
                        mixStringStorage = chant["mixName"].ToString();
                    }

                    // create a mix details button
                    var newGig = Instantiate(MixDetailsPrefab, Gigs_List);
                    var gigTitle = newGig.transform.GetChild(0).gameObject;
                    var mixStatus = newGig.transform.GetChild(1).gameObject;
                    var audioSourceVar = newGig.transform.GetChild(2).gameObject;              
                    mixHolder.Add(newGig);
                    gigTitle.GetComponent<TMP_Text>().text = chantString;
                    // parse the name of the mix into a useable form
                    string mixString = "gigs/" + chantString + "/" + mixStringStorage + ".wav";
                    Debug.Log("Mix String: " + mixString);
                    AudioSource audioSource = audioSourceVar.GetComponent<AudioSource>();
                    StorageReference mixRef = storageReference.Child(mixString);
                    // attempt to download the mix, and display a message based on success
                    mixRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
                        if (!task.IsFaulted && !task.IsCanceled) {
                            mixStatus.GetComponent<TMP_Text>().text = "Mix Ready!";
                         StartCoroutine(GetAudio(Convert.ToString(task.Result), audioSource));
                        } else {
                            mixStatus.GetComponent<TMP_Text>().text = "Mix not ready yet!";
                        }
                    });
                });

            }
        });
    }

    IEnumerator GetAudio(string mixTexture, AudioSource audio) {
        // parse the string into a URL
        string newString = mixTexture.Replace(" ", "%20");      
        // download the Audio
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(newString, AudioType.WAV)) {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError) {
                Debug.Log(www.error);
            } else {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                audio.clip = DownloadHandlerAudioClip.GetContent(www);
            }
        }
    }
}
