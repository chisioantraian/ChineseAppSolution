using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace ChineseApp
{

    public sealed partial class MainPage : Page
    {
        private string wordsPath = @".\Assets\allWords.utf8";
        public MainPage()
        {
            this.InitializeComponent();

            string[] lines = File.ReadAllLines(wordsPath);
            ApplicationView.GetForCurrentView().Title = $"{lines.Length} words";
        }

    }
}
