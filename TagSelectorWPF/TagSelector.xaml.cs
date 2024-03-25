using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
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
using System.Xml.Linq;

namespace TagSelectorWPF
{
    /// <summary>
    /// TagSelector.xaml 的交互逻辑
    /// </summary>
    public partial class TagSelector : UserControl
    {
        #region property
        /// <summary>
        /// A list of data sources that are used to predefine the available options that the user can select from this list
        /// </summary>
        public ObservableCollection<string> Source
        {
            get { return (ObservableCollection<string>)GetValue(sourceProperty); }
            set { SetValue(sourceProperty, value); }
        }

        public static readonly DependencyProperty sourceProperty = DependencyProperty.Register("Source", typeof(ObservableCollection<string>), typeof(TagSelector), new PropertyMetadata(SourceChangedEvent));
        private static void SourceChangedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dep = d as TagSelector;
            if (dep == null) return;
            dep.InitSource((ObservableCollection<string>)e.NewValue);
        }

        /// <summary>
        /// The result of the selection. You can pre-set the selected items or get them directly as a return value
        /// </summary>
        public ObservableCollection<string> Result
        {
            get { return (ObservableCollection<string>)GetValue(resultProperty); }
            set { SetValue(resultProperty, value); }
        }
        public static readonly DependencyProperty resultProperty = DependencyProperty.Register("Result", typeof(ObservableCollection<string>), typeof(TagSelector), new PropertyMetadata(ResultChangedEvent));
        private static void ResultChangedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dep = d as TagSelector;
            if (dep == null) return;
            dep.InitResult(((IEnumerable<string>)e.NewValue).ToList());
        }

        /// <summary>
        /// Prompt word displayed when the input box does not contain characters
        /// </summary>
        public string Tip
        {
            get { return (string)GetValue(tipProperty); }
            set { SetValue(tipProperty, value); }
        }
        public static readonly DependencyProperty tipProperty = DependencyProperty.Register("Tip", typeof(string), typeof(TagSelector), new FrameworkPropertyMetadata(TipChangedEvent));
        private static void TipChangedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dep = d as TagSelector;
            if (dep == null) return;
            dep.TipTextblock.Text = e.NewValue.ToString();
        }
        #endregion

        /// <summary>
        /// Source list with boolean state
        /// </summary>
        internal ObservableCollection<SelectableItem> AllList { get; set; } = new ObservableCollection<SelectableItem>();

        /// <summary>
        /// Providing Drag for Horizontal ScrollViewer
        /// </summary>
        ScrollDragger scrollDragger;

        public TagSelector()
        {
            InitializeComponent();
            AllItemsControl.DataContext = AllList;
            AllList.CollectionChanged += AllList_CollectionChanged;

            scrollDragger = new ScrollDragger(ResultItemsControl, ResultScrollViewer);
        }

        #region Control Events
        /// <summary>
        /// Remove button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var name = (sender as Button).DataContext as string;
            if (name == null) return;
            RemoveSelected(name);
        }
        /// <summary>
        /// Text box press enter key to add a tag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (string.IsNullOrEmpty(textBox.Text)) return;
            AddSelected(textBox.Text);
            textBox.Text = string.Empty;
        }
        /// <summary>
        /// Result list horizontal scrolling
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
                scrollViewer.LineRight();
            else
                scrollViewer.LineLeft();
            e.Handled = true;
        }
        #endregion


        #region Collection Change
        /// <summary>
        /// Add initial values to source list with boolean, subscribe to source list changes
        /// </summary>
        /// <param name="items"></param>
        public void InitSource(ObservableCollection<string> items)
        {
            AllList.Clear();
            foreach (var name in items.Distinct())
                AllList.Add(new SelectableItem { Name = name });

            items.CollectionChanged += SourceList_CollectionChanged;
        }

        /// <summary>
        /// Add or remove to source list with boolean
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SourceList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (string name in e.NewItems)
                    AllList.Add(new SelectableItem { Name = name });
            }
            if (e.OldItems != null)
            {
                foreach (string name in e.OldItems)
                {
                    var item = AllList.FirstOrDefault(x => x.Name == name);
                    if (item != null)
                        AllList.Remove(item);
                }
            }
        }
        /// <summary>
        /// Subscribe to Boolean property change events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SelectableItem item in e.NewItems)
                    item.PropertyChanged += OnPropertyChange;
            }
            if (e.OldItems != null)
            {
                foreach (SelectableItem item in e.OldItems)
                    item.PropertyChanged -= OnPropertyChange;
            }
        }

        /// <summary>
        /// Display to result list when boolean attribute is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropertyChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableItem.IsSelected))
            {
                var removeTargets = new List<string>();
                foreach (var name in Result)
                {
                    var item = AllList.FirstOrDefault(x => x.Name == name);
                    if (item == null) continue;
                    if (!item.IsSelected)
                        removeTargets.Add(name);
                }
                foreach (var item in removeTargets)
                    Result.Remove(item);

                var selectedItems = AllList.Where(x => x.IsSelected).Select(x => x.Name);
                var excepts = selectedItems.Except(Result);
                foreach (var item in excepts)
                    Result.Add(item);
            }
        }

        /// <summary>
        /// Adding a preset result value
        /// </summary>
        /// <param name="items"></param>
        public void InitResult(IEnumerable<string> items)
        {
            Result.Clear();
            foreach (var name in items.Distinct())
                AddSelected(name);
        }
        /// <summary>
        /// Add item and set select state true if exist
        /// </summary>
        /// <param name="name"></param>
        public void AddSelected(string name)
        {
            if (Result.Contains(name)) return;
            Result.Add(name);

            var source = AllList.FirstOrDefault(x => x.Name == name);
            if (source != null)
                source.IsSelected = true;
        }
        /// <summary>
        /// Remove item and set select state false if exist
        /// </summary>
        /// <param name="name"></param>
        public void RemoveSelected(string name)
        {
            if (!Result.Contains(name)) return;
            Result.Remove(name);

            foreach (var item in AllList.Where(x => x.Name == name))
                item.IsSelected = false;
        }
        #endregion
    }
}
