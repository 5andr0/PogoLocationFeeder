using System.Windows.Controls;

namespace PogoLocationFeeder.GUI.Views {
    /// <summary>
    ///     Interaktionslogik für DebugOutputView.xaml
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