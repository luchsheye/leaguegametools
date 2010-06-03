--[[
UNUSED SCRIPT
--init "class" table and member variables
CooldownDisplay = {
	abilityLabels = {};
};

function CooldownDisplay:Init()
	local i;
	for i=0,5 do
		local l = NewLabel();
		SetLabelText(l,"A:"..i);
		SetLabelColor(l,255,0,255,0);
		SetLabelFont(l,"Arial",20);
		self.abilityLabels[i] = l;
	end
end

function CooldownDisplay:UIInit()
local i;
	for i=0,5 do
		local tempRect = {};
		GetUIRect_AbilityByNum(i,tempRect);
		SetComponentPos(self.abilityLabels[i],tempRect.x,tempRect.y);
		SetLabelText(self.abilityLabels[i],tempRect.x);
	end
end

function CooldownDisplay:ProcessingFinished()
	local i = 0;
	for i=0,5 do
		local l = CooldownDisplay.abilityLabels[i];
		local cd = GetAbilityCooldown(i);
		if(cd > 0) then
			SetComponentVisible(l,true);
		else
			SetComponentVisible(l,false);
		end
		SetLabelText(l,""..cd);
		SetLabelColor(l,255,0,255,0);
		
	end
end

CooldownDisplay:Init();
RegisterEvent("processingFinished","CooldownDisplay:ProcessingFinished");
RegisterEvent("interfaceInit","CooldownDisplay:UIInit");
PrintMsg("CooldownDisplay Loaded");
--]]