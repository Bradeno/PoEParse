USE master
go
DROP DATABASE POE
go

--Create Database & Table Structure
CREATE DATABASE POE;
go
USE POE
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
  identified bit NOT NULL DEFAULT 0,
  corrupted bit NOT NULL DEFAULT 0,
  lockedToCharacter bit DEFAULT 0,
  secDescrText varchar(1024) null, 
  frameType tinyint DEFAULT 0,
  x smallint DEFAULT 0,
  y smallint DEFAULT 0,
  inventoryId varchar(128) DEFAULT NULL,
  accountName nvarchar(128) NOT NULL DEFAULT '' FOREIGN KEY REFERENCES Accounts (accountName),
  stashId varchar(128) NOT NULL DEFAULT '' FOREIGN KEY REFERENCES Stashes (stashID) ON DELETE CASCADE,
  socketAmount tinyint NOT NULL DEFAULT 0,
  flavourText varchar(1024) DEFAULT NULL,
  stackSize int not null default 1,
  maxStackSize int not null default 1,
  price nvarchar(128) DEFAULT NULL,
  crafted bit DEFAULT 0,
  enchanted bit DEFAULT 0,
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
  displayMode smallint DEFAULT 0,
  propertyType smallint DEFAULT 0
)


CREATE TABLE Exp_Properties (
  itemId varchar(128) DEFAULT NULL FOREIGN KEY REFERENCES Items (itemId) ON DELETE CASCADE,
  propertyName varchar(128) NOT NULL DEFAULT '',
  amount varchar(128) DEFAULT '0/0',
  displayMode smallint DEFAULT 0,
  progress float DEFAULT 0.00
)


CREATE TABLE Requirements (
  itemId varchar(128) DEFAULT NULL FOREIGN KEY REFERENCES Items (itemId) ON DELETE CASCADE,
  requirementName varchar(128) NOT NULL DEFAULT '',
  requirementValue smallint DEFAULT 0,
  --requirementKey varchar(128) NOT NULL DEFAULT '' UNIQUE,
  displayMode tinyint NOT NULL
)
go

CREATE TABLE Sockets (
  itemId varchar(128) DEFAULT NULL FOREIGN KEY REFERENCES Items (itemId) ON DELETE CASCADE,
  socketGroup tinyint DEFAULT 0,
  socketAttr char(1) DEFAULT NULL
 -- socketGroup2 tinyint DEFAULT 0,
 -- socketAttr2 char(1) DEFAULT NULL,
 -- socketGroup3 tinyint DEFAULT 0,
 -- socketAttr3 char(1) DEFAULT NULL,
 -- socketGroup4 tinyint DEFAULT 0,
 -- socketAttr4 char(1) DEFAULT NULL,
 -- socketGroup5 tinyint DEFAULT 0,
 -- socketAttr5 char(1) DEFAULT NULL,
 -- socketGroup6 tinyint DEFAULT 0,
 -- socketAttr6 char(1) DEFAULT NULL
)
GO

--Create Table Valued Parameters to hold .NET Data Tables.
----------------------------------------------------------
--Holds the JSON Next Change ID
CREATE TYPE dbo.ChangeIdTableType AS TABLE
	(nextChangeId VARCHAR(128))
GO

--Holds Stash & Account Information
CREATE TYPE dbo.StashesTableType AS TABLE
	(accountName NVARCHAR(128), lastCharacterName NVARCHAR(128), id VARCHAR(128), stash NVARCHAR(128), 
	stashType VARCHAR(128), _public BIT)
GO


--Holds Item Information
create type dbo.ItemsTableType as table
	(w tinyint, h tinyint, ilvl smallint, icon varchar(1024), league varchar(128), id varchar(128), [name] nvarchar(128),
	typeLine varchar(128), identified bit, corrupted bit, lockedToCharacter bit, secDescrText varchar(1024),
	frameType tinyint, x smallint, y smallint, inventoryId varchar(128), accountName nvarchar(128), stashId varchar(128),
	note nvarchar(128), flavourTextVal varchar(1024), socketAmount tinyint, isCrafted bit, isEnchanted bit, 
	stackSize int, maxStackSize int)
go

--Holds Socketed Item Information
create type dbo.SocketedItemsTableType as table
	(w tinyint, h tinyint, ilvl smallint, icon varchar(1024), league varchar(128), id varchar(128), accountName nvarchar(128), stashId varchar(128), [name] nvarchar(128),
	typeLine varchar(128), identified bit, corrupted bit, lockedToCharacter bit, secDescrText varchar(1024),
	frameType tinyint, colour char(1))
go


--Holds item Sockets Data
create type dbo.SocketTableType as table
	(id varchar(128), [group] tinyint, attr char(1))
go

--Holds item Properties Data
create type dbo.PropertiesTableType as table
	([name] varchar(128), id varchar(128), displayMode smallint, [type] smallint)
go

--Holds item Properties Data
create type dbo.ExpPropertiesTableType as table
	([name] varchar(128), id varchar(128), amount varchar(128), displayMode smallint, progress float)
go

--Holds Requirements Information
create type dbo.RequirementsTableType as table
	([name] varchar(128), amount smallint, id varchar(128), displayMode tinyint)
go


--Create Stored Procedures to handle Data Tables.
CREATE PROCEDURE usp_AddChangeId
@changeId varchar(128)
AS
BEGIN
	INSERT INTO ChangeId (nextChangeId, processed)
	VALUES(@changeId, 0)
END
GO

CREATE PROCEDURE usp_SetChangeIdProcessed
@changeId varchar(128)
AS
BEGIN
	UPDATE ChangeId
	SET processed = 1
	WHERE nextChangeId = @changeId
END
GO

CREATE PROCEDURE usp_StashParse
@newStashData StashesTableType READONLY
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
@newItemsData ItemsTableType readonly
AS
BEGIN
	MERGE Leagues WITH (HOLDLOCK) AS L
	USING (SELECT DISTINCT league FROM @newItemsData) 
	AS poed (league)
		ON poed.league = L.leagueName
	WHEN NOT MATCHED THEN
		INSERT (leagueName, active, poeTradeId)
		VALUES (poed.league, 1, poed.league);

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
				I.itemId = poed.id,
				I.name = poed.name,
				I.typeLine = poed.typeLine,
				I.identified = poed.identified,
				I.corrupted = poed.corrupted,
				I.lockedToCharacter = poed.lockedToCharacter,
				I.secDescrText = poed.secDescrText,
				I.frameType = poed.frameType,
				I.x = poed.x,
				I.y = poed.y,
				I.inventoryId = poed.inventoryId,
				I.accountName = poed.accountName,
				I.stashId = poed.stashId,
				I.socketAmount = poed.socketAmount,
				I.flavourText = poed.flavourTextVal,
				I.stackSize = poed.stackSize,
				I.maxStackSize = poed.maxStackSize,
				I.price = poed.note,
				I.crafted = poed.isCrafted,
				I.enchanted = poed.isEnchanted
		WHEN NOT MATCHED THEN
			INSERT (w, h, ilvl, icon, league, itemId, name, typeLine, identified, corrupted, lockedToCharacter, secDescrText, frameType, x, y, inventoryId, accountName, stashId, 
						socketAmount, flavourText, stackSize, maxStackSize, price, crafted, enchanted)
			VALUES (poed.w, poed.h, poed.ilvl, poed.icon, poed.league, poed.id, poed.name, poed.typeLine, poed.identified, poed.corrupted, poed.lockedToCharacter, poed.secDescrText,
					poed.frameType, poed.x, poed.y, poed.inventoryId, poed.accountName, poed.stashId, poed.socketAmount, poed.flavourTextVal, poed.stackSize, poed.maxStackSize, poed.note, 
					poed.isCrafted, poed.isEnchanted);


END
GO


CREATE PROCEDURE usp_PropertiesParse
@newPropertiesData PropertiesTableType READONLY
AS
BEGIN
--Update Properties Table
	DELETE FROM Properties WITH (HOLDLOCK)
	WHERE (SELECT npd.id FROM @newPropertiesData npd) = ItemId
	
	INSERT INTO Properties (itemId, propertyName, displayMode, propertyType)
	SELECT npd.id, npd.[name], npd.displayMode, npd.[type] FROM @newPropertiesData npd
END
GO

CREATE PROCEDURE usp_SocketsParse
@newSocketData SocketTableType READONLY
AS
BEGIN
	DELETE FROM Sockets WITH (HOLDLOCK)
	WHERE ItemId IN (SELECT nsd.id FROM @newSocketData nsd)
	
	INSERT INTO Sockets (itemId, socketGroup, socketAttr)
	SELECT * FROM @newSocketData
END
GO


select * from Sockets
