﻿---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by chaoguan.
--- DateTime: 2021/7/20 12:49
---

local String = CS.System.String
local Convert = CS.System.Convert
local AIDefine = CS.AIDesigner.Define
local AIDesignerLogicUtility = CS.AIDesigner.AIDesignerLogicUtility
local AITreeRuntime = CS.AIDesigner.TreeRuntime
local AIArrayType = CS.AIDesigner.ArrayType

local AITreeMgr = require(AIDesignerLogicUtility.LuaFilePathLegalization(AIDefine.CustomSettings.AITreeCenterPath))
local AIVar = require(AIDesignerLogicUtility.LuaFilePathLegalization(AIDefine.CustomSettings.AIVarPath)).AIVar
local AIArrayVar = require(AIDesignerLogicUtility.LuaFilePathLegalization(AIDefine.CustomSettings.AIVarPath)).AIArrayVar

local Find = {}

---@param callBack function
function Find.AllTrees (callBack)
    local tab = {}
    for _, tree in pairs(AITreeMgr._trees) do
        if not tree.__dontShowOnEditor then
            local name = tree:GetName()
            local host = nil
            local path = tree:GetInsID() .. '/' .. name
            table.insert(tab, AITreeRuntime(host, path, name, tree))
        end
    end

    pcall(callBack, tab)
end

local Variable = {}

---@param value any unity类型转lua类型
function Variable._GetLuaValue(type, value)
    if not value then
        return nil
    end

    if type == AIVarType.None then
        return nil
    elseif type == AIVarType.Float then
        return value
    elseif type == AIVarType.Int then
        return value
    elseif type == AIVarType.String then
        return value
    elseif type == AIVarType.Boolean then
        return value
    elseif type == AIVarType.Object then
        return value
    elseif type == AIVarType.Vector2 then
        return value
    elseif type == AIVarType.Vector2Int then
        return value
    elseif type == AIVarType.Vector3 then
        return value
    elseif type == AIVarType.Vector3Int then
        return value
    elseif type == AIVarType.Vector4 then
        return value
    end
end

---@param value any lua类型转unity类型
function Variable._GetUnityValue(type, value)
    if not value then
        return nil
    end

    if type == AIVarType.None then
        return nil
    elseif type == AIVarType.Float then
        return value
    elseif type == AIVarType.Int then
        return value
    elseif type == AIVarType.String then
        return value
    elseif type == AIVarType.Boolean then
        return value
    elseif type == AIVarType.Object then
        return value
    elseif type == AIVarType.Vector2 then
        return value
    elseif type == AIVarType.Vector2Int then
        return value
    elseif type == AIVarType.Vector3 then
        return value
    elseif type == AIVarType.Vector3Int then
        return value
    elseif type == AIVarType.Vector4 then
        return value
    end
end

---@param tree AITree
---@param luaVar table|AIArrayVar
---@param csVar CS.AIDesigner.TreeTaskVariable
function Variable._GetArrayValue(tree, luaVar, csVar)
    if not csVar then
        return
    end

    local getArrayFun = function(tree, luaVar, csVar)
        local tab = csVar.ArrayType == AIArrayType.Regular and luaVar or luaVar:GetValue()
        local count = csVar.ArrayVar.Count
        for i = #tab, count + 1, -1 do
            table.remove(tab, i)
        end

        for i = 0, count - 1 do
            tab[i + 1] = Variable._GetSoloValue(tree, tab[i + 1], csVar.ArrayVar[i])
        end

        if csVar.ArrayType == AIArrayType.Regular then
            luaVar = tab
        else
            luaVar:SetValue(tab)
        end

        return luaVar
    end

    local key = csVar.Key
    local type = Convert.ToInt32(csVar.Type)
    if csVar.IsShared then
        local sharedKey = csVar.SharedKey
        if String.IsNullOrEmpty(sharedKey) then
            if not luaVar or tree:ContainVariable(luaVar._key) then
                luaVar = csVar.ArrayType == AIArrayType.Regular and {} or AIArrayVar.new(key, type)
            end
            luaVar = getArrayFun(tree, luaVar, csVar)
        elseif tree._vars[sharedKey] then
            luaVar = csVar.ArrayType == AIArrayType.Regular and tree._vars[sharedKey]:GetValue() or tree._vars[sharedKey]
        end
    else
        luaVar = getArrayFun(tree, luaVar, csVar)
    end

    return luaVar
end

---@param tree AITree
---@param luaVar any|AIVar
---@param csVar CS.AIDesigner.SharedVariable
function Variable._GetSoloValue(tree, luaVar, csVar)
    if not csVar then
        return
    end
    local key = csVar.Key
    local type = Convert.ToInt32(csVar.Type)
    local value = Variable._GetLuaValue(type, csVar.Value)
    if csVar.IsShared then
        local sharedKey = csVar.SharedKey
        if String.IsNullOrEmpty(sharedKey) then
            if not luaVar or tree:ContainVariable(luaVar._key) then
                luaVar = AIVar.new(key, type, value)
            else
                luaVar:SetValue(value)
            end
        elseif tree._vars[sharedKey] then
            luaVar = tree._vars[sharedKey]
        end
    else
        luaVar = value
    end

    return luaVar
end

---@param luaVar AIVar|AIArrayVar
function Variable.GetTreeValue(luaVar, index)
    if not luaVar then
        return nil
    end
    if luaVar:IsArray() then
        ---@type AIVar
        local sub = luaVar:GetChild(index)
        if not sub then
            return nil
        end
        return Variable._GetUnityValue(luaVar:GetType(), sub:GetValue())
    else
        return Variable._GetUnityValue(luaVar:GetType(), luaVar:GetValue())
    end
end

---@param luaVar AIVar|AIArrayVar
---@param csVar CS.AIDesigner.TreeRefVariable
function Variable.SetTreeValue(luaVar, csVar)
    if not luaVar or not csVar then
        return
    end
    if luaVar:IsArray() then
        local size = csVar.ArrayVar.Count
        luaVar:SetSize(size)
        for i = 0, size - 1 do
            local value = Variable._GetLuaValue(luaVar:GetType(), csVar.ArrayVar[i].Value)
            if value then
                luaVar:SetChildValue(i + 1, value)
            end
        end
    else
        local value = Variable._GetLuaValue(luaVar:GetType(), csVar.Value)
        if value then
            luaVar:SetValue(value)
        end
    end
end

---@param tree AITree
---@param debugID string
---@param csVar CS.AIDesigner.TreeTaskVariable
function Variable.SetTaskValue(tree, debugID, csVar)
    if not tree or not csVar then
        return
    end
    ---@type AITask
    local task = tree:GetTaskByDebugID(debugID)
    if not task then
        return
    end
    local key = csVar.Key
    if not task[key] then
        return
    end

    if csVar.IsArray then
        task[key] = Variable._GetArrayValue(tree, task[key], csVar)
    else
        task[key] = Variable._GetSoloValue(tree, task[key], csVar)
    end
end

local Task = {}

---@param tree AITree
---@param debugID string
---@param disabled boolean
function Task.SetDisabled(tree, debugID, disabled)
    if not tree then
        return
    end
    ---@type AITask
    local task = tree:GetTaskByDebugID(debugID)
    if not task then
        return
    end
    task.disabled = disabled
end

---@param tree AITree
---@param debugID string
---@param abortType number
function Task.SetAbortType(tree, debugID, abortType)
    if not tree then
        return
    end
    ---@type AITask
    local task = tree:GetTaskByDebugID(debugID)
    if not task then
        return
    end
    if task.type == AITaskType.Composite then
        task.abortType = Convert.ToInt32(abortType)
    end
end

return {
    AllTrees = Find.AllTrees,
    SetDisabled = Task.SetDisabled,
    SetAbortType = Task.SetAbortType,
    SetTreeValue = Variable.SetTreeValue,
    GetTreeValue = Variable.GetTreeValue,
    SetTaskValue = Variable.SetTaskValue,
}