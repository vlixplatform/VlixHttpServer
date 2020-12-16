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
using System.ComponentModel;
using System.Threading;
using MaterialIcons;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace Vlix.ServerConfigUI
{
    /* EXAMPLE:
     * 
        <OPLibWPF:OPFrame x:Name="opFrameIncomingData"  BorderThickness="0" CornerRadius="5"  TopLeftContent="Incoming Data" TopLeftIcon="ic_network_wifi"
                                    InnerWidth="600" InnerMargin="10"  InnerWindowMargin="0,50" Visibility="Collapsed" Background="#aa000000">
            <OPLibWPF:OPBorder Margin="0,5,0,0" >
                <local:UCIncomingData Margin="5" />
            </OPLibWPF:OPBorder>
        </OPLibWPF:OPFrame>

     */
    public class OPFrame : ButtonBase
    {
        public OPFrame()
        {
            this.Loaded += new RoutedEventHandler(this.OPFrameLoaded);
            this.IsVisibleChanged += (s,e) =>
            {
                if (s is OPFrame oPFrame) oPFrame.IsShown = oPFrame.Visibility == Visibility.Visible;
            };
        }


        Window _ParentWindow;
        Storyboard StoryboardShow, StoryboardClose;
        public virtual void OPFrameLoaded(object sender, RoutedEventArgs e)
        {
            //if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
            //IsShown = this.Visibility == Visibility.Visible;
            //this.IsVisibleChanged += (s, e2) => { IsShown = this.Visibility == Visibility.Visible; };
            this.CommandBindings.Add(new CommandBinding(OPFrame.CloseCommand, OnCloseMe));
            this.CommandBindings.Add(new CommandBinding(OPFrame.AdditionalButtonClickCommand, OnAdditionalButtonClickMe));
            _ParentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (_ParentWindow == null) return;
            //_ParentWindow.KeyDown -= _ParentWindow_KeyDown;
            if (this.EnableEscapeKey) _ParentWindow.KeyDown += _ParentWindow_KeyDown;
            
            StoryboardShow = (Storyboard)this.FindResource("sbOPFrameShow");
            StoryboardShow = StoryboardShow.Clone();
            StoryboardClose = (Storyboard)this.FindResource("sbOPFrameClose");
            StoryboardClose = StoryboardClose.Clone();

            Storyboard.SetTarget(StoryboardShow, this);
            Storyboard.SetTarget(StoryboardClose, this);
        }
        private void _ParentWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { this.RaiseOnCloseEvent();  }
        }

        private void OnCloseMe(object sender, ExecutedRoutedEventArgs e)
        {
            RaiseOnCloseEvent();
        }
        private void OnAdditionalButtonClickMe(object sender, ExecutedRoutedEventArgs e)
        {
            RaiseOnAdditionalButtonClickEvent();
        }
        //public void Show() { this.Shown = true; }
        void StoryboardShow_Completed(object sender, EventArgs e)
        {
            RaiseOnShowEvent();
        }

        //public void Close()
        //{
        //    this.Shown = false;
        //    //this.StoryboardClose.Completed -= StoryboardClose_Completed;
        //    //this.StoryboardClose.Completed += StoryboardClose_Completed;
        //}
        

        #region Dependency Properties    
        public Visibility TopLeftIconVisibility
        {
            get { return (Visibility)GetValue(TopLeftIconVisibilityProperty); }
            set { SetValue(TopLeftIconVisibilityProperty, value); }
        }
        public static readonly DependencyProperty TopLeftIconVisibilityProperty = DependencyProperty.Register("TopLeftIconVisibility", typeof(Visibility), typeof(OPFrame),
            new PropertyMetadata(Visibility.Visible));

        public MaterialIconType TopLeftIcon
        {
            get { return (MaterialIconType)GetValue(TopLeftIconProperty); }
            set { SetValue(TopLeftIconProperty, value); }
        }
        public static readonly DependencyProperty TopLeftIconProperty = DependencyProperty.Register("TopLeftIcon", typeof(MaterialIconType), typeof(OPFrame),
            new PropertyMetadata(MaterialIconType.ic_none));

        public object TopLeftContent
        {
            get { return (object)GetValue(TopLeftContentProperty); }
            set { SetValue(TopLeftContentProperty, value); }
        }
        public static readonly DependencyProperty TopLeftContentProperty = DependencyProperty.Register("TopLeftContent", typeof(object), typeof(OPFrame), new UIPropertyMetadata(null));

        public double TopLeftFontSize
        {
            get { return (double)GetValue(TopLeftFontSizeProperty); }
            set { SetValue(TopLeftFontSizeProperty, value); }
        }
        public static readonly DependencyProperty TopLeftFontSizeProperty = DependencyProperty.Register("TopLeftFontSize", typeof(double), typeof(OPFrame),
            new PropertyMetadata((double)20));


        public double AdditionalButtonWidth
        {
            get { return (double)GetValue(AdditionalButtonWidthProperty); }
            set { SetValue(AdditionalButtonWidthProperty, value); }
        }
        public static readonly DependencyProperty AdditionalButtonWidthProperty = DependencyProperty.Register("AdditionalButtonWidth", typeof(double), typeof(OPFrame),
            new PropertyMetadata((double)180));
        
        public Thickness InnerWindowMargin
        {
            get { return (Thickness)GetValue(InnerWindowMarginProperty); }
            set { SetValue(InnerWindowMarginProperty, value); }
        }
        public static readonly DependencyProperty InnerWindowMarginProperty = DependencyProperty.Register("InnerWindowMargin", typeof(Thickness), typeof(OPFrame),
            new PropertyMetadata(new Thickness(0)));


        public Thickness InnerMargin
        {
            get { return (Thickness)GetValue(InnerMarginProperty); }
            set { SetValue(InnerMarginProperty, value); }
        }
        public static readonly DependencyProperty InnerMarginProperty = DependencyProperty.Register("InnerMargin", typeof(Thickness), typeof(OPFrame),
            new PropertyMetadata(new Thickness(0)));




        public bool Shown { get { return (bool)GetValue(ShownProperty); } set { SetValue(ShownProperty, value); } }
        public static readonly DependencyProperty ShownProperty = DependencyProperty.Register("Shown", typeof(bool),
            typeof(OPFrame), new FrameworkPropertyMetadata(false, OnShownChanged));

        public static void OnShownChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //if (DesignerProperties.GetIsInDesignMode(sender)) { return; }
            if (sender is OPFrame oPFrame)
            {

                if (oPFrame.StoryboardShow == null)
                {
                    if (oPFrame.Shown)
                    {
                        if (oPFrame.IsShown) return; else oPFrame.IsShown = true;
                        oPFrame.Opacity = 1;
                        oPFrame.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (!oPFrame.IsShown) return; else oPFrame.IsShown = false;
                        oPFrame.Opacity = 0;
                        oPFrame.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    if (oPFrame.Shown)
                    {
                        if (oPFrame.IsShown) return; else oPFrame.IsShown = true;

                        oPFrame.StoryboardShow.Completed -= oPFrame.StoryboardShow_Completed;
                        oPFrame.StoryboardShow.Completed += oPFrame.StoryboardShow_Completed;
                        oPFrame.StoryboardShow?.Begin();
                    }
                    else
                    {
                        if (!oPFrame.IsShown) return; else oPFrame.IsShown = false;
                        oPFrame.StoryboardClose?.Begin();
                        oPFrame.RaiseOnCloseEvent();
                    }
                }
            }
        }


        public bool IsShown
        {
            get { return (bool)GetValue(IsShownReadOnlyPropProperty); }
            protected set { SetValue(IsShownReadOnlyPropPropertyKey, value); }
        }
        private static readonly DependencyPropertyKey IsShownReadOnlyPropPropertyKey = DependencyProperty.RegisterReadOnly("IsShown", typeof(bool), typeof(OPFrame),
             new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));
        public static readonly DependencyProperty IsShownReadOnlyPropProperty = IsShownReadOnlyPropPropertyKey.DependencyProperty;


        public MaterialIconType AdditionalButtonIcon { get { return (MaterialIconType)GetValue(AdditionalButtonIconProperty); } set { SetValue(AdditionalButtonIconProperty, value); } }
        public static readonly DependencyProperty AdditionalButtonIconProperty = DependencyProperty.Register("AdditionalButtonIcon", typeof(MaterialIconType),
            typeof(OPFrame), new PropertyMetadata(MaterialIconType.ic_none));

        public string AdditionalButtonText { get { return (string)GetValue(AdditionalButtonTextProperty); } set { SetValue(AdditionalButtonTextProperty, value); } }
        public static readonly DependencyProperty AdditionalButtonTextProperty = DependencyProperty.Register("AdditionalButtonText", typeof(string),
            typeof(OPFrame), new PropertyMetadata("Additional Button"));

        public bool ShowAdditionalButton { get { return (bool)GetValue(ShowAdditionalButtonProperty); } set { SetValue(ShowAdditionalButtonProperty, value); } }
        public static readonly DependencyProperty ShowAdditionalButtonProperty = DependencyProperty.Register("ShowAdditionalButton", typeof(bool),
            typeof(OPFrame), new PropertyMetadata(false));

        public bool ShowBackButton { get { return (bool)GetValue(ShowBackButtonProperty); } set { SetValue(ShowBackButtonProperty, value); } }
        public static readonly DependencyProperty ShowBackButtonProperty = DependencyProperty.Register("ShowBackButton", typeof(bool),
            typeof(OPFrame), new PropertyMetadata(true));

        public bool EnableEscapeKey { get { return (bool)GetValue(EnableEscapeKeyProperty); } set { SetValue(EnableEscapeKeyProperty, value); } }
        public static readonly DependencyProperty EnableEscapeKeyProperty = DependencyProperty.Register("EnableEscapeKey", typeof(bool),
            typeof(OPFrame), new PropertyMetadata(false));


        public bool ShowCloseButton { get { return (bool)GetValue(ShowCloseButtonProperty); } set { SetValue(ShowCloseButtonProperty, value); } }
        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register("ShowCloseButton", typeof(bool),
            typeof(OPFrame), new PropertyMetadata(true));


        //public new Brush OverlayBackground
        //{
        //    get { return (Brush)GetValue(OverlayBackgroundProperty); }
        //    set { SetValue(OverlayBackgroundProperty, value); }
        //}
        //public new static readonly DependencyProperty OverlayBackgroundProperty = DependencyProperty.Register("OverlayBackground", typeof(Brush), typeof(OPFrame),
        //    new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))));


        public Brush InnerBackground
        {
            get { return (Brush)GetValue(InnerBackgroundProperty); }
            set { SetValue(InnerBackgroundProperty, value); }
        }
        public static readonly DependencyProperty InnerBackgroundProperty = DependencyProperty.Register("InnerBackground", typeof(Brush), typeof(OPFrame),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))));


        public Brush CloseXButtonBrush
        {
            get { return (Brush)GetValue(CloseXButtonBrushProperty); }
            set { SetValue(CloseXButtonBrushProperty, value); }
        }
        public static readonly DependencyProperty CloseXButtonBrushProperty = DependencyProperty.Register("CloseXButtonBrush", typeof(Brush), typeof(OPFrame),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 131, 131, 131))));


        public double InnerWidth
        {
            get { return (double)GetValue(InnerWidthProperty); }
            set { SetValue(InnerWidthProperty, value); }
        }
        public static readonly DependencyProperty InnerWidthProperty = DependencyProperty.Register("InnerWidth", typeof(double), typeof(OPFrame),
            new PropertyMetadata(double.NaN));



        public double InnerHeight
        {
            get { return (double)GetValue(InnerHeightProperty); }
            set { SetValue(InnerHeightProperty, value); }
        }
        public static readonly DependencyProperty InnerHeightProperty = DependencyProperty.Register("InnerHeight", typeof(double), typeof(OPFrame),
            new PropertyMetadata(double.NaN));



        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(OPFrame),
            new PropertyMetadata(new CornerRadius(0)));


        public static readonly RoutedEvent OnShowEvent = EventManager.RegisterRoutedEvent("OnShow", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(OPFrame));
        public event RoutedEventHandler OnShow
        {
            add { AddHandler(OnShowEvent, value); }
            remove { RemoveHandler(OnShowEvent, value); }
        }
        void RaiseOnShowEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(OPFrame.OnShowEvent);
            RaiseEvent(newEventArgs);
        }

        public static readonly RoutedEvent OnCloseEvent = EventManager.RegisterRoutedEvent("OnClose", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(OPFrame));
        public event RoutedEventHandler OnClose
        {
            add { AddHandler(OnCloseEvent, value); }
            remove { RemoveHandler(OnCloseEvent, value); }
        }
        void RaiseOnCloseEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(OPFrame.OnCloseEvent);
            RaiseEvent(newEventArgs);
        }

        public static readonly RoutedEvent OnAdditionalButtonClickEvent = EventManager.RegisterRoutedEvent("OnAdditionalButtonClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(OPFrame));
        public event RoutedEventHandler OnAdditionalButtonClick
        {
            add { AddHandler(OnAdditionalButtonClickEvent, value); }
            remove { RemoveHandler(OnAdditionalButtonClickEvent, value); }
        }


        void RaiseOnAdditionalButtonClickEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(OPFrame.OnAdditionalButtonClickEvent);
            RaiseEvent(newEventArgs);
        }


        public static readonly RoutedCommand CloseCommand = new RoutedCommand();
        public static readonly RoutedCommand AdditionalButtonClickCommand = new RoutedCommand();
        
        #endregion
    }


}
