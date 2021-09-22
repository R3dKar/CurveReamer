using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Логика взаимодействия для AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow(Window owner)
        {
            InitializeComponent();
            Owner = owner;
        }

        private void NavigateGitHub(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/R3dKar");
        }

        private void NavigateSteam(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://steamcommunity.com/id/R3dKar/");
        }
    }
}
