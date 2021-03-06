﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using UnityEngine.UI;

#if !UNITY_EDITOR
    using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
    using System.Threading.Tasks;
#endif

public class DolphinManager : MonoBehaviour
{
    public static DolphinManager instance = null;

    private string dolphinIpAddr = "192.168.0.177"; //177 per Phil
    private Stack<SamEvents> eventStack;

#if UNITY_EDITOR
    private HttpListener _listener;
    private string unityIpAddr = "192.168.0.173";
    private int unityPort = 60000;
#endif

#if !UNITY_EDITOR
        private string holoLensIpAddr = "192.168.0.147";
        private int holoLensPort = 9000;

        private StreamSocket socket;
        private static String serverIp = "192.168.0.140";
        private static int serverPort = 60000;
        private Task exchangeTask;
        private BinaryReader reader = null;
        
        private bool connected;
        private bool exchanging = false;
        private bool exchangeStopRequested = false;
#endif

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this.gameObject);

        DontDestroyOnLoad(this);
        InputHandler.instance.OnColorChanged += ChangeDolphinColor;
    }

    public void Connect()
    {   
        StartCoroutine(HttpMessage.SendSingleColorAllLeds(GameManager.instance.CurrentColor, dolphinIpAddr));
        eventStack = new Stack<SamEvents>();

#if UNITY_EDITOR
        //Invoke("InitializeUnityServer", 4f);

#else
        if (!connected)
            Invoke("InitializeUWPServer", 4f);
#endif
    }

    void Update()
    {
        if (eventStack != null && eventStack.Count != 0)
            HandleDolphinEvent(eventStack.Pop());
    }        

#if UNITY_EDITOR
    void InitializeUnityServer()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://+:" + unityPort.ToString() + "/");
        _listener.Start();        
        _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);      
    }

    private void ListenerCallback(IAsyncResult result)
    {
        HttpListener listener = (HttpListener)result.AsyncState;
        HttpListenerContext context = listener.EndGetContext(result);

        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        string contRead = new StreamReader(request.InputStream).ReadToEnd();
        _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        SamEvents samEvents = new SamEvents();
        samEvents = JsonUtility.FromJson<SamEvents>(contRead);
        eventStack.Push(samEvents);
    }
#endif

#if !UNITY_EDITOR
    private async void InitializeUWPServer()
    {
        StartCoroutine(HttpMessage.SendHttpChange(serverIp, 60001, dolphinIpAddr));
        Debug.Log("Delfino connesso!");

        try
            {        
                Debug.Log("Tentativo di connessione con il server avviato!");                
                socket = new StreamSocket();
                HostName serverHost = new HostName(serverIp);
                await socket.ConnectAsync(serverHost, serverPort.ToString());              
                Stream streamIn = socket.InputStream.AsStreamForRead();
                reader = new BinaryReader(streamIn);          
                Debug.Log("Connesso al server!");
                connected = true;
                RestartExchange();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }                
    }
#endif

    public void ExchangePackets()
    {
#if !UNITY_EDITOR
        Debug.Log("Entro nel loop");

        while (!exchangeStopRequested)
        {
            if (reader == null) continue;
            exchanging = true;

            byte[] received = reader.ReadBytes(65);
            String message = System.Text.Encoding.UTF8.GetString(received);

            Debug.Log("RECEIVED: " + message);

            if (message.ToCharArray()[64] == '}' )
            {
                SamEvents samEvents = new SamEvents();
                samEvents = JsonUtility.FromJson<SamEvents>(message);
                eventStack.Push(samEvents);
            }
                
            else 
            {
                Debug.Log("Ricevuto messaggio doppio!");
                try 
                {
                    while(true)
                        reader.ReadByte();
                } 
                catch (Exception e) { Debug.Log(e.Message); }
            }

            exchanging = false;    
            Debug.Log("Messaggio correttamente smaltito");

        }

#endif
    }

    public void RestartExchange()
    {
#if !UNITY_EDITOR
        try
        { 
            if (exchangeTask != null) 
                StopExchange();
            exchangeStopRequested = false;
            exchangeTask = Task.Run(() => ExchangePackets());
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
#endif
    }


    public void StopExchange()
    {
#if !UNITY_EDITOR
        try
        { 
            exchangeStopRequested = true;
      
            if (exchangeTask != null) 
            {
                exchangeTask.Wait();
                socket.Dispose();
                reader.Dispose();
                socket = null;
                exchangeTask = null;
                reader = null;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
#endif
    }

    //call methods of InputHandler according to the message received from the dolphin
    private void HandleDolphinEvent(SamEvents samEvent)
    {
        if (samEvent.events[0].typ == "touch" && samEvent.events[0].act == false)
        {
            switch (samEvent.events[0].val)
            {
                case "1": InputHandler.instance.OnNextColor();
                          Debug.Log("Next Color!");
                    break;
                case "2": InputHandler.instance.OnPreviousColor();
                          Debug.Log("Previous Color!");
                    break;
                case "5": InputHandler.instance.SubmitColor();
                          Debug.Log("Submit Color!");
                    break;
            }
        }
    }

    private void ChangeDolphinColor()
    {
        StartCoroutine(HttpMessage.SendSingleColorAllLeds(GameManager.instance.CurrentColor, dolphinIpAddr));
    }

    public void MakeDolphinHappy()
    {
        StartCoroutine(HttpMessage.SendMusicMessage("4", dolphinIpAddr));
        StartCoroutine(HttpMessage.SendMoveMessage(HttpMessage.DolphinMoves.moveMouth, dolphinIpAddr));
    }

    public void MakeDolphinSad()
    {
        StartCoroutine(HttpMessage.SendMusicMessage("9", dolphinIpAddr));
        StartCoroutine(HttpMessage.SendMoveMessage(HttpMessage.DolphinMoves.moveMouth, dolphinIpAddr));
    }

}

