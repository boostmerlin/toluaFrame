CtrlManager = {}
local this = CtrlManager
local ctrlList = {}	--控制器列表--

function CtrlManager.Init()

end

--添加控制器--
function CtrlManager.AddCtrl(ctrlName, ctrlObj)
	local old = ctrlList[ctrlName]
	if old then
		logWarn("!! Controller is register before? " .. ctrlName)
	end
	ctrlList[ctrlName] = ctrlObj
end

--获取控制器--
function CtrlManager.GetCtrl(ctrlName)
	local ret = ctrlList[ctrlName]
	return ret
end

--移除控制器--
function CtrlManager.RemoveCtrl(ctrlName)
	ctrlList[ctrlName] = nil
end

function CtrlManager.All()
	return ctrlList
end

--关闭控制器--
function CtrlManager.Clean()
	ctrlList = {}
end