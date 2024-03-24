using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wpf.Net6.Test
{
    class MainWindowViewModel
    {
        public ObservableCollection<string> SourceList { get; set; } = new ObservableCollection<string>()
        {
            "冒险","休闲","竞赛","模拟"
        };


        public ObservableCollection<string> ResultList { get; set; } = new ObservableCollection<string>()
        {
            "冒险"
        };


        public void GetResult()
        {
            MessageBox.Show(string.Join(",", ResultList));
        }
    }
}
