using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            dep.viewModel.InitSource((IEnumerable<string>)e.NewValue);
        }

        public IEnumerable<string> Result
        {
            get { return (IEnumerable<string>)GetValue(resultProperty); }
            set { SetValue(resultProperty, value); }
        }
        public static readonly DependencyProperty resultProperty = DependencyProperty.Register("Result", typeof(IEnumerable<string>), typeof(TagSelectControl), new FrameworkPropertyMetadata(null,
                                  FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ResultChangedEvent));
        private static void ResultChangedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dep = d as TagSelectControl;
            if (dep == null) return;
            dep.viewModel.InitResult((IEnumerable<string>)e.NewValue);
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

        internal TagSelectControlViewModel viewModel;
        public TagSelectControl()
        {
            InitializeComponent();
            ContentGrid.DataContext = viewModel = new TagSelectControlViewModel();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var name = (sender as Button).DataContext as string;
            if (name == null) return;
            viewModel.RemoveSelected(name);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (string.IsNullOrEmpty(textBox.Text)) return;
            viewModel.AddSelected(textBox.Text);
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
    }
}
