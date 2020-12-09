using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace Vlix.ServerConfigUI
{
    public enum CentreMessageState { NoMessage, Processing, Error, Success }


    public partial class CentreMessage : UserControl
    {
        public CentreMessage()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }
            sbMessageProcess = this.tbCenterMessage.FindResource("MessageProcess") as Storyboard;
            sbMessageError = this.tbCenterMessage.FindResource("MessageError") as Storyboard;
            sbMessageSuccess = this.tbCenterMessage.FindResource("MessageSuccess") as Storyboard;
            sbMessageSuccess.Completed += (s, e2) => { this.State = CentreMessageState.NoMessage; _ShowSuccessComplete?.Invoke(); };
            sbMessageError.Completed += (s, e3) => { this.State = CentreMessageState.NoMessage; _ShowErrorComplete?.Invoke(); };

        }
        Storyboard sbMessageProcess, sbMessageError, sbMessageSuccess;
        bool LoadOnceDone = false;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CentreMessage UCD && UCD.ShowDesignTimeMessageProcess)
            {
                tbCenterMessage.Text = this.MessageProcess;
                tbCenterMessage.Visibility = Visibility.Visible;
            }
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }
            if (LoadOnceDone) return;
            if (sender is CentreMessage UC)
            {
                UC.Visibility = Visibility.Visible;
                UC.ShowDesignTimeMessageProcess = false;
                tbDesignTime.Visibility = Visibility.Collapsed;
            }
        }

        public CentreMessageState State { get { return (CentreMessageState)GetValue(StateProperty); } set { SetValue(StateProperty, value); } }
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(CentreMessageState),
            typeof(CentreMessage), new FrameworkPropertyMetadata(CentreMessageState.NoMessage,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnShowMessageChanged));

        public static void OnShowMessageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(sender)) { return; }
            if (sender is CentreMessage UC)
            {
                switch (UC.State)
                {
                    case CentreMessageState.NoMessage:
                        if (!DesignerProperties.GetIsInDesignMode(UC))
                        {
                            UC.sbMessageProcess?.Stop();
                            UC.sbMessageError?.Stop();
                            UC.sbMessageSuccess?.Stop();
                        }
                        UC.gdCenterMessage.Visibility = Visibility.Hidden;
                        break;
                    case CentreMessageState.Error:
                        UC.tbCenterMessage.Text = UC.MessageError;
                        UC.tbCenterMessage.Foreground = UC.MessageErrorColor;
                        UC.tbSubCenterMessage.Foreground = UC.MessageErrorColor;
                        UC.tbSubCenterMessage.Text = UC.SubMessage;
                        if (DesignerProperties.GetIsInDesignMode(UC)) UC.gdCenterMessage.Visibility = Visibility.Visible;
                        else UC.sbMessageError?.Begin();
                        break;
                    case CentreMessageState.Processing:
                        UC.tbCenterMessage.Text = UC.MessageProcess;
                        UC.tbCenterMessage.Foreground = UC.MessageSuccessAndProcessColor;
                        UC.tbSubCenterMessage.Foreground = UC.MessageSuccessAndProcessColor;
                        UC.tbSubCenterMessage.Text = UC.SubMessage;
                        if (DesignerProperties.GetIsInDesignMode(UC)) UC.gdCenterMessage.Visibility = Visibility.Visible;
                        else UC.sbMessageProcess?.Begin();
                        break;
                    case CentreMessageState.Success:
                        UC.tbCenterMessage.Text = UC.MessageSuccess;
                        UC.tbCenterMessage.Foreground = UC.MessageSuccessAndProcessColor;
                        UC.tbSubCenterMessage.Foreground = UC.MessageSuccessAndProcessColor;
                        UC.tbSubCenterMessage.Text = UC.SubMessage;
                        if (DesignerProperties.GetIsInDesignMode(UC)) UC.gdCenterMessage.Visibility = Visibility.Visible;
                        else
                        {
                            UC.sbMessageSuccess?.Begin();
                        }
                        break;
                }
            }
        }

        public string MessageProcess { get { return (string)GetValue(MessageProcessProperty); } set { SetValue(MessageProcessProperty, value); } }
        public static readonly DependencyProperty MessageProcessProperty = DependencyProperty.Register("MessageProcess", typeof(string), typeof(CentreMessage), new FrameworkPropertyMetadata("Centre Message Processing Text...", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMessageChanged));
        public string MessageSuccess { get { return (string)GetValue(MessageSuccessProperty); } set { SetValue(MessageSuccessProperty, value); } }
        public static readonly DependencyProperty MessageSuccessProperty = DependencyProperty.Register("MessageSuccess", typeof(string), typeof(CentreMessage), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMessageChanged));
        public string MessageError { get { return (string)GetValue(MessageErrorProperty); } set { SetValue(MessageErrorProperty, value); } }
        public static readonly DependencyProperty MessageErrorProperty = DependencyProperty.Register("MessageError", typeof(string), typeof(CentreMessage), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMessageChanged));
        public string SubMessage { get { return (string)GetValue(SubMessageProperty); } set { SetValue(SubMessageProperty, value); } }
        public static readonly DependencyProperty SubMessageProperty = DependencyProperty.Register("SubMessage", typeof(string), typeof(CentreMessage), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMessageChanged));


        public static void OnMessageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(sender)) { return; }
            CentreMessage UC = sender as CentreMessage;
            if (UC.tbCenterMessage == null) return;
            if (UC != null)
            {

                switch (UC.State)
                {
                    case CentreMessageState.NoMessage:
                        UC.tbCenterMessage.Text = "";
                        UC.tbSubCenterMessage.Text = "";
                        break;
                    case CentreMessageState.Error:
                        UC.tbCenterMessage.Text = UC.MessageError;
                        UC.tbSubCenterMessage.Text = UC.SubMessage;
                        break;
                    case CentreMessageState.Processing:
                        UC.tbCenterMessage.Text = UC.MessageProcess;
                        UC.tbSubCenterMessage.Text = UC.SubMessage;
                        break;
                    case CentreMessageState.Success:
                        UC.tbCenterMessage.Text = UC.MessageSuccess;
                        UC.tbSubCenterMessage.Text = UC.SubMessage;
                        break;
                }
            }
        }

        EventHandler _OnShowMessageErrorComplete;
        public EventHandler OnShowMessageErrorComplete
        {
            get
            {
                return _OnShowMessageErrorComplete;
            }
            set
            {
                _OnShowMessageErrorComplete = value;
                this.sbMessageError.Completed -= _OnShowMessageErrorComplete;
                this.sbMessageError.Completed += _OnShowMessageErrorComplete;
            }
        }

        EventHandler _OnShowMessageSuccessComplete;
        public EventHandler OnShowMessageSuccessComplete
        {
            get
            {
                return _OnShowMessageSuccessComplete;
            }
            set
            {
                _OnShowMessageSuccessComplete = value;
                this.sbMessageSuccess.Completed -= _OnShowMessageSuccessComplete;
                this.sbMessageSuccess.Completed += _OnShowMessageSuccessComplete;
            }
        }

        public bool ShowDesignTimeMessageProcess { get { return (bool)GetValue(ShowDesignTimeMessageProcessProperty); } set { SetValue(ShowDesignTimeMessageProcessProperty, value); } }
        public static readonly DependencyProperty ShowDesignTimeMessageProcessProperty = DependencyProperty.Register("ShowDesignTimeMessageProcess",
            typeof(bool), typeof(CentreMessage), new FrameworkPropertyMetadata(false, OnShowDesignTimeMessageChanged));

        public static void OnShowDesignTimeMessageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(sender))
            {
                if (sender is CentreMessage UC)
                {
                    if (UC.ShowDesignTimeMessageProcess) { UC.Visibility = Visibility.Visible; UC.State = CentreMessageState.Processing; }
                    else { UC.State = CentreMessageState.NoMessage; UC.Visibility = Visibility.Collapsed; }
                }
            }
        }
        public double MessageFontSize { get { return (double)GetValue(MessageFontSizeProperty); } set { SetValue(MessageFontSizeProperty, value); } }
        public static readonly DependencyProperty MessageFontSizeProperty = DependencyProperty.Register("MessageFontSize", typeof(double), typeof(CentreMessage), new FrameworkPropertyMetadata((double)32));

        public bool EnableSubMessage { get { return (bool)GetValue(EnableSubMessageProperty); } set { SetValue(EnableSubMessageProperty, value); } }
        public static readonly DependencyProperty EnableSubMessageProperty = DependencyProperty.Register("EnableSubMessage", typeof(bool), typeof(CentreMessage), new FrameworkPropertyMetadata(false));

        public double SubMessageFontSize { get { return (double)GetValue(SubMessageFontSizeProperty); } set { SetValue(SubMessageFontSizeProperty, value); } }
        public static readonly DependencyProperty SubMessageFontSizeProperty = DependencyProperty.Register("SubMessageFontSize", typeof(double), typeof(CentreMessage), new FrameworkPropertyMetadata((double)16));


        public FontWeight MessageFontWeight { get { return (FontWeight)GetValue(MessageFontWeightProperty); } set { SetValue(MessageFontWeightProperty, value); } }
        public static readonly DependencyProperty MessageFontWeightProperty = DependencyProperty.Register("MessageFontWeight", typeof(FontWeight), typeof(CentreMessage), new FrameworkPropertyMetadata(FontWeights.Normal));

        public FontWeight SubMessageFontWeight { get { return (FontWeight)GetValue(SubMessageFontWeightProperty); } set { SetValue(SubMessageFontWeightProperty, value); } }
        public static readonly DependencyProperty SubMessageFontWeightProperty = DependencyProperty.Register("SubMessageFontWeight", typeof(FontWeight), typeof(CentreMessage), new FrameworkPropertyMetadata(FontWeights.Normal));



        public Brush MessageSuccessAndProcessColor { get { return (Brush)GetValue(MessageSuccessAndProcessColorProperty); } set { SetValue(MessageSuccessAndProcessColorProperty, value); } }
        public static readonly DependencyProperty MessageSuccessAndProcessColorProperty = DependencyProperty.Register("MessageSuccessAndProcessColor", typeof(Brush), typeof(CentreMessage), new FrameworkPropertyMetadata(Brushes.Green));

        public Brush MessageErrorColor { get { return (Brush)GetValue(MessageErrorColorProperty); } set { SetValue(MessageErrorColorProperty, value); } }
        public static readonly DependencyProperty MessageErrorColorProperty = DependencyProperty.Register("MessageErrorColor", typeof(Brush), typeof(CentreMessage), new FrameworkPropertyMetadata(Brushes.Red));



        public void ShowMessageProcess(string Message = null, string SubMessage = "")
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (Message != null) this.MessageProcess = Message;
                this.SubMessage = SubMessage;
                this.State = CentreMessageState.Processing;
            }));
        }


        Action _ShowSuccessComplete;
        public void ShowMessageSuccess(string Message = null, string SubMessage = "", Action ShowSuccessComplete = null)
        {
            
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                _ShowSuccessComplete = ShowSuccessComplete;
                if (Message != null) this.MessageSuccess = Message;
                this.SubMessage = SubMessage;
                this.State = CentreMessageState.Success;
            }));
        }


        Action _ShowErrorComplete;
        public void ShowMessageError(string Message = null, string SubMessage = "", Action ShowErrorComplete = null)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                _ShowErrorComplete = ShowErrorComplete;
                if (Message != null) this.MessageError = Message;
                this.SubMessage = SubMessage;
                this.State = CentreMessageState.Error;
            }));
        }

        public void StopMessage()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.State = CentreMessageState.NoMessage;
            }));
        }
    }
}
