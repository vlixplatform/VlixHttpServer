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

namespace Vlix.HttpServerConfig
{

    public enum IconLocation { Left, Top }
    public class OPButton : Button
    {
        
        public OPButton()
        {
            this.Loaded += new RoutedEventHandler(this.ButtonLoaded);
        }
        public virtual void ButtonLoaded(object sender, RoutedEventArgs e)
        {
           
            OPButton.OnIconChanged(this, new DependencyPropertyChangedEventArgs());
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            try
            {
                _ParentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            }
            catch { return; }
            if (_ParentWindow == null) return;

            if (this.ButtonModifierKey == ModifierKeys.None)
            {
                _ParentWindow.KeyDown += _Single_KeyDown;                
            }
            else
            {
                RoutedCommand AltHotKeyCommand = new RoutedCommand();
                AltHotKeyCommand.InputGestures.Add(new KeyGesture(this.ButtonHotKey, this.ButtonModifierKey));
                _ParentWindow.CommandBindings.Add(new CommandBinding(AltHotKeyCommand, _Dual_KeyDown));
            }
            
        }

        private void _Single_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == this.ButtonHotKey)
            {
                RaiseEvent(new RoutedEventArgs(ClickEvent));
            }
        }

        private void _Dual_KeyDown(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }

        Window _ParentWindow;

        #region Dependency Properties    
    
        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        public static readonly new DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(OPButton),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 248, 128, 0))));
        

        public new Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }
        public new static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(OPButton),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 131, 131, 131))));

        public new double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }
        public static readonly new DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(double), typeof(OPButton), 
            new PropertyMetadata((double)0));

        public Thickness InnerMargin
        {
            get { return (Thickness)GetValue(InnerMarginProperty); }
            set { SetValue(InnerMarginProperty, value); }
        }
        public static readonly DependencyProperty InnerMarginProperty = DependencyProperty.Register("InnerMargin", typeof(Thickness), typeof(OPButton),
            new PropertyMetadata(new Thickness(0)));

        public Thickness IconMargin
        {
            get { return (Thickness)GetValue(IconMarginProperty); }
            set { SetValue(IconMarginProperty, value); }
        }
        public static readonly DependencyProperty IconMarginProperty = DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(OPButton),
            new PropertyMetadata(new Thickness(0,0,3,0)));



        public Brush ButtonClickedBackground
        {
            get { return (Brush)GetValue(ButtonClickedBackgroundProperty); }
            set { SetValue(ButtonClickedBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ButtonClickedBackgroundProperty = DependencyProperty.Register("ButtonClickedBackground", typeof(Brush), typeof(OPButton),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 204, 150))));

        public Brush ButtonMouseOverBackground
        {
            get { return (Brush)GetValue(ButtonMouseOverBackgroundProperty); }
            set { SetValue(ButtonMouseOverBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ButtonMouseOverBackgroundProperty = DependencyProperty.Register("ButtonMouseOverBackground", typeof(Brush), typeof(OPButton),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 147, 32))));

        public Brush ButtonMouseOverBorderBrush
        {
            get { return (Brush)GetValue(ButtonMouseOverBorderBrushProperty); }
            set { SetValue(ButtonMouseOverBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty ButtonMouseOverBorderBrushProperty = DependencyProperty.Register("ButtonMouseOverBorderBrush", typeof(Brush), typeof(OPButton),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 170, 88, 0))));

        public double ButtonMouseOverBorderThickness
        {
            get { return (double)GetValue(ButtonMouseOverBorderThicknessProperty); }
            set { SetValue(ButtonMouseOverBorderThicknessProperty, value); }
        }
        public static readonly DependencyProperty ButtonMouseOverBorderThicknessProperty = DependencyProperty.Register("ButtonMouseOverBorderThickness", typeof(double), typeof(OPButton), 
            new PropertyMetadata((double)1.5));





        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(double), typeof(OPButton), new PropertyMetadata((double)0));


        public double IconSize { get { return (double)GetValue(IconSizeProperty); } set { SetValue(IconSizeProperty, value); } }
        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(OPButton), new PropertyMetadata((double)16));
        

        public IconLocation IconLocation { get { return (IconLocation)GetValue(IconLocationProperty); } set { SetValue(IconLocationProperty, value); } }
        public static readonly DependencyProperty IconLocationProperty = DependencyProperty.Register("IconLocation", typeof(IconLocation), typeof(OPButton), new PropertyMetadata(IconLocation.Left, OnIconChanged));

        public Visibility IconLeftVisibility { get { return (Visibility)GetValue(IconLeftVisibilityProperty); } internal set { SetValue(IconLeftVisibilityProperty, value); } }
        public static readonly DependencyProperty IconLeftVisibilityProperty = DependencyProperty.Register("IconLeftVisibility", typeof(Visibility), typeof(OPButton), new PropertyMetadata(Visibility.Visible));

        public Visibility IconTopVisibility { get { return (Visibility)GetValue(IconTopVisibilityProperty); } internal set { SetValue(IconTopVisibilityProperty, value); } }
        public static readonly DependencyProperty IconTopVisibilityProperty = DependencyProperty.Register("IconTopVisibility", typeof(Visibility), typeof(OPButton), new PropertyMetadata(Visibility.Collapsed));


        public MaterialIconType Icon { get { return (MaterialIconType)GetValue(IconProperty); } set { SetValue(IconProperty, value); } }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(MaterialIconType), typeof(OPButton), new PropertyMetadata(MaterialIconType.ic_none, OnIconChanged));

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OPButton OPButton)
            {
                if (OPButton.Icon == MaterialIconType.ic_none)
                {
                    OPButton.IconTopVisibility = Visibility.Collapsed;
                    OPButton.IconLeftVisibility = Visibility.Collapsed;
                }
                else
                {
                    if (OPButton.IconLocation == IconLocation.Left)
                    {
                        OPButton.IconLeftVisibility = Visibility.Visible;
                        OPButton.IconTopVisibility = Visibility.Collapsed;
                    }
                    if (OPButton.IconLocation == IconLocation.Top)
                    {
                        OPButton.IconLeftVisibility = Visibility.Collapsed;
                        OPButton.IconTopVisibility = Visibility.Visible;
                    }
                }
            }
        }

        public Key ButtonHotKey
        {
            get { return (Key)GetValue(ButtonHotKeyProperty); }
            set { SetValue(ButtonHotKeyProperty, value); }
        }
        public static readonly DependencyProperty ButtonHotKeyProperty = DependencyProperty.Register("ButtonHotKey", typeof(Key), typeof(OPButton), new FrameworkPropertyMetadata(Key.None, FrameworkPropertyMetadataOptions.Inherits));

        public ModifierKeys ButtonModifierKey
        {
            get { return (ModifierKeys)GetValue(ButtonModifierKeyProperty); }
            set { SetValue(ButtonModifierKeyProperty, value); }
        }
        public static readonly DependencyProperty ButtonModifierKeyProperty = DependencyProperty.Register("ButtonModifierKey", typeof(ModifierKeys), typeof(OPButton), new PropertyMetadata(ModifierKeys.None));



        #endregion
    }


}
