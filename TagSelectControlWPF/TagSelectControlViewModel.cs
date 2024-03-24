using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;
using System.Runtime.Intrinsics.Arm;

namespace TagSelectControlWPF
{
    internal class TagSelectControlViewModel
    {
        public ObservableCollection<SelectableItem> AllList { get; set; } = new ObservableCollection<SelectableItem>();

        public ObservableCollection<string> SelectedList { get; set; } = new ObservableCollection<string>();

        public TagSelectControlViewModel()
        {
            AllList.CollectionChanged += AllList_CollectionChanged;
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
                foreach (var name in SelectedList)
                {
                    var item = AllList.FirstOrDefault(x=> x.Name == name);
                    if (item == null) continue;
                    if(!item.IsSelected)
                        removeTargets.Add(name);
                }
                foreach(var item in removeTargets)
                    SelectedList.Remove(item);

                var selectedItems = AllList.Where(x => x.IsSelected).Select(x => x.Name);
                var excepts = selectedItems.Except(SelectedList);
                foreach (var item in excepts)
                    SelectedList.Add(item);
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
            SelectedList.Clear();
            foreach (var name in items.Distinct())
                AddSelected(name);
        }

        public void AddSelected(string name)
        {
            if (SelectedList.Contains(name)) return;
            SelectedList.Add(name);

            var source = AllList.FirstOrDefault(x => x.Name == name);
            if (source != null)
                source.IsSelected = true;
        }

        public void RemoveSelected(string name)
        {
            if (!SelectedList.Contains(name)) return;
            SelectedList.Remove(name);

            var source = AllList.FirstOrDefault(x => x.Name == name);
            if (source != null)
                source.IsSelected = false;
        }

    }

}
