using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Ginkgo;
using LuaFramework;

public class AppFacade : IEventService
{
    private static AppFacade _instance;

    public static AppFacade Instance
    {
        get{
            if (_instance == null) {
                _instance = new AppFacade();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 启动框架
    /// </summary>
    public void StartUp() {
        this.Subscribe(new SocketCommand());

        AddManager<LuaManager>();
        AddManager<SoundManager>();
        AddManager<NetworkManager>();
        AddManager<ResourceManager>();
        AddManager<GameManager>();
    }

    static GameObject m_GameManager;
    static Dictionary<string, Component> m_Managers = new Dictionary<string, Component>();

    GameObject AppGameManager
    {
        get
        {
            if (m_GameManager == null)
            {
                m_GameManager = GameObject.Find("GameManager");
            }
            return m_GameManager;
        }
    }

    public IEventAggregator EventAggregator
    {
        get
        {
            return new EventAggregator();
        }
    }

    /// <summary>
    /// 添加管理器
    /// </summary>
    public void AddManager(string typeName, Manager obj)
    {
        if (!m_Managers.ContainsKey(typeName))
        {
            m_Managers.Add(typeName, obj);
        }
    }

    /// <summary>
    /// 添加Unity对象
    /// </summary>
    public T AddManager<T>() where T : Component
    {
        Component result = null;
        string typeName = typeof(T).Name;
        m_Managers.TryGetValue(typeName, out result);
        if (result != null)
        {
            return (T)result;
        }
        Component c = AppGameManager.AddComponent<T>();
        m_Managers.Add(typeName, c);
        return default(T);
    }

    /// <summary>
    /// 获取系统管理器
    /// </summary>
    public T GetManager<T>() where T : Component
    {
        string typeName = typeof(T).Name;
        if (!m_Managers.ContainsKey(typeName))
        {
            return null;
        }
        Component manager = null;
        m_Managers.TryGetValue(typeName, out manager);
        return (T)manager;
    }

    /// <summary>
    /// 删除管理器
    /// </summary>
    public void RemoveManager<T>() where T: Manager
    {
        string typeName = typeof(T).Name;
        if (!m_Managers.ContainsKey(typeName))
        {
            return;
        }
        Component manager = null;
        m_Managers.TryGetValue(typeName, out manager);
        GameObject.Destroy((Component)manager);
        m_Managers.Remove(typeName);
    }

    public IObservable<TEvent> OnEvent<TEvent>()
    {
        return EventAggregator.GetEvent<TEvent>();
    }

    public void Subscribe<TEvent>(IObserver<TEvent> ob)
    {
        this.OnEvent<TEvent>().Subscribe(ob);
    }

    public void Subscribe<TEvent>(Action<TEvent> action)
    {
        this.OnEvent<TEvent>().Subscribe(action);
    }

    public void Publish<TEvent>(TEvent eventMessage)
    {
        EventAggregator.Publish(eventMessage);
    }
}

