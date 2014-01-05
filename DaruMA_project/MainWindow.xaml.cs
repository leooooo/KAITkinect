using System;
using System.Collections.Generic;
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
using Microsoft.Kinect;
using System.IO;

namespace DaruMA_project
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Kinectを扱うためのオブジェクト
        /// </summary>
        KinectSensor sensor;

        /// <summary>
        /// Kinectセンサーからの画像情報を受け取る
        /// </summary>
        //追加事項
        private byte[] colorPixels;

        /// <summary>
        /// 画面に表示するビットマップ
        /// </summary>
        //追加事項
        private WriteableBitmap colorBitmap;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 画面がロードされたときに呼び出される。
        /// 初期化の処理はここに記入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onWindowLoaded(object sender, RoutedEventArgs e)
        {
            #region 接続の確認
            //Kinectセンサーの接続を確認
            //接続が確認されたセンサーがあれば
            //それを扱うことにして処理を抜ける。
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }
            #endregion

            #region 接続の有無による分岐
            //Kinectが確認できなかったら
            if (null == this.sensor)
            {

            }

            //Kinecが接続されたときの処理
            if (null != this.sensor)
            {
                //RGBカメラの使用。カラーストリームの準備
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                //距離カメラの使用。デプスストリームの準備
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                //骨格情報の使用。スケルトンストリームの準備
                this.sensor.SkeletonStream.Enable();

                //全てのイベントを一括で処理
                this.sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);

                #region カラー情報の初期化
                //バッファの初期化
                colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth,
                                                  sensor.ColorStream.FrameHeight,
                                                  96.0, 96.0, PixelFormats.Bgr32, null);
                this.MainImage.Source = colorBitmap;

                #endregion

                //センサー作動
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
            #endregion
        }

        /// <summary>
        /// Kinectセンサーのすべての処理をここに記述
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            #region Color系の処理
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (imageFrame != null)
                {
                    //画像情報の幅・高さ取得
                    int frmWidth = imageFrame.Width;
                    int frmHeight = imageFrame.Height;

                    //画像情報をバッファにコピー
                    imageFrame.CopyPixelDataTo(colorPixels);
                    //ビットマップに描画
                    Int32Rect src = new Int32Rect(0, 0, frmWidth, frmHeight);
                    colorBitmap.WritePixels(src, colorPixels, frmWidth * 4, 0);
                }
            }
            #endregion
        }
    }
}
