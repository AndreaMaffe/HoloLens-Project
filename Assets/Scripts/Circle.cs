﻿using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour, IFocusable, IInputClickHandler
{
    private CirclePanel circlePanel;
    private bool focused; //true if the player is currently looking at the circle
    private bool active; //true if game is started and it's possible to change color

    void Start()
    {
        InputHandler.instance.OnColorSubmitted += OnColorSubmitted;
        circlePanel = transform.parent.GetComponent<CirclePanel>();
    }

    void OnColorSubmitted()
    {
        if (active && focused)
        {
            AudioManager.instance.PlayPop1();
            SetColor(GameManager.instance.CurrentColor);
            GameManager.instance.OnCircleColored(this, GameManager.instance.CurrentColor);
        }
    }

    public void SetColor(Color color)
    {
        transform.Find("CircleObject").GetComponent<MeshRenderer>().material.color = color;
        transform.Find("Dolphin_logo").GetComponent<SpriteRenderer>().color = color;
    }

    public void OnFocusEnter()
    {
        focused = true;
    }

    public void OnFocusExit()
    {
        focused = false;
    }

    public void SetActive(bool value)
    {
        active = value;
    }

    //called when user clicks on the circle
    public void OnInputClicked(InputClickedEventData eventData)
    {
        OnColorSubmitted();
    }
}
