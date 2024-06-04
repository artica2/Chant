using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
    public static Utility instance = null;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }
    }

    // the sent chant data is stored in the format CHANTNAME_MESSAGESENT
    // the following function is able to parse strings in this form to quickly retrieve either CHANTNAME or MESSAGESENT
    public string SplitStringAtChar(string a_str, char charToSplit = '_', bool returnFirstHalf = true) {
        string str = new string(a_str);
        string firstString = string.Empty;
        int breakInt = 0;

        for (int i = 0; i < str.Length; i++) {
            if (str[i] != charToSplit) {
                firstString += str[i];
            } else {
                breakInt = i;
                break;

            }
        }
        if (returnFirstHalf) {
            return firstString;
        }
        string secondString = string.Empty;
        for (int i = breakInt + 1; i < str.Length; i++) {
            secondString += str[i];
        }
        return secondString;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
