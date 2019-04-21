
empty_table = {}
CSUtil = LuaFramework.CSUtil;
AppConst = LuaFramework.AppDef;
LuaHelper = LuaFramework.LuaHelper;
ByteBuffer = LuaFramework.ByteBuffer;
LuaBehaviour = LuaFramework.LuaBehaviour

resMgr = LuaHelper.GetResManager();
soundMgr = LuaHelper.GetSoundManager();
networkMgr = LuaHelper.GetNetManager();

WWW = UnityEngine.WWW
GameObject = UnityEngine.GameObject
Object = UnityEngine.Object
SpriteRenderer = UnityEngine.SpriteRenderer
Camera = UnityEngine.Camera
RectTransform = UnityEngine.RectTransform
Canvas = UnityEngine.Canvas
RectTransformUtility = UnityEngine.RectTransformUtility
Ease = DG.Tweening.Ease
RenderTexture = UnityEngine.RenderTexture
Material = UnityEngine.Material
Shader = UnityEngine.Shader
Mathf = UnityEngine.Mathf
TextAnchor=UnityEngine.TextAnchor
PlayerPrefs = UnityEngine.PlayerPrefs
GraphicRaycaster = UnityEngine.UI.GraphicRaycaster
Resources = UnityEngine.Resources
MyGameUtil = LuaFramework.MyGameUtil
Ease = DG.Tweening.Ease

LeanTouch = Lean.Touch.LeanTouch

UIView = require("base.UIView")

BaseCtrl = require("base.BaseCtrl")

Protocal = {
    ScreenTapOnNull = "ScreenTapOnNull",
    EscapeKeyDown = "EscapeKeyDown",
}