---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by chaoguan.
--- DateTime: 2021/7/29 11:47
---

AIUtil = {}

---清理table
---@param t table
function table.clear(t)
    if not t then
        return
    end
    for k, _ in pairs(t) do
        t[k] = nil
    end
end

---@return table
function AIUtil.Split(str, reps)
    local tab = {}
    string.gsub(str, '[^' .. reps .. ']+', function(w)
        table.insert(tab, w)
    end)
    return tab
end

---@param str string
---@return boolean
function AIUtil.StringIsNullOrEmpty(str)
    if not str or "" == str then
        return true
    else
        return false
    end
end

---@param func fun()
function AIUtil.TryCall(func, ...)
    if func then
        func(...)
    end
end

---@param func fun()
function AIUtil.TryPCall(func, ...)
    if func then
        pcall(func, ...)
    end
end

---@class Class
---@field super Class
---@field __cname string 类名
---@field new fun():void 创建接口
---@field ctor fun(self:Class):void 构造函数
---@field discard fun(self:Class):void 回收到对象池中
---@field protected onDiscard fun(self:Class):void 回收时的回调
function AIUtil.Class(classname, super, enableCache, cacheCount)
    local superType = type(super)
    local cls

    if superType ~= "function" and superType ~= "table" then
        superType = nil
        super = nil
    end

    if superType == "function" or (super and super.__ctype == 1) then
        -- inherited from native C++ Object
        cls = {}

        if superType == "table" then
            -- copy fields from super
            for k, v in pairs(super) do
                cls[k] = v
            end
            cls.__create = super.__create
            cls.super = super
        else
            cls.__create = super
            cls.ctor = function()
            end
        end

        cls.__cname = classname
        cls.__ctype = 1

        function cls.new(...)
            local instance = cls.__create(...)
            -- copy fields from class to native object
            for k, v in pairs(cls) do
                instance[k] = v
            end
            instance.class = cls
            instance:ctor(...)
            return instance
        end

    else
        cls = {}
        cls.__enableCache = enableCache

        function cls.__new(...)
            return setmetatable({}, cls)
        end

        function cls.new(...)
            local instance = nil
            if cls.__enableCache then
                --instance = g_LuaPool:GetByClass(cls)
            else
                instance = cls.__new(...)
            end

            instance:ctor(...)
            return instance
        end
        cls.New = cls.new

        ---当类直接当函数调用时，类会被作为第一个参数传递，所以要单独写一个
        function cls.newForCall(self, ...)
            return cls.new(...)
        end

        ---对象丢弃接口，即回收回对象池
        ---@param self Class
        function cls.discard(self)
            if not cls.__enableCache then
                return
            end
            if self.onDiscard then
                self:onDiscard()
            end
            --g_LuaPool:Recycle(self)
        end

        -- inherited from Lua Object
        if super then
            setmetatable(cls, { __index = super })
            cls.super = super
        else
            setmetatable(cls, { __call = cls.newForCall })
            cls.ctor = function()
            end
        end

        cls.__cname = classname
        cls.__ctype = 2 -- lua
        cls.__index = cls
        cls.__call = cls.newForCall

        if enableCache then
            --g_LuaPool:RegisterByClass(cls, cacheCount)
            --g_LuaPool:TryPreload(classname)
        end
    end
    return cls
end