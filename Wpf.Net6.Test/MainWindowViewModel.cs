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
            "data1","data2","data3","data4"
        };


        public ObservableCollection<string> ResultList { get; set; } = new ObservableCollection<string>()
        {
           "data1","dataCustom"
        };


        public void GetResult()
        {
            //ResultList.Add("测试");
            MessageBox.Show(string.Join(",", ResultList));
        }
    }
}
