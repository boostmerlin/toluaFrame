--[[
    SUIManager.  场景UI管理器
    @author: muliang
    @date: 2018-12-28
    @description:
    场景UI,用于可跟随地图移动的UI
]]
local Manager = class("SUIManager", require("base.BaseUIManager"))

function Manager:ctor()
    local go = GameObject.FindWithTag("SceneCanvas");
    if not go then
        error("Can't find SceneCanvas...")
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
SUIManager = {
    Instance = ins,
    --Scene UI层定义 中间值为特效预留
    MAPBG = -10, --地图bg
    CITY = -5,   --城池
    OVERCITY = 0,--城池上
    TEXTLOWER = 20, --文本，在城池特效，行军npc各种特效之上，1-19美术预留
    DEFAULT = 21, -- 默认层.TEXTUPPER
    BOTTOM = 22, --最上层 特殊需求
}

ins:SetDefaultSortingLayer(SUIManager.DEFAULT)

function SPush(view)
    return ins:Push(view)
end

function SClose(view, isDispose)
    return ins:Close(view, isDispose or true)
end