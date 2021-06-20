using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class CSVHandler : MonoBehaviour
{
    //The url is never going to change
    private static string url = "https://nnedigitaldesignstorage.blob.core.windows.net/candidatetasks/Metadata.csv?sp=r&st=2021-03-15T09:12:39Z&se=2024-11-05T17:12:39Z&spr=https&sv=2020-02-10&sr=b&sig=oyj3Qyg4W42%2BO0d7YqmjxmKk0k%2BLVmE243ixdLaq3gk%3D";
    
    public float casheTimeLimitSeconds = 10f;
    private bool useCashed = false;
    private string cashedText; //The whole csv file
    private Dictionary<string, string> cashedValues = new Dictionary<string, string>(); //The specific row, ex. blue

    public void HandleCSVFile(string name, Action<string> callback) //I use the name as an identifier because the name of the object seems to fit the name in the data
    {
        if (useCashed)
        {
            if (cashedValues.ContainsKey(name))
            {
                callback(cashedValues[name]);
            }
            else
            {
                callback(ParseText(cashedText, name));
            }
        }
        else
        {
            cashedValues.Clear();
            StartCoroutine(FetchCSVFile(name, callback));
            useCashed = true;
            Invoke("UseCashedResetter", casheTimeLimitSeconds); //Resets the bool back to false in x seconds
        }
    }
    
    //Sends request to server, and if it gets an answer it displays it
    private IEnumerator FetchCSVFile(string name, Action<string> callback) 
    {
        UnityWebRequest webRequest = new UnityWebRequest(url);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        yield return webRequest.SendWebRequest();
        if(webRequest.isNetworkError || webRequest.isHttpError) 
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            cashedText = webRequest.downloadHandler.text;
            // Show results as text
            callback(ParseText(cashedText, name));
        }
    }
    
    //Sets useCashed bool to false every casheTimeLimitSeconds in Seconds
    private void UseCashedResetter()
    {
        useCashed = false;
    }
    
    //Goes through the answer to find the specific row needed, cashing it for later
    private string ParseText(string text, string name)
    {
        string constructedString = "";
        var rows = text.Trim().Split('\n');
        print(name);
        for (int i = 1; i < rows.Length; i++) //Starting from 1 instead of 0 because the first row is going to be headers and I need to find the correct data
        {
            var column = rows[i].Split(';');
            if (column[1].GetHashCode() == name.GetHashCode()) //Hashing it because comparing strings is expensive
            {
                var firstColumn = rows[0].Split(';');
                for (int j = 0; j < column.Length-1; j++)
                {
                    constructedString = String.Concat(constructedString, firstColumn[j], ':', column[j], ", ", '\n');
                }
                
                constructedString = String.Concat(constructedString, firstColumn[column.Length-1], ':', column[column.Length-1]);
                break;
            }
        }

        constructedString = constructedString.Replace("\r", ""); //The text was overlapping because of Carriage Return char(13), so im replacing it with nothing

        cashedValues.Add(name, constructedString);
        return constructedString;
    }
}
