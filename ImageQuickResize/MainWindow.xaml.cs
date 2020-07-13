using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageQuickResize
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int angleStep = 90;
        int currentAngle = 0;

        string parentImageFolder = string.Empty;
        FileInfo _fileInfo;

        public MainWindow()
        {
            InitializeComponent();

            InitializeControlEvents();
            InitializeControls();
        }

        void InitializeControls()
        {
            ResizeOption_Click(rbAvatar, null);
        }

        void InitializeControlEvents()
        {
            rbAvatar.Click += ResizeOption_Click;
            rbMsgBrd.Click += ResizeOption_Click;
            rb800.Click += ResizeOption_Click;
            btnBrowse.Click += btnBrowse_Click;
            btnResize.Click += btnResize_Click;

            img.Drop += img_Drop;
            this.Drop += img_Drop;
        }

        void img_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                this._fileInfo = new FileInfo(files[0]);
                this.parentImageFolder = this._fileInfo.Directory.FullName;
                img.Source = new Func<BitmapImage>(() =>
                {
                    var bi = new BitmapImage(new Uri(this._fileInfo.FullName));
                    lbDropImageHere.Visibility = Visibility.Collapsed;

                    return bi;
                }).Invoke();
            }
        }

        void btnResize_Click(object sender, RoutedEventArgs e)
        {
            if (img.Source == null)
            {
                return;
            }

            if (Convert.ToDouble(txWidth.Text) < 1) txWidth.Text = "1";
            if (Convert.ToDouble(txHeight.Text) < 1) txHeight.Text = "1";

            ImageSource imgHolder = img.Source;

            var ratioW = Convert.ToDouble(txWidth.Text) / imgHolder.Width;
            var ratioH = Convert.ToDouble(txHeight.Text) / imgHolder.Height;

            var ratio = ratioW < ratioH ? ratioW : ratioH;

            var newWidth = imgHolder.Width * ratio;
            var newHeight = imgHolder.Height * ratio;

            BitmapSource newImg = CreateResizedImage(imgHolder, (int)newWidth, (int)newHeight, 0, currentAngle);
            //newImg.DownloadCompleted += (a, b) =>
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                //Guid photoID = System.Guid.NewGuid();
                string photoID = Path.GetFileNameWithoutExtension(this._fileInfo.Name);
                photoID = string.Format("{0}_{1}_{2}x{3}.jpg", photoID, "resize", txWidth.Text, txHeight.Text);
                String photolocation = photoID.ToString();  //file name 
                photolocation = this.parentImageFolder + "\\" + photolocation;

                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)newImg));

                using (var filestream = new FileStream(photolocation, FileMode.Create))
                {
                    encoder.Save(filestream);
                }

                MessageBox.Show($"Resized image saved as '{photolocation}'");
            };
        }

        void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JPEG Files|*.jpg;PNG Files|*.png;BMP Files|*.bmp";
            ofd.Title = "Open Image";
            ofd.ShowDialog();
            if (ofd.FileName != string.Empty)
            {
                this._fileInfo = new FileInfo(ofd.FileName);
                this.parentImageFolder = this._fileInfo.Directory.FullName;
                img.Source = new BitmapImage(new Uri(ofd.FileName));;
            }
            lbDropImageHere.Visibility = Visibility.Collapsed;
        }

        void ResizeOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender == rbAvatar)
            {
                txWidth.Text = "90";
                txHeight.Text = "90";
            }
            else if (sender == rbMsgBrd)
            {
                txWidth.Text = "640";
                txHeight.Text = "480";
            }
            else if(sender == rb800)
            {
                txWidth.Text = "800";
                txHeight.Text = "800";
            }
        }

        private static BitmapFrame CreateResizedImage(ImageSource source, int width, int height, int margin, int angle = 0)
        {
            var rect = new Rect(margin, margin, width - margin * 2, height - margin * 2);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            if (angle != 0)
            {
                drawingVisual.Transform = new RotateTransform(angle, width / 2, height / 2);
            }

            var resizedImage = new RenderTargetBitmap(
                width, height,         // Resized dimensions
                96, 96,                // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return BitmapFrame.Create(resizedImage);
        }

        private void rotateLeft_Click(object sender, RoutedEventArgs e)
        {
            currentAngle -= angleStep;
            if (currentAngle == 0) currentAngle = 360;
            img.RenderTransform = new RotateTransform(currentAngle);
        }

        private void rotateRight_Click(object sender, RoutedEventArgs e)
        {
            currentAngle += angleStep;
            if (currentAngle == 360) currentAngle = 0;
            img.RenderTransform = new RotateTransform(currentAngle);
        }
    }
}
