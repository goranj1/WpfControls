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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfTransition
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
            
                NewView.Content = new SecondWindow();
            NewView.Width = OldView.ActualWidth;
            NewView.Height = OldView.ActualHeight;

         
            
            var myTranslate = new TranslateTransform ();
            myTranslate.X =OldView.ActualWidth;
            myTranslate.Y = 0;
            var tg = new TransformGroup();
            tg.Children.Add(myTranslate);
     

            NewView.RenderTransform = tg;


           
            
            var duration = new Duration(new TimeSpan(0, 0, 0, 0, 590));
            var anim = new DoubleAnimation(-OldView.ActualWidth, duration);
       //     OldView.BeginAnimation(TranslateTransform.XProperty, anim);

            var sb = new Storyboard();
            sb.Children.Add(anim);

            Storyboard.SetTarget(anim, Cont);
            Storyboard.SetTargetProperty(anim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.X)"));

            sb.Begin();


        }
    }
}
