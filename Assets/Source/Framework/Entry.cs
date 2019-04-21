using UnityEngine;
using System.IO;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LuaFramework
{
    /// <summary>
    /// </summary>
    public class Entry : MonoBehaviour
    {
        private void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = AppDef.GameFrameRate;
        }

        private void OnEnable()
        {
            SpriteAtlasManager.atlasRequested += SpriteAtlasManager_atlasRequested;
        }

        private void OnDisable()
        {
            SpriteAtlasManager.atlasRequested -= SpriteAtlasManager_atlasRequested;
        }

        private void SpriteAtlasManager_atlasRequested(string tag, System.Action<SpriteAtlas> action)
        {
            Debug.Log("SpriteAtlasManager atlasRequested: " + tag);
            ResourceManager rm  = AppFacade.Instance.GetManager<ResourceManager>();
            rm.LoadAsset<SpriteAtlas>("spriteatlas/" + tag, new string[] { tag }, (obj)=>
            {
                if(obj.Length > 0)
                    action((SpriteAtlas)obj[0]);
            });
        }
        void Start()
        {
            LogDeviceInfo();
            AppFacade.Instance.StartUp();   //启动游戏  
        }

        #region LogDeviceInfo
        public static void LogDeviceInfo()
        {
            string text = "Ginkgo";
            string text2 = text;
            text = string.Concat(new object[]
            {
            text2,
            " ("
            });
            text += Application.platform.ToString();
            text2 = text;
            text = string.Concat(new object[]
            {
            text2,
            "deviceModel=",
            SystemInfo.deviceModel,
            ";",
            "deviceType=",
            SystemInfo.deviceType,
            ";",
            "deviceUniqueIdentifier=",
            SystemInfo.deviceUniqueIdentifier,
            ";",
            "graphicsDeviceID=",
            SystemInfo.graphicsDeviceID,
            ";",
            "graphicsDeviceName=",
            SystemInfo.graphicsDeviceName,
            ";",
            "graphicsDeviceVendor=",
            SystemInfo.graphicsDeviceVendor,
            ";",
            "graphicsDeviceVendorID=",
            SystemInfo.graphicsDeviceVendorID,
            ";",
            "graphicsDeviceVersion=",
            SystemInfo.graphicsDeviceVersion,
            ";",
            "graphicsMemorySize=",
            SystemInfo.graphicsMemorySize,
            ";",
            "graphicsShaderLevel=",
            SystemInfo.graphicsShaderLevel,
            ";",
            "npotSupport=",
            SystemInfo.npotSupport,
            ";",
            "operatingSystem=",
            SystemInfo.operatingSystem,
            ";",
            "processorCount=",
            SystemInfo.processorCount,
            ";",
            "processorType=",
            SystemInfo.processorType,
            ";",
            "supportedRenderTargetCount=",
            SystemInfo.supportedRenderTargetCount,
            ";",
            "supports3DTextures=",
            SystemInfo.supports3DTextures,
            ";",
            "supportsAccelerometer=",
            SystemInfo.supportsAccelerometer,
            ";",
            "supportsComputeShaders=",
            SystemInfo.supportsComputeShaders,
            ";",
            "supportsGyroscope=",
            SystemInfo.supportsGyroscope,
            ";",
            "supportsImageEffects=",
            SystemInfo.supportsImageEffects,
            ";",
            "supportsInstancing=",
            SystemInfo.supportsInstancing,
            ";",
            "supportsLocationService=",
            SystemInfo.supportsLocationService,
            ";",
            "supportsRenderToCubemap=",
            SystemInfo.supportsRenderToCubemap,
            ";",
            "supportsShadows=",
            SystemInfo.supportsShadows,
            ";",
            "supportsSparseTextures=",
            SystemInfo.supportsSparseTextures,
            ";",
            "supportsVibration=",
            SystemInfo.supportsVibration,
            ";",
            "systemMemorySize=",
            SystemInfo.systemMemorySize,
            ";",
            "SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)=",
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf),
            ";",
            "SupportsRenderTextureFormat(RenderTextureFormat.ARGB4444)=",
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB4444),
            ";",
            "SupportsRenderTextureFormat(RenderTextureFormat.Depth)=",
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth),
            ";",
            "graphicsDeviceVersion.StartsWith(\"Metal\")=",
            SystemInfo.graphicsDeviceVersion.StartsWith("Metal"),
            ";",
            "currentResolution.width=",
            Screen.currentResolution.width,
            ";",
            "currentResolution.height=",
            Screen.currentResolution.height,
            ";",
            "screen.width=",
            Screen.width,
            ";",
            "screen.height=",
            Screen.height,
            ";",
            "dpi=",
            Screen.dpi,
            ";",
            });

            text += "genuine? " + Application.genuine;
            Debugger.Log("userAgent = " + text.Substring(0, text.Length / 2));
            Debugger.Log("userAgent = " + text.Substring(text.Length / 2));
            Debugger.Log("Application.dataPath = " + Application.dataPath);
            Debugger.Log("Application.persistentDataPath = " + Application.persistentDataPath);
            Debugger.Log("Application.streamingAssetsPath = " + Application.streamingAssetsPath);
            Debugger.Log("Application.temporaryCachePath = " + Application.temporaryCachePath);
            Debugger.Log("Environment.CurrentDirectory = " + System.Environment.CurrentDirectory);
        }
        #endregion

#if UNITY_STANDALONE_WIN
        void ReStart()
        {
            string[] batscripts = new string[]
            {
              "@echo off",
              "echo wscript.sleep 1000 > sleep.vbs",
              "start /wait sleep.vbs",
              "start /d \"{0}\" {1}",
              "del /f /s /q sleep.vbs",
              "exit"
            };
            string path = Application.dataPath + "/../";

            List<string> prefabs = new List<string>();
            prefabs = new List<string>(Directory.GetFiles(Application.dataPath + "/../", "*.exe", SearchOption.AllDirectories));
            foreach (string keyx in prefabs)
            {
                string _path = Application.dataPath;
                _path = _path.Remove(_path.LastIndexOf("/")) + "/";
                Debug.LogError(_path);
                string _name = Path.GetFileName(keyx);
                batscripts[3] = string.Format(batscripts[3], _path, _name);
            }

            string batPath = Application.dataPath + "/../restart.bat";
            if (File.Exists(batPath))
            {
                File.Delete(batPath);
            }
            using (FileStream fileStream = File.OpenWrite(batPath))
            {
                using (StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.GetEncoding("UTF-8")))
                {
                    foreach (string s in batscripts)
                    {
                        writer.WriteLine(s);
                    }
                    writer.Close();
                }
            }
            Application.Quit();
            Application.OpenURL(batPath);
        }
#endif
        private void Update()
        {
#if UNITY_STANDALONE_WIN
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
                if (luaMgr == null)
                {
                    return;
                }
                luaMgr.CallFunction("EscapeKeyDown");
            }
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
            {
#if !UNITY_EDITOR
                ReStart();
#endif
            }
#endif
        }
    }
}