using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using LuaInterface;
using UObject = UnityEngine.Object;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AssetBundleInfo
{
    public AssetBundle m_AssetBundle;
    public Dictionary<string, UObject> loadedObject;
    public int m_ReferencedCount;
    public bool abort = false;//todo: 中途暂停
    public AssetBundleInfo(AssetBundle assetBundle)
    {
        m_AssetBundle = assetBundle;
        loadedObject = new Dictionary<string, UObject>();
        m_ReferencedCount = 1;
    }
}

namespace LuaFramework
{
    public class ResourceManager : Manager
    {
        string m_BaseDownloadingURL = "";
        string[] m_AllManifest = null;
        AssetBundleManifest m_AssetBundleManifest = null;
        Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();
        Dictionary<string, AssetBundleInfo> m_LoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
        Dictionary<string, List<LoadAssetRequest>> m_LoadRequests = new Dictionary<string, List<LoadAssetRequest>>();
#if UNITY_EDITOR
        List<string> gameAssetPaths = new List<string>();
        void RecursiveDir(string path)
        {
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            gameAssetPaths.Add(path.Replace('\\', '/'));
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                RecursiveDir(dir);
            }
        }
#endif

        class LoadAssetRequest
        {
            public Type assetType;
            public string[] assetNames;
            public LuaFunction luaFunc;
            public Action<UObject[]> sharpFunc;
        }

        // Load AssetBundleManifest.
        public void Initialize(string manifestName, Action initOK)
        {
#if UNITY_EDITOR
            foreach (var path in AppDef.GameAssetPaths)
            {
                RecursiveDir(path);
            }
#endif
            m_BaseDownloadingURL = CSUtil.GetRelativePath();
            LoadAsset<AssetBundleManifest>(manifestName, new string[] { "AssetBundleManifest" }, delegate (UObject[] objs)
            {
                if (objs.Length > 0)
                {
                    m_AssetBundleManifest = objs[0] as AssetBundleManifest;
                    m_AllManifest = m_AssetBundleManifest.GetAllAssetBundles();
                }
                if (initOK != null) initOK();
            });

        }


        public void LoadPrefab(string abName, string assetName, Action<UObject[]> func)
        {
            LoadAsset<GameObject>(abName, new string[] { assetName }, func);
        }

        public void LoadPrefab(string abName, string[] assetNames, Action<UObject[]> func)
        {
            LoadAsset<GameObject>(abName, assetNames, func);
        }

        /// <summary>
        /// lua 调用返回的是UserData
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="assetNames"></param>
        /// <param name="func"></param>

        public void LoadPrefab(string abName, string[] assetNames, LuaFunction func)
        {
            LoadAsset<GameObject>(abName, assetNames, null, func);
        }
        public void LoadSpriteFromAtlas(string abName, string[] assetNames, LuaFunction func)
        {
#if UNITY_EDITOR
            string dir = "SpriteAtlas/";
#else
            string dir = "spriteatlas/";
#endif
            LoadAsset<SpriteAtlas>(dir + abName, new string[] { abName }, (objs) =>
            {
                List<Sprite> result = new List<Sprite>();
                if (objs.Length > 0)
                {
                    SpriteAtlas spriteAtlas = objs[0] as SpriteAtlas;
                    foreach (var assetName in assetNames)
                    {
                        Sprite s = spriteAtlas.GetSprite(Path.GetFileNameWithoutExtension(assetName));
                        if (s)
                            result.Add(s);
                    }
                }
                if (func != null)
                {
                    if (result.Count > 0)
                        func.Call((object)result.ToArray());
                    else
                        func.Call();
                    func.Dispose();
                }
            });
        }
        public void LoadSprite(string abName, string[] assetNames, LuaFunction func)
        {
            LoadAsset<Texture2D>(abName, assetNames, (objs) =>
          {
              List<Sprite> result = new List<Sprite>();
              foreach (Texture2D texture in objs)
              {
                  if (texture)
                  {
                      Sprite s = Sprite.Create(texture, Rect.MinMaxRect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                      result.Add(s);
                  }
              }

              if (func != null)
              {
                  if (result.Count > 0)
                      func.Call((object)result.ToArray());
                  else
                      func.Call();
                  func.Dispose();
              }
          }, null);
        }

        string GetRealAssetPath(string abName)
        {
            if (abName.Equals(AppDef.AssetDir))
            {
                return abName;
            }
            abName = abName.ToLower();
            if (!abName.EndsWith(AppDef.ExtName))
            {
                abName += AppDef.ExtName;
            }
            if (AppDef.DebugMode)
            {
                for (int i = 0; i < m_AllManifest.Length; i++)
                {
                    string path = Path.GetFileName(m_AllManifest[i]);    //字符串操作函数都会产生GC
                    if (path.Equals(abName))
                    {
                        return m_AllManifest[i];
                    }
                }
                Debug.LogWarning("GetRealAssetPath Error:>>" + abName);
                return null;
            }
            else
            {
                return abName;
            }
        }

#if NET_4_6
        async
#endif
        public void LoadBytes(string name, LuaFunction func)
        {
#if UNITY_EDITOR
            if (AppDef.DebugMode)
            {
                {
                    TextAsset obj = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Configs/" + name);
                    if (func != null && obj != null)
                    {
                        func.Call(new LuaByteBuffer(obj.bytes));
                        func.Dispose();
                        func = null;
                    }
                    else
                    {
                        Debugger.LogWarning("Load Bytes failed : " + name);
                    }
                }
                return;
            }
#endif
            string url = m_BaseDownloadingURL + name;
#if NET_4_6
            await MyLoadAsset(url, func);
#else
            StartCoroutine(MyLoadAsset(url, func));
#endif
        }

        IEnumerator MyLoadAsset(string uri, LuaFunction func)
        {
            var download = new WWW(uri);
            yield return download;

            if (func != null && string.IsNullOrEmpty(download.error))
            {
                func.Call(new LuaByteBuffer(download.bytes));
                func.Dispose();
                func = null;
            }
        }

        string[] type2Ext(Type t)
        {
            if (t == typeof(GameObject))
            {
                return new string[] { ".prefab", ".fbx" };
            }
            else if (t == typeof(Texture2D))
            {
                return new string[] { ".png", ".jpg", ".psd" };
            }
            else if (t == typeof(SoundData))
            {
                return new string[] { ".prefab" };
            }
            else if (t == typeof(UnityEngine.U2D.SpriteAtlas))
            {
                return new string[] { ".spriteatlas" };
            }
            return null;
        }

        /// <summary>
        /// 载入素材
        /// </summary>
#if NET_4_6
        async
#endif
        public void LoadAsset<T>(string abName, string[] assetNames, Action<UObject[]> action = null, LuaFunction func = null) where T : UObject
        {
#if UNITY_EDITOR
            if (AppDef.DebugMode)
            {
                List<UObject> result = new List<UObject>();
                var exts = type2Ext(typeof(T));
                abName = abName.ToLower();
                AssetBundleInfo abi = GetLoadedAssetBundle(abName);
                if (abi == null)
                {
                    abi = AddLoadedAssetBundle(abName, null);
                }
                foreach (string assetName in assetNames)
                {
                    UObject obj;
                    if (abi.loadedObject.TryGetValue(assetName, out obj))
                    {
                        result.Add(obj);
                        continue;
                    }
                    string hasExt = Path.GetExtension(assetName);
                    string nameNoExt = Path.GetFileNameWithoutExtension(assetName).ToLower();
                    string mergeName = abName.Replace(nameNoExt, "");
                    foreach (var path in gameAssetPaths)
                    {
                        string prefix = path;
                        {
                            prefix += mergeName;
                            if (!prefix.EndsWith("/"))
                            {
                                prefix += "/";
                            }
                        }
                        if (!string.IsNullOrEmpty(hasExt) || exts == null)
                        {
                            string ff = string.Format("{0}{1}", prefix, assetName);
                            obj = AssetDatabase.LoadAssetAtPath<T>(ff);
                            if (obj)
                            {
                                result.Add(obj);
                                abi.loadedObject[assetName] = obj;
                                continue;
                            }
                        }
                        else
                        {
                            foreach (var ext in exts)
                            {
                                string ff = string.Format("{0}{1}{2}", prefix, assetName, ext);
                                obj = AssetDatabase.LoadAssetAtPath<T>(ff);
                                if (obj)
                                {
                                    result.Add(obj);
                                    abi.loadedObject[assetName] = obj;
                                    continue;
                                }
                            }
                        }
                    }
                }

                if (result.Count == 0)
                {
                    Debug.LogWarning("Editor LoadAsset, no asset:  " + string.Join(",", assetNames));
                }

                if (action != null)
                {
                    action(result.ToArray());
                    action = null;
                }
                if (func != null)
                {
                    if (result.Count > 0)
                        func.Call((object)result.ToArray());
                    else
                        func.Call();
                    func.Dispose();
                    func = null;
                }
                return;
            }
#endif
            abName = GetRealAssetPath(abName);
            LoadAssetRequest request = new LoadAssetRequest();
            request.assetType = typeof(T);
            request.assetNames = assetNames;
            request.luaFunc = func;
            request.sharpFunc = action;

            List<LoadAssetRequest> requests = null;
            if (!m_LoadRequests.TryGetValue(abName, out requests))
            {
                requests = new List<LoadAssetRequest>();
                requests.Add(request);
                m_LoadRequests.Add(abName, requests);
#if NET_4_6
                await OnLoadAsset<T>(abName);
#else
                StartCoroutine(OnLoadAsset<T>(abName));
#endif
            }
            else
            {
                requests.Add(request);
            }
        }

        IEnumerator OnLoadAsset<T>(string abName) where T : UObject
        {
            AssetBundleInfo bundleInfo = GetLoadedAssetBundle(abName);
            if (bundleInfo == null)
            {
                yield return StartCoroutine(OnLoadAssetBundle(abName, typeof(T)));
                bundleInfo = GetLoadedAssetBundle(abName);
                if (bundleInfo == null)
                {
                    m_LoadRequests.Remove(abName);
                    Debugger.LogError("OnLoadAsset Error --->>>" + abName);
                    yield break;
                }
            }
            List<LoadAssetRequest> list = null;
            if (!m_LoadRequests.TryGetValue(abName, out list))
            {
                m_LoadRequests.Remove(abName);
                yield break;
            }
            for (int i = 0; i < list.Count; i++)
            {
                string[] assetNames = list[i].assetNames;
                if (assetNames == null)
                {
                    continue;
                }
                AssetBundle ab = bundleInfo.m_AssetBundle;
                var loadedCache = bundleInfo.loadedObject;
                UObject obj;
                List<UObject> result = new List<UObject>();
                for (int j = 0; j < assetNames.Length; j++)
                {
                    string assetPath = assetNames[j];
                    assetPath = assetPath.Substring(assetPath.LastIndexOf("/") + 1);
                    if (loadedCache.TryGetValue(assetPath, out obj))
                    {
                        result.Add(obj);
                    }
                    else
                    {
                        AssetBundleRequest request = ab.LoadAssetAsync(assetPath, list[i].assetType);
                        yield return request;
                        obj = request.asset;
                        loadedCache.Add(assetPath, obj);
                        result.Add(obj);
                    }
                }
                if (list[i].sharpFunc != null)
                {
                    list[i].sharpFunc(result.ToArray());
                    list[i].sharpFunc = null;
                }
                if (list[i].luaFunc != null)
                {
                    list[i].luaFunc.Call((object)result.ToArray());
                    list[i].luaFunc.Dispose();
                    list[i].luaFunc = null;
                }
                //bundleInfo.m_ReferencedCount++;
            }
            m_LoadRequests.Remove(abName);
        }

        IEnumerator OnLoadAssetBundle(string abName, Type type)
        {
            string url = m_BaseDownloadingURL + abName;
            WWW download = null;
            if (type == typeof(AssetBundleManifest))
                download = new WWW(url);
            else
            {
                string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                if (dependencies.Length > 0)
                {
                    m_Dependencies.Add(abName, dependencies);
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        string depName = dependencies[i];
                        AssetBundleInfo bundleInfo = null;
                        if (m_LoadedAssetBundles.TryGetValue(depName, out bundleInfo))
                        {
                            bundleInfo.m_ReferencedCount++;
                        }
                        else if (!m_LoadRequests.ContainsKey(depName))
                        {
                            //LoadAssetRequest request = new LoadAssetRequest();
                            //List<LoadAssetRequest> requests = null;
                            //{
                            //    requests = new List<LoadAssetRequest>();
                            //    requests.Add(request);
                            //    m_LoadRequests.Add(depName, requests);
                            //}
                            yield return StartCoroutine(OnLoadAssetBundle(depName, type));
                        }
                    }
                }
                download = WWW.LoadFromCacheOrDownload(url, m_AssetBundleManifest.GetAssetBundleHash(abName), 0);
            }
            yield return download;
            if (!string.IsNullOrEmpty(download.error))
            {
                Debugger.LogError("WWW load Asset Error for: " + download.error);
                yield break;
            }
            //fix bug: cards.unity
            if (GetLoadedAssetBundle(abName) == null)
            {
                AssetBundle assetObj = download.assetBundle;
                AddLoadedAssetBundle(abName, assetObj);
            }
        }
        AssetBundleInfo AddLoadedAssetBundle(string abName, AssetBundle ab)
        {
            if (ab != null || AppDef.DebugMode)
            {
                var abi = new AssetBundleInfo(ab);
                m_LoadedAssetBundles.Add(abName, abi);
                return abi;
            }
            return null;
        }
        AssetBundleInfo GetLoadedAssetBundle(string abName)
        {
            AssetBundleInfo bundle = null;
            //abName = Path.GetFileNameWithoutExtension(abName);
            m_LoadedAssetBundles.TryGetValue(abName, out bundle);
            if (bundle == null) return null;

            // No dependencies are recorded, only the bundle itself is required.
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies))
                return bundle;

            // Make sure all dependencies are loaded
            foreach (var dependency in dependencies)
            {
                AssetBundleInfo dependentBundle;
                m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
                if (dependentBundle == null) return null;
            }
            return bundle;
        }

        /// <summary>
        /// 此函数交给外部卸载专用，自己调整是否需要彻底清除AB
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="isThorough"></param>
        public void UnloadAssetBundle(string abName, bool isThorough = false)
        {
            if (AppDef.DebugMode) return;
            abName = GetRealAssetPath(abName);
            if (!string.IsNullOrEmpty(abName))
            {
                Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + abName);
                UnloadAssetBundleInternal(abName, isThorough);
                UnloadDependencies(abName, isThorough);
                Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + abName);
            }
        }

        void UnloadDependencies(string abName, bool isThorough)
        {
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies))
                return;

            // Loop dependencies.
            foreach (var dependency in dependencies)
            {
                UnloadAssetBundleInternal(dependency, isThorough);
            }
            m_Dependencies.Remove(abName);
        }

        void UnloadAssetBundleInternal(string abName, bool isThorough)
        {
            AssetBundleInfo bundle = GetLoadedAssetBundle(abName);
            if (bundle == null) return;
            if (--bundle.m_ReferencedCount <= 0)
            {
                if (m_LoadRequests.ContainsKey(abName))
                {
                    return;     //如果当前AB处于Async Loading过程中，卸载会崩溃，只减去引用计数即可
                }
                if (bundle.m_AssetBundle != null)
                {
                    bundle.m_AssetBundle.Unload(isThorough);
                    bundle.m_AssetBundle = null;
                }
                //if(isThorough) //simple it.
                {
                    bundle.loadedObject.Clear();
                    m_LoadedAssetBundles.Remove(abName);
                }
                Resources.UnloadUnusedAssets();
                GC.Collect();
            }
        }
    }
}

