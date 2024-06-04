using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
using System.Web;

public class ListAllGigs : MonoBehaviour
{
    public GameObject GigDetailsPrefab;
    public Transform Gigs_List;
    public int numOfListItems;

    FirebaseStorage storage;
    StorageReference storageReference;
    Texture2D downloadedTexture;
    GameObject panel;

    private List<GameObject> gigHolder = new List<GameObject>();
    string URL = "https://firebasestorage.googleapis.com/v0/b/chant-app-fcc98.appspot.com/o/gigs%2Fgig.jpg?alt=media&token=ab1ef9f1-bfb3-4259-b8b1-6e3cb4e37d34";

    void Start() {
        // find the display panel and make sure its not being displayed
        GameObject[] panelFinder = GameObject.FindGameObjectsWithTag("GigPanel");
        panel = panelFinder[0];
        panel.SetActive(false);        
    }

    public void ViewGigsButton()
    {
        //Call the getAllGigs coroutine passing the email and password
        GetAllGigs();
    }

    private void GetAllGig() //old code
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        Query allGigsQuery = db.Collection("gigs");
        allGigsQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot allGigsQuerySnapshot = task.Result;
            foreach (DocumentSnapshot documentSnapshot in allGigsQuerySnapshot.Documents)
            {
                Debug.Log(System.String.Format("Document data for {0} document:", documentSnapshot.Id));
                Dictionary<string, object> gig = documentSnapshot.ToDictionary();

                var newGig = Instantiate(GigDetailsPrefab, Gigs_List);
                var gigTitle = newGig.transform.GetChild(0).gameObject;
                var gigDate = newGig.transform.GetChild(1).gameObject;

                foreach (KeyValuePair<string, object> pair in gig)
                {
                    Debug.Log(System.String.Format("{1}", pair.Value));
                }
                // Newline to separate entries
                Debug.Log("");
            }
        });
    }

    private void GetAllGigs()
    {
        // clear the previous gigs (to avoid duplicates)
        foreach (GameObject currentGig in gigHolder) {
            Destroy(currentGig);
        }
        // Retrieve the gig data
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        Query allGigsQuery = db.Collection("gigs");
        allGigsQuery.GetSnapshotAsync().ContinueWithOnMainThread(task => {
        QuerySnapshot allGigsQuerySnapshot = task.Result;
            // for each gig
            foreach (DocumentSnapshot documentSnapshot in allGigsQuerySnapshot.Documents)
            {
                // Storage variables
                string gigNameStorage = string.Empty;
                string artMainNameStorage = string.Empty;
                string artThumbNameStorage = string.Empty;
                Dictionary<string, object> gigs = documentSnapshot.ToDictionary();
                var newGig = Instantiate(GigDetailsPrefab, Gigs_List);
                // instantiate the gig info button
                gigHolder.Add(newGig);
                GigInfoTemplate GigInfo = newGig.GetComponent<GigInfoTemplate>();
                GigInfoButton GigButton = newGig.GetComponent<GigInfoButton>();
                panel.SetActive(true);
                GigInfo.Initialize();
                GigButton.Initialize();
                var gigTitle = newGig.transform.GetChild(0).gameObject;
                var gigDate = newGig.transform.GetChild(1).gameObject;
                var gigPic = newGig.transform.GetChild(2).gameObject;
                var gigDesc = newGig.transform.GetChild(3).gameObject;
                var gigArt = newGig.transform.GetChild(4).gameObject;

                foreach (KeyValuePair<string, object> pair in gigs)
                {
                    // parse the document data and load it into strings
                    if (pair.Key == "name")
                    {
                        if (pair.Value is string)
                        {
                            GigInfo.gigTitle.text = (string)pair.Value;
                            gigNameStorage = (string)pair.Value;
                        }
                    }
                    if(pair.Key == "artMain") {
                        artMainNameStorage = (string)pair.Value;
                    }
                    if(pair.Key == "artThumb") {
                        artThumbNameStorage = (string)pair.Value;
                    }
                    if (pair.Key == "startDate")
                    {
                        if (pair.Value is Firebase.Firestore.Timestamp)
                        {
                            var timestamp = (Firebase.Firestore.Timestamp)pair.Value;
                            var myDateTime = timestamp.ToDateTime();
                            GigInfo.gigDate.text = myDateTime.ToString();
                            //gigDate.GetComponent<TMP_Text>().text = myDateTime.ToString();
                        }
                        else
                        {
                            Debug.Log("Retrieved date is not The correct Firestore.Timestamp class!");
                        }
                    }
                    if (pair.Key == "description")
                    {
                        GigInfo.gigDesc.text = (string)pair.Value;
                    }
                }
                // parse the information drawn about the images
                storage = FirebaseStorage.DefaultInstance;
                storageReference = storage.GetReferenceFromUrl("gs://chant-app-fcc98.appspot.com/");
                string artMainString = "gigs/" + gigNameStorage + "/" + artMainNameStorage + ".jpg";               
                string artThumbString = "gigs/" + gigNameStorage + "/" + artThumbNameStorage + ".jpg"; 
                // find the storage reference for the main image and download it
                StorageReference artMainImage = storageReference.Child(artMainString);
                artMainImage.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
                {
                    if (!task.IsFaulted && !task.IsCanceled) {
                        RawImage rawImage = gigPic.GetComponent<RawImage>();
                        StartCoroutine(GetTexture(Convert.ToString(task.Result), GigInfo.gigArt));
                    } else {
                        Debug.Log(task.Exception);
                    }
                });
                // find the storage reference for the thumbnail image and parse it
                StorageReference artThumbImage = storageReference.Child(artThumbString);
                artThumbImage.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
                    if (!task.IsFaulted && !task.IsCanceled) {
                        RawImage rawImage = gigPic.GetComponent<RawImage>();
                        StartCoroutine(GetTexture(Convert.ToString(task.Result), GigInfo.gigThumb));
                    } else {
                        Debug.Log(task.Exception);
                    }
                });

            }
        });
        
    }

    IEnumerator GetTexture(string URLTexture, RawImage image) {
        // parse the URL and use UnityWebRequest to download the corresponding texture
        string newString = URLTexture.Replace(" ", "%20"); // cant handle spaces properly
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(newString);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        } else {
            downloadedTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            downloadedTexture = CropTexture(downloadedTexture);
            image.texture = downloadedTexture;

        }
    }

    // a function to crop the downloaded images to fit better
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
