using UnityEngine;
using System.Runtime.InteropServices;

public static class NativeCaller
{
//#if UNITY_EDITOR || UNITY_STANDALONE

#if UNITY_ANDROID
    static AndroidJavaClass javaUtils;
    static AndroidJavaObject unityActivity;
    static NativeCaller()
    {
        javaUtils = new AndroidJavaClass("com.arthur.slx.Utils");
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
    }

    public static string getUid()
    {
        return javaUtils.CallStatic<string>("getUniqueID");
    }

    public static void ShowText(string msg, string body)
    {
        unityActivity.Call("ShareText", msg, body);
    }
#elif UNITY_IOS
    [DllImport("__Internal")]
    private static extern string _getUUID();
    public static string getUid()
    {
        return _getUUID();
    }
#endif
}

