local tconcat = table.concat
local tinsert = table.insert
local srep = string.rep
local type = type
local pairs = pairs
local tostring = tostring
local next = next
local Debugger = Debugger
local log_enable = true
local log_level = 1
--delete print.
print = function()end

LOG_LEVEL = {
    DEBUG = 1
}

function ConfigLog(enable, level)
    log_enable = enable
    log_level = level
end

local function getLogStr(typestr, ...)
    local str_table = {typestr}
    local info = debug.getinfo(3, "Sl")
    tinsert(str_table, string.format("%s:%d|", info.short_src, info.currentline))
    local arg = { ... }
    for i=1, #arg do
        tinsert(str_table, tostring(arg[i]))
    end

    return tconcat(str_table, " ")
end
--输出日志--
function log(...)
    if not log_enable or 1 < log_level then
        return
    end
    Debugger.Log(getLogStr("[DEBUG]", ...))
end

--警告日志--
function logWarn(...)
    if not log_enable or 2 < log_level then
        return
    end
    Debugger.LogWarning(getLogStr("[WARN]", ...))
end

--错误日志--
function logError(...)
    if not log_enable or 3 < log_level then
        return
    end
    Debugger.LogError(getLogStr("[ERROR]", ...))
end


function destroy(obj)
    Object.Destroy(obj)
end

function instantiate(prefab)
    return Object.Instantiate(prefab)
end

function setClickEvent(obj,func,data)
    if (obj)then
        if (data)then
            EventListener.Get(obj):SetOnClickParameterEvent(func,data)
        else
            EventListener.Get(obj):SetOnClickEvent(func)
        end
    end
end

function newObject(prefab, parentTransform, worldpositionStay, setactive)
    worldpositionStay = worldpositionStay or false

    local obj = Object.Instantiate(prefab, parentTransform, worldpositionStay)
    if obj.SetActive and type(setactive) == "boolean" then
        obj:SetActive(setactive)
    end
    return obj
end

function cloneObject(obj)
    return Object.Instantiate(obj, obj.transform.parent);
end

function clearChildren(parent, ignore)
    local count = parent.transform.childCount
    for i = count,1,-1 do
        local child = parent.transform:GetChild(i - 1)
        if (child ~= ignore.transform)then
            destroy(child.gameObject)
        end
    end
end

function clearChildrenByName(parent, name)
    local count = parent.transform.childCount
    for i = count,1,-1 do
        local child = parent.transform:GetChild(i - 1)
        if (child.name == name)then
            destroy(child.gameObject)
        end
    end
end

function childTransform(transform, str)
    return transform:Find(str)
end

function childGameObject(transform, str)
    local trans = transform:Find(str)
    if trans then
        return trans.gameObject
    end
    return nil
end

function subGetComponent(transform, nodeName, componentName)  
    local t = childTransform(transform, nodeName)  
    return t and t:GetComponent(componentName) or nil
end

function GButton(transform, nodeName)    
    return subGetComponent(transform, nodeName, "Button")
end

function GText(transform, nodeName)    
    return subGetComponent(transform, nodeName, "Text")
end

function GImage(transform, nodeName)    
    return subGetComponent(transform, nodeName, "Image")
end

function GSlider(transform, nodeName)    
    return subGetComponent(transform, nodeName, "Slider")
end

function GInput(transform, nodeName)    
    return subGetComponent(transform, nodeName, "InputField")
end

--获取文件路径
function stripExtension(str)
    local idx = str:match(".+()%.%w+$")
    if idx  then
        return str:sub(1, idx-1)
    else
        return str
    end
end

function getFileName(str)
    if not string.find(str, "/") then
        return str
    end
    return str:sub(str:find("/[^/]*$") + 1)
end

--获取路径，不包括文件名
function getPath(str)
    return string.match(str, "(.+)/[^/]*%.%w+$")
end

--获取扩展名
function getExtension(str)
    return str:match(".+%.(%w+)$")
end

--color like 3DE90F or Color.green or #3DE90F
function ToColor(color_or_hex)
    if color_or_hex.r then
        return color_or_hex
    end
    local start = 1
    if string.byte(color_or_hex, 1, 1) == 35 then --'#'
        start = start + 1
    end
	local s
	s=string.sub(color_or_hex,start,start+1)
    local r=tonumber(s,16)/255
    start = start + 2
	s=string.sub(color_or_hex,start,start+1)
    local g=tonumber(s,16)/255
    start = start + 2
	s=string.sub(color_or_hex,start,start+1)
    local b=tonumber(s,16)/255
    start = start + 2
    s=string.sub(color_or_hex,start,start+1)
    local v = tonumber(s,16)
    local a=v and v / 255 or nil
	return Color.New(r, g, b, a)
end

function IsNil(unityObject)
    return unityObject==nil or tolua.isnull(unityObject)
end

function tableToString(obj,max_level,tight)
    max_level = max_level or 32

    local getIndent, quoteStr, wrapKey, wrapVal, isArray, dumpObj
    getIndent = function(level)
        if tight then
            return ""
        else
            return string.rep("  ", level)
        end
    end
    quoteStr = function(str)
        str = string.gsub(str, "[%c\\\"]", {
            ["\t"] = "\\t",
            ["\r"] = "\\r",
            ["\n"] = "\\n",
            ["\""] = "\\\"",
            ["\\"] = "\\\\",
        })
        return '"' .. str .. '"'
    end
    wrapKey = function(val)
        if type(val) == "number" then
            return "  [" .. val .. "]"
        elseif type(val) == "string" then
            return "  [" .. quoteStr(val) .. "]"
        else
            return "  [" .. tostring(val) .. "]"
        end
    end
    wrapVal = function(val, level)
        if type(val) == "table" then
            if level > max_level then
                return tostring(val)
            else
                return dumpObj(val, level)
            end
        elseif type(val) == "number" then
            return val
        elseif type(val) == "string" then
            return quoteStr(val)
        else
            return tostring(val)
        end
    end
    local isArray = function(arr)
        local count = 0
        for k, v in pairs(arr) do
            count = count + 1
        end
        for i = 1, count do
            if arr[i] == nil then
                return false
            end
        end
        return true, count
    end
    dumpObj = function(obj, level)
        if type(obj) ~= "table" then
            return wrapVal(obj,level)
        end
        level = level + 1
        local addstr = ""
        for i = 1, level do
            addstr = addstr .. "  "
        end
        local tokens = {}
        tokens[#tokens + 1] = string.sub(addstr, 0, -6) .. "{"
        local ret, count = isArray(obj)
        if ret then
            for i = 1, count do
                tokens[#tokens + 1] = addstr .. getIndent(level) .. wrapVal(obj[i], level) .. ","
            end
        else
            for k, v in pairs(obj) do
                tokens[#tokens + 1] = addstr .. getIndent(level) .. wrapKey(k) .. "=" .. wrapVal(v, level) .. ","
            end
        end
        tokens[#tokens + 1] = addstr .. getIndent(level - 1) .. "}"
        if tight then
            return tconcat(tokens, " ")
        else
            return tconcat(tokens, "\n")
        end
    end

    return dumpObj(obj, 0)
end

function logTable(obj, prefix, max_level)
    if log_enable == false or log_level ~= 1 then
        return
    end

    if prefix then
        prefix = tostring(prefix)
        Debugger.Log(string.format("%s\n%s", prefix, tableToString(obj, max_level)))
    else
        Debugger.Log(tableToString(obj, max_level))
    end
end

-- string extend function
function string.trim(str)
    return str:gsub("^%s*(.-)%s*$", "%1")
end

function string.split(str, sep)
    local sep, fields = sep or ",", {}
    local pattern = string.format("([^%s]+)", sep)
    str:gsub(pattern, function(c) tinsert(fields, c) end)
    return fields
end

function string.endWith(self, str)
    local l1 = #self
    local l2 = #str
    if (l2 > l1) then return false end

    return string.match(string.sub(self, l1 - l2 + 1, l1), str) ~= nil
end

function string.first(str)
    if str == nil or str == "" then
        return ""
    end
    if #str >= 3 then
        local one = string.byte(str, 1, 1) 
        local two = string.byte(str, 2, 2)
        local three = string.byte(str, 2, 2)
        if one >= 228 and one <= 233 
        and two >= 128 and two <= 190 
        and three >=128 and three <= 190 then
            return string.sub(str, 1, 3)
        end
    end
    return string.sub(str, 1, 1)
end

-- 计算utf8字符串字符数, 各种字符都按一个字符计算
-- 例如utf8len("1你好") => 3
function string.len_utf8(str)
    local newstr, len_ch = string.gsub(str, '[\128-\255][\128-\255][\128-\255]', ' ')
    local len = #newstr
    -- c*3+e = #str
    -- c+e = #newstr
    -- local len_ch = (#str - len) / 2
    return len, len_ch, len - len_ch
end

-- table extend function

function table.find(self, element)
    if self == nil then
        return false
    end

    for i, value in ipairs(self) do
        if value == element then
            return i
        end
    end
    return false
end

function table.containsValue(self, element)
    if self == nil then
        return false
    end

    for _, value in pairs(self) do
        if value == element then
            return true
        end
    end
    return false
end

function table.containsKey(self, element)
    if self == nil then
        return false
    end

    for k, _ in pairs(self) do
        if k == element then
            return true
        end
    end
    return false
end

-- input table must be array
function table.removeRange(self, idx, count)
    if count <= 0 then return end

    local len = #self
    local endidx = idx + count - 1
    if (endidx > len) then
        error("table_removeRange out of range")
    end


    for i = idx, len - count do
        self[i] = self[i + count]
    end

    for i = len - count + 1, len do
        self[i] = nil
    end
end

function table.copy(from, to)
    local k = 0
    for i, v in ipairs(from) do
        to[i] = v
        k = i
    end
    for j = k+1, #to do
        to[j] = nil
    end
end

function table.swap(t, i, j)
    t[i], t[j] = t[j], t[i]
end

function table.clone(t)
    local lookup_table = {}
    local function _copy(t)
        if type(t) ~= "table" then
            return t
        elseif lookup_table[t] then
            return lookup_table[t]
        end
        local newt = {}
        lookup_table[t] = newt
        for key, value in pairs(t) do
            newt[_copy(key)] = _copy(value)
        end
        return setmetatable(newt, getmetatable(t))
    end
    return _copy(t)
end

function table.clone2(st)
    local tab = {}
    for k, v in pairs(st or {}) do
        if type(v) ~= "table" then
            tab[k] = v
        else
            tab[k] = table.clone2(v)
        end
    end
    return tab
end

function table.getCount(self)
    local count = 0

    for k, v in pairs(self) do
        count = count + 1
    end

    return count
end

function table.indexof(self, obj)
    for i, v in ipairs(self) do
        if v == obj then
            return i
        end
    end
end

function table.dumpCSArray(self)
    print("------------dumpArray")
    local len = self.Length
    for i = 0, len - 1 do
        Debugger.Log(string.format("Key: %d, Value: %s", i, tostring(self[i])))
    end
    print("------------dumpArray  end")
end

function table.dumpCSList(self)
    print("------------dumpList")
    local len = self.Count
    for i = 0, len - 1 do
        Debugger.Log(string.format("Key: %d, Value: %s", i, tostring(self[i])))
    end
    print("------------dumpList  end")
end

function table.removeItem(list, item, removeAll)
    local any = false
    if not list or type(list) ~= "table" then
        return
    end

    local rmCount = 0
    for i = 1, #list do
        if list[i - rmCount] == item then
            table.remove(list, i - rmCount)
            any = true
            if removeAll then
                rmCount = rmCount + 1
            else
                break
            end
        end
    end
    return any
end

function table.clear(t)
    for i, _ in ipairs(t) do
        t[i] = nil
    end
end

function table.reverse(list)
    local result = {}
    for i = #list, 1, -1 do
        tinsert(result, list[i])
    end
    return result
end

function table.reverse2(list)
    local n = #list
    local m = math.div(n, 2)
    for i = 1, m do
        list[i], list[n - i + 1] = list[n - i + 1], list[i]
    end
    return list
end

function table.fromCSList(list)
    local result = {}
    local len = list.Count - 1
    for i = 0, len do
        tinsert(result, list[i])
    end
    return result
end

function table.fromCSArray(a)
    local result = {}
    local len = a.Length - 1
    for i = 0, len do
        tinsert(result, a[i])
    end
    return result
end

function table.sortedPairs(t, comparor)
    local a = {}
    for n in pairs(t) do
        a[#a + 1] = n
    end
    table.sort(a, comparor)
    local i = 0
    return function()
        i = i + 1
        return a[i], t[a[i]]
    end
end

function math.div(x, y)
    return math.floor((x) / (y))
end

function math.mode(x, y)
    return x - math.div(x, y) * y
end

-- saturate(x)
-- Returns x, clamped to the range [0,1]
function math.clamp01(x)
    if x < 0 then return 0 end
    if x > 1 then return 1 end
    return x
end

function math.clamp(x, min, max)
    if x < min then return min end
    if x > max then return max end
    return x
end

function math.smoothstep(from, to, t)
    t = math.clamp01(t);
    t = -2 * t * t * t + 3 * t * t;
    return to * t + from * (1 - t);
end

function __setVector2Transform(transform, name, x, y, z)
    local pos = transform[name]
    if not pos then
        return
    end
    local newPos = Vector3.New(x or pos.x, y or pos.y, z or pos.z)
    transform[name] = newPos
    return newPos
end

function setXYZ(transform, x, y, z)
    return __setVector2Transform(transform,"position",x,y,z)
end

function setYZ(transform, y, z)
    return __setVector2Transform(transform,"position",nil,y,z)
end

function setZ(transform, z)
    return __setVector2Transform(transform,"position",nil,nil,z)
end

function setAngleXYZ(transform, x, y, z)
    return __setVector2Transform(transform,"eulerAngles",x,y,z)
end

function setAngleYZ(transform, y, z)
    return __setVector2Transform(transform,"eulerAngles",nil,y,z)
end

function setAngleZ(transform, z)
    return __setVector2Transform(transform,"eulerAngles",nil,nil,z)
end

function setLocalXYZ(transform, x, y, z)
    return __setVector2Transform(transform,"localPosition",x,y,z)
end

function setLocalYZ(transform, y, z)
    return __setVector2Transform(transform,"localPosition",nil,y,z)
end

function setLocalZ(transform, z)
    return __setVector2Transform(transform,"localPosition",nil,nil,z)
end

function setScaleXYZ(transform, x, y, z)
    return __setVector2Transform(transform,"localScale",x,y,z)
end

function setScaleYZ(transform, y, z)
    return __setVector2Transform(transform,"localScale",nil,y,z)
end

function setScaleZ(transform, z)
    return __setVector2Transform(transform,"localScale",nil,nil,z)
end

function setAnchorXY(transform, x, y)
    return __setVector2Transform(transform,"anchoredPosition",x,y,nil)
end

function setAnchorY(transform, y)
    return __setVector2Transform(transform,"anchoredPosition",nil,y,nil)
end

function __G__ERRORTRACE(errorMessage)
    logError("----------------------------------------")
    logError("LUA ERROR CATCHED: " .. tostring(errorMessage))
    logError(debug.traceback("", 2))
    logError("----------------------------------------")
end