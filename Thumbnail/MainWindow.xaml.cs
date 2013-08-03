using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Thumbnail
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (txtFolder.Text.Length == 0)
            {
                MessageBox.Show("您先选择一个路径，然后填写缩略图的高或者宽，高和宽至少指定一个。");
                return;
            }
            int width = 0;
            int height = 0;
            if (txtWidth.Text.Length > 0)
            {
                width = Int32.Parse(txtWidth.Text);
            }
            if (txtHeight.Text.Length > 0)
            {
                height = Int32.Parse(txtHeight.Text);
            }
            if (width == 0 && height == 0)
            {
                MessageBox.Show("必须指定生成缩略图的宽或者高。");
                return;
            }

            int toWidth = width;
            int toHeight = height;
            
            StringCollection sc = GetAllFiles(txtFolder.Text, null);
            StringBuilder sb = new StringBuilder("");
            foreach (string path in sc)
            {
                toWidth = width;
                toHeight = height;
                using (System.Drawing.Image originalImage = System.Drawing.Image.FromFile(path))
                {
                    if (toWidth > originalImage.Width)
                    {
                        continue;
                    }
                    if (toHeight > originalImage.Height)
                    {
                        continue;
                    }
                    if (toWidth == 0)
                    {
                        toWidth = originalImage.Width * toHeight / originalImage.Height;
                    }
                    if (toHeight == 0)
                    {
                        toHeight = originalImage.Height * toWidth / originalImage.Width;
                    }
                    string fileExtension = GetFileExtension(path);
                    string destinationPath = path.Replace(fileExtension, "_s_mobile_" + width.ToString() + fileExtension);
                    sb.Append(destinationPath).Append('\r').Append('\n');

                    SaveThumbnail(originalImage, toWidth, toHeight, destinationPath, fileExtension);
                }
            }

            txtHtml.Text = sb.ToString();
        }

        #region Helper

        

        public static StringCollection GetAllFiles(string path, StringCollection sc)
        {
            if (sc == null)
            {
                sc = new StringCollection();
            }
            string[] fileList = Directory.GetFileSystemEntries(path);
            // 遍历所有的文件和目录
            foreach (string file in fileList)
            {
                // 先当作目录处理，如果存在这个目录就递归获取该目录下面的文件
                if (Directory.Exists(file))
                {
                    //string pathName = System.IO.Path.GetFileName(file).ToLower();
                    GetAllFiles(file, sc);
                }
                else
                {
                    // 如果是文件，仅取图片文件
                    if (file.Contains("_s_mobile_"))
                    {
                        continue;
                    }
                    string fileExtension = GetFileExtension(file);
                    if (!String.IsNullOrEmpty(fileExtension))
                    {
                        if (ContainString(fileExtension.ToLower(), ".jpg", ".jpeg", ".png", ".gif", ".bmp"))
                        {
                            sc.Add(file);
                        }
                    }
                }
            }
            return sc;
        }

        public static bool ContainString(string s, params string[] stringSet)
        {
            bool ret = false;
            foreach (string element in stringSet)
            {
                if (element.ToLower() == s.ToLower())
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        public static string GetFileExtension(string fileName)
        {
            int length = fileName.Length;
            int startIndex = length;
            while (--startIndex >= 0)
            {
                char ch = fileName[startIndex];
                if (ch == '.')
                {
                    return fileName.Substring(startIndex, length - startIndex);
                }
                if (((ch == System.IO.Path.DirectorySeparatorChar) || (ch == System.IO.Path.AltDirectorySeparatorChar)) || (ch == System.IO.Path.VolumeSeparatorChar))
                {
                    break;
                }
            }
            return string.Empty;
        }
        #endregion

        private static void SaveThumbnail(System.Drawing.Image originBitmap, int width, int height, string directory, string filename, string extension)
        {
            string destinationPath = directory + filename + extension;
            SaveThumbnail(originBitmap, width, height, destinationPath, extension);
        }

        private static void SaveThumbnail(System.Drawing.Image originBitmap, int width, int height, string destinationPath, string extension)
        {
            using (var newImage = new Bitmap(width, height))
            {
                using (var graphic = GetGraphic(originBitmap, newImage))
                {
                    graphic.DrawImage(originBitmap, 0, 0, width, height);
                    using (var encoderParameters = new EncoderParameters(1))
                    {
                        encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                        newImage.Save(destinationPath,
                                    ImageCodecInfo.GetImageEncoders()
                                        .Where(x => x.FilenameExtension.Contains(extension.ToUpperInvariant()))
                                        .FirstOrDefault(),
                                    encoderParameters);
                    }
                }
            }
        }

        private static Graphics GetGraphic(System.Drawing.Image originImage, Bitmap newImage)
        {
            newImage.SetResolution(originImage.HorizontalResolution, originImage.VerticalResolution);
            var graphic = Graphics.FromImage(newImage);
            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            return graphic;
        }

        private void txtSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtFolder.Text  = folderDialog.SelectedPath;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (txtFolder.Text.Length == 0)
            {
                MessageBox.Show("注意这是一个危险操作，将删除本程序生成的缩略图， 您先选择一个路径吧。");
                return;
            }

            StringCollection sc = GetAllDeleteFiles(txtFolder.Text, null);
            StringBuilder sb = new StringBuilder("");
            foreach (string path in sc)
            {
                File.Delete(path);

                sb.Append(path).Append('\r').Append('\n');
                
            }
            txtHtml.Text = sb.ToString();

        }

        public static StringCollection GetAllDeleteFiles(string path, StringCollection sc)
        {
            if (sc == null)
            {
                sc = new StringCollection();
            }
            string[] fileList = Directory.GetFileSystemEntries(path);
            // 遍历所有的文件和目录
            foreach (string file in fileList)
            {
                // 先当作目录处理，如果存在这个目录就递归获取该目录下面的文件
                if (Directory.Exists(file))
                {
                    GetAllDeleteFiles(file, sc);
                }
                else
                {
                    // 如果是文件，仅取图片文件
                    if (file.Contains("_s_mobile_"))
                    {
                        sc.Add(file);
                    }
                }
            }
            return sc;
        }

        private void miGetAll_Click(object sender, RoutedEventArgs e)
        {
            StringCollection sc = GetFiles(txtFolder.Text, null);
            
            StringBuilder sb = new StringBuilder("");
            foreach (string path in sc)
            {
                sb.Append(path).Append('\r').Append('\n');

            }
            txtHtml.Text = sb.ToString();
            MessageBox.Show("搞定");

        }

        public static StringCollection GetFiles(string path, StringCollection sc)
        {
            if (sc == null)
            {
                sc = new StringCollection();
            }
            string[] fileList = Directory.GetFileSystemEntries(path);
            // 遍历所有的文件和目录
            foreach (string file in fileList)
            {
                // 先当作目录处理，如果存在这个目录就递归获取该目录下面的文件
                if (Directory.Exists(file))
                {
                    GetFiles(file, sc);
                }
                else
                {
                    // 如果是文件，仅取图片文件
                    sc.Add(file);
                }
            }
            return sc;
        }
    }
}
