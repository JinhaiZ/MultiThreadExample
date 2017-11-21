using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClassLibrary1;
using WpfAppliTh;

namespace WpfAppThreadExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void premier_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(new ThreadStart(NombrePremier.Premier));
            t.Start();
        }

        private void ballon_Click(object sender, RoutedEventArgs e)
        {
            WindowBallon wb = new WindowBallon();
            wb.Show();
        }
    }
}
