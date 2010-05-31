using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace RecommendedItemTool
{
    public class HeroData
    {
        public string codeName;
        public string name;
        public BitmapImage icon;

        public HeroData()
        {
        }
        public HeroData(string cName, string rName)
        {
            codeName = cName;
            name = rName;
        }
        public override string ToString()
        {
            return name;
        }
    }
}
