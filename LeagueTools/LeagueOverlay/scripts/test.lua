--[[
RegisterEvent("update","updateFunction");

testLabel = NewLabel();
SetLabelText(testLabel,"OMG THIS IS AMAZING");
SetLabelColor(testLabel,255,0,255,0);
SetLabelFont(testLabel,"Comic Sans",20);
function updateFunction(elapsed)
	x = GetComponentX(testLabel);
	x = x + 2;
	if(x > 300) then
		RemoveComponent(testLabel);
	end
	SetComponentPos(testLabel,x,2);
	--PrintMsg(GetmsTime().."ms run time :: elapsed time:"..elapsed);
end
--]]