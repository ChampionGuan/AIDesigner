---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by chaoguan.
--- DateTime: 2020/12/3 20:51
---

---@alias Float number|Fix
---@alias Int number
---@alias String string
---@alias Boolean boolean
---@alias Object any
---@alias Vector2 UnityEngine.Vector2|FVector2
---@alias Vector2Int UnityEngine.Vector2Int|FVector2
---@alias Vector3 UnityEngine.Vector3|FVector3
---@alias Vector3Int UnityEngine.Vector3Int|FVector3
---@alias Vector4 UnityEngine.Vector4|FVector4

---@class AITask:Class
---@field debugID string 调试ID
---@field name string 节点名称
---@field type AITaskType 节点类型
---@field blockingUpType AIBlockingUpType 阻塞类型
---@field disabled boolean 无效标识
---@field tree AITree
---@field view any
---@field master any
---@field context AIContext
---@field parent AITask 父节点
---@field children AITask[] 孩子们节点
local AITask = AIUtil.Class("AITask")

function AITask:ctor()
    self.__onTaskUpdate = nil
end

function AITask:OnAwake()
end

function AITask:OnReset()
end

function AITask:OnEnter()
end

function AITask:OnExit()
end

function AITask:OnPause(paused)
end

function AITask:OnUpdate()
    return AITaskState.Success
end

function AITask:OnLateUpdate()
end

function AITask:OnDestroy()
end

---@param index Int
function AITask:CanRunChild(index)
    return true
end

---@return Int
function AITask:LeftRepetitions()
    return 0
end

function AITask:InternalUpdate()
    --- note: only action and conditional tasks that have running state!
    self.state = self:OnUpdate()
    --- call back
    AIUtil.TryCall(self.__onTaskUpdate, self)
end

---@param treeName String
function AITask:LoadRefTree(treeName)
    return self.tree:LoadRefTree(self, treeName)
end

---@param rootTask AITask
function AITask:RemoveRefTree(rootTask)
    self.tree:RemoveRefTree(rootTask)
end

---@return number,boolean
function AITask:AddTimer(cdMax, cdMin, listener, funcUpdate)
    local timer = self.tree:GetTimer()
    if not timer then
        return nil, false
    end
    return timer:AddTimer(cdMax, cdMin, listener, funcUpdate)
end

---@param id number
function AITask:RemoveTimer(id)
    local timer = self.tree:GetTimer()
    if not timer then
        return
    end
    timer:Discard(id)
end

---@param var AIVar
function AITask:IsNull(var)
    if not var or not var:GetValue() then
        return true
    end
    return false
end

---@param msg string
function AITask:Log(msg, ...)
    if self.context.logLevel == AILogLevel.Log or self.context.logLevel == AILogLevel.All then
        self.tree:Log("[TaskPath:%s][%s]", self:_GetPath(), string.format(msg, ...))
    end
end

---@param msg string
function AITask:Warn(msg, ...)
    if self.context.logLevel == AILogLevel.Warn or self.context.logLevel == AILogLevel.All then
        self.tree:Warn("[TaskPath:%s][%s]", self:_GetPath(), string.format(msg, ...))
    end
end

---@param msg string
function AITask:Error(msg, ...)
    if self.context.logLevel == AILogLevel.Error or self.context.logLevel == AILogLevel.All then
        self.tree:Error("[TaskPath:%s][%s]", self:_GetPath(), string.format(msg, ...))
    end
end

---@return Int
function AITask:_GetSiblingIndex()
    if not self.parent then
        return 0
    end
    for index, task in ipairs(self.parent.children) do
        if task == self then
            return index
        end
    end
    return 0
end

---@return string
function AITask:_GetPath()
    local path = ""
    local parent = self
    while true do
        if not parent then
            break
        end
        if not parent.___name then
            local name = AIUtil.Split(parent.name, ".")
            parent.___name = name[#name]
        end
        if not parent.parent then
            path = parent.___name .. path
        else
            path = string.format("[%s]", parent:_GetSiblingIndex()) .. "/" .. parent.___name .. path
        end
        parent = parent.parent
    end
    return path
end

---@class AITicker:Class
---@field _after AITask
local AITicker = AIUtil.Class("AITicker")

---@param entry AITask
---@param after AITask
function AITicker:Tick(entry, after)
    if not entry then
        return
    end
    self._after = after
    self._entry = entry
    self:_TickTask(entry)
end

---@param entry AITask
function AITicker:LateUpdate(entry)
    if not entry then
        return
    end
    self:_LateUpdateChild(entry)
end

---@param task AITask
function AITicker:_TickTask(task)
    if task.disabled or task.tree._interrupted then
        return
    end

    if not self._after then
        task:OnEnter()
    end

    for index, child in ipairs(task.children) do
        if task:CanRunChild(index) then
            self:_TickTask(child)
        elseif not self._after then
            child.state = nil
        end
        if self._after and self._after == child then
            self._after = nil
        end
    end

    if self._after or task.tree._interrupted then
        return
    end

    --- repeater task
    if task:LeftRepetitions() > 0 then
        task.state = AITaskState.BlockingUp
        task.tree:AddBlockingUpTask(self._entry, task)
        return
    end

    --- internal update
    task:InternalUpdate()
    if task.state == AITaskState.Running then
        task.tree:AddBlockingUpTask(self._entry, task)
    else
        task:OnExit()
    end
end

---@param task AITask
function AITicker:_LateUpdateChild(task)
    if not task then
        return
    end
    task:OnLateUpdate()
    for _, child in ipairs(task.children) do
        self:_LateUpdateChild(child)
    end
end

---@class AIAction:AITask
local AIAction = AIUtil.Class("AIAction", AITask)

function AIAction:ctor()
    AITask.ctor(self)
    self.type = AITaskType.Action
end

---@class AICondition:AITask
local AICondition = AIUtil.Class("AICondition", AITask)

function AICondition:ctor()
    AITask.ctor(self)
    self.type = AITaskType.Condition
end

---@class AIComposite:AITask
---@field abortType AIAbortType
local AIComposite = AIUtil.Class("AIComposite", AITask)

function AIComposite:ctor()
    AITask.ctor(self)
    self.type = AITaskType.Composite
end

---@class AIDecorator:AITask
local AIDecorator = AIUtil.Class("AIDecorator", AITask)

function AIDecorator:ctor()
    AITask.ctor(self)
    self.type = AITaskType.Decorator
end

---@class AIEntry:AITask
local AIEntry = AIUtil.Class("AIEntry", AITask)

function AIEntry:ctor()
    AITask.ctor(self)
    self.type = AITaskType.Entry
end

return
{
    AITicker = AITicker,
    AIEntry = AIEntry,
    AIAction = AIAction,
    AICondition = AICondition,
    AIComposite = AIComposite,
    AIDecorator = AIDecorator,
}

