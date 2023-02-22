using DistanceSensorAppDemo;
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

namespace DistanceSensorApp
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

   
        private Point? mousePos;

        private void ViewportSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((MatrixTransform)DataCanvas.RenderTransform).Matrix = new Matrix(
                e.NewSize.Width / DataCanvas.ActualWidth,
                0, 0,
                e.NewSize.Height / DataCanvas.ActualHeight,
                0, 0);
        }

        private void ViewportMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var viewport = (UIElement)sender;
            viewport.CaptureMouse();
            mousePos = e.GetPosition(viewport);
        }

        private void ViewportMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).ReleaseMouseCapture();
            mousePos = null;
        }

        private void ViewportMouseMove(object sender, MouseEventArgs e)
        {
            if (mousePos.HasValue)
            {
                var pos = e.GetPosition((UIElement)sender);
                var matrix = transform.Matrix;
                matrix.Translate(pos.X - mousePos.Value.X, pos.Y - mousePos.Value.Y);
                transform.Matrix = matrix;
                mousePos = pos;
            }
        }

        private void ViewportMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var pos = e.GetPosition((UIElement)sender);
            var matrix = transform.Matrix;
            var scale = e.Delta > 0 ? 1.1 : 1 / 1.1;
            matrix.ScaleAt(scale, scale, pos.X, pos.Y);
            transform.Matrix = matrix;
        }
    }
}
