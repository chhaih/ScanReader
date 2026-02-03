using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Internal.Vectors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;

namespace ScanReader
{
    public partial class Form1 : Form
    {
        //private VideoCaptureDevice videoSource;
        private BarcodeReader barcodeReader;
        private bool isScanning;

        private VideoCapture capture;
        private Timer timer;
        private Mat frame;
        Bitmap curBitmap;
        int t = 0;
        string barcodeValue = null;

        public Form1()
        {
            InitializeComponent();
            InitializeBarcodeReader();
            InitializeWebcam();
        }

        private void InitializeBarcodeReader()
        {
            barcodeReader = new BarcodeReader
            {
                Options = new DecodingOptions
                {
                    PossibleFormats = new List<BarcodeFormat>
                {
                    BarcodeFormat.QR_CODE,
                    BarcodeFormat.CODE_128,
                    BarcodeFormat.EAN_13,
                    BarcodeFormat.DATA_MATRIX

                }
                }
            };
        }

        private void InitializeWebcam()
        {
            // 获取摄像头数量
            int cameraCount = 0;
            for (int i = 0; i < 10; i++)
            {
                using (var testCapture = new VideoCapture(i))
                {
                    if (testCapture.IsOpened())
                    {
                        comboBox1.Items.Add($"摄像头 {i}");
                        cameraCount++;
                        testCapture.Release();
                    }
                }
            }

            if (cameraCount > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "DAT文件|*.jpg|所有文件|*.*";// 设置文件类型过滤器
                openFileDialog.Title = "选择要上传的文件";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var fileName = openFileDialog.FileName;
                        pictureBox1.Image = Bitmap.FromFile(fileName);
                        //string result = TesseractOCR.RecognizeText(fileName);
                        //if (result != null)
                        //{
                        //    textBox1.Text = result;
                        //}
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"读取文件时出错: {ex.Message}");
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0) return;

            // 释放之前的摄像头
            if (capture != null)
            {
                capture.Release();
                capture.Dispose();
            }

            // 打开选中的摄像头
            capture = new VideoCapture(comboBox1.SelectedIndex);

            if (!capture.IsOpened())
            {
                MessageBox.Show("无法打开摄像头");
                return;
            }

            // 设置定时器更新画面
            timer = new Timer();
            timer.Interval = 33; // 约30fps
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (capture != null && capture.IsOpened())
                {
                    frame = new Mat();
                    capture.Read(frame);
                    if (!frame.Empty())
                    {
                        // 转换为Bitmap并显示
                        var oldImage = pictureBox1.Image;
                        curBitmap = frame.ToBitmap();
                        pictureBox1.Image = curBitmap;
                        if (oldImage != null)
                        {
                            oldImage.Dispose();
                        }

                        t += 1;
                        // 限制短时间内重复读取值
                        if (barcodeValue == null || barcodeValue == "")
                        {
                            Task.Run(() =>
                            {
                                ReaderBarcode(curBitmap);
                            });
                        }
                        else
                        {
                            if (t > 30)
                            {
                                Task.Run(() =>
                                {
                                    ReaderBarcode(curBitmap);
                                });
                                t = 0;
                            }
                        }

                        //if (t > 3000)
                        //{
                        //    t = 1;
                        //}
                    }
                }
            }
            catch
            {
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (frame != null)
                {
                    var oldImage = pictureBox1.Image;
                    var curBitmap = frame.ToBitmap();
                    pictureBox2.Image = curBitmap;
                    if (oldImage != null)
                    {
                        oldImage.Dispose();
                    }
                    Task.Run(() =>
                    {
                        ReaderBarcode(curBitmap);
                    });
                }
            }
            catch
            {
            }
        }

        // 解析条码过程
        private void ReaderBarcode(Bitmap bitmap)
        {
            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    var result = barcodeReader.Decode(bitmap);
                    if (result != null)
                    {
                        textBox_barcode.AppendText(result.Text + "\n\r");
                        barcodeValue = result.Text;
                        //t = 0;
                        //var oldImage = pictureBox2.Image;
                        //var curBitmap2 = (Bitmap)bitmap.Clone();
                        //pictureBox2.Image = curBitmap2;
                        //if (oldImage != null)
                        //{
                        //    oldImage.Dispose();
                        //}
                    }
                }
                catch
                {
                }

                //    // 解码条码                 

            }), null);
        }

        // 解析条码过程
        //private void ReaderBarcode2(Bitmap bitmap)
        //{
        //    this.BeginInvoke(new Action(() =>
        //    {
        //        // 解码条码                 
        //        var result = barcodeReader.Decode(bitmap);
        //        if (result != null)
        //        {
        //            textBox_barcode.AppendText(result.Text + "\n\r");
        //            var oldImage = pictureBox2.Image;
        //            var curBitmap2 = (Bitmap)bitmap.Clone();
        //            pictureBox2.Image = curBitmap2;
        //            if (oldImage != null)
        //            {
        //                oldImage.Dispose();
        //            }
        //            //pictureBox2.Image = bitmap;
        //        }
        //        else
        //        {
        //        }
        //    }), null);
        //}

        private void button_clear_Click(object sender, EventArgs e)
        {
            var oldImage = pictureBox2.Image;
            pictureBox2.Image = null;
            if (oldImage != null)
            {
                oldImage.Dispose();
            }
            textBox_barcode.Text = "";
        }

        private void btn_newFrm_Click(object sender, EventArgs e)
        {
            Form2 frm2 = new Form2();
            frm2.ShowDialog(); 
        }
    }
}
