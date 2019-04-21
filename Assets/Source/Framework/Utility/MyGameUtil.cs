using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using Lean.Touch;
using DG.Tweening;
using UnityEngine.UI;

namespace LuaFramework
{
    public static class MyGameUtil
    {
        public static void AddTapOn2DObject(GameObject go, LuaFunction func)
        {
            if (go.GetComponent<SceneSelectable>() == null && go.activeSelf)
            {
                SceneSelectable com = go.AddComponent<SceneSelectable>();
                com.luaCallBack = func;
                com.onTaped.AddListener(
                    (finger, gameObject) =>
                    {
                        if (com.luaCallBack != null)
                        {
                            com.luaCallBack.Call<GameObject, LeanFinger>(gameObject, finger);
                        }
                    }
                );
            }
        }

        public static void RemoveTapOn2DObject(GameObject go)
        {
            var com = go.GetComponent<SceneSelectable>();
            if (com)
            {
                if (com.luaCallBack != null)
                {
                    com.luaCallBack.Dispose();
                }
                Component.Destroy(com);
            }
        }

        public static ScreenTap SetScreenTapEnable(GameObject go)
        {
            ScreenTap comSelect = go.GetComponent<ScreenTap>();
            if (comSelect == null)
            {
                comSelect = go.AddComponent<ScreenTap>();
                comSelect.Search = ScreenTap.SearchType.GetComponent;
            }
            return comSelect;
        }

        public static Vector2 GetSpriteSize(Sprite sprite)
        {
            if (sprite)
            {
                return sprite.bounds.size;
            }
            else
            {
                return Vector2.zero;
            }
        }

        public static void SetScreenTapDisable(GameObject go)
        {
            ScreenTap comSelect = go.GetComponent<ScreenTap>();
            if (comSelect != null)
            {
                ScreenTap.Destroy(comSelect);
            }
        }

        public static void SetPosition(GameObject go, float x, float y, float z)
        {
            go.transform.position = new Vector3(x, y, z);
        }

        public static void SetPosition(GameObject go, float x, float y)
        {
            var trans = go.transform;
            trans.position = new Vector3(x, y, trans.position.z);
        }

        public static void AddPosition(GameObject go, float x, float y, float z = 0)
        {
            go.transform.position += new Vector3(x, y, z);
        }

        public static Vector2 GetSpritePixelSize(GameObject go)
        {
            if (go)
            {
                var spriteRenderer = go.GetComponent<SpriteRenderer>();
                if (spriteRenderer && spriteRenderer.sprite)
                {
                    Vector3 scale = go.transform.lossyScale;
                    Vector2 size = spriteRenderer.sprite.rect.size;
                    size.x *= scale.x;
                    size.y *= scale.y;
                    return size;
                }
            }
            return Vector2.zero;
        }

        static Dictionary<int, Tween> tweenCached = new Dictionary<int, Tween>();
        public static Tween DOUIAlpha(this CanvasGroup canvas, float from, float to, float duration)
        {
            if (canvas == null)
            {
                return null;
            }
            canvas.alpha = from;
            if (from != to)
            {
                Tween t = DOTween.To(() => canvas.alpha, x => canvas.alpha = x, to, duration);
                return t;
            }
            else
            {
                return null;
            }
        }

        public static void DOBackAway(Transform transform, float endValue, float duration, bool isX, float alphaDelay = 0.1f)
        {
            var canvas = transform.GetComponent<CanvasGroup>();
            if (canvas == null)
            {
                canvas = transform.gameObject.AddComponent<CanvasGroup>();
            }
            {
                transform.DOKill();
                Tween tt;
                if (isX)
                {
                    tt = transform.DOLocalMoveX(endValue, duration).SetEase(Ease.InBack);
                }
                else
                {
                    tt = transform.DOLocalMoveY(endValue, duration).SetEase(Ease.InBack);
                }
                Tween t;
                if (alphaDelay > 0)
                {
                    t = canvas.DOUIAlpha(canvas.alpha, 0.0f, duration);
                    t.SetDelay(alphaDelay);
                }
                else
                {
                    t = canvas.DOUIAlpha(canvas.alpha, 0.0f, duration);
                }
                tweenCached[canvas.GetInstanceID()] = t;
            }
        }

        public static void DOForwardIn(Transform transform, float endValue, float duration, bool isX)
        {
            var canvas = transform.GetComponent<CanvasGroup>();
            if (canvas == null)
            {
                canvas = transform.gameObject.AddComponent<CanvasGroup>();
            }
            int id = canvas.GetInstanceID();
            if (tweenCached.ContainsKey(id))
            {
                tweenCached[id].Kill();
                tweenCached.Remove(id);
            }
            canvas.DOUIAlpha(canvas.alpha, 1, duration);
            transform.DOKill();
            if (isX)
            {
                transform.DOLocalMoveX(endValue, duration).SetEase(Ease.OutBack);
            }
            else
            {
                transform.DOLocalMoveY(endValue, duration).SetEase(Ease.OutBack);
            }
        }

        public static int SetGameObjectActive(GameObject go, bool active)
        {
            if (!go) return 0;

            bool lastMaskVisible = go.activeSelf;
            if (lastMaskVisible != active)
            {
                go.SetActive(active);
            }
            return lastMaskVisible ? 1 : 0;
        }

        public static Tween DoRawImageAlpha(RawImage image, float to, float duration)
        {
            if (!image)
            {
                return null;
            }
            return DG.Tweening.DOTween.To(() => image.color.a, x =>
            {
                var c = image.color;
                c.a = x;
                image.color = c;
            }, to, duration);
        }
    }
}
