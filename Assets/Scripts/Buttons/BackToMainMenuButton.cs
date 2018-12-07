﻿using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenuButton : MonoBehaviour, HoloToolkit.Unity.InputModule.IInputClickHandler
{
    public void OnInputClicked(InputClickedEventData eventData)
    {
        Instantiate(Resources.Load<GameObject>("Prefabs/MainPanel"), new Vector3(0, 0, 2f), Quaternion.identity);
        Destroy(GameObject.Find("CirclePanel(Clone)"));
    }
}