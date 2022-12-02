﻿---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by chaoguan.
--- DateTime: 2020/12/4 20:56
---

local AIAction = require("AIDesigner.Base.AITask").AIAction

---@class SystemAI.TestArray:AIAction
---@field arrayVector3 AIVar[]|Vector3
---@field arrayInt Int[]|ComparisonOperation
---@field vec3 AIVar|Vector3
local TestArray = AIUtil.Class("TestArray", AIAction)

function TestArray:OnUpdate()
    for _, value in ipairs(self.arrayVector3) do
        print(value:GetValue())
    end
    return AITaskState.Success
end

return TestArray