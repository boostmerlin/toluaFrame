using UnityEngine;
using LuaInterface;
using System.Collections.Generic;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.Events;

namespace LuaFramework
{
    public class LuaBehaviour : MonoBehaviour
    {
        class UIItemContext
        {
            public UIItemContext(LuaFunction func, object para, GameObject go)
            {
                this.luaFunc = func;
                this.para = para;
                gameObject = go;
                ActionVoid = null;
                ActionFloat = null;
                ActionBool = null;
                ActionString = null;
                FuncValidateInput = null;
                ActionInt = null;
            }
            public LuaFunction luaFunc;
            public GameObject gameObject;
            public object para;
            //for less gc
            public UnityAction ActionVoid;
            public UnityAction<float> ActionFloat;
            public UnityAction<bool> ActionBool;
            public UnityAction<int> ActionInt;
            public UnityAction<string> ActionString;
            public InputField.OnValidateInput FuncValidateInput;
            public UnityAction cleanFunction;
        }

        public static LuaBehaviour AddBehaviour(GameObject go, LuaTable viewTable, LuaTable widgets = null, bool addCanvasGroup = false)
        {
            var behaviour = go.GetComponent<LuaBehaviour>();
            if (!behaviour)
            {
                behaviour = go.AddComponent<LuaBehaviour>();
            }
            if (addCanvasGroup && !go.GetComponent<CanvasGroup>())
            {
                behaviour.canvasGroup = go.AddComponent<CanvasGroup>();
            }
            behaviour.bindLuaView(viewTable, widgets != null, widgets);

            return behaviour;
        }

        public CanvasGroup canvasGroup
        {
            get; private set;
        }

        private LuaTable luaViewTable;
        private bool needBehaviorCall = false;

        private Dictionary<int, UIItemContext> mRegisteredItems = new Dictionary<int, UIItemContext>();

        static void recursiveTranverse(Transform root, System.Action<Transform> func)
        {
            if (root != null)
            {
                func(root);
            }
            if (root.childCount == 0)
            {
                return;
            }
            foreach (Transform t in root)
            {
                recursiveTranverse(t, func);
            }
        }

        private void bindLuaView(LuaTable viewTable, bool autoInject, LuaTable widgets)
        {
            luaViewTable = viewTable;
            string name = viewTable["__cname"] as string;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            if (autoInject && widgets != null)
            {
                var dict = widgets.ToDictTable();
                Dictionary<string, KeyValuePair<string, string>> widgetsMap = new Dictionary<string, KeyValuePair<string, string>>();
                #region analyze widgets table
                foreach (var d in dict)
                {
                    if (d.Key is double)
                    {
                        if (d.Value is string)
                        {
                            string value = (string)d.Value;
                            string key = value;
                            int pos = value.LastIndexOf('/');
                            if(pos > 0)
                            {
                                key = value.Substring(pos + 1);
                            }
                            widgetsMap.Add(key, new KeyValuePair<string, string>(value, ""));
                        }
                        else
                        {
                            Debugger.LogError("widgets declare must be string path of ui widget: " + name);
                            continue;
                        }
                    }
                    else if (d.Key is string)
                    {
                        string value = (string)d.Key;
                        if (d.Value is LuaTable)
                        {
                            LuaTable luaTable = (LuaTable)d.Value;
                            string v1 = luaTable[1] as string;
                            string v2 = luaTable[2] as string;
                            if (v1 != null && v2 != null)
                            {
                                widgetsMap.Add(value, new KeyValuePair<string, string>(v1, v2));
                            }
                            else
                            {
                                Debugger.LogError("widgets declare must be string path and string type:  " + name);
                            }
                        }
                        else if (d.Value is string)
                        {
                            widgetsMap.Add(value, new KeyValuePair<string, string>((string)d.Value, ""));
                        }
                        else
                        {
                            Debugger.LogError("widgets declare must be string path of ui widget: " + name);
                        }
                    }
                }
                #endregion
                //quicker.
                List<string> removeKey = new List<string>(widgetsMap.Count);
                recursiveTranverse(transform, (t) =>
                {
                    removeKey.Clear();
                    foreach (var d in widgetsMap)
                    {
                        string path = d.Value.Key;
                        Transform tt = null;
                        if(path != ".")
                        {
                            tt = t.Find(path);
                        }
                        else
                        {
                            tt = t;
                        }
                        if (tt)
                        {
                            string type = d.Value.Value;
                            Component com = null;
                            if (string.IsNullOrEmpty(type))
                            {
                                var coms = tt.GetComponents<UnityEngine.EventSystems.UIBehaviour>();
                                if (coms.Length > 0)
                                {
                                    com = coms[coms.Length - 1];
                                }
                                else
                                {
                                    viewTable[d.Key] = tt.gameObject;
                                }
                            }
                            else
                            {
                                com = tt.GetComponent(type);
                            }
                            if (com)
                            {
                                viewTable[d.Key] = com;
                            }
                            removeKey.Add(d.Key);
                        }
                    }
                    foreach (var k in removeKey)
                    {
                        widgetsMap.Remove(k);
                    }
                });
            }
            //Debugger.Log("Auto inject {0} takes: {1}", name, stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();

            if (widgets != null)
                widgets.Dispose();
        }

        public void AddListener(Component component, LuaFunction luafunc, object extraParam = null, string eventIdentifier = null)
        {
            if (component == null || luafunc == null)
            {
                return;
            }
            UIItemContext componentContext;
            int id = component.GetInstanceID();
            if (mRegisteredItems.TryGetValue(id, out componentContext))
            {
                if (luafunc != componentContext.luaFunc)
                {
                    componentContext.luaFunc.Dispose();
                    componentContext.luaFunc = luafunc;
                }
                //update param
                componentContext.para = extraParam;
            }
            else
            {
                GameObject go = component.gameObject;
                componentContext = new UIItemContext(luafunc, extraParam, go);
                mRegisteredItems.Add(id, componentContext);
            }
            if (component is Button)
            {
                Button btn = component as Button;

                if(componentContext.ActionVoid == null)
                {
                    componentContext.ActionVoid = delegate ()
                    {
                        componentContext.luaFunc.Call(componentContext.gameObject, componentContext.para);
                    };
                    componentContext.cleanFunction = delegate ()
                    {
                        if (componentContext.ActionVoid != null)
                        {
                            btn.onClick.RemoveListener(componentContext.ActionVoid);
                        }
                    };
                    btn.onClick.AddListener(componentContext.ActionVoid);
                }
            }
            else if (component is Slider)
            {
                Slider slider = component as Slider;

                if (componentContext.ActionFloat == null)
                {
                    componentContext.ActionFloat = delegate (float v)
                    {
                        componentContext.luaFunc.Call(componentContext.gameObject, v, componentContext.para);
                    };
                    componentContext.cleanFunction = delegate ()
                    {
                        if (componentContext.ActionFloat != null)
                        {
                            slider.onValueChanged.RemoveListener(componentContext.ActionFloat);
                        }
                    };
                    slider.onValueChanged.AddListener(componentContext.ActionFloat);
                }
            }
            else if (component is InputField)
            {
                InputField input = component as InputField;
                if (eventIdentifier == null)
                {
                    eventIdentifier = string.Empty;
                }
                switch (eventIdentifier)
                {
                    case "onValueChanged":
                        if (componentContext.ActionString == null)
                        {
                            componentContext.ActionString = delegate (string v)
                            {
                                componentContext.luaFunc.Call(componentContext.gameObject, v, componentContext.para);
                            };
                            componentContext.cleanFunction = delegate ()
                            {
                                if (componentContext.ActionString != null)
                                {
                                    input.onValueChanged.RemoveListener(componentContext.ActionString);
                                }
                            };
                            input.onValueChanged.AddListener(componentContext.ActionString);
                        }
                        break;
                    case "onValidateInput":
                        if (componentContext.FuncValidateInput == null)
                        {
                            componentContext.FuncValidateInput = delegate (string v, int index, char added)
                            {
                                return componentContext.luaFunc.Invoke<GameObject, string, int, char, object, char>(componentContext.gameObject, v, index, added, componentContext.para);
                            };
                            componentContext.cleanFunction = delegate ()
                            {
                                if (componentContext.FuncValidateInput != null)
                                {
                                    input.onValidateInput = null;
                                }
                            };
                            input.onValidateInput = componentContext.FuncValidateInput;
                        }
                        break;
                    default: // "onEndEdit":
                        if (componentContext.ActionString == null)
                        {
                            componentContext.ActionString = delegate (string v)
                            {
                                componentContext.luaFunc.Call(componentContext.gameObject, v, componentContext.para);
                            };
                            componentContext.cleanFunction = delegate ()
                            {
                                if (componentContext.ActionString != null)
                                {
                                    input.onEndEdit.RemoveListener(componentContext.ActionString);
                                }
                            };
                            input.onEndEdit.AddListener(componentContext.ActionString);
                        }
                        break;
                }
            }
            else if (component is Toggle)
            {
                Toggle toggle = component as Toggle;

                if (componentContext.ActionBool == null)
                {
                    componentContext.ActionBool = delegate (bool v)
                    {
                        componentContext.luaFunc.Call(componentContext.gameObject, v, componentContext.para);
                    };
                    componentContext.cleanFunction = delegate ()
                    {
                        if (componentContext.ActionBool != null)
                        {
                            toggle.onValueChanged.RemoveListener(componentContext.ActionBool);
                        }
                    };
                    toggle.onValueChanged.AddListener(componentContext.ActionBool);
                }
            }
            else if (component is Dropdown)
            {
                Dropdown dropdown = component as Dropdown;
                if (componentContext.ActionInt == null)
                {
                    componentContext.ActionInt = delegate (int v)
                    {
                        componentContext.luaFunc.Call(componentContext.gameObject, v, componentContext.para);
                    };
                    componentContext.cleanFunction = delegate ()
                    {
                        if (componentContext.ActionInt != null)
                        {
                            dropdown.onValueChanged.RemoveListener(componentContext.ActionInt);
                        }
                    };
                    dropdown.onValueChanged.AddListener(componentContext.ActionInt);
                }
            }
            else
            {
                Debugger.LogError("Unknow UGUI Event type: {0}. component should be one of [Button, Slider, InputField, Toggle, Dropdown]", component.GetType().Name);
            }
        }

        /// <summary>
        /// 添加单击事件
        /// </summary>
        public void AddClick(GameObject go, LuaFunction luafunc)
        {
            this.AddClick(go, luafunc, null);
        }

        public void AddClick(GameObject go, LuaFunction luafunc, object extraPara)
        {
            if (go == null || luafunc == null) return;

            AddListener(go.GetComponent<Button>(), luafunc, extraPara);
        }

        /// <summary>
        /// 删除事件
        /// </summary>
        /// <param name="go"></param>
        public int ReleaseListener(Object go)
        {
            if (go == null) return 0;
            UIItemContext bc;
            int id = go.GetInstanceID();
            if (mRegisteredItems.TryGetValue(id, out bc))
            {
                bc.luaFunc.Dispose();
                bc.luaFunc = null;
                if (bc.cleanFunction != null)
                {
                    bc.cleanFunction();
                    bc.cleanFunction = null;
                }
                mRegisteredItems.Remove(id);
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 清除事件
        /// </summary>
        public void ClearListener()
        {
            foreach (var de in mRegisteredItems)
            {
                if (de.Value.luaFunc != null)
                {
                    de.Value.luaFunc.Dispose();
                }
                if(de.Value.cleanFunction != null)
                {
                    de.Value.cleanFunction();
                }
            }
            mRegisteredItems.Clear();
        }

        //-----------------------------------------------------------------
        protected void OnDestroy()
        {
            ClearListener();
            if (luaViewTable != null)
            {
                luaViewTable.Dispose();
                luaViewTable = null;
            }
        }

        #region for animation
        UIAnimation uiAnimation;
        private void Awake()
        {
            var animator = GetComponent<Animator>();
            if (animator)
            {
                uiAnimation = new UIAnimation(animator);
            }
        }

        public bool HasUIAnimation(bool isShow)
        {
            return uiAnimation != null && (isShow ? uiAnimation.HasShowAnimation() : uiAnimation.HasHideAnimation());
        }

        LuaFunction showCallBack;
        public void ShowAnimation(LuaFunction func)
        {
            if(showCallBack != null && showCallBack != func)
            {
                showCallBack.Dispose();
            }
            showCallBack = func;
            if(uiAnimation != null)
                uiAnimation.Show();
        }

        LuaFunction hideCallBack;
        public void HideAnimation(LuaFunction func)
        {
            if (hideCallBack != null && hideCallBack != func)
            {
                hideCallBack.Dispose();
            }
            hideCallBack = func;
            if (uiAnimation != null)
                uiAnimation.Hide();
        }

        public void DisableAnimator()
        {
            if(uiAnimation != null)
            {
                uiAnimation.SetAnimatorEnable(false);
            }
        }

        void OnShowed()
        {
            if (showCallBack != null)
            {
                showCallBack.Call();
                showCallBack.Dispose();
                showCallBack = null;
            }
        }

        void OnHided()
        {
            if (hideCallBack != null)
            {
                hideCallBack.Call();
                hideCallBack.Dispose();
                hideCallBack = null;
            }
        }
        #endregion
    }
}