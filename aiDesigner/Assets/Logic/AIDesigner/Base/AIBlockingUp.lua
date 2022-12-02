﻿---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by chaoguan.
--- DateTime: 2021/12/28 11:34
---

---@class AIBlockingUp
---@field type AIBlockingUpType
---@field facade AIBlockingUpFacade
---@field parent AIBlockingUp
---@field children AIBlockingUp[]
---@field waitingForAdd AIBlockingUp[]
---@field task AITask
---@field entry AITask
---@field after AITask
local AIBlockingUp = AIUtil.Class("AIBlockingUp")

---@param facade AIBlockingUpFacade
---@param type AIBlockingUpType
function AIBlockingUp:ctor(facade, type)
    self.facade = facade
    self.type = type
    self.children = {}
    self.waitingForAdd = {}
end

---@param entry AITask
---@param task AITask
function AIBlockingUp:Init(entry, task)
    self.start, self.task, self.entry, self.after, self.parent = false, task, entry, nil, nil
end

function AIBlockingUp:Destroy()
    local count = #self.waitingForAdd
    for index = count, 1, -1 do
        ---@type AIBlockingUp
        local blockingUp = table.remove(self.waitingForAdd, index)
        blockingUp:Destroy()
    end

    count = #self.children
    for index = count, 1, -1 do
        self.children[index]:Destroy()
    end

    self.facade:Remove(self)
end

---@param blockingUp AIBlockingUp
function AIBlockingUp:AddChild(blockingUp)
    if blockingUp.parent then
        blockingUp.parent:RemoveChild(blockingUp)
    end
    blockingUp.start = false
    blockingUp.parent = self
    table.insert(self.waitingForAdd, 1, blockingUp)
end

---@param blockingUp AIBlockingUp
function AIBlockingUp:RemoveChild(blockingUp)
    local result = false
    for index, tab in ipairs(self.children) do
        if tab == blockingUp then
            result = true
            table.remove(self.children, index)
            break
        end
    end
    for index, tab in ipairs(self.waitingForAdd) do
        if tab == blockingUp then
            result = true
            table.remove(self.waitingForAdd, index)
        end
    end

    if result then
        blockingUp.parent = nil
    end
end

---@return Int
function AIBlockingUp:GetChildCount()
    return #self.waitingForAdd + #self.children
end

function AIBlockingUp:Tick()
    local count = #self.children
    if count > 0 then
        for index = count, 1, -1 do
            self.children[index]:Tick()
        end
    else
        self:OnTick()
    end
end

function AIBlockingUp:PostProcess()
    if self.task and self.task.disabled then
        self:Destroy()
        return
    end

    if not self.start then
        self.start = true
        self.entry, self.after = self:_FindTreeBranch(self.task)

        if not self.entry then
            self:Destroy()
            return
        end
    end

    local count = #self.waitingForAdd
    for index = count, 1, -1 do
        table.insert(self.children, 1, table.remove(self.waitingForAdd, index))
    end

    count = #self.children
    for index = count, 1, -1 do
        self.children[index]:PostProcess()
    end
end

function AIBlockingUp:OnTick()
end

---@param task AITask
---@return AITask, AITask
function AIBlockingUp:_FindTreeBranch(task)
    if not task then
        return nil, nil
    end

    local curr, parent, after, prev, flag = task, task.parent, nil, nil, false
    while true do
        for _, child in ipairs(parent.children) do
            if not child.disabled then
                if flag and not after and not child.state then
                    after = prev
                end
                if flag and (child.state == AITaskState.Running or child.state == AITaskState.BlockingUp) then
                    return curr, after
                end
                if not flag and child == curr then
                    flag = true
                end
                if flag then
                    prev = child
                end
            end
        end

        if self.parent and self.parent.task == parent then
            return curr, after
        end
        if parent == self.entry then
            return parent, after
        end
        if parent.state == AITaskState.Success or parent.state == AITaskState.Failure then
            return nil, nil
        end

        prev = nil
        flag = false
        curr = parent
        parent = parent.parent
    end
end

---@return boolean
function AIBlockingUp:_TickTreeBranch()
    if self.after then
        local task = self.task.parent
        while task ~= self.after.parent and task ~= self.entry do
            task:InternalUpdate()
            task = task.parent
        end
        self.task.tree:GetTaskTicker():Tick(self.entry, self.after)
    end
    self:Destroy()
end

---@class AIRunningAbort:AIBlockingUp
---@field _abortTasks AITask[]
local AIRunningAbort = AIUtil.Class("AIRunningAbort", AIBlockingUp)

---@param entry AITask
---@param task AITask
function AIRunningAbort:Init(entry, task)
    AIBlockingUp.Init(self, entry, task)
    if self._abortTasks then
        table.clear(self._abortTasks)
    else
        self._abortTasks = {}
    end
    self:_CollectAbortTask()
end

function AIRunningAbort:OnTick()
    --- check condition abort tasks
    for _, task in ipairs(self._abortTasks) do
        task:InternalUpdate()
        if task.state ~= AITaskState.Success then
            self.task:OnExit()
            self:_TickTreeBranch()
            return
        end
    end

    --- running update
    self.task:InternalUpdate()
    --- running over note: task update state will interrupt!!
    if self.task and self.task.state ~= AITaskState.Running then
        self.task:OnExit()
        self:_TickTreeBranch()
    end
end

function AIRunningAbort:_CollectAbortTask()
    if not self.task or not self.task.parent then
        return
    end

    --- abort self
    self:_CollectAbortSelfTask(self.task)
    --- abort lower priority
    for _, broth in ipairs(self.task.parent.children) do
        if broth == self.task then
            break
        end
        self:_CollectAbortLowerPriorityTask(broth)
    end
end

---@param task AITask
function AIRunningAbort:_CollectAbortSelfTask(task)
    local parent = task.parent
    if not parent then
        return
    end
    if parent.abortType ~= AIAbortType.Self and parent.abortType ~= AIAbortType.Both then
        self:_CollectAbortSelfTask(parent)
        return
    end
    for _, child in ipairs(parent.children) do
        if child == task then
            break
        end
        if child.type == AITaskType.Condition and child.state == AITaskState.Success then
            table.insert(self._abortTasks, child)
        end
    end
    self:_CollectAbortSelfTask(parent)
end

---@param task AITask
function AIRunningAbort:_CollectAbortLowerPriorityTask(task)
    if task.abortType ~= AIAbortType.LowerPriority and task.abortType ~= AIAbortType.Both then
        return
    end
    for _, child in ipairs(task.children) do
        if child.abortType == AIAbortType.LowerPriority or child.abortType == AIAbortType.Both then
            self:_CollectAbortLowerPriorityTask(child)
        elseif child.type == AITaskType.Condition and child.state == AITaskState.Success then
            table.insert(self._abortTasks, child)
        end
    end
end

---@class AIRepeaterAbort:AIBlockingUp
local AIRepeaterAbort = AIUtil.Class("AIRepeaterAbort", AIBlockingUp)

function AIRepeaterAbort:OnTick()
    local leftCount = self.task:LeftRepetitions()
    --- repeater task only one child!
    if leftCount > 0 then
        self.task.tree:GetTaskTicker():Tick(self.task.children[1])
    end
    --- repeater over
    if leftCount <= 0 and self:GetChildCount() <= 0 then
        self.task:InternalUpdate()
        self.task:OnExit()
        self:_TickTreeBranch()
    end
end

---@class AIParallelAbort:AIBlockingUp
local AIParallelAbort = AIUtil.Class("AIParallelAbort", AIBlockingUp)

function AIParallelAbort:Tick()
    local count, finished = #self.children, false
    if count > 0 then
        ---@type AIBlockingUp
        local child = nil
        for index = count, 1, -1 do
            child = self.children[index]
            child:Tick()
            --- check finished !!
            if self:_CheckFinished(child.task) then
                finished = true
                break
            end
        end
    end

    if self:GetChildCount() <= 0 or finished then
        self:OnTick()
    end
end

function AIParallelAbort:OnTick()
    self.task:InternalUpdate()
    self.task:OnExit()
    self:_TickTreeBranch()
end

---@param task AITask
---@return boolean
function AIParallelAbort:_CheckFinished(task)
    if task.state == AITaskState.Running then
        return false
    end

    while task.parent ~= self.task do
        task = task.parent
        task:InternalUpdate()
    end

    if self.type == AIBlockingUpType.Parallel then
        return task.state == AITaskState.Failure
    elseif self.type == AIBlockingUpType.ParallelSelector then
        return task.state == AITaskState.Success
    elseif self.type == AIBlockingUpType.ParallelComplete then
        return task.state == AITaskState.Success or task.state == AITaskState.Failure
    end
end

---@class AIBlockingUpFacade:Class
---@field _tree AITree
---@field _root AIBlockingUp
---@field _using AIBlockingUp[]
---@field _idles AIBlockingUp[]
local AIBlockingUpFacade = AIUtil.Class("AIBlockingUpFacade", AIBlockingUp)

local AIBlockingUpClass = {
    [AIBlockingUpType.Running] = AIRunningAbort,
    [AIBlockingUpType.Repeater] = AIRepeaterAbort,
    [AIBlockingUpType.Parallel] = AIParallelAbort,
    [AIBlockingUpType.ParallelSelector] = AIParallelAbort,
    [AIBlockingUpType.ParallelComplete] = AIParallelAbort,
}

---@param tree AITree
function AIBlockingUpFacade:ctor(tree)
    self._using = {}
    self._idles = {}
    self._tree = tree
    self._root = AIBlockingUp.new(self)
    self._root.start = true
end

function AIBlockingUpFacade:Tick()
    self._root:Tick()
end

function AIBlockingUpFacade:PostProcess()
    self._root:PostProcess()
end

function AIBlockingUpFacade:Clear()
    self._root:Destroy()
end

---@return boolean
function AIBlockingUpFacade:IsRunning()
    return #self._using > 0
end

---@param entry AITask
---@param task AITask
function AIBlockingUpFacade:AddTask(entry, task)
    if not task then
        return
    end

    ---@type AIBlockingUp
    local blockingUp = self:_Temp(entry, task)
    if not blockingUp then
        return
    end

    local depth, currDepth = 0, -1
    ---@type AIBlockingUp
    local parent, grandfather = nil, nil
    ---@param tab AIBlockingUp
    for _, tab in ipairs(self._using) do
        --- find children
        parent = tab.task.parent
        while parent do
            if parent == blockingUp.task then
                if not tab.parent or not tab.parent.task then
                    blockingUp:AddChild(tab)
                else
                    grandfather = tab.parent.task.parent
                    while true do
                        if grandfather == blockingUp.task then
                            break
                        elseif not grandfather then
                            blockingUp:AddChild(tab)
                            break
                        end
                        grandfather = grandfather.parent
                    end
                end
                break
            end
            parent = parent.parent
        end

        --- find parent
        depth = 0
        parent = blockingUp.task.parent
        while parent do
            if parent == tab.task then
                if depth < currDepth or currDepth == -1 then
                    tab:AddChild(blockingUp)
                    currDepth = depth
                end
                break
            end
            depth = depth + 1
            parent = parent.parent
        end
    end

    if not blockingUp.parent then
        self._root:AddChild(blockingUp)
    end

    table.insert(self._using, blockingUp)
end

---@param blockingUp AIBlockingUp
function AIBlockingUpFacade:Remove(blockingUp)
    if not blockingUp then
        return
    end
    local parent = blockingUp.parent
    if parent then
        parent:RemoveChild(blockingUp)
    end
    for index, tab in ipairs(self._using) do
        if tab == blockingUp then
            self:_Recycle(table.remove(self._using, index))
            break
        end
    end
end

---@param entry AITask
---@param task AITask
function AIBlockingUpFacade:_Temp(entry, task)
    local type = task.blockingUpType or AIBlockingUpType.Running
    if not AIBlockingUpClass[type] then
        return
    end
    if not self._idles[type] then
        self._idles[type] = {}
    end
    ---@type AIBlockingUp
    local blockingUp = table.remove(self._idles[type])
    if not blockingUp then
        blockingUp = AIBlockingUpClass[type].new(self, type)
    end
    if blockingUp then
        blockingUp:Init(entry, task)
    end
    return blockingUp
end

---@param value AIBlockingUp
function AIBlockingUpFacade:_Recycle(value)
    if not value then
        return
    end
    table.insert(self._idles[value.type], value)
end

return AIBlockingUpFacade