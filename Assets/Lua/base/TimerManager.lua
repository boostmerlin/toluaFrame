--[[
    @author: muliang
    @date: 2018-3-1
    @description:
    a timer util.
]]
local TimerManager = class("TimerManager")
local UpdateBeat = UpdateBeat
local Time = Time
local list = list
local ilist = ilist
local TimerItem = class("TimerItem")

local TIMER_ID = 1

function TimerItem:ctor(init_time, id)
    if id then
        self.timerId = id
    else
        self.timerId = TIMER_ID
        TIMER_ID = TIMER_ID + 1
    end

    self.isBackDown = true --是否是倒时计，否则时间会相加
    self.endTime = 0       --当到达endTime时会停止
    self.isEnd = false     --是否结束
    self.autoDel = true    --是否自动从Manager删除
    self._initTime = init_time  --启始时间
    self.time = init_time  
    self.onBeat = nil --当时间有变化时
    self.onTimeup = nil --当时间到达endTime 时
    self.onDel = nil --移除时
end

function TimerItem:_beat(t)
    if self.isEnd then
        return false
    end
    if self.isBackDown then
        self.time = self.time - t
        if self.onBeat then
            self.onBeat(self, -t)
        end
        if self.time <= self.endTime then
            self.time = self.endTime
            self.isEnd = true
            if self.onTimeup then
                self.onTimeup(self)
            end
            return true
        end
    else
        self.time = self.time + t
        if self.onBeat then
            self.onBeat(self, t)
        end
        if self.endTime > 0 and self.time >= self.endTime then
            self.time = self.endTime
            self.isEnd = true
            if self.onTimeup then
                self.onTimeup(self)
            end
            return true
        end
    end
    return false
end

function TimerItem:toString(formatter)
    if formatter then
        return formatter(self.time)
    else
        local s = self.time
        if s < 0 then
            return "negative time"
        end
        return string.format("%.2d:%.2d:%.2d", s / (60*60), s / 60 % 60, s % 60)
    end
end

function TimerItem:Reset(new_init_time)
    self.isEnd = false
    if not new_init_time and type(new_init_time) == "number" then
        self._initTime = new_init_time
    end
    self.time = self._initTime
end

function TimerManager:ctor(beatInterval, unscaled)
    self.time = beatInterval
    self.duration = beatInterval
    self.unscaled = unscaled
    self._timerDriver = UpdateBeat:CreateListener(self._timeBeat, self)
    UpdateBeat:AddListener(self._timerDriver)	
    self.running = true
    self._timerItems = {}
    self.__listtimerItems = list:new()
end

function TimerManager:GetTimerItem(init_time, unique_id)
    if unique_id and self._timerItems[unique_id] then
        logError("[TimerManager:GetTimerItem] there's a timer id: " .. unique_id)
        return nil
    end

    local item = TimerItem.new(init_time)
    if unique_id then
        item.timerId = unique_id
    else
        unique_id = item.timerId
    end
    self._timerItems[unique_id] = item
    self.__listtimerItems:push(item)

    return item
end

function TimerManager:_remove(i, v)
    self._timerItems[v.timerId] = nil
    self.__listtimerItems:remove(i)
    if self.onDel then
        self.onDel()
    end
end

function TimerManager:DelTimerItem(unique_id)
    local v = self._timerItems[unique_id]
    if not v then
        log("[TimerManager:DelTimerItem] no timer id: " .. unique_id)
        return false
    end

    local iter = self.__listtimerItems:find(v)
    if iter then
       self:_remove(iter, v)
    end

    return true
end

function TimerManager:_timeBeat()
    if not self.running then
		return
	end
	local delta = self.unscaled and Time.unscaledDeltaTime or Time.deltaTime	
	self.time = self.time - delta
    if self.time <= 0 then
        -- test ```
        --local t = self.duration - self.time
        local t = self.duration
        for iter, v in ilist(self.__listtimerItems) do
            if v:_beat(t) then
                if v.autoDel then
                    self:_remove(iter, v)
                end
            end
        end
        self.time = self.time + self.duration
	end
end

function TimerManager:Dispose()
    self.running = false
	if self._timerDriver then
		UpdateBeat:RemoveListener(self._timerDriver)	
    end
    self._timerItems = {}
    self.__listtimerItems:clear()
end

return TimerManager