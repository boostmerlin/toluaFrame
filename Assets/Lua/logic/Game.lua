local lpeg = require "lpeg"
local json = require "cjson"
local MsgPacker = require "base.MsgPacker"

CommonCtrl = nil

Game = {}
local this = Game

local HEART_BEAT_INTERVAL = 30*2
local HEART_CHECK_MAX = 3

local _G = _G

function Game.InitViewPanels()
    local names = PanelNames or {}
    for i = 1, #names do
        local name = names[i]
		_G[name] = require ("view/"..name)
	end
end

function Game.InitModels()
end

function Game.OnInitOK()
    --profiler:start()
    MsgPacker.init()
    CtrlManager.Init()
    Game.InitModels()
    --profiler:stop()

    this.InitViewPanels();

    if LeanTouch.OnFingerDown then
        LeanTouch.OnFingerDown = LeanTouch.OnFingerDown + this.OnFingerDown                      
    else
        LeanTouch.OnFingerDown = this.OnFingerDown
    end
    log('LuaFramework InitOK--->>>');
    this.fx_click_root = GUIManager.Instance:getLayerTransform(GUIManager.MOUSE)
    Network.onNetState = this.onNetState
    GUIManager.Instance:SetViewPath("view")

    GPush("UITestCase")
end

function Game.OnFingerDown(finger)
    local prefab = Resources.Load("Fx_click")
    if not prefab then
        return
    end

    local pos = finger.ScreenPosition
    pos = GUIManager.Instance:ScreenPointToLocalPoint(pos, this.fx_click_root)
    local obj = newObject(prefab, this.fx_click_root)
    obj.transform.localPosition = pos
    Object.Destroy(obj, 0.5)
end


function Game.onNetState(connected)
    if connected then
        return
    end

    this.StopHearbeat()
    if Network.TryReconnect() then
        Network.onNetRetryMax = function()
            this.ShowErrorDialog(StrRes.NET_ERROR)
        end
    end
end

function Game.ShowErrorDialog(strmsg)
end

function Game.AddHeartbeatCounter(n)
    this.heart_beat_count = this.heart_beat_count + n
end

function Game._heartBeat()
    if this.heart_beat_count > HEART_CHECK_MAX then
        this.onNetState()
        return
    end
    this.AddHeartbeatCounter(1)
    Network.Send("cs_request_heart_beat", {})
end

function Game.StartHeartbeat()
    this.StopHearbeat()
    --heart beat timer.
    local timer = Timer.New(Game._heartBeat, HEART_BEAT_INTERVAL, -1, true)
    timer:Start()
    Game.heart_beat_timer = timer
end

function Game.StopHearbeat()
    if Game.heart_beat_timer then
        Game.heart_beat_timer:Stop()
        Game.heart_beat_timer = nil
    end
    Game.heart_beat_count = 0
end

--测试协同--
function Game.test_coroutine()
    log("test_coroutine start ,,,,,,,,,,,,,,,,,,,,,,,,,,")    
    coroutine.wait(3);	
end

--测试lpeg--
function Game.test_lpeg_func()
	logWarn("test_lpeg_func-------->>");
	-- matches a word followed by end-of-string
	local p = lpeg.R"az"^1 * -1
end

function Game.test_out()
    log("AppUtil.deviceId: ", AppUtil.deviceId)
end

function Game.OnDestroy()
    CommonCtrl:Dispose()
    if LeanTouch.OnFingerDown then
        LeanTouch.OnFingerDown = LeanTouch.OnFingerDown - this.OnFingerDown                      
    end
    this.fx_click_root = nil
end

function Game.setServer(ip, port)
    log("===================setServer to: ", ip, port)
    AppConst.SocketPort = port
    AppConst.SocketAddress = ip
end
