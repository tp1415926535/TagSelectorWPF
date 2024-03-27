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
            "Adventure","Action","Sports","Simulation","Platformer","RPG","First-person shooter","Action-adventure","Fighting","Real-time strategy","Racing","Shooter"
        };


        public ObservableCollection<string> ResultList { get; set; } = new ObservableCollection<string>()
        { };


        public void AddItem()
        {
            ResultList.Add("data1");
            ResultList.Add("dataCustom");
        }

        public void GetResult()
        {
            MessageBox.Show(string.Join(",", ResultList));
        }
    }
}
