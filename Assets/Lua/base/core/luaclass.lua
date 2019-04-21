local handler_cache = setmetatable({}, {__mode = "k"})

_setmetatableindex = function(t, index)
    if type(t) == "userdata" then
        local peer = tolua.getpeer(t)
        if not peer then
            peer = {}
            tolua.setpeer(t, peer)
        end
        _setmetatableindex(peer, index)
    else
        local mt = getmetatable(t)
        if not mt then mt = {} end
        if not mt.__index then
            mt.__index = index
            setmetatable(t, mt)
        elseif mt.__index ~= index then
            _setmetatableindex(mt, index)
        end
    end
end

--bind function to a object.
function handler(obj, func)
    if not func then
        return nil
    end
    local cached = handler_cache[func]
    if not cached then
        cached = {}
        handler_cache[func] = cached
    end
    local retfunc = cached[obj]
    if not retfunc then
        retfunc = function(...)
            func(obj, ...)
        end
        cached[obj] = retfunc
    end
    return retfunc
end

function class(classname, ...) --参数一：所要创建的类名，参数二：可选参数，可以使function，也可以是table，userdata等
    local cls = {__cname = classname}
    local supers = {...}
    for _, super in ipairs(supers) do --遍历可选参数
        local superType = type(super)
          if superType == "function" then
            --如果是个function，那么就让cls的create方法指向他
            cls.__create = super
        elseif superType == "table" then --如果是个table
            if super[".isclass"] then
                 cls.__create = function() return super:create() end
            else
                -- 如果是个纯lua类，自己定义的那种，比如a={}
                cls.__supers = cls.__supers or {}
                cls.__supers[#cls.__supers + 1] = super--不断加到__supers的数组中
                if not cls.super then
                    cls.super = super
                end
            end
        
        end
    end

    cls.__index = cls
    if not cls.__supers or #cls.__supers == 1 then
        setmetatable(cls, {__index = cls.super})
    else
        setmetatable(cls, {__index = function(_, key)
            local supers = cls.__supers
            for i = 1, #supers do
                local super = supers[i]
                if super[key] then return super[key] end
            end
        end})
    end

    if not cls.ctor then
        -- 增加一个默认构造函数
        cls.ctor = function() end
    end
    cls.new = function(...) --新建方法，这个也是比较重要的方法
        local instance
        if cls.__create then 
            --如果有create方法，那么就调用，正常情况下，自定义的cls是没有create方法的。
            --会不断的向上寻找元类的index，直到找到原生如sprite，然后调用sprite:create()
            --返回一个原生对象，通过调试代码，可以得出这些
            
            instance = cls.__create(...)
        else
            instance = {}--没有，说明根目录是普通类
        end
        --这个方法也比较关键，设置instance的元类index，谁调用new了，就把他设置为instance的元类index
        --具体可以看代码
        _setmetatableindex(instance, cls)
        instance.class = cls
        instance:ctor(...)--调用构造函数
        return instance
    end
    -- cls.create = function(_, ...)
    --     return cls.new(...)
    -- end
    cls.classname = function()
        return cls.__cname
    end

    return cls
end