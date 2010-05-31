RegisterEvent("levelUp","autoLevelAbilityFunction");

function autoLevelAbilityFunction(level)
--Input -> level=the level which was leveled up to (int).
--Function -> automatically levels up an ability


lfile = assert(io.open("abtest.txt","r")); --open the file containing the level up info

while true do 

	name = lfile:read(); -- get the next line of the file

	if (name==nil) then 
	break;
	end

	if (name == GetHeroName()) then --check if the line is the hero name you want, if not, keep looping
		temp = lfile:read(); --get the next line, string containing leveling up info
		abNum = tonumber(string.sub(temp,level,level)); --get which ability to level up for this level
		SendKeyDown(0x1D); --send control key down
		
		--Check which ability is going to be leveled, send the key for it

			if (abNum==0) then
			SendKeyPress(0x10);
		
			elseif (abNum==1) then
			SendKeyPress(0x11);

			elseif (abNum==2) then
			SendKeyPress(0x12);

			elseif (abNum==3) then
			SendKeyPress(0x13);
			end

		SendKeyUp(0x1D); --send control key up

		return;
	end

end


end