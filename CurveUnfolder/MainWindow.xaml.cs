using System.Windows;
using System.Windows.Input;

namespace CurveUnfolder
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            (DataContext as MainWindowViewModel).Owner = this;
        }

        private void SvgRenderPointClick(object sender, MouseButtonEventArgs e)
        {
            ((sender as System.Windows.Shapes.Path).DataContext as SvgPoint).IsSelected = !((sender as System.Windows.Shapes.Path).DataContext as SvgPoint).IsSelected;
        }

        private void SvgRenderPointRightClick(object sender, MouseButtonEventArgs e)
        {
            (DataContext as MainWindowViewModel).SetStartingPoint(((sender as System.Windows.Shapes.Path).DataContext as SvgPoint));
        }
    }
}
