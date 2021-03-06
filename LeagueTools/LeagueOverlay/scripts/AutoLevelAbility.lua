--Ability Auto-Leveler Script

lastSeenLevel = -1;
testLabel = NewLabel();
SetLabelText(testLabel,"L");
SetLabelColor(testLabel,255,0,255,0);
SetLabelFont(testLabel,"Miramonte",16);
SetComponentPos(testLabel,350,2);

function autoLevelAbility_UIInit()
	local yPos = GetResolutionY() / 19;	
	local xPos = 5;
	SetComponentPos(testLabel,xPos,yPos);
end

function autoLevelAbilityFunction(level)
--Input -> level=the level which was leveled up to (int).
--Function -> automatically levels up an ability

local str = string.format("%i",level);
SetLabelText(testLabel,""..GetHeroName().." is level "..str);

if(lastSeenLevel==-1) then
lastSeenLevel=level-1;
end

local lfile = io.open("abilities.txt","r"); --open the file containing the level up info
if (lfile==nil) then return; end;

while true do 
	
	local name = lfile:read(); -- get the next line of the file

	if(name == nil) then 
		return;

	elseif (name == GetHeroName()) then --check if the line is the hero name you want, if not, keep looping
		local temp = lfile:read(); --get the next line, string containing leveling up info
		--levelDiff = level-lastSeenLevel;
		
		for n=level,lastSeenLevel+1,-1 do
				if (string.len(temp) ==0) then return; end;
				local abNum = string.sub(temp,n,n); --get which ability to level up for this level
				SetLabelText(testLabel,""..name.." is level "..str.." and picked ability "..abNum);
				SendKeyDown(0x1D); --send control key down
				
				--Check which ability is going to be leveled, send the key for it

					if (abNum=="q") then
					SendKeyPress(0x10);
		
					elseif (abNum=="w") then
					SendKeyPress(0x11);

					elseif (abNum=="e") then
					SendKeyPress(0x12);

					elseif (abNum=="r") then
					SendKeyPress(0x13);
					end

				SendKeyUp(0x1D); --send control key up

		end
		lastSeenLevel = level;
		return;
	end

end
lastSeenLevel = level;


end


RegisterEvent("levelUp","autoLevelAbilityFunction");
RegisterEvent("interfaceInit","autoLevelAbility_UIInit");