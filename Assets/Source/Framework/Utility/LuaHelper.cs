using UnityEngine;
using System.Reflection;
using LuaInterface;

namespace LuaFramework {
    public static class LuaHelper {

        /// <summary>
        /// getType
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        public static System.Type GetType(string classname) {
            Assembly assb = Assembly.GetExecutingAssembly();  //.GetExecutingAssembly();
            System.Type t = null;
            t = assb.GetType(classname); ;
            if (t == null) {
                Debug.LogWarning("LuaHelper.GetType NULL: " + classname);
                t = assb.GetType(classname);
            }
            return t;
        }

		public static GameManager GetGameManager()
		{
			return AppFacade.Instance.GetManager<GameManager>();
		}

        /// <summary>
        /// 资源管理器
        /// </summary>
        public static ResourceManager GetResManager() {
            return AppFacade.Instance.GetManager<ResourceManager>();
        }

        /// <summary>
        /// 网络管理器
        /// </summary>
        public static NetworkManager GetNetManager() {
            return AppFacade.Instance.GetManager<NetworkManager>();
        }

        /// <summary>
        /// 音乐管理器
        /// </summary>
        public static SoundManager GetSoundManager() {
            return AppFacade.Instance.GetManager<SoundManager>();
        }
    }
}