using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RecommendedItemTool
{
    /// <summary>
    /// Interaction logic for AutoAbilityTool.xaml
    /// </summary>
    public partial class AutoAbilityTool : UserControl
    {
        Rectangle[] abilities = new Rectangle[4];
        public AutoAbilityTool()
        {
            InitializeComponent();
            
            for (int i = 0; i < 4; i++)
            {
                abilities[i] = new Rectangle();
                canvas.Children.Add(abilities[i]);
                Canvas.SetLeft(abilities[i], 10+i*100);
                Canvas.SetTop(abilities[i], 10);
                
            }

            


        }
    }
}
