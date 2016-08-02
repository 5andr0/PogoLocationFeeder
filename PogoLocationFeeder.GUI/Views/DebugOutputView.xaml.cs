using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PogoLocationFeeder.GUI.Views {
    /// <summary>
    /// Interaktionslogik für DebugOutputView.xaml
    /// </summary>
    public partial class DebugOutputView : UserControl {
        public DebugOutputView() {
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e) {
            Debug.ScrollToEnd();
        }
    }
}
