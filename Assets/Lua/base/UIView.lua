--[[
    UI Base View. 配合BaseUIManager来管理，变可单独使用
    @author: muliang
    @date: 2018-12-26
    @description:


]]

local MsgDispatcher = require "base.MsgDispatcher"
local UIView = class("UIView", require("base.BaseView"))

-----------------------以下为内部函数--------------------

local OUT_SCREEN = Vector3.New(99999, 0, 0)

local function _PublicDeclare(self)
    -- self.gameObject = false
    -- self.transform = false
    -- self.luaBehavior = false

    -- --for auto inject.
    -- self.widgets = false
    -- --switch for auto inject widgets
    -- self.autoInject = false
    -- self.needBehaviorCall = false
    -- --attach viewCtrl for history reason
    -- self.viewCtrl = false
    -- --parent view if has any
    -- self.parentView = false

    -- --redefine this can load asset from other place.
    -- self.assetBundle = false
    -- self.viewName = false

    --if not specified, use manager default.
    self.sortingLayer = false
    -- auto close if it has a Close Button
    self.autoClose = false
    -- dispose flag. true always dispose.
    self.dispose = false
    --with black mask
    self.withMask = false
    self.maskColor = false

    --net processor priority
    self.priority = false
    --notify when asset loaded over for outer use
    self.onViewAction = false

    self.canvasGroup = false
    self.uiManager = false

    self.viewCtrl = false
end

local function _PrivateDeclare(self)
    -- self._visible = false
    -- self.__lastVisible = false
    -- self._originScale = false
    -- self._originPosition = false
    -- self._originLocalPosition = false
    -- self._originRotation = false
    -- self._updateHandle = false
    -- self._viewID = false
    -- self._childView = false
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
    UIView.super.ctor(self, ...)
    _PublicDeclare(self)
    _PrivateDeclare(self)
    
    self.enableShowAnimation = true
    self.enableHideAnimation = true
    self.canvasGroup = true
    self.needBehaviorCall = false

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
    UIView.super.onCreate(self)
    MsgDispatcher.Reg(self, self.priority)
    
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
    UIView.super.onDispose(self)

    MsgDispatcher.UnReg(self)
    run_func(self._childView, "onDispose")
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

function UIView:CloseSelf(isDispose)
    if self.uiManager then
        local dispose = (type(isDispose) == "boolean") and isDispose or self.dispose 
        self.uiManager:Close(self._viewID, dispose)
    end
end

return UIView