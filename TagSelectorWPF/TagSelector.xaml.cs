using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
            dep.viewModel.InitSource((ObservableCollection<string>)e.NewValue);
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
            dep.viewModel.InitResult((ObservableCollection<string>)e.NewValue);
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
        /// Allow add tag by input
        /// <para>是否允许输入标签</para> 
        /// </summary>
        public bool AllowInput
        {
            get { return (bool)GetValue(allowInputProperty); }
            set { SetValue(allowInputProperty, value); }
        }
        public static readonly DependencyProperty allowInputProperty = DependencyProperty.Register("AllowInput", typeof(bool), typeof(TagSelector), new FrameworkPropertyMetadata(default));

        /// <summary>
        /// Allow add custom tag
        /// <para>是否允许存在不在Source内的自定义标签</para> 
        /// </summary>
        public bool AllowCustom
        {
            get { return (bool)GetValue(allowCustomProperty); }
            set { SetValue(allowCustomProperty, value); }
        }
        public static readonly DependencyProperty allowCustomProperty = DependencyProperty.Register("AllowCustom", typeof(bool), typeof(TagSelector), new FrameworkPropertyMetadata(default));

        /// <summary>
        /// Auto complete suggestion when type in textbox
        /// <para>在文本框中输入时自动联想补全</para> 
        /// </summary>
        public bool AutoComplete
        {
            get { return (bool)GetValue(autoCompleteProperty); }
            set { SetValue(autoCompleteProperty, value); }
        }
        public static readonly DependencyProperty autoCompleteProperty = DependencyProperty.Register("AutoComplete", typeof(bool), typeof(TagSelector), new FrameworkPropertyMetadata(default));
        #endregion

        /// <summary>
        /// Providing Drag for Horizontal ScrollViewer
        /// <para>为横向滚动视图提供拖动</para> 
        /// </summary>
        ScrollDragger scrollDragger;

        /// <summary>
        /// ViewModel, binding to Show
        /// <para>ViewModel，绑定显示</para> 
        /// </summary>
        TagSelectorViewModel viewModel;

        public TagSelector()
        {
            InitializeComponent();

            viewModel = new TagSelectorViewModel(this);
            AllItemsControl.DataContext = viewModel.AllList;
            ResultItemsControl.DataContext = viewModel.SelectedList;

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
            if (AllowCustom || (!AllowCustom && Source.Contains(name)))
            {
                if (!Result.Contains(name))
                    Result.Add(name);
            }
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

        /// <summary>
        /// Suggestion TextBox
        /// from "https://stackoverflow.com/a/51685375/15891701"
        /// </summary>
        private string _currentInput = "";
        private string _currentSuggestion = "";
        private string _currentText = "";
        private int _selectionStart;
        private int _selectionLength;
        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AutoComplete) return;
            if (Source == null || Result == null) return;
            var input = InputBox.Text;
            if (input.Length >= _currentInput.Length && input != _currentSuggestion)
            {
                _currentSuggestion = Source.FirstOrDefault(x => x.StartsWith(input) && !Result.Contains(x));
                if (_currentSuggestion != null)
                {
                    _currentText = _currentSuggestion;
                    _selectionStart = input.Length;
                    _selectionLength = _currentSuggestion.Length - input.Length;

                    InputBox.Text = _currentText;
                    InputBox.Select(_selectionStart, _selectionLength);
                }
            }
            _currentInput = input;
        }
        #endregion
    }
}
