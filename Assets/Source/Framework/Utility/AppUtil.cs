using UnityEngine;

public static class AppUtil
{
    public static string deviceModel
    {
        get
        {
            return SystemInfo.deviceModel;
        }
    }

    public static string processorType
    {
        get
        {
            return SystemInfo.processorType;
        }
    }

    public static int screenWidth
    {
        get
        {
            return Screen.width;
        }
    }

    public static int screenHeight
    {
        get
        {
            return Screen.height;
        }
    }

    public static string deviceId
    {
        get
        {
            string ret = SystemInfo.deviceUniqueIdentifier;
#if UNITY_EDITOR || UNITY_STANDALONE
#elif UNITY_ANDROID
            ret = NativeCaller.getUid();
#elif UNITY_IOS
            ret = NativeCaller.getUid();
#endif
            return ret;
        }
    }
}
