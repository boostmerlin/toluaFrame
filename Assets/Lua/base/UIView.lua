--[[
    UI Base View. 配合BaseUIManager来管理，变可单独使用
    @author: muliang
    @date: 2018-12-26
    @description:


]]

local MsgDispatcher = require "base.MsgDispatcher"
local UpdateBeat = UpdateBeat

local reservedFields = {
    "netHandler",
    "isInstance",
    "transform",
    "gameObject",
    "luaBehavior",
    "enableShowAnimation",
    "enableHideAnimation",
    "widgets",
    "sortingLayer",
    "autoClose",
    "withMask",
    "maskColor",
    "autoInject",
    "needBehaviorCall",
    "assetBundle",
    "viewName",
    "priority",
    "dispose",
    "onViewAction",
    "viewCtrl",
    "parentView",
    "canvasGroup",

    "_visible",
    "_viewID",
    "_viewID",
    "uiManager",
    "_updateHandle",
}

local ViewIDGen = 0

local __creator = function ()
    local ins = {
        netHandler = true,
        isInstance = true,
    }
    local mt = {
        __tostring = function (t)
            return string.format( "UIView %s:%s:ID=%d", t.assetBundle, t.viewName, t._viewID)
        end
    }
    if DEBUG then --for debug error check.
        mt.__newindex = function (t, k, v)
            local vv = t[k]
            if v ~= false and table.containsValue(reservedFields, k) then
                error(string.format( "[%s] is Reserved Property in UIView, do not change it's type.", k))
            end
            rawset(t, k, v)
        end
    end
    setmetatable(ins, mt)
    return ins
end

local UIView = class("UIView", __creator)

-----------------------以下为内部函数--------------------

local OUT_SCREEN = Vector3.New(99999, 0, 0)

local function _PublicDeclare(self)
    self.gameObject = false
    self.transform = false
    self.luaBehavior = false

    --for auto inject.
    self.widgets = false
    --if not specified, use manager default.
    self.sortingLayer = false
    -- auto close if it has a Close Button
    self.autoClose = false
    -- dispose flag. true always dispose.
    self.dispose = false
    --with black mask
    self.withMask = false
    self.maskColor = false
    --switch for auto inject widgets
    self.autoInject = false
    self.enableShowAnimation = false
    self.enableHideAnimation = false
    self.needBehaviorCall = false
    --redefine this can load asset from other place.
    self.assetBundle = false
    self.viewName = false
    --net processor priority
    self.priority = false
    --notify when asset loaded over for outer use
    self.onViewAction = false
    --attach viewCtrl for history reason
    self.viewCtrl = false
    --parent view if has any
    self.parentView = false

    self.canvasGroup = false
    self.uiManager = false
end

local function _PrivateDeclare(self)
    self._visible = false
    self.__lastVisible = false
    self._originScale = false
    self._originPosition = false
    self._originLocalPosition = false
    self._originRotation = false
    self._updateHandle = false
    self._viewID = false
    self._childView = false
end

local function run_func(t, name)
    if not t then
        return
    end
    for _, v in ipairs(t) do
        local f = v[name]
        if f then
            f(v)
        end
    end
end

function UIView:setAlpha(alpha)
    local canvasGroup = self.luaBehavior.canvasGroup
    if not IsNil(canvasGroup) then
        canvasGroup.blocksRaycasts = self._visible
        if type(alpha) == "number" then
            canvasGroup.alpha = alpha
        end
    end
end

-------------------------生命周期函数---------------------
function UIView:ctor(viewCtrl, ...)
    _PublicDeclare(self)
    _PrivateDeclare(self)

    ViewIDGen = ViewIDGen + 1
    self._viewID = ViewIDGen
    self._childView = {}
    self._originPosition = Vector3.zero
    self._originLocalPosition = Vector3.zero
    self.autoInject = true
    self.enableShowAnimation = true
    self.enableHideAnimation = true
    self.needBehaviorCall = false
    self.canvasGroup = true
    self.assetBundle = self:classname()
    self.viewName = self.assetBundle

    if viewCtrl and type(viewCtrl) == "string" then
        local viewCtrlClass = require(viewCtrl)
        self.viewCtrl = viewCtrlClass.new(...)
    end
end

function UIView:onError()
    self:__onViewAction("onError")
    run_func(self._childView, "onError")
end

--对象加载完
function UIView:onCreate()
    MsgDispatcher.Reg(self, self.priority)
    if self.Update then
        if not self._updateHandle then
            self._updateHandle = UpdateBeat:CreateListener(self.Update, self)
        end
        UpdateBeat:AddListener(self._updateHandle)	
    end
    
    if self.autoClose then
        local btn
        if type(self.autoClose) == "string" then
            btn = self[self.autoClose]
        else
            btn =  self["Close"]
                or self["Cancel"]
                or self["btnClose"]
                or self["btnCancel"]
                or self["buttonClose"]
                or self["buttonCancel"]
        end
        if btn then
            self:AddListener(btn, function ()
                self:CloseSelf(self.dispose)
            end)
        end
    end

    self:__onViewAction("onCreate")
end

function UIView:__onViewAction(action)
    local onViewAction = self.onViewAction
    if onViewAction and type(onViewAction) == "function" then
        onViewAction(action, self)
    end
    if self.uiManager then
        self.uiManager:_fireNotify(action, self)
    end
end

function UIView._showAction(self)
    if self.luaBehavior and not self.parentView then
        self.transform.localPosition = self._originLocalPosition
        if not self.enableShowAnimation then
            self.transform.localScale = self._originScale
            self:setAlpha(1)
        else
            self:setAlpha(nil)
        end
    end
end

function UIView:_recordVisiualState(visible)
    self.__lastVisible = visible
end

function UIView:onShow()
    self._visible = true
    UIView._showAction(self)
    run_func(self._childView, "onShow")
    self:__onViewAction("onShow")
end

function UIView:onShowed()
    run_func(self._childView, "onShowed")
    Event.Brocast("ViewOnShow",self)
end

function UIView:onHide()
    Event.Brocast("ViewOnHide",self)
    self._visible = false
    run_func(self._childView, "onHide")
end

function UIView:onActive()
    self._visible = true
    run_func(self._childView, "onActive")
end

function UIView:onDeactive()
    self._visible = false
    run_func(self._childView, "onDeactive")
end

function UIView._hideAction(self)
    if self.luaBehavior and not self.parentView then
        self.transform.position = OUT_SCREEN
        self:setAlpha(nil)
    end
end

function UIView:onHided()
    UIView._hideAction(self)
    run_func(self._childView, "onHided")
end

function UIView:onDispose()
    MsgDispatcher.UnReg(self)
    run_func(self._childView, "onDispose")
    self.gameObject = false
    self.transform = false
    self.luaBehavior = false
    if self._updateHandle then
        UpdateBeat:RemoveListener(self._updateHandle)
        self._updateHandle = false
    end
    local viewCtrl = self.viewCtrl
    if viewCtrl and type(viewCtrl.Dispose) == "function" then
        viewCtrl:Dispose()
        self.viewCtrl = false
    end
    self.uiManager = false
end

function UIView:showAnimation(overcallback)
    if self:IsLoaded() then
        if self.luaBehavior:HasUIAnimation(true) then
            setScaleXYZ(self.transform, 0.01, 0.01)
            self.luaBehavior:ShowAnimation(overcallback)
        else
            local func = self.uiManager.defaultShowAnimation
            if func then
                self.transform.rotation = self._originRotation
                self.luaBehavior:DisableAnimator()
                func(self, overcallback)
            else
                overcallback()
            end
        end
    else
        overcallback()
    end
end

function UIView:hideAnimation(overcallback)
    if self:IsLoaded() then
        if self.luaBehavior:HasUIAnimation(false) then
            self.luaBehavior:HideAnimation(overcallback)
        else
            local func = self.uiManager.defaultHideAnimation
            if func then
                self.luaBehavior:DisableAnimator()
                func(self, overcallback)
            else
                overcallback()
            end
        end
    else
        overcallback()
    end
end

-------------------以下为帮助函数-------------------------
function UIView:AddListener(obj, functor, extraParam, eventIdentifier)
    if not obj or not functor then
        logError("[UIView:AddListener] Can't add nil listener.")
        return
    end
    local behavior = self.luaBehavior
    if behavior then
        behavior:AddListener(obj, functor, extraParam, eventIdentifier)
    else
        logError("[UIView:AddListener] luaBehavior is nil")
    end
end

--[[
    release single listener for object.
]]
function UIView:RemoveListener(obj)
    if not obj then
        logError("[UIView:RemoveListener] Can't remove nil listener.")
        return
    end
    local behavior = self.luaBehavior
    if behavior then
        behavior:ReleaseListener(obj)
    else
        logError("[UIView:RemoveListener] luaBehavior is nil")
    end
end

function UIView:ID()
    return self._viewID
end

function UIView:IsLoaded()
    return self.gameObject and IsNil(self.gameObject) == false
end

function UIView:AddChildView(view, obj)
    local i = #self._childView
    self._childView[i+1] = view
    view.parentView = self
    if not IsNil(obj) then
        view:SetViewObject(obj)
    end
    return i
end

function UIView:GetChildAt(index)
    local n = self:GetChildCount()
    if index > 0 and index <= n then
        return self._childView[index]
    else
        logError("[UIView:GetChildAt] Error Index: " .. index)
    end
end

function UIView:GetChildByName(name)
    for i, v in ipairs(self._childView) do
        if v:classname() == name then
            return v, i
        end
    end
    return nil
end

function UIView:RemoveChildView(view)
    table.removeItem(self._childView, view)
end

function UIView:RemoveChildAt(index)
    local n = self:GetChildCount()
    if index > 0 and index <= n then
        table.remove(self._childView, index)
    else
        logError("[UIView:RemoveChildAt] Error Index: " .. index)
    end
end

function UIView:GetChildCount()
    return #self._childView
end

--[[
    为View绑定GameObject用于复用
]]
function UIView:SetViewObject(obj, newInstance)
    if IsNil(obj) then
        logError("something wrong, ui asset load error.")
        return
    end
    local go
    if newInstance then
        go = self.uiManager:_attachToCanvas(self, obj)
    else
        go = obj
    end
    self.gameObject = go
    self.transform = go.transform
    self:ResetOriginRTS()
    --self.gameObject.name = string.format("%s_%d", self.gameObject.name, self._viewID)

    --为了只改一个地方字段名。。。
    self.luaBehavior = LuaBehaviour.AddBehaviour(go, self, self.autoInject and self.widgets or nil, self.canvasGroup)
    self.__animator = go:GetComponent("Animator")
    self:onCreate()
end

function UIView:ResetOriginRTS()
    self._originRotation = self.transform.rotation
    self._originPosition = self.transform.position
    self._originLocalPosition = self.transform.localPosition
    self._originScale = self.transform.localScale
end

function UIView:GetOriginPosition()
    return self._originPosition
end

function UIView:GetOriginLocalPosition()
    return self._originLocalPosition
end

function UIView:Visible()
    if not self.gameObject or not self.gameObject.activeSelf then
        return false
    end
    local p = self.parentView
    if p then
        return p._visible
    else
        return self._visible
    end
end

function UIView:SetActive(active)
    active = active or false
    if self:IsLoaded() then
        self.gameObject:SetActive(active)
    end
end

function UIView:CloseSelf(isDispose)
    if self.uiManager then
        local dispose = (type(isDispose) == "boolean") and isDispose or self.dispose 
        self.uiManager:Close(self._viewID, dispose)
    end
end

return UIView