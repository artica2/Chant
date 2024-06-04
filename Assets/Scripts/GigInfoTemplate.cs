using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// A storage class that holds information retrieved from firebase
// The information is then used by GigInfoButton to transfer it to a large display panel
public class GigInfoTemplate : MonoBehaviour {

    public TMP_Text gigTitle;
    public TMP_Text gigDate;
    public RawImage gigThumb;
    public RawImage gigArt;
    public TMP_Text gigDesc;

    public void Initialize() {

        gigTitle = transform.GetChild(0).gameObject.GetComponent<TMP_Text>();
        gigDate = transform.GetChild(1).gameObject.GetComponent<TMP_Text>();
        gigThumb = transform.GetChild(2).gameObject.GetComponent<RawImage>();
        gigDesc = transform.GetChild(3).gameObject.GetComponent<TMP_Text>();
        gigArt = transform.GetChild(4).gameObject.GetComponent<RawImage>();
        gigArt.enabled = false;
    }

    void Update() {

    }

}

