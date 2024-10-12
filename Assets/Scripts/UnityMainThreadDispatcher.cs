using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance = null;
    private readonly Queue<System.Action> _executionQueue = new Queue<System.Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<UnityMainThreadDispatcher>();
            if (_instance == null)
            {
                _instance = new GameObject("UnityMainThreadDispatcher").AddComponent<UnityMainThreadDispatcher>();
            }
        }
        
        return _instance;
    }

    public void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(System.Action action)
    {
        lock (_executionQueue)
        {
            Debug.Log("UnityMainThreadDispatcher Enqueue");
            _executionQueue.Enqueue(action);
        }
    }
}
