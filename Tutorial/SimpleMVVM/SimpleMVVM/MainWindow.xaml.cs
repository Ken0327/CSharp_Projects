using System.Windows;

namespace SimpleMVVM
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ModelViewBinding();
        }

        private void ModelViewBinding()
        {
            UserViewModel VM = new UserViewModel();
            this.DataContext = VM;
        }
    }
}
