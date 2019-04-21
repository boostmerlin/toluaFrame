--[[
    UI BaseUIManager. 管理控制UIView
    @author: muliang
    @date: 2018-12-27
    @description:

]]

local BaseUIManager = class("BaseUIManager")
local UIView = require "base.UIView"

local LateUpdateBeat = LateUpdateBeat
local tremove = table.remove
local tinsert = table.insert
local tclear = table.clear
local spairs = table.sortedPairs
local _defaultMaskColor = Color.New(0, 0, 0, 0.41)
local currentMaskColor = _defaultMaskColor

local MinScaleShow = 0.25
local MinScaleHide = 0.20
local AnimShowTime = 0.34
local AnimHideTime = 0.1

local function defaultShowAnim(view, callback)
    setScaleXYZ(view.transform, MinScaleShow, MinScaleShow)
    view:setAlpha(1)
    view.transform:DOScale(view._originScale, AnimShowTime):SetEase(Ease.OutBounce):SetUpdate(true):OnComplete(callback)
end

local function defaultHideAnim(view, callback)
    view.transform:DOScale(MinScaleHide, AnimHideTime):SetUpdate(true):OnComplete(callback)
end

local function _PrivateDeclare(self)
    self._handle = LateUpdateBeat:CreateListener(self._onEndFrame, self)
    LateUpdateBeat:AddListener(self._handle)

    self._viewType2View = {}

    --映射当前canvas结点结构
    self._viewStack = {}
    --manage layers.
    self._layerMap = {}
    self._pushQueue = {nil, nil, nil, nil}
    self._closeQueue = {nil, nil, nil, nil}
    self._viewActionNotifies = {}

    --self._pendingQueue = list:new()

    self.__findCache = {} --speed up for push

    self._blackMaskGameObject = nil
    self._lastMaskVisible = nil
    self._currentMaskVisible = nil
    self._viewPath = nil
end

local function _newLayerRectTransform(name, parent, sortingLayer)
    local go = GameObject(name)
    local t = go:AddComponent(typeof(RectTransform))
    t:SetParent(parent, false)
    t.sizeDelta = Vector2.zero
    t.anchorMin = Vector2.zero
    t.anchorMax = Vector2.one
    t.anchoredPosition = Vector2.zero
    if sortingLayer then
        local cv = go:AddComponent(typeof(Canvas))
        cv.overrideSorting = true
        cv.sortingOrder = sortingLayer
        local gr = go:AddComponent(typeof(UnityEngine.UI.GraphicRaycaster))
    end
    return t
end

local function setDisposingState(v, isDispose)
    if isDispose then
        v.__disposing = isDispose
    else
        v.__disposing = v.dispose
    end
end

local function reverse_comparer(a, b)
    return a > b
end

local function getidentifier(id)
    local nameorid = nil
    local idType = 1
    if type(id) == "table" then
        nameorid = id:classname()
    elseif type(id) == "string" then
        nameorid = id
    elseif type(id) == "number" then
        nameorid = id
        idType = 2
    else
        logError("[BaseUIManager:FindView] wrong id type.")
    end
    return nameorid, idType
end

function BaseUIManager:ctor(rootCanvas, assetLoader, camera)
    _PrivateDeclare(self)
    if not rootCanvas then
        error("rootCanvas should not be null")
    end
    if not assetLoader then
        error("assetLoader should not be null")
    end
    if not camera then
        local canvas = rootCanvas:GetComponent("Canvas")
        camera = canvas.worldCamera
    end
    self.camera = camera
    self.rootCanvas = rootCanvas
    self.assetLoader = assetLoader
    --NOTE: 没有使用SortingLayer, 所以所有Manager的Order是共享的。
    self.defaultSortingLayer = 0
    self.transitionBlackMask = true

    self:SetDefaultHideAnim(defaultHideAnim)
    self:SetDefaultShowAnim(defaultShowAnim)
end

function BaseUIManager:SetViewPath(viewPath)
    if viewPath and type(viewPath) == "string" then
        local code = string.byte(viewPath, #viewPath)
        if code ~= 46 or code ~= 47 then
            self._viewPath = viewPath .. "."
        else
            self._viewPath = viewPath
        end
    end
end

--[[
    显示一个View
    * UIView具体类型， 如UITestCase
    * UIView的实例
    * View 名, 如 "UITestCase"
    当View不是实例时候，会复用之前的View,所以数据也是之前缓存的
    @param view table , view类型或者类型实例
    @param param table or bool:
    hidePrev, bool, 隐藏前一个
    hideOther, bool. hide other on the same layer.
    Push(view, {hideOther = true})
    Push(view, true) --隐藏前一个
    当使用参数时，不要在Push后修改SortingLayer！
    @return 返回一个view的实例，注意 当失败时返回空。
    防止错误地再次修改参数
]]
function BaseUIManager:Push(view, param)
    if type(view) == "string" and self._viewPath then
        view = require(self._viewPath .. view)
    end
    local typename = view.classname()
    local isPushing, pushingview = self:_isPushing(view)
    if isPushing then
        logWarn("[BaseUIManager:Push] **** Repeat pushing View is in pending: " .. typename)
        return pushingview
    end


    if not view.isInstance then
        local viewIns = self:FindView(view)
        if viewIns then
            logWarn("[BaseUIManager:Push] View is pushed: " .. typename)
            return viewIns
        end
        for i=#self._closeQueue, 1, -1 do
            local v = self._closeQueue[i]
            if type(v) == "table" and v.classname() == typename then
                tremove(self._closeQueue, i)
                break
            end
        end

        local mv = self:_getManagedView(typename)
        if not mv then
            local viewType = view
            view = viewType.new()
        else
            view = mv
        end
    else
        for i=#self._closeQueue, 1, -1 do
            local v = self._closeQueue[i]
            if type(v) == "table" and v:ID() == view:ID() then
                tremove(self._closeQueue, i)
                break
            end
        end
    end

    if not view.sortingLayer then
        view.sortingLayer = self.defaultSortingLayer
    end
    local hidePrevious = param
    local hideOther = false
    if type(param) == "table" then
        hidePrevious = param.hidePrev
        hideOther = param.hideOther
    end
    if hideOther then
        self:PopLayer(view.sortingLayer, false)
    else
        if hidePrevious then
            self:Pop(view.sortingLayer, false)
        end
    end
    view.uiManager = self
    tinsert(self._pushQueue, view)
    --log(string.format("About to show view of %s:%d on Layer: %d" ,view.viewName, view:ID(), view.sortingLayer))

    return view
end

function BaseUIManager:DumpViewStack()
    for k, layerViews in spairs(self._viewStack, reverse_comparer) do
        log("[DumpViewStack] layer:" , k)
        for i=#layerViews, 1, -1 do
            local v = layerViews[i]
            log(">> viewname, viewid: ", k, v:classname(), v:ID())
        end
    end
end

function BaseUIManager:SetDefaultShowAnim(func)
    self.defaultShowAnimation = func or defaultShowAnim
end

function BaseUIManager:SetDefaultHideAnim(func)
    self.defaultHideAnimation = func or defaultHideAnim
end

--[[
    设置Push的默认层级
    @param layerOrder Int type 整数层级
]]
function BaseUIManager:SetDefaultSortingLayer(layerOrder, maskColor)
    _defaultMaskColor = maskColor or _defaultMaskColor
    if layerOrder then
        local canvas = self.rootCanvas:GetComponent("Canvas")
        self.defaultSortingLayer = layerOrder
        canvas.sortingOrder = layerOrder
        local trans = self._layerMap[layerOrder]
        if not trans then
            trans = _newLayerRectTransform("layer" .. layerOrder, self.rootCanvas)
            self:_addLayer(layerOrder, trans)
        end
        if not self._blackMaskGameObject then
            local obj = _newLayerRectTransform("__blackMask", trans).gameObject
            local image = obj:AddComponent(typeof(UnityEngine.UI.RawImage))
            self._blackMaskImage = image
            image.color = _defaultMaskColor
            self._blackMaskGameObject = obj
            self._blackMaskGameObject:SetActive(false)
            self._blackMaskTrans = obj.transform
        end
    end
end

--[[
    从层上弹出一个UI, 如果不指定Layer, 就弹出默认层
    @param: sortingLayer, int 可以为空，或者指定一个层
    @param: isDispose, boolean 是否删除缓存
]]
function BaseUIManager:Pop(sortingLayer, isDispose)
    sortingLayer = self:_checkLayer(sortingLayer)

    for i=#self._pushQueue, 1, -1 do
        local v = self._pushQueue[i]
        if v.sortingLayer == sortingLayer then
            --CHECK on view stack?
            tremove(self._pushQueue, i)
            return
        end
    end

    local v = self:_peekView(sortingLayer)
    if v then
        setDisposingState(v, isDispose)
        tinsert(self._closeQueue, sortingLayer)
        --tinsert(self._closeQueue, v)
    else
        logWarn("[BaseUIManager:Pop] no view on Layer" .. sortingLayer)
    end
end

--[[
    关闭指定的View, vieworid可以是：
    * UIView具体类型， 如UITestCase，关闭找到的第一个
    * UIView具体类型名："UITestCase"，关闭找到的第一个
    * UIView的ID,  view:ID(), 关闭指定的ID
    @param: vieworid, class or id
    @param: isDispose, 不否destroy掉go
]]
function BaseUIManager:Close(vieworid, isDispose)
    local nameorid, idType = getidentifier(vieworid)
    if not nameorid then
        return
    end
    for i=#self._pushQueue, 1, -1 do
        local v = self._pushQueue[i]
        if idType == 1 and v:classname() == nameorid
        or idType == 2 and v:ID() == nameorid then
            if not self:_checkViewExist(v) then
                v:onDispose()
            end
            tremove(self._pushQueue, i)
            return
        end
    end

    local v = self:FindView(vieworid, false)
    if v then
        setDisposingState(v, isDispose)
        tinsert(self._closeQueue, v)
    else
        logWarn("[BaseUIManager:Close] Error close a view: " .. tostring(vieworid))
    end
end

--[[
    移除指定层上的UI
]]
function BaseUIManager:PopLayer(sortingLayer, isDispose)
    sortingLayer = self:_checkLayer(sortingLayer)

    for i=#self._pushQueue, 1, -1 do
        local v = self._pushQueue[i]
        if v.sortingLayer == sortingLayer then
            v:onDispose()
            tremove(self._pushQueue, i)
        end
    end

    local layerViews = self._viewStack[sortingLayer]
    if not layerViews or #layerViews == 0 then
        logWarn("[BaseUIManager:PopLayer] no views on Layer: " .. sortingLayer)
        return
    end

    for _, v in ipairs(layerViews) do
        if v then
            setDisposingState(v, isDispose)
            tinsert(self._closeQueue, v)
        end
        --tinsert(self._closeQueue, v.sortingLayer)
    end
end

--[[
    移除所有UI
]]
function BaseUIManager:PopAll(isDispose)
    for k, _ in pairs(self._viewStack) do
        self:PopLayer(k, isDispose)
    end
end

local function activeOrDeactiveView(v, isactive)
    if isactive then
        if v.__lastVisible then
            v:onActive()
            UIView._showAction(v)
        end
    else
        v:onDeactive()
        UIView._hideAction(v)
    end
end

--[[
    @param: vieworid, class or id, 详见Close的参数说明
]]
function BaseUIManager:Active(vieworid)
    local v = self:FindView(vieworid, false)
    if v then
        local visible = v:Visible()
        --force acitive
        v:_recordVisiualState(true)
        if not visible then
            activeOrDeactiveView(v, true)
            if v.withMask then
                self:_adjustMask(v.sortingLayer)
            end
        else
            logWarn("[BaseUIManager:Active] view already active visible? ", vieworid)
            return v
        end
        
    else
        logWarn("[BaseUIManager:Active] No View: " .. tostring(vieworid))
    end
    return v
end

function BaseUIManager:ActiveLayer(sortingLayer)
    sortingLayer = self:_checkLayer(sortingLayer)
    local layerViews = self._viewStack[sortingLayer]
    local n = #layerViews
    if not layerViews or n == 0 then
        logWarn("[BaseUIManager:ActiveLayer] no views on Layer: " .. sortingLayer)
        return
    end
    local withMask = false
    for _, v in ipairs(layerViews) do
        if v then
            local visible = v:Visible()
            if not visible then
                activeOrDeactiveView(v, true)
                if v.withMask then
                    withMask = true
                end
            else
                --v:_recordVisiualState(true)
                logWarn("[BaseUIManager:ActiveLayer] view already active visible? ", v)
            end
        end
    end
    if withMask then
        local layer = self:_topMostMaskView()
        self:_adjustMask(layer)
    end
    --self:_setMaskVisible(self._lastMaskVisible)
end

function BaseUIManager:ActiveAll()
    for k, _ in pairs(self._viewStack) do
        self:ActiveLayer(k)
    end
end

--[[
    @param: vieworid, class or id, 详见Close的参数说明
]]
function BaseUIManager:Deactive(vieworid)
    local v = self:FindView(vieworid, false)
    if v then
        local visible = v:Visible()
        --v:_recordVisiualState(visible)
        if visible then
            activeOrDeactiveView(v, false)
        else
            logWarn("[BaseUIManager:Deactive] view already inactive? ", vieworid)
        end
        local vv, layer = self:_topMostMaskView()
        if vv and vv:ID() == v:ID() then
            self:_setMaskVisible(false, true)
        end
    else
        logWarn("[BaseUIManager:Deactive] No View: " .. tostring(vieworid))
    end
    return v
end

function BaseUIManager:DeactiveLayer(sortingLayer)
    sortingLayer = self:_checkLayer(sortingLayer)
    local layerViews = self._viewStack[sortingLayer]
    local n = #layerViews
    if not layerViews or n == 0 then
        logWarn("[BaseUIManager:DeactiveLayer] no views on Layer: " .. sortingLayer)
        return
    end
    local vv, layer = self:_topMostMaskView()
    for _, v in ipairs(layerViews) do
        if v then
            local visible = v:Visible()
            if visible then
                v:_recordVisiualState(visible)
                activeOrDeactiveView(v, false)
            else
                logWarn("[BaseUIManager:DeactiveLayer] view already inactive? ", v:classname())
            end
            if vv and vv:ID() == v:ID() then
                self:_setMaskVisible(false, true)
            end
        end
    end
end

function BaseUIManager:DeactiveAll(...)
    local arg = {...}
    for k, _ in pairs(self._viewStack) do
        if not table.find(arg, k) then
            self:DeactiveLayer(k)
        end
    end
end

--[[
    查找一个可见的view, id可以是：
    * UIView具体类型， 如UITestCase
    * UIView具体类型名："UITestCase"
    * UIView的ID,  view:ID()
    * 一个UIVIew的实例
    @param vieworid table or int type
    @param all 返回第一个或者所有
    @return 返回值两个参数, UIView和UIView数组，查找到的第一个和所有，
    当all为false时，第二个参数为空
]]
function BaseUIManager:FindView(vieworid, all)
    if type(vieworid) == "table" and vieworid.isInstance then
        return vieworid
    end

    local nameorid, idType = getidentifier(vieworid)

    if not all then
        for _, v in ipairs(self.__findCache) do
            if idType == 2 and v:ID() == nameorid then
                return v, nil
            elseif idType == 1 and v:classname() == nameorid then
                return v, nil
            end
        end
    end
    tclear(self.__findCache)
    for _, vv in pairs(self._viewStack) do
        for i=#vv, 1, -1 do
            local v = vv[i]
            if idType == 2 and v:ID() == nameorid then
                tinsert(self.__findCache, v)
                return v, self.__findCache
            elseif idType == 1 and v:classname() == nameorid then
                tinsert(self.__findCache, v)
            end
        end
    end
    return self.__findCache[1], self.__findCache
end

--[[
    Dispose this manager when game exit.
]]
function BaseUIManager:Dispose()
    self:Clean()
    if self._handle then
        LateUpdateBeat:RemoveListener(self._handle)
        self._handle = nil
    end
    self.rootCanvas = nil
    self.camera = nil
    for _, v in pairs(self._layerMap) do
        destroy(v.gameObject)
    end
    if self._blackMaskGameObject then
        destroy(self._blackMaskGameObject)
        self._blackMaskTrans = nil
        self._blackMaskGameObject = nil
        self._blackMaskImage = nil
    end
    self._viewActionNotifies = {}
end

--[[
    清楚当前管理的所有UI
]]
function BaseUIManager:Clean()
    for _, v in pairs(self._viewType2View) do
        for _, vv in pairs(v) do
            self:_unManageView(vv)
        end
    end
    self.__findCache = {}
    self._viewStack = {}
end

function BaseUIManager:AddNotify(func, para)
    if self._viewActionNotifies[func] then
        return
    end
    self._viewActionNotifies[func] = para and (function (...) func(para, ...) end) or func
end

function BaseUIManager:RemoveNotify(func)
    self._viewActionNotifies[func] = nil
end

--[[
    转化为该UI的屏幕空间
    @param worldPosV3, vector3, 世界坐标
    @return vector2, screen space position
]]
function BaseUIManager:WorldToScreenPoint(worldPosV3)
    return RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosV3)
end

--[[
    转化到指点节点的局部坐标
    @param rectTransform, type RectTransform, Local的相对节点
    @param sreenPoint, type vector2, screen space position
    @return type vector2, local position in screen space
]]
function BaseUIManager:ScreenPointToLocalPoint(sreenPoint, rectTransform)
    local out
    local hit, localPos = RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform or self.rootCanvas, sreenPoint, self.camera, out)
    return localPos, hit
end

--[[
    世界转局部
    @param worldPosV3, type vector3, world space position
    @param rectTransform, type RectTransform, Local的相对节点, 默认为rootCanvas
    @return type vector2, local position in screen space
]]
function BaseUIManager:WorldPointToLocalPoint(worldPosV3, rectTransform)
    return self:ScreenPointToLocalPoint(self:WorldToScreenPoint(worldPosV3), rectTransform or self.rootCanvas)
end
------------------END PUB-----------------------------

function BaseUIManager:_addLayer(order, root)
    if self._layerMap[order] then
        logError("_addLayer exist already: " .. order)
        return
    end
    self._layerMap[order] = root
end

function BaseUIManager:getLayerTransform(layerOrder)
    local layer = self._layerMap[layerOrder]
    if not layer then
        layer = _newLayerRectTransform("layer" .. layerOrder, self.rootCanvas, layerOrder)
        self:_addLayer(layerOrder, layer)
    end
    return layer
end

function BaseUIManager:_isPushing(view)
    for _, v in ipairs(self._pushQueue) do
        if view.isInstance then
            if v:ID() == view:ID() then
                return true, v
            end
        else
            if view:classname() == v:classname() then
                return true, v
            end
        end
    end
    return false
end

function BaseUIManager:_getView(view)
    if not view.isInstance then
        local typename = view.classname()
        local mv = self:_getManagedView(typename)
        if not mv then
            local viewType = view
            view = viewType.new()
        else
            view = mv
        end
    end
    return view
end

function BaseUIManager:_closeView(view, layer)
    if self:_manageViewStack(view, layer, true) then
        self:_hideView(view, function ()
            if true or view.withMask then
                self:_adjustMask(view.sortingLayer)
            end
            --logError("_closeView: ", view.__disposing, view:classname(), view._viewID )
            if view.__disposing then
                view.__disposing = nil
                self:_unManageView(view)
            else
                if view.transform then
                    view.transform:SetAsLastSibling()
                end
            end
        end)
    end
end

function BaseUIManager:_onEndFrame()
    if #self._closeQueue > 0 then
        for k = 1, #self._closeQueue do
            local item = self._closeQueue[k]
            self._closeQueue[k] = nil
            if type(item) == "table" then
                self:_closeView(item, item.sortingLayer)
            else --use less now. remove by layer
                local layer = item
                --if has view push on the layer.
                local pending = false
                -- for i=#self._pushQueue, 1, -1 do
                --     local v = self._pushQueue[i]
                --     if v.sortingLayer == layer then
                --         tremove(self._pushQueue, i)
                --         pending = true
                --         break
                --     end
                -- end
                if not pending then
                    local view = self:_peekView(layer)
                    if not view then
                        logWarn("-[BaseUIManager:Pop] no view on Layer" .. layer)
                    else
                        self:_closeView(view, layer)
                    end
                end
            end
        end
    end

    if #self._pushQueue > 0 then
        local item = tremove(self._pushQueue, 1)
        item.__pushing = true
        self:_manageViewStack(item, item.sortingLayer, false)
        if item:IsLoaded() then
            if self:_checkViewExist(item) then
                self:_attachToCanvas(item)
                self:_showView(item)
                self:_adjustLayer(item)
            end
        else
            --load asset of the view
            self.assetLoader(item.assetBundle, item.viewName, function (go)
                if IsNil(go) then
                    item:onError()
                    return
                end
                if self:_checkViewExist(item) then
                    local newGo = self:_attachToCanvas(item, go)
                    if newGo then
                        item:SetViewObject(newGo)
                    end
                    self:_showView(item)
                    self:_adjustLayer(item)
                else
                end
            end)
        end
    end
end

function BaseUIManager:_showView(view)
    view:onShow()
    if view.enableShowAnimation then
        view:showAnimation(function ()
            view:onShowed()
            --view.__pushing = nil
            Event.Brocast(Protocal.SwitchView,view,true)
        end)
    else
        view:onShowed()
        --view.__pushing = nil
        Event.Brocast(Protocal.SwitchView,view,true)
    end
end

function BaseUIManager:_getManagedView(typename)
    local name = typename
    local vv = self._viewType2View[name]
    if vv then
        local _, v = next(vv)
        return v
    end
end

function BaseUIManager:_manageView(v)
    local name = v.classname()
    local vid = v:ID()
    --logError("_manageView: ", name, vid)
    local vv = self._viewType2View[name]
    if not vv then
        vv = {}
        self._viewType2View[name] = vv
    end
    if vv[vid] then
        logError(string.format("View [%s:%d] already in manage", name, vid))
        return
    end
    vv[vid] = v
end

function BaseUIManager:_unManageView(v)
    local name = v.classname()
    local vid = v:ID()
    --log(string.format("[BaseUIManager:_unManageView] view: %s:%d", name, vid))
    local vv = self._viewType2View[name]
    if not vv then
        v:onDispose()
        return
    end
    if vv[vid] then
        vv[vid] = nil
        if not IsNil(v.gameObject) then
            Object.Destroy(v.gameObject)
        end
    else
        logWarn(string.format("repeat dispose view, view %s:%d not managed", name, vid))
    end
    v:onDispose()
end

function BaseUIManager:_removeFromFindCache(view)
    for i, v in ipairs(self.__findCache) do
        if v:ID() == view:ID() then
            tremove(self.__findCache, i)
            break
        end
    end
end

function BaseUIManager:_manageViewStack(view, layer, isRemove)
    local success = false
    local layerViews = self._viewStack[layer]
    if not layerViews then
        layerViews = {}
        self._viewStack[layer] = layerViews
    end
    if isRemove then
        for i, v in ipairs(layerViews) do
            if v == view then
                success = true
                tremove(layerViews, i)
                break
            end
        end
        self:_removeFromFindCache(view)
    else
        tinsert(layerViews, view)
        success = true
    end
    return success
end

--is in viewstack?
function BaseUIManager:_checkViewExist(view)
    for _, v in ipairs(self._closeQueue) do
        if v == view then
            return false
        end
    end

    local layerViews = self._viewStack[view.sortingLayer]
    if layerViews then
        for _, v in ipairs(layerViews) do
            if v == view then
                return true
            end
        end
    end
    return false
end

function BaseUIManager:_attachToCanvas(view, prefab)
    local layer = self:getLayerTransform(view.sortingLayer)
    if prefab then
        self:_manageView(view)
        return Object.Instantiate(prefab, layer, false)
    else
        local trans = view.transform
        if trans then
            if not layer:Equals(trans.parent) then
                trans:SetParent(layer, false)
            end
        else
            logError("[BaseUIManager:attachToCanvas] Error for nil transform.")
        end
    end
end

function BaseUIManager:_adjustLayer(view)
    --self:DumpViewStack()
    local layerViews = self._viewStack[view.sortingLayer]
    local trans = view.transform
    for i=#layerViews, 1, -1 do
        local v = layerViews[i]
        if v == view then
            --log("_adjustLayer to: " .. tostring(v) .. "  " .. i)
            trans:SetSiblingIndex(i)
            break
        end
    end
    if true or view.withMask then
        self:_adjustMask()
    end
end

function BaseUIManager:_topMostMaskView()
    for k, layerViews in spairs(self._viewStack, reverse_comparer) do
        for i=#layerViews, 1, -1 do
            local v = layerViews[i]
            if v.withMask then
                return v, k
            end
        end
    end
end

function BaseUIManager:_adjustMask(layerOrder)
    --find top most with mask
    local topView, layer = self:_topMostMaskView()
    if not topView or not topView:Visible() then
        self:_setMaskVisible(false)
        return
    end

    self:_setMaskVisible(true)
    local layerTrans = self:getLayerTransform(layer)
    self._blackMaskTrans:SetParent(layerTrans)
    local mastIndex = self._blackMaskTrans:GetSiblingIndex()
    local index = topView.transform:GetSiblingIndex()
    local layerViews = self._viewStack[layer]
    local n = layerViews and #layerViews or 0
    if mastIndex < index then
        index = index - 1
    end
    --log("top most mask view: ", topView:classname(), index, n)
    local bindex = math.clamp(index, 0, n)
    self._blackMaskTrans:SetSiblingIndex(bindex)
    local maskColor = topView.maskColor
    if maskColor then
        self._blackMaskImage.color = maskColor
        currentMaskColor = maskColor
    else
        self._blackMaskImage.color = _defaultMaskColor
        currentMaskColor = _defaultMaskColor
    end
end

function BaseUIManager:_peekView(layer, remove)
    local layerViews = self._viewStack[layer]
    if not layerViews then
        return
    end
    local v = layerViews[#layerViews]
    if v then
        assert(layer == v.sortingLayer, "Do not support modify view Layer after contruct.")
    end
    if remove then
        tremove(layerViews)
    end
    return v
end

function BaseUIManager:_hideView(view, callback)
    view:onHide()
    if view.enableHideAnimation then
        view:hideAnimation(function ()
            view:onHided()
            if callback then
                callback()
            end
            Event.Brocast(Protocal.SwitchView,view,false)
        end)
    else
        view:onHided()
        if callback then
            callback()
        end
        Event.Brocast(Protocal.SwitchView,view,false)
    end
end

function BaseUIManager:_checkLayer(sortingLayer)
    if not sortingLayer or type(sortingLayer) ~= "number" then
        sortingLayer = self.defaultSortingLayer
    end
    return sortingLayer
end

function BaseUIManager:_setMaskVisible(visible, setlast)
    if self._currentMaskVisible == visible then
        return
    end

    if self.transitionBlackMask then
        if visible then
            local last = MyGameUtil.SetGameObjectActive(self._blackMaskGameObject, visible) > 0
            if setlast then
                self._lastMaskVisible = last
            end
            MyGameUtil.DoRawImageAlpha(self._blackMaskImage, currentMaskColor.a, 0.2)
        else
            local t = MyGameUtil.DoRawImageAlpha(self._blackMaskImage, 0, 0.2)
            if t then
                t:OnComplete(function()
                    local last = MyGameUtil.SetGameObjectActive(self._blackMaskGameObject, false) > 0
                    if setlast then
                        self._lastMaskVisible = last
                    end
                end)
            end
        end
    else
        local last = MyGameUtil.SetGameObjectActive(self._blackMaskGameObject, visible) > 0
        if setlast then
            self._lastMaskVisible = last
        end
    end
    self._currentMaskVisible = visible
end

function BaseUIManager:_fireNotify(action, view)
    for _, v in pairs(self._viewActionNotifies) do
        if v and type(v) == "function" then
            v(action, view)
        end
    end
end

return BaseUIManager