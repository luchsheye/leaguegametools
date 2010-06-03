--init "class" table and member variables
EnemyDisplay = {
	teamRect = -1;
	championImages = {};
	sSpell1Images = {};
	sSpell1CooldownLbl = {};
	sSpell1Cooldown = {};
	sSpell2Images = {};
	sSpell2CooldownLbl = {};
	sSpell2Cooldown = {};
	summonerInfoTable = {};
	currentTeam = 0;

	initialized = false;
};

function EnemyDisplay:Init()

end

function EnemyDisplay:UIInit()
	
	local i;
	local yPos = GetResolutionY() / 2 - 300;	
	PrintMsg(":"..yPos);
	self.teamRect = NewRectangle();
	SetComponentPos(self.teamRect,GetResolutionX()- 40, yPos);
	SetRectangleSize(self.teamRect,20,20);
	SetRectangleBgColor(self.teamRect,255,255,0,0);
	SetRectangleClickEvent(self.teamRect,"EnemyDisplay:SwapTeams()");
	yPos = yPos + 30;
	for i = 0, 4 do
		self.championImages[i] = NewRectangle();
		SetComponentPos(self.championImages[i],GetResolutionX()- 40, yPos);
		SetRectangleSize(self.championImages[i],40,40);
		SetRectangleBgColor(self.championImages[i],255,255,0,0);
		SetRectangleClickEvent(self.championImages[i],"EnemyDisplay:RectangleClick("..i..")");

		self.sSpell1Images[i] = NewRectangle();
		SetComponentPos(self.sSpell1Images[i],GetResolutionX()- 60, yPos);
		SetRectangleSize(self.sSpell1Images[i],20,20);
		SetRectangleClickEvent(self.sSpell1Images[i],"EnemyDisplay:SummonerSpellClick(0,"..i..")");
		
		self.sSpell1CooldownLbl[i] = NewLabel();
		SetComponentPos(self.sSpell1CooldownLbl[i],GetResolutionX()- 60, yPos);
		SetLabelFont(self.sSpell1CooldownLbl[i],"Tahoma",13);
		SetLabelText(self.sSpell1CooldownLbl[i],"0");
		SetLabelColor(self.sSpell1CooldownLbl[i],255,255,255,255);
		SetComponentVisible(self.sSpell1CooldownLbl[i],false);
		self.sSpell1Cooldown[i] = 0;

		self.sSpell2Images[i] = NewRectangle();
		SetComponentPos(self.sSpell2Images[i],GetResolutionX()- 60, yPos + 20);
		SetRectangleSize(self.sSpell2Images[i],20,20);
		SetRectangleClickEvent(self.sSpell2Images[i],"EnemyDisplay:SummonerSpellClick(1,"..i..")");

		self.sSpell2CooldownLbl[i] = NewLabel();
		SetComponentPos(self.sSpell2CooldownLbl[i],GetResolutionX()- 60, yPos + 20);
		SetLabelFont(self.sSpell2CooldownLbl[i],"Tahoma",13);
		SetLabelText(self.sSpell2CooldownLbl[i],"0");
		SetLabelColor(self.sSpell2CooldownLbl[i],255,255,255,255);
		SetComponentVisible(self.sSpell2CooldownLbl[i],false);
		self.sSpell2Cooldown[i] = 0;

		yPos = yPos + 50;
	end

	self:UpdateTeamDisplay();
	
	self.initialized = true;
end

function EnemyDisplay:SwapTeams()

end

function EnemyDisplay:UpdateTeamDisplay()
	local imageDir = GetLeagueDir().."air\\assets\\images\\";

	for i = 0, 4 do
		local summonerInfo = {};
		GetSummonerInfo(self.currentTeam,i,summonerInfo);
		SetRectangleImage(self.championImages[i],imageDir.."champions\\"..summonerInfo.championCodeName.."_Square_0.png");
		SetRectangleImage(self.sSpell1Images[i],imageDir.."spells\\"..summonerInfo.spell1CodeName..".png");
		SetRectangleImage(self.sSpell2Images[i],imageDir.."spells\\"..summonerInfo.spell2CodeName..".png");

		self.summonerInfoTable[i] = summonerInfo;
	end
end

function EnemyDisplay:RectangleClick(cNum)
	if(IsKeyDown(162)) then
		SendAllyChatMessage(self.summonerInfoTable[cNum].championName.." re");
	else
		SendAllyChatMessage(self.summonerInfoTable[cNum].championName.." mia");
	end
end

function EnemyDisplay:SummonerSpellClick(sNum,cNum)
	local cdTable;
	if(sNum == 0) then
		cdTable = self.sSpell1Cooldown;
		SetComponentVisible(self.sSpell1CooldownLbl[cNum],true);
	else
		cdTable = self.sSpell2Cooldown;
		SetComponentVisible(self.sSpell2CooldownLbl[cNum],true);
	end
	if(IsKeyDown(162)) then
		--SendAllyChatMessage(self.summonerInfoTable[cNum].championName.." re");
	else
		--SendAllyChatMessage(self.summonerInfoTable[cNum].championName.." mia");
		cdTable[cNum] = 10;
	end
end


function EnemyDisplay:ProcessingFinished()

end

function EnemyDisplay:UIUpdate(elapsed)
	if (self.initialized == false) then return; end;
	local eSeconds = elapsed / 1000;
	for i = 0, 4 do
		local temp = self.sSpell1Cooldown[i] - eSeconds;
		if(temp > 0) then
			self.sSpell1Cooldown[i] = temp;
			SetLabelText(self.sSpell1CooldownLbl[i],""..math.ceil(self.sSpell1Cooldown[i]));
		elseif(self.sSpell1Cooldown[i] > 0) then
			SetComponentVisible(self.sSpell1CooldownLbl[i],false);
			self.sSpell1Cooldown[i] = 0;
		end
		temp = self.sSpell2Cooldown[i] - eSeconds;
		if(temp > 0) then
			self.sSpell2Cooldown[i] = temp;
			SetLabelText(self.sSpell2CooldownLbl[i],""..math.ceil(self.sSpell2Cooldown[i]));
		elseif(self.sSpell2Cooldown[i] > 0) then
			SetComponentVisible(self.sSpell2CooldownLbl[i],false);
			self.sSpell1Cooldown[i] = 0;
		end
	end
end


EnemyDisplay:Init();
RegisterEvent("processingFinished","EnemyDisplay:ProcessingFinished");
RegisterEvent("interfaceInit","EnemyDisplay:UIInit");
RegisterEvent("update","EnemyDisplay:UIUpdate");
PrintMsg("EnemyDisplay Loaded");