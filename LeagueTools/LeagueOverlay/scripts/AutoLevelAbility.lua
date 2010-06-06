--Ability Auto-Leveler Script

lastSeenLevel = -1;

function autoLevelAbilityFunction(level)
--Input -> level=the level which was leveled up to (int).
--Function -> automatically levels up an ability


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
				local abNum = string.sub(temp,n,n); --get which ability to level up for this level
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
