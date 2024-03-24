using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TagSelectControlWPF
{
    /// <summary>
    /// TagSelectControl.xaml 的交互逻辑
    /// </summary>
    public partial class TagSelectControl : UserControl
    {
        #region property
        public IEnumerable<string> Source
        {
            get { return (IEnumerable<string>)GetValue(sourceProperty); }
            set { SetValue(sourceProperty, value); }
        }

        public static readonly DependencyProperty sourceProperty = DependencyProperty.Register("Source", typeof(IEnumerable<string>), typeof(TagSelectControl), new PropertyMetadata(SourceChangedEvent));
        private static void SourceChangedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dep = d as TagSelectControl;
            if (dep == null) return;
            dep.InitSource((IEnumerable<string>)e.NewValue);
        }

        public ObservableCollection<string> Result
        {
            get { return (ObservableCollection<string>)GetValue(resultProperty); }
            set { SetValue(resultProperty, value); }
        }
        public static readonly DependencyProperty resultProperty = DependencyProperty.Register("Result", typeof(IEnumerable<string>), typeof(TagSelectControl), new FrameworkPropertyMetadata(null,
                                  FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ResultChangedEvent));
        private static void ResultChangedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dep = d as TagSelectControl;
            if (dep == null) return;
            dep.InitResult(((IEnumerable<string>)e.NewValue).ToList());
        }


        public string Tip
        {
            get { return (string)GetValue(tipProperty); }
            set { SetValue(tipProperty, value); }
        }
        public static readonly DependencyProperty tipProperty = DependencyProperty.Register("Tip", typeof(string), typeof(TagSelectControl), new FrameworkPropertyMetadata(TipChangedEvent));
        private static void TipChangedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dep = d as TagSelectControl;
            if (dep == null) return;
            dep.TipTextblock.Text = e.NewValue.ToString();
        }
        #endregion
        internal ObservableCollection<SelectableItem> AllList { get; set; } = new ObservableCollection<SelectableItem>();

        public TagSelectControl()
        {
            InitializeComponent(); 
            AllItemsControl.DataContext = AllList;      
            AllList.CollectionChanged += AllList_CollectionChanged;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var name = (sender as Button).DataContext as string;
            if (name == null) return;
            RemoveSelected(name);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (string.IsNullOrEmpty(textBox.Text)) return;
            AddSelected(textBox.Text);
            textBox.Text = string.Empty;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
                scrollViewer.LineRight();
            else
                scrollViewer.LineLeft();
            e.Handled = true;
        }




        private void AllList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (SelectableItem item in e.NewItems)
                    item.PropertyChanged += OnPropertyChange;
            if (e.OldItems != null)
                foreach (SelectableItem item in e.OldItems)
                    item.PropertyChanged -= OnPropertyChange;
        }

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

        public void InitSource(IEnumerable<string> items)
        {
            AllList.Clear();
            foreach (var name in items.Distinct())
                AllList.Add(new SelectableItem { Name = name });
        }

        public void InitResult(IEnumerable<string> items)
        {
            Result.Clear();
            foreach (var name in items.Distinct())
                AddSelected(name);
        }

        public void AddSelected(string name)
        {
            if (Result.Contains(name)) return;
            Result.Add(name);

            var source = AllList.FirstOrDefault(x => x.Name == name);
            if (source != null)
                source.IsSelected = true;
        }

        public void RemoveSelected(string name)
        {
            if (!Result.Contains(name)) return;
            Result.Remove(name);

            var source = AllList.FirstOrDefault(x => x.Name == name);
            if (source != null)
                source.IsSelected = false;
        }
    }
}
