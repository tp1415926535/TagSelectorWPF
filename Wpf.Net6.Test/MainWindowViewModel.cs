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
            "冒险","台湾前途前往台湾掐头去尾","工业区完好无缺二号桥威尔和乔恩乔恩七年企鹅"
        };


        public void GetResult()
        {
            //ResultList.Add("测试");
            MessageBox.Show(string.Join(",", ResultList));
        }
    }
}
