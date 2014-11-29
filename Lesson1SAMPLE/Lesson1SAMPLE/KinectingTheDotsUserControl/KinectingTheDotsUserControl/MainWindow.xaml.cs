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
//using Microsoft.Kinect;

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

using Microsoft.Win32;



namespace KinectingTheDotsUserControl
{
    public partial class MainWindow : Window
    {

        Runtime runtime = Runtime.Kinects[0];

        public enum game_states_t { MAIN_MENU, PLAY, PRACTICE, CHOOSE_AVATAR, NEW_SAVE_LOAD, GAME_ON };
        private game_states_t game_state = game_states_t.MAIN_MENU;

        public string[] game_file;

        public const bool DEBUG = true;

        public SoundPlayer transition = new SoundPlayer("swoosh.wav");
        public SoundPlayer selection = new SoundPlayer("selection-click.wav");

        public Ball ball;

        public int renderingCounter = 0;
        public const int renderingStep = 10;

        public const int screen_width = 1366;
        public const int screen_height = 768;


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


            xamlMainMenu.setMainWindow(this);
            xamlChooseAvatar.setMainWindow(this);
            xamlNewSaveLoad.setMainWindow(this);
            xamlPractice.setMainWindow(this);


            // Hide your kids, hide your wife
            xamlMainMenu.Visibility = Visibility.Visible;

            Play.Visibility = Visibility.Collapsed;
            xamlPractice.Visibility = Visibility.Collapsed;
            GameOn.Visibility = Visibility.Collapsed;
            xamlChooseAvatar.Visibility = Visibility.Collapsed;
            xamlNewSaveLoad.Visibility = Visibility.Collapsed;


            activateHandlersFor(game_state);


            runtime.VideoFrameReady += runtime_VideoFrameReady;
            runtime.SkeletonFrameReady += runtime_SkeletonFrameReady;

            Random r = new Random();

            float dx = 100;//r.Next(-1000, 1000)/1000;
            float dy = 100;//r.Next(-1000, 1000)/1000;
            float dz = 0;//r.Next(-1000, 1000)/1000;

            float radius = 64 / 2; //hardcoded because fuck you, that's why

            // because we use fullscreen. fuck you.
            int window_width = screen_width;
            int window_height = screen_height;

            float x = 0;//r.Next(-window_width/2, window_width/2);
            float y = 0;//r.Next(-window_height / 2, window_height / 2);
            float z = 5; // to be tweeked?

            float distLR = screen_width * 100;
            float distUD = screen_height * 100;
            float distFB = 10;

            ball = new Ball(x, y, z, dx, dy, dz, radius, screen_width, screen_height, window_width, window_height, distLR, distUD, distFB);

        }

        public void updateBall()
        {
            ball.updatePosition();
            ball.checkWallCollisions();
            //ball.checkJointCollision();

            /*
            renderingCounter += 1;
            if( (renderingCounter%renderingStep) == 0)
            {
            */
            
            Ball_2D.Height = ball.getSize();
            Ball_2D.Width = Ball_2D.Height;
            

            float x = ball.getX2D();
            float y = ball.getY2D();

            Canvas.SetLeft(Ball_2D, x);
            Canvas.SetTop(Ball_2D, y);

            //}
        }

        private void drawSkeleton1(SkeletonData data)
        {
            updateBall();

            //SetEllipsePosition(Ball_2D, data.Joints[JointID.HandRight]);

            //if (DEBUG) Console.WriteLine("Ball at: x={0} y={1}, size={2}", ball.getX2D(), ball.getY2D(), ball.getSize());
           
            if (DEBUG)
            {
                
                SetEllipsePosition(P1J1, data.Joints[JointID.AnkleLeft]);
                SetEllipsePosition(P1J2, data.Joints[JointID.AnkleRight]);

                SetEllipsePosition(P1J3, data.Joints[JointID.ElbowLeft]);
                SetEllipsePosition(P1J4, data.Joints[JointID.ElbowRight]);

                SetEllipsePosition(P1J5, data.Joints[JointID.FootLeft]);
                SetEllipsePosition(P1J6, data.Joints[JointID.FootRight]);

                SetEllipsePosition(P1J7, data.Joints[JointID.HandLeft]);
                SetEllipsePosition(P1J8, data.Joints[JointID.HandRight]);

                SetEllipsePosition(P1J9, data.Joints[JointID.Head]);

                SetEllipsePosition(P1J10, data.Joints[JointID.HipCenter]);
                SetEllipsePosition(P1J11, data.Joints[JointID.HipLeft]);
                SetEllipsePosition(P1J12, data.Joints[JointID.HipRight]);

                SetEllipsePosition(P1J13, data.Joints[JointID.KneeLeft]);
                SetEllipsePosition(P1J14, data.Joints[JointID.KneeRight]);

                SetEllipsePosition(P1J15, data.Joints[JointID.ShoulderCenter]);
                SetEllipsePosition(P1J16, data.Joints[JointID.ShoulderLeft]);
                SetEllipsePosition(P1J17, data.Joints[JointID.ShoulderRight]);

                SetEllipsePosition(P1J18, data.Joints[JointID.Spine]);

                SetEllipsePosition(P1J19, data.Joints[JointID.WristLeft]);
                SetEllipsePosition(P1J20, data.Joints[JointID.WristRight]);
            }

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

                //if (game_state != game_states_t.GAME_ON)
                if (game_state == game_states_t.PLAY || game_state == game_states_t.PRACTICE)
                {
                    RightHand.Visibility = Visibility.Collapsed;
                    drawSkeleton1(data);
                }
                else
                {
                    RightHand.Visibility = Visibility.Visible;
                }

            }

            checkGameStateButtons(game_state);

        }


        private void SetEllipsePosition(Ellipse ellipse, Joint joint)
        {

            Microsoft.Research.Kinect.Nui.Vector vector = new Microsoft.Research.Kinect.Nui.Vector();
            vector.X = ScaleVector(screen_width, joint.Position.X);
            vector.Y = ScaleVector(screen_height, -joint.Position.Y);
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

            if (DEBUG)
            {
                //xamlPractice.videoImage.Source = source;
            }
        }


        public void CheckButton(HoverButton button, Ellipse thumbStick)
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

        public void changeGameState(game_states_t new_state, UIElement from, UIElement to)
        {
            deactivateHandlersFor(game_state);
            game_state = new_state;
            transitionFromTo(from, to);
            activateHandlersFor(game_state);
        }

        private void checkGameStateButtons(game_states_t game_state)
        {
            if (game_state == game_states_t.MAIN_MENU)
            {
                xamlMainMenu.checkMainMenuButtons();
            }
            else if (game_state == game_states_t.CHOOSE_AVATAR)
            {
                xamlChooseAvatar.checkChooseAvatarButtons();
            }
            else if (game_state == game_states_t.NEW_SAVE_LOAD)
            {
                xamlNewSaveLoad.checkNewSaveLoadButtons();
            }
            else if (game_state == game_states_t.PRACTICE)
            {
                xamlPractice.checkPracticeButtons();
            }
        }

        private void activateHandlersFor(game_states_t state)
        {

            if (DEBUG) Console.WriteLine("Activating event handlers for game state: {0}", state);

            if (state == game_states_t.MAIN_MENU)
            {
                xamlMainMenu.setMainMenuHandlers();
            }
            else if (state == game_states_t.CHOOSE_AVATAR)
            {
                xamlChooseAvatar.setChooseAvatarHandlers();
            }
            else if (state == game_states_t.NEW_SAVE_LOAD)
            {
                xamlNewSaveLoad.setNewSaveLoadHandlers();
            }
            else if (state == game_states_t.PRACTICE)
            {
                xamlPractice.setPracticeHandlers();
            }

        }

        private void deactivateHandlersFor(game_states_t state)
        {

            if (DEBUG) Console.WriteLine("Deactivating event handlers for game state: {0}", state);

            if (state == game_states_t.MAIN_MENU)
            {
                xamlMainMenu.removeMainMenuHandlers();
            }
            else if (state == game_states_t.CHOOSE_AVATAR)
            {
                xamlChooseAvatar.removeChooseAvatarHandlers();
            }
            else if (state == game_states_t.NEW_SAVE_LOAD)
            {
                xamlNewSaveLoad.removeNewSaveLoadHandlers();
            }
            else if (state == game_states_t.PRACTICE)
            {
                xamlPractice.removePracticeHandlers();
            }

        }

        private void transitionFromTo(UIElement from, UIElement to)
        {
            from.Visibility = Visibility.Collapsed;
            to.Visibility = Visibility.Visible;
        }


    }
}
