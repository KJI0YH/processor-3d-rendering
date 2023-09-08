using Lab1.Parser;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace Lab1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const Key OPEN_FILE_KEY = Key.O;
        private const Key CLOSE_APP_KEY = Key.Escape;

        private OpenFileDialog openFileDialog;
        private ObjParser parser;

        public MainWindow()
        {
            InitializeComponent();

            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Wavefront files (.obj)|*.obj";

            parser = new ObjParser();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case OPEN_FILE_KEY:
                    if (openFileDialog.ShowDialog() == true)
                    {
                        Model model = parser.Parse(openFileDialog.FileName);
                    }
                    break;
                case CLOSE_APP_KEY:
                    Application.Current.Shutdown();
                    break;
                default:
                    break;
            }
        }
    }
}
