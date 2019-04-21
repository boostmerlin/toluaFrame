using UnityEngine;
using System.Collections;
using LuaFramework;

public class Manager : MonoBehaviour {
    protected AppFacade facade = AppFacade.Instance;
    protected T GetManager<T>() where T : Component
    {
        return facade.GetManager<T>();
    }
}
