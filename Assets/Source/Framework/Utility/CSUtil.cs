using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using LuaFramework;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LuaFramework
{
    public class CSUtil
    {
        public static int Int(object o)
        {
            return Convert.ToInt32(o);
        }

        public static float Float(object o)
        {
            return (float)Math.Round(Convert.ToSingle(o), 2);
        }

        public static long Long(object o)
        {
            return Convert.ToInt64(o);
        }

        public static int Random(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static float Random(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static long GetTime()
        {
            TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            return (long)ts.TotalMilliseconds;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        public static T Add<T>(GameObject go) where T : Component
        {
            if (go != null)
            {
                T ts = go.GetComponent<T>();
                if (ts)
                {
                    return ts;
                }
                return go.AddComponent<T>();
            }
            return null;
        }

        public static T Add<T>(Transform t) where T : Component
        {
            if (t != null)
            {
                return Add<T>(t.gameObject);
            }
            return null;
        }

        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        public static string md5(string source)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            string destString = "";
            for (int i = 0; i < md5Data.Length; i++)
            {
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            destString = destString.PadLeft(32, '0');
            return destString;
        }

        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string md5file(string file)
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fs);
                fs.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }

        /// <summary>
        /// 清除所有子节点
        /// </summary>
        public static void ClearChildren(Transform go)
        {
            if (go == null) return;
            for (int i = go.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(go.GetChild(i).gameObject);
            }
        }

        public static void ClearMemory()
        {
            GC.Collect(); Resources.UnloadUnusedAssets();
            LuaManager mgr = AppFacade.Instance.GetManager<LuaManager>();
            if (mgr != null) mgr.LuaGC();
        }


        public static string AppContentPath()
        {
            string path = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    path = "jar:file://" + Application.dataPath + "!/assets/";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    path = Application.dataPath + "/Raw/";
                    break;
                default:
                    path = Application.dataPath + "/" + AppDef.AssetDir + "/";
                    break;
            }
            return path;
        }
        /// <summary>
        /// 取得数据存放目录
        /// </summary>
        public static string DataPath
        {
            get
            {
                string game = AppDef.AppName.ToLower();
                string ret = "c:/" + game + "/";
                if (Application.isMobilePlatform)
                {
                    ret = Application.persistentDataPath + "/" + game + "/";
                }
                else if (AppDef.StreamAssetsMode)
                {
                    ret = Application.dataPath + "/" + AppDef.AssetDir + "/";
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    int i = Application.dataPath.LastIndexOf('/');
                    ret = Application.dataPath.Substring(0, i + 1) + game + "/";
                }
                if (!Directory.Exists(ret))
                {
                    Directory.CreateDirectory(ret);
                }
                return ret;
            }
        }

        public static string GetRelativePath()
        {
            if (Application.isEditor)
                return "file://" + Environment.CurrentDirectory.Replace("\\", "/") + "/Assets/" + AppDef.AssetDir + "/";
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
                return "file:///" + DataPath;
            else // For standalone player.
                return "file://" + Application.streamingAssetsPath + "/";
        }

        /// <summary>
        /// 网络可用
        /// </summary>
        public static bool NetAvailable
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }

        /// <summary>
        /// 是否是无线
        /// </summary>
        public static bool IsWifi
        {
            get
            {
                return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            }
        }

        /// <summary>
        /// 执行Lua方法
        /// </summary>
        public static object[] CallMethod(string module, string func, params object[] args)
        {
            LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
            if (luaMgr == null) return null;
            return luaMgr.CallFunction(module + "." + func, args);
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}