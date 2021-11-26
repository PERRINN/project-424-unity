using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class H_RetrieveText : MonoBehaviour
{

    public string members;
    public Text membersText;
    void Start()
    {
        StartCoroutine(GetText());
    }

    IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://perrinn.com/directory");
        yield return www.SendWebRequest();

        
        
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;


        //print data into
        members = www.downloadHandler.text;
        membersText.text = members;


    }
}