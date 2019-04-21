require "base.include"
require "base.define"
require "logic.Game"

function Main()
    ConfigLog(true, LOG_LEVEL.DEBUG)
    DEBUG = AppDef.DebugMode
    log("DebugMode ? ", AppDef.DebugMode)

    Game.OnInitOK()
end

function ScreenTap(finger)
    log("Screen tap on void")
    Event.Brocast(Protocal.ScreenTapOnNull)
end

function EscapeKeyDown()
    log("Escape key down.")
    Event.Brocast(Protocal.EscapeKeyDown)
end