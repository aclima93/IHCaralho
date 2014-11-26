﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using Coding4Fun.Kinect.Wpf.Controls;
using Microsoft.Research.Kinect.Nui;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;

using System.Xml;
using System.Windows.Markup;
using System.IO;
using System.Text;


namespace KinectingTheDotsUserControl
{
    public partial class MainWindow : Window
    {

        Runtime runtime = Runtime.Kinects[0];

        public enum game_states_t { MAIN_MENU, PLAY, PRACTICE, CHOOSE_AVATAR, NEW_SAVE_LOAD, GAME_ON };
        public game_states_t game_state = game_states_t.MAIN_MENU;

        private static double _topBoundary;
        private static double _bottomBoundary;
        private static double _leftBoundary;
        private static double _rightBoundary;
        private static double _itemLeft;
        private static double _itemTop;


      public MainWindow()
        {

            //Runtime runtime = new Runtime();

            //Runtime runtime;

            InitializeComponent();
            this.WindowState = System.Windows.WindowState.Maximized;
            this.WindowStyle = System.Windows.WindowStyle.None;

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Unloaded += new RoutedEventHandler(MainWindow_Unloaded);


            /*
            StreamReader mysr = new StreamReader("xaml");
            WindowContent.Content = (Page)XamlReader.Load(mysr.BaseStream);
            */

            
            MainMenu.Visibility = Visibility.Visible;
            NewSaveLoad.Visibility = Visibility.Collapsed;
            Play.Visibility = Visibility.Collapsed;
            /*
            MainMenu.Visibility = Visibility.Collapsed;
            NewSaveLoad.Visibility = Visibility.Visible;
            Play.Visibility = Visibility.Collapsed;
            */
            

            MainMenuItem1.Click += new RoutedEventHandler(MainMenuItem1_Click);
            MainMenuItem2.Click += new RoutedEventHandler(MainMenuItem2_Click);

            MainMenuItem3.Click += new RoutedEventHandler(MainMenuItem3_Click);
            MainMenuItem4.Click += new RoutedEventHandler(MainMenuItem4_Click);

            runtime.VideoFrameReady += runtime_VideoFrameReady;
            runtime.SkeletonFrameReady += runtime_SkeletonFrameReady;

        }

      private void transitionFromTo(UIElement from, UIElement to)
      {
          from.Visibility = Visibility.Collapsed;
          to.Visibility = Visibility.Visible;
      }

      private void checkMainMenuButtons() {

          CheckButton(MainMenuItem1, RightHand);
          CheckButton(MainMenuItem2, RightHand);
          CheckButton(MainMenuItem3, RightHand);
          CheckButton(MainMenuItem4, RightHand);

      }

      private void UnregisterEvents()
      {

          /*
          KinectSensor.KinectSensors.StatusChanged -= KinectSensors_StatusChanged;
          this.Kinect.SkeletonFrameReady -= Kinect_SkeletonFrameReady;
          this.Kinect.ColorFrameReady -= Kinect_ColorFrameReady; 
          */
           
      } 

        void MainMenuItem1_Click(object sender, RoutedEventArgs e)
        {

            SoundPlayer correct = new SoundPlayer("swoosh.wav");
            correct.Play();

            game_state = game_states_t.PLAY;

        }
        void MainMenuItem2_Click(object sender, RoutedEventArgs e)
        {

            SoundPlayer correct = new SoundPlayer("swoosh.wav");
            correct.Play();

            game_state = game_states_t.PRACTICE;

        }
        void MainMenuItem3_Click(object sender, RoutedEventArgs e)
        {

            SoundPlayer correct = new SoundPlayer("swoosh.wav");
            correct.Play();

            game_state = game_states_t.CHOOSE_AVATAR;

        }
        void MainMenuItem4_Click(object sender, RoutedEventArgs e)
        {

            SoundPlayer correct = new SoundPlayer("swoosh.wav");
            correct.Play();

            game_state = game_states_t.NEW_SAVE_LOAD;
            transitionFromTo(MainMenu, NewSaveLoad);
        }

      
       
        private static void CheckButton(HoverButton button, Ellipse thumbStick)
        {


            if (IsItemMidpointInContainer(button, thumbStick))
            {
                button.Hovering();
            }
            else
            {
                button.Release();
            }
        }
       

        public static bool IsItemMidpointInContainer(FrameworkElement container, FrameworkElement target)
        {
            FindValues(container, target);

            if (_itemTop < _topBoundary || _bottomBoundary < _itemTop)
            {
                //Midpoint of target is outside of top or bottom
                return false;
            }

            if (_itemLeft < _leftBoundary || _rightBoundary < _itemLeft)
            {
                //Midpoint of target is outside of left or right
                return false;
            }

            return true;
        }


        private static void FindValues(FrameworkElement container, FrameworkElement target)
        {
            var containerTopLeft = container.PointToScreen(new Point());
            var itemTopLeft = target.PointToScreen(new Point());

            _topBoundary = containerTopLeft.Y;
            _bottomBoundary = _topBoundary + container.ActualHeight;
            _leftBoundary = containerTopLeft.X;
            _rightBoundary = _leftBoundary + container.ActualWidth;

            //use midpoint of item (width or height divided by 2)
            _itemLeft = itemTopLeft.X + (target.ActualWidth / 2);
            _itemTop = itemTopLeft.Y + (target.ActualHeight / 2);
        }


        void runtime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonSet = e.SkeletonFrame;

            SkeletonData data = (from s in skeletonSet.Skeletons
                                 where s.TrackingState == SkeletonTrackingState.Tracked
                                 select s).FirstOrDefault();


            if (data != null)
            {
                SetEllipsePosition(RightHand, data.Joints[JointID.HandRight]);
            }

            if (game_state == game_states_t.MAIN_MENU)
            {
                checkMainMenuButtons();
            }

        }


        private void SetEllipsePosition(Ellipse ellipse, Joint joint)
        {

            Microsoft.Research.Kinect.Nui.Vector vector = new Microsoft.Research.Kinect.Nui.Vector();
            vector.X = ScaleVector(1280, joint.Position.X);
            vector.Y = ScaleVector(1024, -joint.Position.Y);
            vector.Z = joint.Position.Z;

            Joint updatedJoint = new Joint();
            updatedJoint.ID = joint.ID;
            updatedJoint.TrackingState = JointTrackingState.Tracked;
            updatedJoint.Position = vector;

            Canvas.SetLeft(ellipse, updatedJoint.Position.X);
            Canvas.SetTop(ellipse, updatedJoint.Position.Y);
        }

        private float ScaleVector(int length, float position)
        {
            float value = (((((float)length) / 1f) / 2f) * position) + (length / 2);
            if (value > length)
            {
                return (float)length;
            }
            if (value < 0f)
            {
                return 0f;
            }
            return value;
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            runtime.Uninitialize();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Since only a color video stream is needed, RuntimeOptions.UseColor is used.
            runtime.Initialize(Microsoft.Research.Kinect.Nui.RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking);

            //You can adjust the resolution here.
            runtime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution1280x1024, ImageType.Color);
        }

        void runtime_VideoFrameReady(object sender, Microsoft.Research.Kinect.Nui.ImageFrameReadyEventArgs e)
        {
            PlanarImage image = e.ImageFrame.Image;
        
            BitmapSource source = BitmapSource.Create(image.Width, image.Height, 0, 0,
                PixelFormats.Bgr32, null, image.Bits, image.Width * image.BytesPerPixel);
            videoImage.Source = source;
        }


    }
}
