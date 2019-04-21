local json = require "cjson"
FileUtil = {}

local this = FileUtil
local dataPath = LuaFramework.CSUtil.DataPath

local function getFullFileName(name)
    return dataPath .. name
end

function FileUtil.ReadJson(fileName)
    local f
    local func= function()
        f = assert(io.open(getFullFileName(fileName),'r'), "Can't open " .. fileName)
    end
    local status = pcall(func)
    if not status then
        f = nil
    end
    if f==nil then
        return 
    end
	local t = f:read("*a")
	f:close()
	local rdata = assert(json.decode(t))

    return rdata
end

function FileUtil.SaveToJson(fileName, data)
    local fullPath = getFullFileName(fileName)
	local f = io.open(fullPath,'w')
    if f==nil then
        logError("Can't open file for write: ", fullPath)
        return
    end
    local str = json.encode(data)
	f:write(str)
	f:close()
end