#if NET_4_6
using LuaFramework;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

//test not work for lua, for lua wrap invoke.
public class CoroutineWaiter : INotifyCompletion
{
    bool _isDone;
    Action _continuation;
    public void Complete()
    {
        _isDone = true;
        if (_continuation != null)
        {
            _continuation();
        }
    }
    public bool IsCompleted
    {
        get { return _isDone; }
    }

    public void GetResult()
    {
        //no result.
        //todo: capture exception.
    }

    void INotifyCompletion.OnCompleted(Action continuation)
    {
        _continuation = continuation;
    }
}

public class CoroutineWrapper
{
    public static IEnumerator WaitReturnNull(
         CoroutineWaiter awaiter, IEnumerator instruction)
    {
        yield return instruction;
        awaiter.Complete();
    }
}

public class CoroutineRunner : MonoBehaviour
{

}

public static class AsyncUtil {
    static CoroutineRunner _ins;
    static CoroutineRunner runer
    {
        get
        {
            if(_ins == null)
            {
                if (_ins == null)
                {
                    _ins = new GameObject("CoroutineRunner")
                        .AddComponent<CoroutineRunner>();
                }
            }
            return _ins;
        }
    }
    public static CoroutineWaiter GetAwaiter(this IEnumerator coroutine)
    {
        var awaiter = new CoroutineWaiter();
        runer.StartCoroutine(CoroutineWrapper.WaitReturnNull(awaiter, coroutine));
        return awaiter;
    }
}
#endif