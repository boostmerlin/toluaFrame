--[[
    BaseCtrl. Base controller.
    @author: muliang
    @date: 2018-1-3
    @description:
    usually put logic and net message handling here
]]
local MsgDispatcher = require "base.MsgDispatcher"
require "base.CtrlManager"

local BaseCtrl = class("BaseCtrl")
function BaseCtrl:ctor()
    if self.netHandler == nil then
        self.netHandler = true
        self.priority = nil
    end
    MsgDispatcher.Reg(self, self.priority)
    CtrlManager.AddCtrl(self:classname(), self)
end

function BaseCtrl:Dispose()
    MsgDispatcher.UnReg(self)
    CtrlManager.RemoveCtrl(self:classname())
end

return BaseCtrl