---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by chaoguan.
--- DateTime: 2020/11/23 18:52
---

---@class TaskArrayVarConfig
---@field value any
---@field sharedKey string

---@class TaskVarConfig
---@field key string
---@field value any|TaskArrayVarConfig|TaskArrayVarConfig[]
---@field type AIVarType
---@field arrayType AIArrayType
---@field isShared boolean
---@field sharedKey string

---@class TreeVarConfig
---@field type AIVarType
---@field key string
---@field value any|any[]

---@class TaskConfig
---@field hashID number
---@field pathID number 路径
---@field vars TaskVarConfig[] 变量
---@field refTask boolean
---@field abortType AIAbortType

---@class TaskRelationConfig
---@field task TaskConfig 节点
---@field children TaskConfig[] 叶子节点

---@class TreeConfig
---@field name string 名称
---@field pathPrefix string 路径前缀
---@field tree TreeRuntimeConfig
---@field paths table<number,string>

---@class TreeRuntimeConfig
---@field tickInterval number tick间隔
---@field pauseWhenComplete boolean 完成暂停
---@field resetValuesOnRestart boolean 重启时重置数据
---@field vars TreeVarConfig[] 树变量
---@field entry TaskRelationConfig 树节点

---@class TaskEditorConfig
---@field pathID number 路径
---@field offset number
---@field comment string
---@field expanded boolean

---@class TreeEditorConfig
---@field tasks TaskEditorConfig[]
---@field trees TreeConfig[] 树节点

---AIUtil
require("AIDesigner.Base.AIUtil")
---AIDefine
require("AIDesigner.Base.AIDefine")
---@type AIVar
local AIVar = require("AIDesigner.Base.AIVar").AIVar
---@type AIArrayVar
local AIArrayVar = require("AIDesigner.Base.AIVar").AIArrayVar
---@type AITicker
local AITaskTicker = require("AIDesigner.Base.AITask").AITicker
---@type AITimer
local AITimer = require("AIDesigner.Base.AITimer")
---@type AIBlockingUpFacade
local AIBlockingUp = require("AIDesigner.Base.AIBlockingUp")

---@type number
local _uniqueID = 0
---@return number
local UniqueID = function()
    _uniqueID = _uniqueID + 1
    return _uniqueID
end

---@class AITree:Class
---@field _insID number
---@field _config TreeConfig
---@field _context AIContext
---@field _vars table<string, AIVar>
---@field _trigger any
---@field _entry AITask
---@field _start boolean
---@field _pause boolean
---@field _awake boolean
---@field _interrupted boolean
local AITree = AIUtil.Class("AITree")
---@param config TreeConfig
---@param context AIContext
function AITree:ctor(config, context)
    self._insID = UniqueID()
    self._config = config
    self._context = context
    self._trigger = nil
    self._vars = self:_LoadTreeVars(config.tree.vars)
    self._entry = self:_LoadTask(config.tree.entry, config.paths)
    self._timer = AITimer.new()
    self._taskTicker = AITaskTicker.new()
    self._blockingUp = AIBlockingUp.new(self)
    self._interrupted = false
    self._start = false
    self._pause = false
    self._awake = false
    self.__onAddRefTree = nil
    self.__onStateUpdate = nil
    self.__dontShowOnEditor = nil
end
---@return number
function AITree:GetName()
    return self._config.name
end
---@return number
function AITree:GetInsID()
    return self._insID
end
---@return AITimer
function AITree:GetTimer()
    return self._timer
end
---@return AIContext
function AITree:GetContext()
    return self._context
end
---@return AITicker
function AITree:GetTaskTicker()
    return self._taskTicker
end
---@return boolean
function AITree:IsRunning()
    return self._start and not self._pause
end
function AITree:Start()
    if self._start then
        return
    end
    if not self._awake then
        self:_AwakeChild(self._entry)
    end
    self._awake = true
    self._start = true
    self._pause = false
    self._tickValue = 0
    self._blockingUp:Clear()
    AIUtil.TryCall(self.__onStateUpdate, self._start, self._pause)
end
function AITree:Restart()
    if self._start then
        self:Ended()
    end

    if self._config.tree.resetValuesOnRestart then
        for _, var in ipairs(self._vars) do
            var:ResetDefault()
        end
    end

    self:Start()
end
function AITree:Ended()
    if not self._start then
        return
    end
    self:_ResetChild(self._entry)
    self._start = false
    self._pause = false
    self._blockingUp:Clear()
    AIUtil.TryCall(self.__onStateUpdate, self._start, self._pause)
end
function AITree:Destroy()
    if not self._awake then
        return
    end
    self:_DestroyChild(self._entry)
end
function AITree:Interrupt()
    if not self._awake then
        return
    end
    self._tickValue = self._config.tree.tickInterval or 0
    self._interrupted = true
    self._blockingUp:Clear()
end
---@param paused boolean
function AITree:Pause(paused)
    if not self._start or self._pause == paused then
        return
    end
    self:_PauseChild(self._entry, paused)
    self._pause = paused
    AIUtil.TryCall(self.__onStateUpdate, self._start, self._pause)
end
function AITree:Tick()
    if not self._start or self._pause then
        return
    end

    if self._tickValue > 0 then
        self._tickValue = self._tickValue - 1
        return
    end
    self._tickValue = self._config.tickInterval or 0
    self:_TickTree()
end
function AITree:ForceTick()
    if not self._start then
        return
    end

    self:Interrupt()
    self:_TickTree()
end
function AITree:LateUpdate()
    if not self._start or self._pause then
        return
    end
    ---note:目前没有LateUpdate的需求，出于性能考虑，此处注释
    ---self._taskTicker:LateUpdate(self._entry)
end
---@param msg string
function AITree:Log(msg, ...)
    if self._context.logLevel == AILogLevel.Log or self._context.logLevel == AILogLevel.All then
        Debug.LogFormat("[AITree:%s] %s", self:GetName(), string.format(msg, ...))
    end
end
---@param msg string
function AITree:Warn(msg, ...)
    if self._context.logLevel == AILogLevel.Warn or self._context.logLevel == AILogLevel.All then
        Debug.LogWarningFormat("[AITree:%s] %s", self:GetName(), string.format(msg, ...))
    end
end
---@param msg string
function AITree:Error(msg, ...)
    if self._context.logLevel == AILogLevel.Error or self._context.logLevel == AILogLevel.All then
        Debug.LogErrorFormat("[AITree:%s] %s", self:GetName(), string.format(msg, ...))
    end
end
---@param entry AITask
---@param task AITask
function AITree:AddBlockingUpTask(entry, task)
    if not task or task.tree ~= self then
        return
    end
    self._blockingUp:AddTask(entry, task)
end
---@return AIVar[]
function AITree:GetVariables()
    return self._vars
end
---@param key string
function AITree:GetVariable(key)
    if not self._vars[key] then
        return nil
    end
    return self._vars[key]:GetValue()
end
---@param key string
---@param value any
function AITree:SetVariable(key, value)
    if not self._vars[key] then
        return
    end
    self._vars[key]:SetValue(value)
end
---@param key string
---@return boolean
function AITree:ContainVariable(key)
    if not key then
        return false
    end
    if self._vars[key] then
        return true
    else
        return false
    end
end
---@param trigger any
function AITree:SetTrigger(trigger)
    self._trigger = trigger
end
---@return any
function AITree:GetTrigger()
    return self._trigger
end
---@param debugID string
function AITree:GetTaskByDebugID(debugID)
    local result = nil
    local findTask = function(task)
        if result or task.debugID ~= debugID then
            return
        end
        result = task
    end
    self:_RecursionTask(self._entry, findTask)
    return result
end
---@param key string
---@param type AIVarType
---@param value any
function AITree:AddVariable(key, type, value)
    if self._vars[key] then
        if type == self._vars[key]:GetType() then
            self._vars[key]:SetValue(value)
        else
            self:Error("[AddVariable type error][key:%s]", key)
        end
    else
        self._vars[key] = AIVar.new(key, type, value)
    end
end
---@param parent AITask
---@param refTreeName String
function AITree:LoadRefTree(parent, refTreeName)
    if not parent or AIUtil.StringIsNullOrEmpty(refTreeName) then
        return nil
    end
    ---@type boolean|TreeConfig
    local result, config = pcall(require, self._config.pathPrefix .. refTreeName)
    if not result or not config then
        self:Error(string.format("Ref Tree 不存在：名称(%s)", refTreeName))
        return nil
    end
    ---@type TreeRuntimeConfig
    local refTreeConfig = config.tree
    if not refTreeConfig.entry or not refTreeConfig.entry.children or not refTreeConfig.entry.children[1] then
        return nil
    end
    self._vars = self:_LoadTreeVars(refTreeConfig.vars, self._vars)
    ---@type AITask
    local child = self:_LoadTask(refTreeConfig.entry.children[1], config.paths, parent)
    if child then
        --- dynamic tree mark!!
        child.___refTreeName = refTreeName
        table.insert(parent.children, child)
        AIUtil.TryCall(self.__onAddRefTree, child, true)
    end
    return child
end
---@param rootTask AITask
function AITree:RemoveRefTree(rootTask)
    if not rootTask or rootTask.parent then
        return
    end
    for index, child in ipairs(rootTask.parent.children) do
        if child == rootTask then
            child.___refTreeName = nil
            table.remove(rootTask.parent.children, index)
            AIUtil.TryCall(self.__onAddRefTree, rootTask, false)
            break
        end
    end
end
function AITree:_TickTree()
    self._interrupted = false
    self:_TickTimer()

    if self._blockingUp:IsRunning() then
        self._blockingUp:Tick()
    else
        self._taskTicker:Tick(self._entry)
    end
    self._blockingUp:PostProcess()
    self._trigger = nil

    if self._config.pauseWhenComplete and not self._blockingUp:IsRunning() then
        self:Pause(true)
    end
end
function AITree:_TickTimer()
    if not self._lastRealtime then
        self._lastRealtime = self._context:GetRealtime()
    else
        local realtime = self._context:GetRealtime()
        if realtime then
            self._timer:Tick(realtime - self._lastRealtime)
            self._lastRealtime = realtime
        end
    end
end
---@param task AITask
function AITree:_AwakeChild(task)
    if not task then
        return
    end
    task:OnAwake()
    --task:Log("[OnAwake]")
    for _, child in ipairs(task.children) do
        self:_AwakeChild(child)
    end
end
---@param task AITask
function AITree:_PauseChild(task, paused)
    if not task then
        return
    end
    task:OnPause(paused)
    --task:Log("[OnPause:%s]", paused)
    for _, child in ipairs(task.children) do
        self:_PauseChild(child, paused)
    end
end
---@param task AITask
function AITree:_ResetChild(task)
    if not task then
        return
    end
    task:OnReset()
    --task:Log("[OnReset]")
    for _, child in ipairs(task.children) do
        self:_ResetChild(child)
    end
end
---@param task AITask
function AITree:_DestroyChild(task)
    if not task then
        return
    end
    task:OnDestroy()
    --task:Log("[OnDestroy]")
    for _, child in ipairs(task.children) do
        self:_DestroyChild(child)
    end
end
---@param task AITask
---@param callBack fun(task:AITask)
function AITree:_RecursionTask(task, callBack)
    if not task then
        return
    end
    if callBack then
        pcall(callBack, task)
    end
    if task.children then
        for _, child in ipairs(task.children) do
            self:_RecursionTask(child, callBack)
        end
    end
end
---@param config TreeVarConfig[]
function AITree:_LoadTreeVars(config, vars)
    if not vars then
        vars = {}
    end
    if not config then
        return vars
    end
    for _, varConfig in ipairs(config) do
        if not vars[varConfig.key] then
            vars[varConfig.key] = self._context:GetVariable(varConfig.key)
            if not vars[varConfig.key] then
                if varConfig.arrayType then
                    local arrayVar = AIArrayVar.new(varConfig.key, varConfig.type, {})
                    for _, value in ipairs(varConfig.value) do
                        arrayVar:AddChildValue(self._context:ParseVarValue(varConfig.type, value))
                    end
                    vars[varConfig.key] = arrayVar
                else
                    vars[varConfig.key] = AIVar.new(varConfig.key, varConfig.type, self._context:ParseVarValue(varConfig.type, varConfig.value), false)
                end
            elseif varConfig.type ~= vars[varConfig.key]:GetType() then
                self:Error(string.format("[Load Tree var type error] [external incoming variable type exception!] [key:%s] [type:%s]", varConfig.key, varConfig.type))
            end
        elseif varConfig.type ~= vars[varConfig.key]:GetType() then
            self:Error(string.format("[Load Tree var type error] [inconsistent with the main tree variable type!] [key:%s] [type:%s]", varConfig.key, varConfig.type))
        end
    end
    return vars
end
---@param task AITask
---@param config TaskVarConfig
function AITree:_LoadTaskVars(task, config)
    if not task or not config then
        return
    end
    if config.arrayType then
        if config.isShared then
            if config.sharedKey then
                ---@type AIArrayVar
                local sharedVar = self._vars[config.sharedKey]
                if sharedVar and sharedVar:IsArray() then
                    task[config.key] = config.arrayType == AIArrayType.Shared and sharedVar or sharedVar:GetValue()
                end
            end
            if not task[config.key] then
                local arrayVar = nil
                if config.arrayType == AIArrayType.Shared then
                    task[config.key] = AIArrayVar.new(config.key, config.type, {})
                    arrayVar = task[config.key]:GetValue()
                else
                    task[config.key] = {}
                    arrayVar = task[config.key]
                end
                for _, value in ipairs(config.value) do
                    local tab = nil
                    if value.sharedKey then
                        tab = self._vars[value.sharedKey]
                    end
                    if not tab then
                        tab = AIVar.new(config.key, config.type, self._context:ParseVarValue(config.type, value.value))
                    end
                    table.insert(arrayVar, tab)
                end
            end
        else
            task[config.key] = {}
            for _, value in ipairs(config.value) do
                table.insert(task[config.key], value)
            end
        end
    else
        if config.isShared then
            if config.sharedKey then
                task[config.key] = self._vars[config.sharedKey]
            elseif task[config.key] then
                task[config.key]:SetValue(self._context:ParseVarValue(config.type, config.value))
            end
            if not task[config.key] then
                task[config.key] = AIVar.new(config.key, config.type, self._context:ParseVarValue(config.type, config.value))
            end
        else
            task[config.key] = self._context:ParseVarValue(config.type, config.value)
        end
    end
end
---@param config TaskRelationConfig
---@param parent AITask
---@param paths table<number,string>
function AITree:_LoadTask(config, paths, parent)
    if not config then
        return nil
    end

    local path = paths[config.task.pathID]
    if AIUtil.StringIsNullOrEmpty(path) then
        return nil
    end

    ---@type boolean|AITask
    local result, taskClass = pcall(require, path)
    if not result or not taskClass then
        self:Error(string.format("Task文件路径不存在：文件路径=%s", path))
        return nil
    end

    ---@type AITask
    local task = taskClass.new()
    task.name = path
    task.disabled = config.task.disabled
    task.config = config
    task.tree = self
    task.view = self._context.view
    task.master = self._context.master
    task.context = self._context
    task.parent = parent
    task.children = {}

    if task.type == AITaskType.Composite then
        task.abortType = config.task.abortType or AIAbortType.None
    end
    if config.task.vars then
        for _, varConfig in ipairs(config.task.vars) do
            self:_LoadTaskVars(task, varConfig)
        end
    end
    if config.children then
        for _, childConfig in ipairs(config.children) do
            local child = self:_LoadTask(childConfig, paths, task)
            if child then
                table.insert(task.children, child)
            end
        end
    end

    --- reference tree task
    if config.task.refTask and config.task.vars then
        ---@type TaskVarConfig
        local refTreeVar = config.task.vars[1]
        if not refTreeVar or AIUtil.StringIsNullOrEmpty(refTreeVar.value) then
            return nil
        end
        if not self:LoadRefTree(task, refTreeVar.value) then
            return nil
        end
    end

    return task
end
return AITree