using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OCR.WPF.Imaging;
using OCR.WPF.UI;

namespace OCR.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            ModelView = new MainModelView();
            DataContext = ModelView;
        }

        public MainModelView ModelView
        {
            get;
            set;
        }

        private void MainWindow_OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.Bitmap))
                e.Effects = DragDropEffects.Copy;
        }


        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length != 1)
            {
                MessageService.ShowError(this, "Drop only one file at the time !");
                return;
            }

            ModelView.OpenPicture(files.Single());
        }

        private void RangeBase_OnValueChanged(object sender, DragCompletedEventArgs dragCompletedEventArgs)
        {
            if (ModelView != null && ModelView.ApplyCharacterIsolationCommand != null)
                ModelView.ApplyCharacterIsolationCommand.Execute(null);
        }
        private void GradientRangeBase_OnValueChanged(object sender, DragCompletedEventArgs dragCompletedEventArgs)
        {
            if (ModelView != null && ModelView.ApplyEdgeDetectionCommand != null)
                ModelView.ApplyEdgeDetectionCommand.Execute(null);
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ModelView != null && ModelView.ApplyCharacterIsolationCommand != null)
                ModelView.ApplyCharacterIsolationCommand.Execute(null);
        }

        public double CurrentBrightness
        {
            get;
            set;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {

            if (ModelView.CharacterIsolation != null && ModelView.CharacterIsolation.Output != null)
            {
                var pos = e.GetPosition(IsolationOutput);
                var brightness = ModelView.CharacterIsolation.GetPixel((int) pos.X, (int) pos.Y).GetBrightness();

                CurrentBrightness = brightness;
            }
            }
            catch (Exception)
            {
                
            }
            base.OnMouseMove(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
