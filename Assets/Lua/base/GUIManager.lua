--[[
    GUIManager Graphcis UI
    @author: muliang
    @date: 2018-12-28
    @description:

]]
local Manager = class("GUIManager", require("base.BaseUIManager"))

function Manager:ctor()
    local go = GameObject.FindWithTag("GUICanvas");
    if not go then
        error("Can't find GUICanvas...")
        return
    end
    local canvasRoot = go:GetComponent("RectTransform")
    Manager.super.ctor(self, canvasRoot, 
        function(abName, assetName, callback)
            resMgr:LoadPrefab(abName, {assetName}, function(gos)
                if gos.Length > 0 then
                    callback(gos[0])
                end
            end)
        end
    )
end

local ins = Manager.new()
GUIManager = {
    Instance = ins,
    --UI层定义 中间值为特效预留
    BOTTOM = 0,
    DEFAULT = 1*10, --
    POPUP1 = 2*10, -- 弹出层1
    POPUP2 = 3*10, -- 弹出层2
    HUD = 4*10,    --功能最上层，跑马灯等
    GUIDE = 5*10,  --新手引导
    MOUSE = 6*10,  --鼠标特效
}

GUIManager.MASK_COLOR_LIGHT = Color.New(0, 0, 0, 0.41)
GUIManager.MASK_COLOR_DARK = Color.New(0, 0, 0, 0.55)

ins:SetDefaultSortingLayer(GUIManager.DEFAULT, GUIManager.MASK_COLOR_LIGHT)

local function ShowAnim(view, callback)
    setScaleXYZ(view.transform, 1.1, 1.1)
    view:setAlpha(1)
    view.transform:DOScale(view._originScale, 0.1):SetUpdate(true):OnComplete(callback)
end

local function HideAnim(view, callback) 
    local t1 = MyGameUtil.DOUIAlpha(view.luaBehavior.canvasGroup, 1, 0, 0.14)
    if t1 then
       -- t1:SetEase(Ease.OutQuad)
        t1:Play()
    end
    local t2 = view.transform:DOScale(1.1, 0.1):SetUpdate(true):SetEase(Ease.OutExpo):OnComplete(callback)
    t2:SetDelay(0.07)
end

ins:SetDefaultHideAnim(HideAnim)
ins:SetDefaultShowAnim(ShowAnim)

function GPush(view, param)
    return ins:Push(view, param)
end

function GPop(sortingLayer, isDispose)
    return ins:Pop(sortingLayer, isDispose)
end

function GFind(view,all)
    return ins:FindView(view,all)
end

function GClose(view,isDispose)
    ins:Close(view,isDispose)
end

function IsPanelVisible(view)
    local panel = GFind(view)
    if (not panel)then
        return false
    end
    return panel:Visible()
end