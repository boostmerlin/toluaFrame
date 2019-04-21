---------HallPanel---------
local MyFlag = class("MyFlag", UIView)

--传递自定义参数 方法1
function MyFlag:ctor(x, y)
    MyFlag.super.ctor(self)
    --禁用掉打开动画
    self.enableShowAnimation = false
    self.x = x
    self.y = y
    -- self.assetBundle = "MyFlag"
    -- self.assetName = "MyFlag"
end

function MyFlag:onCreate()
    --记得调用子类的函数
    MyFlag.super.onCreate(self)

    setLocalXYZ(self.transform, self.x, self.y)
end

function MyFlag:onDispose()
    MyFlag.super.onDispose(self)
end

---------UITestDialog---------
local UITestDialog = class("UITestDialog", UIView)

function UITestDialog:ctor()
    UITestDialog.super.ctor(self)
    self.widgets = {
        ["btnClose"] = {"closeButton", "Button"},
        "btnPopLayer",
        "btnPopAll",
    }
    --为button绑定一个Close事件，不要写路径，只要button的变量名
    self.autoClose = "btnClose"
end
function UITestDialog:onCreate()
    --记得调用子类的函数
    UITestDialog.super.onCreate(self)

    --可以不使用widget绑定，自已手动查找ui控件，兼容原来流程
    self._text = GText(self.transform, "Text")

    --使用参数
    self._text.text = self.show_text
    self:AddListener(self.btnPopLayer, function (go, para)
       GUIManager.Instance:PopLayer(GUIManager.DEFAULT, true)
    end)

    self:AddListener(self.btnPopAll, function (go, para)
        GUIManager.Instance:PopAll(true)
     end)
end

function UITestDialog:onShow()
    log("UITestDialog----- onShow")
    UITestDialog.super.onShow(self)
end

function UITestDialog:onShowed()
    log("UITestDialog----- onShowed")
    UITestDialog.super.onShowed(self)
end

function UITestDialog:onHide()
    log("UITestDialog----- onHide")
    UITestDialog.super.onHide(self)
end

function UITestDialog:onHided()
    log("UITestDialog----- onHided")
    UITestDialog.super.onHided(self)
end

-----------------------

local UITestCase = class("UITestCase", UIView)
function UITestCase:ctor()
    UITestCase.super.ctor(self)

    --[[
        使用widgets
        1.autoInject = true. 默认打开。不使用这个时候，手动置为false
        2.指定widgets
        有三种形式：
        self.widgets = {
            ["bind_name"] = {"path/xxx", "type"}, --type: Button, Image etc.
            ["bind_name"] = {"path/xxx"},
            "xxx", --bind_name 和path都是xxx
        }
    ]]
    --self.autoInject = false
    self.widgets = {
        --如果transform下有多个UGUI组件，必须指定类型，否则默认获取最后一个
        ["btnDiaglog"] = {"btnDiaglog", "Button"},
        --使用全路径，查找得更快!
        ["btnDiaglog2"] = {"btngroups/btnDiaglog2", "Button"},

        ["btnPush2"] = {"btnPush2", "Button"},

        ["btnPush2andPop1"] = {"btnPush2andPop1", "Button"},
        "btnMutipleInstance",
        "btnToggleActive",
        "btnPopWithMask",
        "btnPopWithMask2",
    }
end

--如果定义了Update 函数，View会在每帧调用Update
function UITestCase:Update()
    --log("xxx")
end

function UITestCase:onCreate()
    log("----- onCreate")
    UITestCase.super.onCreate(self)
    Event.AddListener(Protocal.EscapeKeyDown , UITestCase.escape)
    
    self:AddListener(self.btnDiaglog, function (go, para)
        log("UITestCase click: " .. go.name .. "  " .. para)
        local diag = GPush(UITestDialog)
        --增加参数
    end, "test click para") --listener 可以带参数，在点击回调里获取

    self:AddListener(self.btnDiaglog2, function (go, para)
        log("UITestCase click: " .. go.name)
        local diag = GPush(UITestDialog)
        --自定义push参数 方法2
        --push 失败会返回空，注意判断,日志中会有警告
        if diag then
            diag.show_text = "这里是Push自定义参数"
        end
        local t = FrameTimer.New(function ()
            log("------frame delay frame passed.")
            --GPush(UITestDialog)
            GPop()

            GPush(MyFlag)
        end, 15, 1)
        t:Start()
    end)

    self:AddListener(self.btnPush2, function (go)
        log("UITstCase click: " .. go.name)
        GPush(UITestDialog)
        GPop()
        GPush(UITestDialog)
        GPop()
        GPush(UITestDialog)
    end)

    self:AddListener(self.btnPush2andPop1, function (go)
        log("UITstCase click: " .. go.name)
        GPush(UITestDialog)
        
        local t = FrameTimer.New(function ()
            log("------frame delay frame passed.")
            GPop()
        end, 4, 1)
        t:Start()
    end)

    self:AddListener(self.btnMutipleInstance, handler(self,self.onMutilInstanceClick),self)

    self.toggleFlag = true
    self:AddListener(self.btnToggleActive, function ()
        if self.toggleFlag then
            self.toggleFlag = false
            GUIManager.Instance:DeactiveLayer()
        else
            self.toggleFlag = true
            GUIManager.Instance:ActiveLayer()
        end
    end)

    self:AddListener(self.btnPopWithMask, function ()
        local diag = GPush(UITestDialog)
        --push 失败会返回空，注意判断,日志中会有警告
        if diag then
            diag.withMask = true
            diag.show_text = "--! :) :("
        end
    end)

    self:AddListener(self.btnPopWithMask2, function ()
        local diag = GPush(UITestDialog)
        if diag then
            --少这样做，不停修改sortingLayer可能造成奇怪的层级问题
            diag.sortingLayer = GUIManager.HUD
            diag.withMask = true
        end
    end)
end

function UITestCase.escape()
    GPop()
end

function UITestCase:onMutilInstanceClick(go)
    log("UITstCase click: " .. go.name)
    local x, y = -100, -340
    for i = 1, 10 do
        local flag = MyFlag.new(x + 50 * i , y)
        GPush(flag)
    end
end

function UITestCase:onShow()
    log("----- onShow")
    UITestCase.super.onShow(self)
end

function UITestCase:onShowed()
    log("----- onShowed")
    UITestCase.super.onShowed(self)
end

function UITestCase:onHide()
    log("----- onHide")
    UITestCase.super.onHide(self)
end

function UITestCase:onHided()
    log("----- onHided")
    UITestCase.super.onHided(self)
end

function UITestCase:onDispose()
    log("----- onDispose")
    UITestCase.super.onDispose(self)
    Event.RemoveListener(Protocal.EscapeKeyDown)
end

--[[
    测试 自定义动画
]]
function UITestCase:showAnimation(overcallback)
    local trans = self.transform
    setAnchorXY(trans, -2000)
    trans:DOLocalMoveX(0, 0.65):OnComplete(function ()
        overcallback()
    end)
end

function UITestCase.Test1()
    GUIManager.Instance:SetDefaultSortingLayer(GUIManager.BOTTOM)
    local view = UITestCase.new()
    --指定UI层级
    view.sortingLayer = GUIManager.DEFAULT
    GPush(view)
    --test useless push
    GPush(view)
end

return UITestCase