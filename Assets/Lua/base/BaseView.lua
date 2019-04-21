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
            return string.format( "BaseView %s:%s:ID=%d", t.assetBundle, t.viewName, t._viewID)
        end
    }
    -- if DEBUG then --for debug error check.
    --     mt.__newindex = function (t, k, v)
    --         local vv = t[k]
    --         if v ~= false and table.containsValue(reservedFields, k) then
    --             error(string.format( "[%s] is Reserved Property in BaseView, do not change it's type.", k))
    --         end
    --         rawset(t, k, v)
    --     end
    -- end
    setmetatable(ins, mt)
    return ins
end

local BaseView = class("BaseView", __creator)

local function _PublicDeclare(self)
    self.gameObject = false
    self.transform = false
    self.luaBehavior = false

    --for auto inject.
    self.widgets = false
    --switch for auto inject widgets
    self.autoInject = false
    self.needBehaviorCall = false
    --attach viewCtrl for history reason
    self.viewCtrl = false
    --parent view if has any
    self.parentView = false

    --redefine this can load asset from other place.
    self.assetBundle = false
    self.viewName = false
    
    self.enableShowAnimation = false
    self.enableHideAnimation = false
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

function BaseView:ctor(...)
    _PublicDeclare(self)
    _PrivateDeclare(self)

    ViewIDGen = ViewIDGen + 1
    self._viewID = ViewIDGen
    self._childView = {}
    self._originPosition = Vector3.zero
    self._originLocalPosition = Vector3.zero
    self.autoInject = true
    self.needBehaviorCall = true
    self.assetBundle = self:classname()
    self.viewName = self.assetBundle
end

function BaseView:onCreate()
    if self.Update then
        if not self._updateHandle then
            self._updateHandle = UpdateBeat:CreateListener(self.Update, self)
        end
        UpdateBeat:AddListener(self._updateHandle)	
    end
end

function BaseView:_recordVisiualState(visible)
    self.__lastVisible = visible
end

function BaseView:onDispose()
    self.gameObject = false
    self.transform = false
    self.luaBehavior = false
    if self._updateHandle then
        UpdateBeat:RemoveListener(self._updateHandle)
        self._updateHandle = false
    end
end

function BaseView:showAnimation(overcallback)
    
end

function BaseView:hideAnimation(overcallback)
end

-------------------以下为帮助函数-------------------------
function BaseView:AddListener(obj, functor, extraParam, eventIdentifier)
    if not obj or not functor then
        logError("[BaseView:AddListener] Can't add nil listener.")
        return
    end
    local behavior = self.luaBehavior
    if behavior then
        behavior:AddListener(obj, functor, extraParam, eventIdentifier)
    else
        logError("[BaseView:AddListener] luaBehavior is nil")
    end
end

--[[
    release single listener for object.
]]
function BaseView:RemoveListener(obj)
    if not obj then
        logError("[BaseView:RemoveListener] Can't remove nil listener.")
        return
    end
    local behavior = self.luaBehavior
    if behavior then
        behavior:ReleaseListener(obj)
    else
        logError("[BaseView:RemoveListener] luaBehavior is nil")
    end
end

function BaseView:ID()
    return self._viewID
end

function BaseView:IsLoaded()
    return self.gameObject and IsNil(self.gameObject) == false
end

function BaseView:AddChildView(view, obj)
    local i = #self._childView
    self._childView[i+1] = view
    view.parentView = self
    if not IsNil(obj) then
        view:SetViewObject(obj)
    end
    return i
end

function BaseView:GetChildAt(index)
    local n = self:GetChildCount()
    if index > 0 and index <= n then
        return self._childView[index]
    else
        logError("[BaseView:GetChildAt] Error Index: " .. index)
    end
end

function BaseView:GetChildByName(name)
    for i, v in ipairs(self._childView) do
        if v:classname() == name then
            return v, i
        end
    end
    return nil
end

function BaseView:RemoveChildView(view)
    table.removeItem(self._childView, view)
end

function BaseView:RemoveChildAt(index)
    local n = self:GetChildCount()
    if index > 0 and index <= n then
        table.remove(self._childView, index)
    else
        logError("[BaseView:RemoveChildAt] Error Index: " .. index)
    end
end

function BaseView:GetChildCount()
    return #self._childView
end

--[[
    为View绑定GameObject用于复用
]]
function BaseView:SetViewObject(obj, attachToCanvas)
    if IsNil(obj) then
        logError("something wrong, ui asset load error.")
        return
    end
    local go
    if attachToCanvas and self.uiManager then
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

function BaseView:ResetOriginRTS()
    self._originRotation = self.transform.rotation
    self._originPosition = self.transform.position
    self._originLocalPosition = self.transform.localPosition
    self._originScale = self.transform.localScale
end

function BaseView:GetOriginPosition()
    return self._originPosition
end

function BaseView:GetOriginLocalPosition()
    return self._originLocalPosition
end

function BaseView:Visible()
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

function BaseView:SetActive(active)
    active = active or false
    if self:IsLoaded() then
        self.gameObject:SetActive(active)
    end
end

function BaseView:Dispose()
    if self.disposed then
        return
    end
    if self:IsLoaded() then
        self.disposed = true
        GameObject.Destroy(self.gameObject)
        self:onDispose()
    end
end

return BaseView