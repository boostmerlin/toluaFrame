local WorkMonitor = class("WorkMonitor")

function WorkMonitor:ctor()
    self._mission = {}
    self._states = {}
    self._all_callback = nil
    self._progress_callback = nil
    self._count = 0
    self._isSequence = false
end

function WorkMonitor:_addMission(func, name)
    local state = {
        name = name
    }
    local monitor = self
    state._finish = false
    state.finish = function (st)
        st._finish = true
        self._count = self._count + 1
        local all = self._count == #self._mission
        -- for _, v in pairs(monitor._states) do
        --     if not v._finish then
        --         all = false
        --         break
        --     end
        -- end
        if all and monitor._all_callback then
            monitor._all_callback(self._count)
            return
        end
        if self._isSequence then
            local func = self._mission[self._count + 1]
            if func then
                func(self:GetState(func))
            end
        end
    end
    table.insert(self._mission, func)
    self._states[func] = state
    return state
end

function WorkMonitor:Add(func, name)
    return self:_addMission(func, name or tostring(func))
end

function WorkMonitor:GetState(func)
    return self._states[func]
end

function WorkMonitor:Run()
    if #self._mission == 0 then
        log("no mission to run.")
        return
    end
    for _, v in ipairs(self._mission) do
        if v then
            v(self:GetState(v))
        end
    end
end

function WorkMonitor:RunSequentially()
    if #self._mission == 0 then
        log("no mission to run.")
        return
    end
    self._isSequence = true
    local func = self._mission[1]
    if func then
        local stat =  self:GetState(func)
        func(stat)
    end
end

function WorkMonitor:AddMultiple(...)
    local arg = {...}
    for i, v in ipairs(arg) do
        self:_addMission(v, tostring(i))
    end
end

function WorkMonitor:All(notify_callback)
    if notify_callback and type(notify_callback) == "function" then
        self._all_callback = notify_callback
    end
end

return WorkMonitor