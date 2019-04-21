#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
#define USE_DEVICE_ID
#endif
using UnityEngine;

public class AppDef
{
#if UNITY_EDITOR
    public static bool DebugMode = true;                          //调试模式-用于内部测试
#else
	public static bool DebugMode = false;
#endif
    //是否从StreamAssets下读取资源
    public static bool StreamAssetsMode = !DebugMode;

#if USE_DEVICE_ID
    public static bool useDeviceId = true;
#else
    public static bool useDeviceId = false;
#endif

    public const bool LuaBundleMode = false;                    //Lua代码AssetBundle模式
                                                                /// </summary>
    public const bool UpdateMode = false;                       //更新模式-默认关闭 
    public const bool LuaByteMode = false;                       //Lua字节码模式-默认关闭 
    public const int TimerInterval = 1;
    public const int GameFrameRate = 60;                        //游戏帧频
    public const string GameResDir = "Assets/GameRes/";
    public const string ArtsDir = "Assets/Arts/";

    public static readonly string[] GameAssetPaths = new string[] {
            GameResDir,
            ArtsDir,
        };

    public const string AppName = "LuaFramework";               //应用程序名称
    public const string LuaTempDir = "Lua/";                    //临时目录
    public const string AppPrefix = AppName + "_";              //应用程序前缀
    public const string ExtName = ".unity3d";                   //素材扩展名
    public const string AssetDir = "StreamingAssets";           //素材目录 
    public const string WebUrl = "http://localhost:8081/";      //测试更新地址

    public static int SocketPort = 0;                           //Socket服务器端口
    public static string SocketAddress = string.Empty;          //Socket服务器地址

    public static string FrameworkRoot
    {
        get
        {
            return Application.dataPath + "/" + AppName;
        }
    }
}
