using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NativeReceiver : MonoBehaviour
{
    static NativeReceiver _instance;
    public static NativeReceiver Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<NativeReceiver>();

                if (_instance == null)
                {
                    _instance = new GameObject("NativeReceiver").AddComponent<NativeReceiver>();
                }

                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    //for identify calling method.
    Dictionary<int, Action<bool>> _delegates;
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
            _delegates = new Dictionary<int, Action<bool>>();
        }
        else
        {
            if (this != _instance)
            {
                Destroy(this.gameObject);
            }
        }
    }
    void OnDestroy()
    {
        if (_delegates != null)
        {
            _delegates.Clear();
            _delegates = null;
        }
    }
    //TODO
}
