using DistanceSensorAppDemo;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
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

            ((MainViewModel)this.DataContext).CirclePoints = circles;
           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            PrintDialog printDialog = new PrintDialog();// 获取窗口的宽度和高度
            // double windowWidth = this.ActualWidth;
            // double windowHeight = this.ActualHeight;
            //
            // // 创建一个RenderTargetBitmap对象，将窗口转换为位图
            // RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)windowWidth, (int)windowHeight, 96, 96, PixelFormats.Pbgra32);
            // renderBitmap.Render(this);
            //
            // // 创建一个Image控件，将RenderTargetBitmap设置为其Source
            // Image printImage = new Image();
            // printImage.Source = renderBitmap;
            // printImage.Stretch = Stretch.None;
            // printImage.HorizontalAlignment = HorizontalAlignment.Left;
            // printImage.VerticalAlignment = VerticalAlignment.Top;
            //
            // // 创建一个DrawingVisual对象
            // DrawingVisual drawingVisual = new DrawingVisual();
            //
            // // 在DrawingVisual中绘制Image控件
            // using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            // {
            //     drawingContext.DrawImage(renderBitmap, new Rect(new Point(0, 0), new Size(windowWidth, windowHeight)));
            // }

            // 将DrawingVisual对象传递给打印机
            printDialog.PrintVisual(this, "打印当前窗口");

            printDialog.ShowDialog();
        }
    }
}
