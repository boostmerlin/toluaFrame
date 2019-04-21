--[[
like Unity Brocast Event System in lua.
]]

local EventLib = require "base.eventlib"

local Event = {}
local events = {}
local intercept = {}

function Event.AddListener(event,handler,obj)
	if not event or type(event) ~= "string" then
		error("event parameter in addlistener function has to be string, " .. type(event) .. " not right.")
	end
	if not handler or type(handler) ~= "function" then
		error("handler parameter in addlistener function has to be function, " .. type(handler) .. " not right")
	end

	if not events[event] then
		--create the Event with name
		events[event] = EventLib:new(event)
	end

	--conn this handler
	events[event]:connect(handler,obj)
end

function Event.Brocast(event,...)
	local intercepts = intercept[event]
	if intercepts then
		local len = #intercepts
		local interceptEvent = intercepts[len]
		local obj = interceptEvent[2]
		if obj then
			interceptEvent[1](obj,...)
		else
			interceptEvent[1](...)
		end
		if len == 1 then
			intercept[event] = nil
		else
			intercept[event][len] = nil
		end
	elseif events[event] then
		events[event]:fire(...)
	end
end

function Event.Intercept(event,func,obj)
	if not intercept[event] then
		intercept[event] = {}
	end
	table.insert(intercept[event], {func,obj})
end

function Event.RemoveListener(event,handler,obj)
	if events[event] then
		events[event]:disconnect(handler,obj)
	end
	if intercept[event] then
		intercept[event] = nil
	end
end

return Event
