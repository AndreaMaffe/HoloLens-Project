﻿using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class PlayButton : MonoBehaviour, IInputClickHandler 
{

    public void OnInputClicked(InputClickedEventData eventData)
    {
       transform.root.GetComponent<CirclePanel>().Play();
    }


    
}



