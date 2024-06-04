using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// A class that gets information from a GigInfoTemplate and loads it into a large
// diplay panel
public class GigInfoButton : MonoBehaviour
{
    public GameObject panel;
    public GameObject[] panelFinder;

    private RawImage artMain;
    private TMP_Text GigTitle;
    private TMP_Text GigDate; 
    private TMP_Text GigDescription;
    
    public void Initialize()
    {
        // Find the large gig information panel, and retrieve its components
        panelFinder = GameObject.FindGameObjectsWithTag("GigPanel");
        panel = panelFinder[0];
        artMain = panel.transform.GetChild(0).gameObject.GetComponent<RawImage>();
        GigTitle = panel.transform.GetChild(1).gameObject.GetComponent<TMP_Text>();
        GigDate = panel.transform.GetChild(2).gameObject.GetComponent<TMP_Text>();
        GigDescription = panel.transform.GetChild(3).gameObject.GetComponent<TMP_Text>();
        // make sure the panel isn't displaying
        panel.SetActive(false);
    }

    public void OnClicked(Button button) {
        GigInfoTemplate gigInfoButton = button.transform.parent.GetComponent<GigInfoTemplate>();
        // display the panel
        panel.SetActive(true);
        // load the information into the panel
        artMain.texture = gigInfoButton.gigArt.texture;
        GigTitle.text = gigInfoButton.gigTitle.text;
        GigDate.text = gigInfoButton.gigDate.text;
        GigDescription.text = gigInfoButton.gigDesc.text;
    }


}
