using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommendedItemTool
{
    public class ItemData
    {
        public string id;
        public string name;
        public string description;
        public string iconFile;
        public bool isHealth,isMagicResist,isHealthRegen,isArmor;
        public Int32 type;
        public ItemData(string id)
        {

            isHealth = false;
            this.id = id;
        }
    }
}
