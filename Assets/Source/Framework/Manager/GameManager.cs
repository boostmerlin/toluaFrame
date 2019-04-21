using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace LuaFramework {
    public class GameManager : Manager {
        protected static bool initialize = false;
        private List<string> downloadFiles = new List<string>();
		private GameObject startpanel = null;
        void Awake() {
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        void Init() {
            DontDestroyOnLoad(gameObject);  //防止销毁自己
            CheckExtractResource(); //释放资源
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void CheckExtractResource() {
            bool isExists = Directory.Exists(CSUtil.DataPath) &&
              Directory.Exists(CSUtil.DataPath + "lua/") && File.Exists(CSUtil.DataPath + "files.txt");
            if (isExists || AppDef.DebugMode) {
                StartCoroutine(OnUpdateResource());
                return;   //文件已经解压过了，自己可添加检查文件列表逻辑
            }
            StartCoroutine(OnExtractResource());    //启动释放协成 
        }

		private GameObject CreateLoadUI()
		{
			GameObject load = Resources.Load<GameObject> ("UIExtract");
			GameObject parent = GameObject.FindGameObjectWithTag ("GUICanvas");
			return GameObject.Instantiate (load, parent.transform, false);
		}

        IEnumerator OnExtractResource() 
		{
			string dataPath = CSUtil.DataPath;  //数据目录
            string resPath = CSUtil.AppContentPath(); //游戏包资源目录

            if (Directory.Exists(dataPath)) Directory.Delete(dataPath, true);
            Directory.CreateDirectory(dataPath);

            string infile = resPath + "files.txt";
            string outfile = dataPath + "files.txt";
            if (File.Exists(outfile))
            {
                File.Delete(outfile);
            }

            string message = "正在解包文件:>files.txt";
            Debug.Log(infile);
            Debug.Log(outfile);
            if (Application.platform == RuntimePlatform.Android) {
                WWW www = new WWW(infile);
                yield return www;

                if (www.isDone) {
                    File.WriteAllBytes(outfile, www.bytes);
                }
                yield return 0;
            } else File.Copy(infile, outfile, true);
            yield return new WaitForEndOfFrame();

            //释放所有文件到数据目录
            string[] files = File.ReadAllLines(outfile);
			float total = files.Length;
			int count = 0;
            foreach (var file in files) {
                string[] fs = file.Split('|');
                infile = resPath + fs[0];  //
                outfile = dataPath + fs[0];

                message = "正在解包文件:>" + fs[0];
                Debug.Log("正在解包文件:>" + infile);

                string dir = Path.GetDirectoryName(outfile);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                if (Application.platform == RuntimePlatform.Android) {
                    WWW www = new WWW(infile);
                    yield return www;

                    if (www.isDone) {
                        File.WriteAllBytes(outfile, www.bytes);
                    }
                    yield return 0;
                } else {
                    if (File.Exists(outfile)) {
                        File.Delete(outfile);
                    }
                    try
                    {
                        File.Copy(infile, outfile, true);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("File extract error: " + e.ToString());
                    }
                }
                yield return new WaitForEndOfFrame();
				count += 1;
            }
            message = "解包完成!!!";
            yield return new WaitForSeconds(0.1f);
            message = string.Empty;
            //释放完成，开始启动更新资源
            StartCoroutine(OnUpdateResource());
        }

        /// <summary>
        /// 启动更新下载，这里只是个思路演示，此处可启动线程下载更新
        /// </summary>
        IEnumerator OnUpdateResource() {
            if (!AppDef.UpdateMode) {
                OnResourceInited();
                yield break;
            }
            string dataPath = CSUtil.DataPath;  //数据目录
            string url = AppDef.WebUrl;
            string message = string.Empty;
            string random = DateTime.Now.ToString("yyyymmddhhmmss");
            string listUrl = url + "files.txt?v=" + random;
            Debug.LogWarning("LoadUpdate---->>>" + listUrl);

            WWW www = new WWW(listUrl); yield return www;
            if (www.error != null) {
                OnUpdateFailed(string.Empty);
                yield break;
            }
            if (!Directory.Exists(dataPath)) {
                Directory.CreateDirectory(dataPath);
            }
            File.WriteAllBytes(dataPath + "files.txt", www.bytes);
            string filesText = www.text;
            string[] files = filesText.Split('\n');

            for (int i = 0; i < files.Length; i++) {
                if (string.IsNullOrEmpty(files[i])) continue;
                string[] keyValue = files[i].Split('|');
                string f = keyValue[0];
                string localfile = (dataPath + f).Trim();
                string path = Path.GetDirectoryName(localfile);
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
                string fileUrl = url + f + "?v=" + random;
                bool canUpdate = !File.Exists(localfile);
                if (!canUpdate) {
                    string remoteMd5 = keyValue[1].Trim();
                    string localMd5 = CSUtil.md5file(localfile);
                    canUpdate = !remoteMd5.Equals(localMd5);
                    if (canUpdate) File.Delete(localfile);
                }
                if (canUpdate) {   //本地缺少文件
                    Debug.Log(fileUrl);
                    message = "downloading>>" + fileUrl;
                    /*
                    www = new WWW(fileUrl); yield return www;
                    if (www.error != null) {
                        OnUpdateFailed(path);   //
                        yield break;
                    }
                    File.WriteAllBytes(localfile, www.bytes);
                     */
                    //这里都是资源文件，用线程下载
                    //BeginDownload(fileUrl, localfile);
                    while (!(IsDownOK(localfile))) { yield return new WaitForEndOfFrame(); }
                }
            }
            yield return new WaitForEndOfFrame();

            message = "更新完成!!";

            OnResourceInited();
        }

        void OnUpdateFailed(string file) {
            string message = "更新失败!>" + file;
        }

        /// <summary>
        /// 是否下载完成
        /// </summary>
        bool IsDownOK(string file) {
            return downloadFiles.Contains(file);
        }

        /// <summary>
        /// 资源初始化结束
        /// </summary>
        public void OnResourceInited() {
            GetManager<ResourceManager>().Initialize(AppDef.AssetDir, delegate() {
                Debug.Log("Initialize OK!!!");
                this.OnInitialize();
            });
        }

        void OnInitialize() {
            GetManager<LuaManager>().InitStart();
            GetManager<NetworkManager>().OnInit();
            initialize = true;
        }
    }
}