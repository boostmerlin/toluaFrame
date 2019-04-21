--[[
    @author: muliang
    @date: 2018-11-27
    @description:
    net handler 应该是table, 并且一定含有 [netHandler] 字段
    可选地可能含有 [priority] 增加优化级排序, 数字越大，优先级越高。
]]

local MsgDispatcher = {}
local NetMessageHandlers = {}
local Priorities = {}

function MsgDispatcher.Dispatch(pbHead, pbName, pbMsg)
    local hasHandler = false
    for _, p in ipairs(Priorities) do
        local handlers = NetMessageHandlers[p]
        for _, handler in ipairs(handlers) do
            if handler.netHandler then
                local func = handler[pbName]
                if func ~= nil then
                    hasHandler = true
                    local flag, ret
                    flag = true
                    --flag, ret = xpcall(func, __G__ERRORTRACE, handler, pbMsg, pbHead)
                    ret = func(handler, pbMsg, pbHead)
                    if flag and ret then
                        break
                    end
                end
            end
        end
    end

    if not hasHandler then
        logWarn("No Msg handlder for msg:", pbName)
    end
    return hasHandler
end

function MsgDispatcher.Reg(netHandler, priority)
    if type(netHandler) == "table" and netHandler.netHandler ~= nil then
        --if has been registered
        if MsgDispatcher.Has(netHandler) then
            logWarn("!!! netHandler has been registered before: " .. tostring(netHandler))
            return
        end

        local priority = priority or 1
        assert(type(priority) == "number", "[MsgDispatcher.Reg] Priority should be number.")
        if #Priorities == 0 then
            table.insert(Priorities, priority)
        end
        if not table.find(Priorities, priority) then
            for i, v in ipairs(Priorities) do
                if priority > v then
                    table.insert(Priorities, i, priority)
                    break
                end
            end
        end

        local hh = NetMessageHandlers[priority]
        if not hh then
            hh = {}
            NetMessageHandlers[priority] = hh
        end
        table.insert(hh, netHandler)
    else
        --logError("MsgDispatcher.Reg NetMessageHandler should be table with [netHandler] Flag")
    end
end

function MsgDispatcher.Has(netHandler, isCheckName)
    for _, v in pairs(NetMessageHandlers) do
        for _, vv in ipairs(v) do
            if vv == netHandler then
                return true
            end
        end
    end
    return false
end

function MsgDispatcher.UnReg(netHandler)
    if type(netHandler) == "table" then
        for p, v in pairs(NetMessageHandlers) do
            for i, vv in ipairs(v) do
                if vv == netHandler then
                    table.remove(v, i)
                    if #v == 0 then
                        table.removeItem(Priorities, p)
                    end
                    return true
                end
            end
        end
    else
        logWarn("NetMessageHandler should be table")
    end
    return false
end

function MsgDispatcher.Dump()
    --for debug 
end

return MsgDispatcher