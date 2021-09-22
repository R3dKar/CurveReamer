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
using System.Windows.Shapes;

namespace CurveReamer
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class GCodeSettingsWindow : Window
    {
        public GCodeSettingsWindow(Window owner)
        {
            InitializeComponent();
            Owner = owner;
            (DataContext as GCodeSettingsWindowViewModel).Owner = this;
        }
    }
}
