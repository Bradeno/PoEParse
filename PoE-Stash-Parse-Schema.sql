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



