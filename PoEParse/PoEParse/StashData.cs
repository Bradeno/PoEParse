using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoEParse
{
    public struct StashData
    {
        public string next_change_id { get; set; }
        public Stash[] stashes { get; set; }
    }

    public struct Stash
    {
        public string accountName { get; set; }
        public string lastCharacterName { get; set; }
        public string id { get; set; }
        public string stash { get; set; }
        public string stashType { get; set; }
        public Item[] items { get; set; }
        public bool _public { get; set; }
    }

    public struct Item
    {
        public bool verified { get; set; }
        public int w { get; set; }
        public int h { get; set; }
        public int ilvl { get; set; }
        public string icon { get; set; }
        public bool support { get; set; }
        public string league { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string typeLine { get; set; }
        public bool identified { get; set; }
        public bool corrupted { get; set; }
        public bool lockedToCharacter { get; set; }
        public string secDescrText { get; set; }
        public string[] explicitMods { get; set; }
        public string descrText { get; set; }
        public int frameType { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public string inventoryId { get; set; }
        public string[] cosmeticMods { get; set; }
        public string note { get; set; }
        public string[] flavourText { get; set; }
        public string[] implicitMods { get; set; }
        public string[] craftedMods { get; set; }
        public bool duplicated { get; set; }
        public int talismanTier { get; set; }
        public bool isRelic { get; set; }
        public string[] utilityMods { get; set; }
        public string[] enchantMods { get; set; }
        public int stackSize { get; set; }
        public int maxStackSize { get; set; }
        public string artFilename { get; set; }
        public string prophecyText { get; set; }
        public string prophecyDiffText { get; set; }
        public Socket[] sockets { get; set; }
        public Socketeditem[] socketedItems { get; set; }
        public Nextlevelrequirement1[] nextLevelRequirements { get; set; }
        public Property1[] properties { get; set; }
        public Additionalproperty[] additionalProperties { get; set; }
        public Requirement[] requirements { get; set; }
    }

    public struct Socket
    {
        public int group { get; set; }
        public string attr { get; set; }
    }

    public struct Property1
    {
        public string name { get; set; }
        public object[][] values { get; set; }
        public int displayMode { get; set; }
        public int type { get; set; }
    }

    public struct Additionalproperty
    {
        public string name { get; set; }
        public object[][] values { get; set; }
        public int displayMode { get; set; }
        public float progress { get; set; }
    }

    public struct Requirement
    {
        public string name { get; set; }
        public object[][] values { get; set; }
        public int displayMode { get; set; }
    }

    public struct Socketeditem
    {
        public bool verified { get; set; }
        public int w { get; set; }
        public int h { get; set; }
        public int ilvl { get; set; }
        public string icon { get; set; }
        public bool support { get; set; }
        public string league { get; set; }
        public string id { get; set; }
        public object[] sockets { get; set; }
        public string name { get; set; }
        public string typeLine { get; set; }
        public bool identified { get; set; }
        public bool corrupted { get; set; }
        public bool lockedToCharacter { get; set; }
        public string secDescrText { get; set; }
        public string[] explicitMods { get; set; }
        public string descrText { get; set; }
        public int frameType { get; set; }
        public int socket { get; set; }
        public string colour { get; set; }
        public object[] socketedItems { get; set; }
        public Requirement1[] requirements { get; set; }
        public Nextlevelrequirement[] nextLevelRequirements { get; set; }
        public Property2[] properties { get; set; }
        public Additionalproperty1[] additionalProperties { get; set; }
    }

    public struct Property2
    {
        public string name { get; set; }
        public object[][] values { get; set; }
        public int displayMode { get; set; }
        public int type { get; set; }
    }

    public struct Additionalproperty1
    {
        public string name { get; set; }
        public object[][] values { get; set; }
        public int displayMode { get; set; }
        public float progress { get; set; }
    }

    public struct Requirement1
    {
        public string name { get; set; }
        public object[][] values { get; set; }
        public int displayMode { get; set; }
    }

    public struct Nextlevelrequirement
    {
        public string name { get; set; }
        public object[][] values { get; set; }
        public int displayMode { get; set; }
    }

    public struct Nextlevelrequirement1
    {
        public string name { get; set; }
        public object[][] values { get; set; }
        public int displayMode { get; set; }
    }
}