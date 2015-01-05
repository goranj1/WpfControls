using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfCustomControls
{


    [TemplatePart(Name = PartTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PartPopup, Type = typeof(Popup))]
    [TemplatePart(Name = PartListBox, Type = typeof(ListBox))]
    public class AutoCompleteBox : Control
    {
        #region Control Parts
        public const string PartTextBox = "PART_TextBox";
        public const string PartPopup = "PART_Popup";
        public const string PartListBox = "PART_ListBox";
        #endregion

        #region Components
        private ListBox _partListBox;
        private Popup _partPopup;
        private TextBox _partTextBox;
        #endregion

        private bool _isTemplateApplied;
        static AutoCompleteBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox), new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _partTextBox = GetTemplateChild(PartTextBox) as TextBox;
            if (_partTextBox == null)
            {
                throw new InvalidOperationException("Associated ControlTemplate has a bad part configuration. Expected a TextBox part.");
            }

            _partPopup = GetTemplateChild(PartPopup) as Popup;
            if (_partPopup == null)
            {
                throw new InvalidOperationException("Associated ControlTemplate has a bad part configuration. Expected a Popup part.");
            }

            _partListBox = GetTemplateChild(PartListBox) as ListBox;
            if (_partListBox == null)
            {
                throw new InvalidOperationException("Associated ControlTemplate has a bad part configuration. Expected a ListBox part.");
            }

            _isTemplateApplied = true;

            CaptureWindowMovementForPopupPlacement();
            RegisterKeyboardAndMouseEventHandlers();
            OnConfigurationChanged();     
        }


        private bool _isTextChangedSubscribed = false;
        private void OnConfigurationChanged()
        {
            if (_isTemplateApplied)
            {
             //   _partTextBox.Text = string.Empty;
                if (Results != null)
                {
                    Results.Clear();
                }

                if (_isTextChangedSubscribed)
                {
                    _partTextBox.TextChanged -= _partTextBox_TextChanged;
                }

                _partTextBox.TextChanged+=_partTextBox_TextChanged;
                _isTextChangedSubscribed = true;
                //SubscribeToQueryEvents();
            }
        }

        private void _partTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textLen = ((TextBox) e.Source).Text.Length;
            if (textLen < MinimumCharacters)
            {
                _partPopup.IsOpen = false;
                IsBusy = false;
            }
            else if (textLen >= MinimumCharacters)
            {
            //    IsBusy = true;
                _partPopup.IsOpen = true;
            }
            Text = ((TextBox) e.Source).Text;
        }

        #region Dependency Properties
       
        #region Results 

        public const string ResultsPropertyName = "Results";

        private static readonly DependencyProperty ResultsProperty = DependencyProperty.Register(
            ResultsPropertyName,
            typeof(ObservableCollection<object>),
            typeof(AutoCompleteBox),
            new FrameworkPropertyMetadata(new ObservableCollection<object>(),new PropertyChangedCallback(ResultsChanged)));

        private static void ResultsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AutoCompleteBox) d).IsBusy = false;
        }


        public ObservableCollection<object> Results
        {
            get { return (ObservableCollection<object>)GetValue(ResultsProperty); }
            set { SetValue(ResultsProperty, value); }
        }
        #endregion


        #region MinimumCharacters
        /// <summary>
        /// The <see cref="MinimumCharacters" /> dependency property's name.
        /// </summary>
        public const string MinimumCharactersPropertyName = "MinimumCharacters";

        /// <summary>
        /// Gets or sets the value of the <see cref="MinimumCharacters" />
        /// property. This is a dependency property.
        /// </summary>
        public int MinimumCharacters
        {
            get
            {
                return (int)GetValue(MinimumCharactersProperty);
            }
            set
            {
                SetValue(MinimumCharactersProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="MinimumCharacters" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumCharactersProperty = DependencyProperty.Register(
            MinimumCharactersPropertyName,
            typeof(int),
            typeof(AutoCompleteBox),
            new UIPropertyMetadata(2, OnMinimumCharactersChanged, OnMinimumCharactersCoerceValue));

        private static void OnMinimumCharactersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (AutoCompleteBox)d;
            self.OnConfigurationChanged();
        }

        private static object OnMinimumCharactersCoerceValue(DependencyObject d, object baseValue)
        {
            var value = (int)baseValue;
            if (value <= 0)
            {
                return 1;
            }

            return value;
        }

        #endregion

        #region Text
        public const string TextPropertyName = "Text";

        [Bindable(true)]
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            TextPropertyName,
            typeof(string),
            typeof(AutoCompleteBox), new FrameworkPropertyMetadata(default(string),FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        #endregion


        #region ItemTemplate

        public const string ItemTemplatePropertyName = "ItemTemplate";

        [Bindable(true)]
        public DataTemplate ItemTemplate
        {
            get
            {
                return (DataTemplate)GetValue(ItemTemplateProperty);
            }
            set
            {
                SetValue(ItemTemplateProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="ItemTemplate" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            ItemTemplatePropertyName,
            typeof(DataTemplate),
            typeof(AutoCompleteBox),
            new UIPropertyMetadata(null));

        #endregion   

        #region IsBusy (readonly)

        public const string IsBusyPropertyName = "IsBusy";

        private static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            IsBusyPropertyName,
            typeof(bool),
            typeof(AutoCompleteBox),
            new FrameworkPropertyMetadata(false));


        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }
        #endregion
       
        
        #endregion

        private void SetResultText(IResultItem resultItem)
        {

            _partTextBox.Text = resultItem.Title;
            _partPopup.IsOpen = false;
          
            _partTextBox.CaretIndex = _partTextBox.Text.Length;
            _partTextBox.Focus();
        }

        private void CaptureWindowMovementForPopupPlacement()
        {
            var window = Window.GetWindow(_partTextBox);
            if (window != null)
            {
                //TODO use weak event
                window.LocationChanged += (sender, args) =>
                {
                    if (!_partPopup.IsOpen)
                    {
                        return;
                    }

                    var offset = _partPopup.HorizontalOffset;
                    _partPopup.HorizontalOffset = offset + 1;
                    _partPopup.HorizontalOffset = offset;
                };
                //WeakEventManager.AddHandler(
                //    window,
                //    "LocationChanged",
                //    (sender, args) =>
                //    {
                //        if (!_partPopup.IsOpen)
                //        {
                //            return;
                //        }

                //        var offset = _partPopup.HorizontalOffset;
                //        _partPopup.HorizontalOffset = offset + 1;
                //        _partPopup.HorizontalOffset = offset;
                //    });
            }
        }
        private void RegisterKeyboardAndMouseEventHandlers()
        {
            _partTextBox.PreviewKeyDown += (sender, args) =>
            {
                if (args.Key == Key.Up && _partListBox.SelectedIndex > 0)
                {
                    _partListBox.SelectedIndex--;
                }
                else if (args.Key == Key.Down && _partListBox.SelectedIndex < _partListBox.Items.Count - 1)
                {
                    _partListBox.SelectedIndex++;
                }
                else if ((args.Key == Key.Return || args.Key == Key.Enter) && _partListBox.SelectedIndex != -1)
                {
                    SetResultText((IResultItem)_partListBox.SelectedItem);
                    args.Handled = true;
                }
                else if (args.Key == Key.Escape)
                {
                    _partPopup.IsOpen = false;
                    args.Handled = true;
                }

                _partListBox.ScrollIntoView(_partListBox.SelectedItem);
            };

            _partListBox.PreviewTextInput += (sender, args) => { _partTextBox.Text += args.Text; };

            _partListBox.PreviewKeyDown += (sender, args) =>
            {
                if ((args.Key == Key.Return || args.Key == Key.Enter) && _partListBox.SelectedIndex != -1)
                {
                    SetResultText((IResultItem)_partListBox.SelectedItem);
                    args.Handled = true;
                }
                else if (args.Key == Key.Escape)
                {
                    _partPopup.IsOpen = false;
                    args.Handled = true;
                    _partTextBox.CaretIndex = _partTextBox.Text.Length;
                    _partTextBox.Focus();
                }
            };

            _partListBox.PreviewMouseDown += (sender, args) =>
            {
                var listboxItem = FindParent<ListBoxItem>((DependencyObject)args.OriginalSource);
                if (listboxItem != null)
                {
                    SetResultText((IResultItem)listboxItem.DataContext);
                    args.Handled = true;
                }
            };
        }

        public static T FindParent<T>(DependencyObject startingObject) where T : DependencyObject
        {
            DependencyObject parent = GetParent(startingObject);

            while (parent != null)
            {
                var foundElement = parent as T;

                if (foundElement != null)
                {
                    return foundElement;
                }

                parent = GetParent(parent);
            }

            return null;
        }

        private static DependencyObject GetParent(DependencyObject element)
        {
            var visual = element as Visual;
            var parent = (visual == null) ? null : VisualTreeHelper.GetParent(visual);

            if (parent == null)
            {
                // Check for a logical parent when no visual was found.
                var frameworkElement = element as FrameworkElement;

                if (frameworkElement != null)
                {
                    parent = frameworkElement.Parent ?? frameworkElement.TemplatedParent;
                }
                else
                {
                    var frameworkContentElement = element as FrameworkContentElement;

                    if (frameworkContentElement != null)
                    {
                        parent = frameworkContentElement.Parent ?? frameworkContentElement.TemplatedParent;
                    }
                }
            }

            return parent;
        }
    }

    public interface IResultItem
    {
        string Id { get; set; }
        string Title { get; set; }
    }

    public class DataProvider:IDataProvider
    {
        private readonly Func<string, IEnumerable> _dataRetrieverFunc;
        public DataProvider(Func<string, IEnumerable> dataRetrieverFunc)
        {
            _dataRetrieverFunc = dataRetrieverFunc;
        }

        public IEnumerable GetData(string text)
        {
            return _dataRetrieverFunc(text);
        }
    }

    public interface IDataProvider
    {
        IEnumerable GetData(string text);
    }

    //class SomeEventWeakEventManager : WeakEventManager
    //{

    //    private SomeEventWeakEventManager()
    //    {

    //    }

    //    /// <summary>
    //    /// Add a handler for the given source's event.
    //    /// </summary>
    //    public static void AddHandler(EventSource source,
    //                                  EventHandler<SomeEventEventArgs> handler)
    //    {
    //        if (source == null)
    //            throw new ArgumentNullException("source");
    //        if (handler == null)
    //            throw new ArgumentNullException("handler");

    //        CurrentManager.ProtectedAddHandler(source, handler);
    //    }

    //    /// <summary>
    //    /// Remove a handler for the given source's event.
    //    /// </summary>
    //    public static void RemoveHandler(EventSource source,
    //                                     EventHandler<SomeEventEventArgs> handler)
    //    {
    //        if (source == null)
    //            throw new ArgumentNullException("source");
    //        if (handler == null)
    //            throw new ArgumentNullException("handler");

    //        CurrentManager.ProtectedRemoveHandler(source, handler);
    //    }

    //    /// <summary>
    //    /// Get the event manager for the current thread.
    //    /// </summary>
    //    private static SomeEventWeakEventManager CurrentManager
    //    {
    //        get
    //        {
    //            Type managerType = typeof(SomeEventWeakEventManager);
    //            SomeEventWeakEventManager manager =
    //                (SomeEventWeakEventManager)GetCurrentManager(managerType);

    //            // at first use, create and register a new manager
    //            if (manager == null)
    //            {
    //                manager = new SomeEventWeakEventManager();
    //                SetCurrentManager(managerType, manager);
    //            }

    //            return manager;
    //        }
    //    }



    //    /// <summary>
    //    /// Return a new list to hold listeners to the event.
    //    /// </summary>
    //    protected override ListenerList NewListenerList()
    //    {
    //        return new ListenerList<SomeEventEventArgs>();
    //    }


    //    /// <summary>
    //    /// Listen to the given source for the event.
    //    /// </summary>
    //    protected override void StartListening(object source)
    //    {
    //        EventSource typedSource = (EventSource)source;
    //        typedSource.SomeEvent += new EventHandler<SomeEventEventArgs>(OnSomeEvent);
    //    }

    //    /// <summary>
    //    /// Stop listening to the given source for the event.
    //    /// </summary>
    //    protected override void StopListening(object source)
    //    {
    //        EventSource typedSource = (EventSource)source;
    //        typedSource.SomeEvent -= new EventHandler<SomeEventEventArgs>(OnSomeEvent);
    //    }

    //    /// <summary>
    //    /// Event handler for the SomeEvent event.
    //    /// </summary>
    //    void OnSomeEvent(object sender, SomeEventEventArgs e)
    //    {
    //        DeliverEvent(sender, e);
    //    }
    //}
}
