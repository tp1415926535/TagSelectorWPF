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
        ///  <para>来源列表，用于提供给用户预设可选的项</para> 
        /// </summary>
        public ObservableCollection<string> Source
        {
            get { return (ObservableCollection<string>)GetValue(sourceProperty); }
            set { SetValue(sourceProperty, value); }
        }

        public static readonly DependencyProperty sourceProperty = DependencyProperty.Register("Source", typeof(ObservableCollection<string>), typeof(TagSelector), new PropertyMetadata(SourceChangedEvent));
        private static void SourceChangedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)//only trigger once in binding
        {
            var dep = d as TagSelector;
            if (dep == null) return;
            dep.InitSource((ObservableCollection<string>)e.NewValue);
        }

        /// <summary>
        /// The result of the selection. You can pre-set the selected items or get them directly as a return value
        ///  <para>选择的结果，可以设置预设结果或直接作为返回值获取</para> 
        /// </summary>
        public ObservableCollection<string> Result
        {
            get { return (ObservableCollection<string>)GetValue(resultProperty); }
            set { SetValue(resultProperty, value); }
        }
        public static readonly DependencyProperty resultProperty = DependencyProperty.Register("Result", typeof(ObservableCollection<string>), typeof(TagSelector), new PropertyMetadata(ResultChangedEvent));
        private static void ResultChangedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)//only trigger once in binding
        {
            var dep = d as TagSelector;
            if (dep == null) return;
            dep.InitResult((ObservableCollection<string>)e.NewValue);
        }

        /// <summary>
        /// Prompt word displayed when the input box does not contain characters
        /// <para>输入框为空时显示的提示词</para> 
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


        /// <summary>
        /// Allow add custom tag
        /// <para>是否允许输入自定义的标签</para> 
        /// </summary>
        public bool AllowInput
        {
            get { return (bool)GetValue(allowProperty); }
            set { SetValue(allowProperty, value); }
        }
        public static readonly DependencyProperty allowProperty = DependencyProperty.Register("AllowInput", typeof(bool), typeof(TagSelector), new FrameworkPropertyMetadata(default));
        #endregion

        /// <summary>
        /// Source list with boolean state
        /// <para>来源列表转换到带布尔状态，用于绑定</para> 
        /// </summary>
        internal ObservableCollection<SelectableItem> AllList { get; set; } = new ObservableCollection<SelectableItem>();

        /// <summary>
        /// SelectedList, different from Result Source so that can check if outside add item
        /// <para>绑定到控件的结果列表，用于区分是否是外部传入的变更</para> 
        /// </summary>
        internal ObservableCollection<string> SelectedList { get; set; } = new ObservableCollection<string>();


        /// <summary>
        /// Providing Drag for Horizontal ScrollViewer
        /// <para>为横向滚动视图提供拖动</para> 
        /// </summary>
        ScrollDragger scrollDragger;

        public TagSelector()
        {
            InitializeComponent();
            AllItemsControl.DataContext = AllList;
            ResultItemsControl.DataContext = SelectedList;
            AllList.CollectionChanged += AllList_CollectionChanged;

            scrollDragger = new ScrollDragger(ResultItemsControl, ResultScrollViewer);
        }

        #region Control Events
        /// <summary>
        /// Remove button's click
        /// <para>移除按钮的点击事件</para> 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var name = (sender as Button).DataContext as string;
            if (name == null) return;
            Result.Remove(name);
        }
        /// <summary>
        /// Text box press enter key to add a tag
        /// <para>按下回车添加一项</para> 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (string.IsNullOrEmpty(textBox.Text)) return;
            string name = textBox.Text;
            if (!Result.Contains(name))
                Result.Add(name);
            //AddSelected(textBox.Text);
            textBox.Text = string.Empty;
        }
        /// <summary>
        /// Result list horizontal scrolling
        /// <para>结果列表横向滚动</para> 
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
        /// <para>添加初始值到带布尔值的列表，并订阅列表源变更事件</para> 
        /// </summary>
        /// <param name="items"></param>
        public void InitSource(ObservableCollection<string> items)
        {
            AllList.Clear();
            if (items == null) return;
            foreach (var name in items.Distinct())
                AllList.Add(new SelectableItem { Name = name });

            items.CollectionChanged += SourceList_CollectionChanged;
        }

        /// <summary>
        /// Add or remove to source list with boolean
        /// <para>带布尔值的列表对应源变更时</para> 
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
        /// <para>订阅布尔属性变更事件</para> 
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
        /// <para>变更布尔属性时，显示到结果列表</para> 
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
        /// <para>添加预设结果值</para> 
        /// </summary>
        /// <param name="items"></param>
        public void InitResult(ObservableCollection<string> items)
        {
            SelectedList.Clear();
            if (items == null) return;
            foreach (var name in items.Distinct())
                AddSelected(name);

            items.CollectionChanged += ResultList_CollectionChanged;
        }
        /// <summary>
        /// Add or remove to result list from outside
        /// <para>带布尔值的列表对应源变更时</para> 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResultList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (string name in e.NewItems)
                    AddSelected(name);
            }
            if (e.OldItems != null)
            {
                foreach (string name in e.OldItems)
                    RemoveSelected(name);
            }
        }
        /// <summary>
        /// Add item and set selected state true if exist in source.
        /// Effect to show, should be only called by CollectionChange and init.
        /// <para>添加项目，如果来源中存在则设置选中状态为true。用于显示，应该只被订阅的变更事件和初始化调用</para> 
        /// </summary>
        /// <param name="name"></param>
        public void AddSelected(string name)
        {
            if (SelectedList.Contains(name)) return;
            if (!AllowInput && !AllList.Any(x => x.Name == name)) return;
            SelectedList.Add(name);

            var source = AllList.FirstOrDefault(x => x.Name == name);
            if (source != null)
                source.IsSelected = true;
        }
        /// <summary>
        /// Remove item and set selected state false if exist in source.
        /// Effect to show, should be only called by CollectionChange.
        /// <para>删除项目，如果来源中存在则将选中状态设置为false。用于显示，应该只被订阅的变更事件调用</para> 
        /// </summary>
        /// <param name="name"></param>
        public void RemoveSelected(string name)
        {
            if (!SelectedList.Contains(name)) return;
            SelectedList.Remove(name);

            foreach (var item in AllList.Where(x => x.Name == name))
                item.IsSelected = false;
        }
        #endregion
    }
}
