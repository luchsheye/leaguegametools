--init "class" table and member variables
EnemyDisplay = {
	teamRect = -1;
	borderRect = -1;
	championImages = {};
	miaImages = {};
	reImages = {};
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
	local imageDir = GetToolDir().."scripts\\Images\\";

	if(self.initialized == false) then 
		self.teamRect = NewRectangle(); 
		self.borderRect = NewRectangle(); 
	end;
	SetComponentPos(self.teamRect,GetResolutionX()- 60, yPos);
	SetRectangleSize(self.teamRect,60,20);
	SetRectangleBgColor(self.teamRect,255,4,61,124);
	SetRectangleClickEvent(self.teamRect,"EnemyDisplay:SwapTeams()");

	SetComponentPos(self.borderRect,GetResolutionX()- 60, yPos);
	SetRectangleSize(self.borderRect,60,20);
	SetRectangleImage(self.borderRect,imageDir.."teamBorder.png");

	yPos = yPos + 25;
	for i = 0, 4 do
		if(self.initialized  == false) then 
			self.championImages[i] = NewRectangle(); 
			self.sSpell1Images[i] = NewRectangle();
			self.sSpell1CooldownLbl[i] = NewLabel();
			self.sSpell2Images[i] = NewRectangle();
			self.sSpell2CooldownLbl[i] = NewLabel();
			self.miaImages[i] = NewRectangle();
			self.reImages[i] = NewRectangle();
			
		end
		SetComponentPos(self.championImages[i],GetResolutionX()- 40, yPos);
		SetRectangleSize(self.championImages[i],40,40);
		SetRectangleBgColor(self.championImages[i],255,255,0,0);

		SetComponentPos(self.sSpell1Images[i],GetResolutionX()- 60, yPos);
		SetRectangleSize(self.sSpell1Images[i],20,20);
		SetRectangleClickEvent(self.sSpell1Images[i],"EnemyDisplay:SummonerSpellClick(0,"..i..")");
		
		SetComponentPos(self.sSpell1CooldownLbl[i],GetResolutionX()- 60, yPos);
		SetLabelFont(self.sSpell1CooldownLbl[i],"Tahoma",13);
		SetLabelText(self.sSpell1CooldownLbl[i],"0");
		SetLabelColor(self.sSpell1CooldownLbl[i],255,255,255,255);
		SetComponentVisible(self.sSpell1CooldownLbl[i],false);
		self.sSpell1Cooldown[i] = 0;

		SetComponentPos(self.sSpell2Images[i],GetResolutionX()- 60, yPos + 20);
		SetRectangleSize(self.sSpell2Images[i],20,20);
		SetRectangleClickEvent(self.sSpell2Images[i],"EnemyDisplay:SummonerSpellClick(1,"..i..")");

		SetComponentPos(self.sSpell2CooldownLbl[i],GetResolutionX()- 60, yPos + 20);
		SetLabelFont(self.sSpell2CooldownLbl[i],"Tahoma",13);
		SetLabelText(self.sSpell2CooldownLbl[i],"0");
		SetLabelColor(self.sSpell2CooldownLbl[i],255,255,255,255);
		SetComponentVisible(self.sSpell2CooldownLbl[i],false);
		self.sSpell2Cooldown[i] = 0;

		--mia/re buttons
		SetComponentPos(self.miaImages[i],GetResolutionX()- 60, yPos + 40);
		SetRectangleSize(self.miaImages[i],30,15);
		SetRectangleClickEvent(self.miaImages[i],"SendAllyChatMessage(EnemyDisplay.summonerInfoTable["..i.."].championName..\" mia\");");
		SetRectangleImage(self.miaImages[i],imageDir.."MIABtn.png");

		SetComponentPos(self.reImages[i],GetResolutionX()- 30, yPos + 40);
		SetRectangleSize(self.reImages[i],30,15);
		SetRectangleClickEvent(self.reImages[i],"SendAllyChatMessage(EnemyDisplay.summonerInfoTable["..i.."].championName..\" re\");");
		SetRectangleImage(self.reImages[i],imageDir.."REBtn.png");

		yPos = yPos + 70;
	end
	
	self:UpdateTeamDisplay();
	
	self.initialized = true;
end

function EnemyDisplay:SwapTeams()
	if(self.currentTeam == 0) then
		self.currentTeam = 1;		
		SetRectangleBgColor(self.teamRect,255,94,2,130);
	else
		self.currentTeam = 0;
		SetRectangleBgColor(self.teamRect,255,4,61,124);
	end
	self:UpdateTeamDisplay();
end

function EnemyDisplay:UpdateTeamDisplay()
	local imageDir = GetLeagueDir().."air\\assets\\images\\";
	local summonerCount = 5;--GetSummonerCount(self.currentTeam);
	for i = 0, (summonerCount-1) do
		local summonerInfo = {};
		GetSummonerInfo(self.currentTeam,i,summonerInfo);
		SetRectangleImage(self.championImages[i],imageDir.."champions\\"..summonerInfo.championCodeName.."_Square_0.png");
		SetRectangleImage(self.sSpell1Images[i],imageDir.."spells\\"..summonerInfo.spell1CodeName..".png");
		SetRectangleImage(self.sSpell2Images[i],imageDir.."spells\\"..summonerInfo.spell2CodeName..".png");

		SetComponentVisible(self.championImages[i],true);
		SetComponentVisible(self.sSpell1Images[i],true);
		SetComponentVisible(self.sSpell2Images[i],true);
		SetComponentVisible(self.sSpell1CooldownLbl[i],true);
		SetComponentVisible(self.sSpell2CooldownLbl[i],true);

		self.summonerInfoTable[i] = summonerInfo;
		self.sSpell1Cooldown[i] = 0;
		self.sSpell2Cooldown[i] = 0;
		SetComponentVisible(self.sSpell1CooldownLbl[i],false);
		SetComponentVisible(self.sSpell2CooldownLbl[i],false);
	end
	for i = summonerCount, 4 do
		SetComponentVisible(self.championImages[i],false);
		SetComponentVisible(self.sSpell1Images[i],false);
		SetComponentVisible(self.sSpell2Images[i],false);
		SetComponentVisible(self.sSpell1CooldownLbl[i],false);
		SetComponentVisible(self.sSpell2CooldownLbl[i],false);
	end
end


function EnemyDisplay:SummonerSpellClick(sNum,cNum)
	if(sNum == 0) then
		self.sSpell1Cooldown[cNum] = self.summonerInfoTable[cNum].spell1Cooldown;
		SetComponentVisible(self.sSpell1CooldownLbl[cNum],true);
	else
		self.sSpell2Cooldown[cNum] = self.summonerInfoTable[cNum].spell2Cooldown;
		SetComponentVisible(self.sSpell2CooldownLbl[cNum],true);
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
			self.sSpell2Cooldown[i] = 0;
		end
	end
end


EnemyDisplay:Init();
RegisterEvent("processingFinished","EnemyDisplay:ProcessingFinished");
RegisterEvent("interfaceInit","EnemyDisplay:UIInit");
RegisterEvent("update","EnemyDisplay:UIUpdate");
PrintMsg("EnemyDisplay Loaded");