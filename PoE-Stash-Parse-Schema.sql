USE master
go
DROP DATABASE POE
go

--Create Database & Table Structure
CREATE DATABASE POE;
go
USE braden_test
go

CREATE TABLE Accounts 
(
  accountName nvarchar(128) NOT NULL PRIMARY KEY,
  lastCharacterName nvarchar(128) DEFAULT NULL,
  lastSeen datetime
)

CREATE TABLE Stashes (
  stashId varchar(128) NOT NULL DEFAULT '' PRIMARY KEY,
  stashName nvarchar(128) DEFAULT NULL,
  stashType varchar(128) DEFAULT NULL,
  publicStash tinyint DEFAULT 0,
)


CREATE TABLE ChangeId (
  id int NOT NULL identity(1,1),
  nextChangeId varchar(128) NOT NULL DEFAULT(''),
  processed tinyint DEFAULT 0,
  PRIMARY KEY (id,nextChangeId)
)

CREATE TABLE Leagues (
  leagueName varchar(128) NOT NULL DEFAULT '' PRIMARY KEY,
  active tinyint DEFAULT 0,
  poeTradeId varchar(128) DEFAULT ''
)

CREATE TABLE Items (
  w tinyint NOT NULL DEFAULT 0,
  h tinyint NOT NULL DEFAULT 0,
  ilvl smallint NOT NULL DEFAULT 0,
  icon varchar(1024) DEFAULT NULL,
  league varchar(128) NOT NULL DEFAULT '' FOREIGN KEY REFERENCES Leagues (leagueName),
  itemId varchar(128) NOT NULL DEFAULT '' PRIMARY KEY,
  name varchar(128) DEFAULT NULL,
  typeLine varchar(128) DEFAULT NULL,
  identified tinyint NOT NULL DEFAULT 0,
  verified tinyint NOT NULL DEFAULT 0,
  corrupted tinyint NOT NULL DEFAULT 0,
  lockedToCharacter tinyint DEFAULT 0,
  frameType tinyint DEFAULT 0,
  x smallint DEFAULT 0,
  y smallint DEFAULT 0,
  inventoryId varchar(128) DEFAULT NULL,
  accountName nvarchar(128) NOT NULL DEFAULT '' FOREIGN KEY REFERENCES Accounts (accountName),
  stashId varchar(128) NOT NULL DEFAULT '' FOREIGN KEY REFERENCES Stashes (stashID) ON DELETE CASCADE,
  socketAmount tinyint NOT NULL DEFAULT 0,
  linkAmount tinyint NOT NULL DEFAULT 0,
  available tinyint NOT NULL DEFAULT 0,
  addedTs bigint DEFAULT 0,
  updatedTs bigint DEFAULT 0,
  flavourText varchar(1024) DEFAULT NULL,
  price varchar(128) DEFAULT NULL,
  crafted tinyint DEFAULT 0,
  enchanted tinyint DEFAULT 0,
)

CREATE TABLE Mods (
  itemId varchar(128) DEFAULT NULL FOREIGN KEY REFERENCES Items (itemId) ON DELETE CASCADE,
  modName varchar(256) NOT NULL DEFAULT '0',
  modValue1 varchar(256) DEFAULT '0',
  modValue2 varchar(128) DEFAULT '0',
  modValue3 varchar(128) DEFAULT '0',
  modValue4 varchar(128) DEFAULT '0',
  modType varchar(10) DEFAULT 'IMPLICIT',
  modKey varchar(128) DEFAULT NULL UNIQUE,
)

CREATE TABLE Properties (
  itemId varchar(128) DEFAULT NULL FOREIGN KEY REFERENCES Items (itemId) ON DELETE CASCADE,
  propertyName varchar(128) NOT NULL DEFAULT '',
  propertyValue1 varchar(128) DEFAULT '',
  propertyValue2 varchar(128) DEFAULT ''
)


CREATE TABLE Requirements (
  itemId varchar(128) DEFAULT NULL FOREIGN KEY REFERENCES Items (itemId) ON DELETE CASCADE,
  requirementName varchar(128) NOT NULL DEFAULT '',
  requirementValue smallint DEFAULT 0,
  requirementKey varchar(128) NOT NULL DEFAULT '' UNIQUE,
)


CREATE TABLE Sockets (
  itemId varchar(128) DEFAULT NULL FOREIGN KEY REFERENCES Items (itemId) ON DELETE CASCADE,
  socketGroup tinyint DEFAULT 0,
  socketAttr char(1) DEFAULT NULL
)
GO

--Create Table Valued Parameters to hold .NET Data Tables.
----------------------------------------------------------
--Holds the JSON Next Change ID
CREATE TYPE dbo.ChangeIdTableType AS TABLE
	(nextChangeId VARCHAR(128), stashes NVARCHAR(128))
GO

--Holds Stash & Account Information
CREATE TYPE dbo.StashesParseType AS TABLE
	(accountName NVARCHAR(128), lastCharacterName NVARCHAR(128), id VARCHAR(128), stash NVARCHAR(128), 
	stashType VARCHAR(128), items NVARCHAR(128), _public BIT)
GO

--Holds Item Information (From Items Struct)
create type dbo.ItemsParseType as table
	(verified tinyint, w tinyint, h tinyint, ilvl smallint, icon varchar(1024), support tinyint, league varchar(128), id varchar(128), name varchar(128),
	typeLine varchar(128), identified tinyint, corrupted tinyint, lockedToCharacter tinyint, secDescrText varchar(128), explicitMods nvarchar(128), descrText varchar(128),
	frameType tinyint, x smallint, y smallint, inventoryId varchar(128), cosmeticMods nvarchar(128), note nvarchar(128), flavourText varchar(1024), implicitMods nvarchar(128),
	craftedMods nvarchar(128), duplicated tinyint, talismanTier int, isRelic tinyint, utilityMods nvarchar(128), enchantMods nvarchar(128), stackSize int, maxStackSize int, artFileName nvarchar(128),
	prophecyText varchar(128), prophecyDiffText varchar(128), sockets nvarchar(128), socketedItems nvarchar(128), nextLevelRequirements nvarchar(128), properties nvarchar(128), 
	additionalProperties nvarchar(128), requirements nvarchar(128), accountName nvarchar(128), stashId varchar(128))
go


--Create Stored Procedures to handle Data Tables.

CREATE PROCEDURE usp_AddChangeId
@newId dbo.ChangeIdTableType READONLY
AS
BEGIN
	DECLARE @nextId VARCHAR(128)
	SET @nextId = (SELECT nextChangeId FROM @newId)
	INSERT INTO ChangeId (nextChangeId, processed)
	VALUES(@nextId, 1)
END
GO

CREATE PROCEDURE usp_StashParse
@newStashData StashesParseType READONLY
AS
BEGIN
--Update Account Table	
	MERGE Accounts WITH (HOLDLOCK) AS A
	USING (SELECT DISTINCT nsd.accountName, nsd.lastCharacterName, GETDATE() FROM @newStashData nsd)
	AS poed (accountName, lastCharacterName, lastSeen)
		ON poed.accountName = A.accountName
	WHEN MATCHED THEN
		UPDATE SET A.lastCharacterName = poed.lastCharacterName, A.lastSeen = poed.lastSeen
	WHEN NOT MATCHED THEN
		INSERT (accountName, lastCharacterName, lastSeen)
		VALUES (poed.accountName, poed.lastCharacterName, poed.lastSeen);

--Update Stashes Table
	MERGE Stashes WITH (HOLDLOCK) AS S
	USING (SELECT DISTINCT nsd.id, nsd.stash, nsd.stashType, nsd._public FROM @newStashData nsd)
	AS poed (stashId, stashName, stashType, publicStash)
		ON poed.stashId = S.stashId
	WHEN MATCHED THEN
		UPDATE SET S.stashName = poed.stashName, 
					S.stashType = poed.stashType,
					S.publicStash = poed.publicStash
	WHEN NOT MATCHED THEN
		INSERT (stashId, stashName, stashType, publicStash)
		VALUES (poed.stashId, poed.stashName, poed.stashType, poed.publicStash);
END
GO



CREATE PROCEDURE usp_BaseItemsParse
@newItemsData ItemsParseType readonly
AS
BEGIN
--Update Items Table	
	MERGE Items WITH (HOLDLOCK) AS I
	USING (SELECT * FROM @newItemsData nid) AS poed
		ON poed.id = I.itemId
	WHEN MATCHED THEN
		UPDATE 
			SET I.w = poed.w,
				I.h = poed.h,
				I.ilvl = poed.ilvl,
				I.icon = poed.icon,
				I.league = poed.league,
				I.name = poed.name,
				I.typeLine = poed.typeLine,
				I.identified = poed.identified,
				I.verified = poed.verified,
				I.corrupted = poed.corrupted,
				I.lockedToCharacter = poed.lockedToCharacter,
				I.frameType = poed.frameType,
				I.x = poed.x,
				I.y = poed.y,
				I.inventoryId = poed.inventoryId,
				I.accountName = poed.accountName,
				I.stashId = poed.stashId,
				I.socketAmount = poed.socketAmount,
				I.linkAmount = poed.linkAmount,
				I.available = poed.available,
				I.addedTs = poed.addedTs,
				I.updatedTs = poed.updatedTs,
				I.flavourText = poed.flavourText,
				I.price = poed.price,
				I.crafted = poed.crafted,
				I.enchanted = poed.enchanted
		WHEN NOT MATCHED THEN
			INSERT (w, h, ilvl, icon, league, itemId, name, typeLine, identified, verified, corrupted, lockedToCharacter, frameType, x, y, inventoryId, accountName, stashId, 
						socketAmount, linkAmount, available, addedTs, updatedTs, flavourText, price, crafted, enchanted)
			VALUES (poed.w, poed.h, poed.ilvl, poed.icon, poed.league, poed.id, poed.name, poed.typeLine, poed.identified, poed.verified, poed.corrupted, poed.lockedToCharacter, 
					poed.frameType, poed.x, poed.y, poed.inventoryId, poed.accountName, poed.stashId, poed.socketAmount, poed.linkAmount, poed.available, poed.addedTs, poed.updatedTs, 
					poed.flavourText, poed.price, poed.crafted, poed.enchanted);
END
GO
