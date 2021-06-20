using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum UIState
{
    none,
    interactionState,
    interactionDoneState,
    csvState
}
public class UIHandler : MonoBehaviour
{
    [SerializeField] private RectTransform interactionInfo;
    [SerializeField] private Image crosshair;
    [SerializeField] private TextMeshProUGUI csvUIContainer;

    private Dictionary<UIState, Action> changeStateDictionary = new Dictionary<UIState, Action>();
        
    public UIState _currentUIState;


    private void Start()
    {
        PopulateDictionary();
    }

    public void HandleUIState(UIState newUIState)
    {
        if (_currentUIState == newUIState)
        {
            return;
        }

        if (changeStateDictionary.ContainsKey(newUIState))
        {
            changeStateDictionary[newUIState]();
            _currentUIState = newUIState;
        }
        else
        {
            Debug.LogError("State not found, insert new state into dictionary");
        }
        
        
    }

    public void UpdateCSVText(string text)
    {
        csvUIContainer.text = text;
    }
    
    //Could honestly be a switch statement, but since HandleUIState is called a lot, I would rather utilize dictionary powers to avoid comparison
    private void PopulateDictionary()
    {
        changeStateDictionary.Add(UIState.none,
            () =>
            {
                crosshair.color = Color.white;
                interactionInfo.gameObject.SetActive(false);
                csvUIContainer.gameObject.SetActive(false);
            });
        
        changeStateDictionary.Add(UIState.interactionState,
            () =>
            {
                crosshair.color = Color.red;
                interactionInfo.gameObject.SetActive(true);
            });
        
        changeStateDictionary.Add(UIState.interactionDoneState,
            () =>
            {
                interactionInfo.gameObject.SetActive(false);
            });
        
        changeStateDictionary.Add(UIState.csvState,
            () =>
            {
                csvUIContainer.gameObject.SetActive(true);
                interactionInfo.gameObject.SetActive(false);
            });
    }
}
