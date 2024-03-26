using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagSelectorWPF
{
    internal class TagSelectorViewModel
    {
        /// <summary>
        /// UserControl, to get/set property
        /// 用户控件，用于设置或获取属性
        /// </summary>
        public TagSelector tagSelectorControl { get; set; }

        /// <summary>
        /// Source list with boolean state, only for binding in control.
        /// <para>来源列表转换到带布尔状态，仅用于绑定到控件</para> 
        /// </summary>
        internal ObservableCollection<SelectableItem> AllList { get; set; } = new ObservableCollection<SelectableItem>();

        /// <summary>
        /// SelectedList, only for displaying in control. Add or remove item should Set Contorl's Result property
        /// <para>绑定到控件的结果列表，仅用于显示。添加或删除项需要设置控件的Result属性</para> 
        /// </summary>
        internal ObservableCollection<string> SelectedList { get; set; } = new ObservableCollection<string>();


        public TagSelectorViewModel(TagSelector tagSelector)
        {
            tagSelectorControl = tagSelector;

            AllList.CollectionChanged += AllList_CollectionChanged;
        }

        /// <summary>
        /// Add initial values to AllList, subscribe to 'Source' changes
        /// <para>添加初始值到AllList，并订阅来源属性'Source'项变更</para> 
        /// </summary>
        /// <param name="items"></param>
        public void InitSource(ObservableCollection<string> items)
        {
            AllList.Clear();
            if (items == null) return;
            foreach (var name in items.ToList().Distinct())
                AllList.Add(new SelectableItem { Name = name });

            items.CollectionChanged += SourceList_CollectionChanged;
        }

        /// <summary>
        /// Add or remove to AllList when 'Source' change
        /// <para>候选列表'Source'源列表变更时，对应添加或移除AllList</para> 
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
        /// AllList item Subscribe to Boolean property change events
        /// <para>AllList每项订阅布尔属性变更事件</para> 
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
        /// Change 'Result' when boolean attribute is changed
        /// <para>变更布尔属性时，改变结果属性'Result'</para> 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropertyChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableItem.IsSelected))
            {
                var removeTargets = new List<string>();
                foreach (var name in SelectedList)
                {
                    var item = AllList.FirstOrDefault(x => x.Name == name);
                    if (item == null) continue;
                    if (!item.IsSelected)
                        removeTargets.Add(name);
                }
                foreach (var item in removeTargets)
                    tagSelectorControl.Result.Remove(item);//set control property instead of viewModel list

                var selectedItems = AllList.Where(x => x.IsSelected).Select(x => x.Name);
                var excepts = selectedItems.Except(SelectedList);
                foreach (var item in excepts)
                    tagSelectorControl.Result.Add(item);
            }
        }

        /// <summary>
        /// Adding a preset result value, subscribe to 'Result' changes
        /// <para>添加预设结果值，订阅结果属性'Result'项变更</para> 
        /// </summary>
        /// <param name="items"></param>
        public void InitResult(ObservableCollection<string> items)
        {
            SelectedList.Clear();
            if (items == null) return;
            foreach (var name in items.ToList().Distinct())
                AddSelected(name);

            items.CollectionChanged += ResultList_CollectionChanged;
        }
        /// <summary>
        /// 'Result' item changed, effect to SelectedList and set AllList item boolean
        /// <para>结果属性'Result'源变更时，同步到SelectedList并且设置AllList对应项的布尔值</para> 
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
        /// Add SelectedList item and set selected state true if exist in AllList.
        /// For displaying, should be only called by CollectionChange and init.
        /// <para>添加SelectedList项目，如果AllList中存在则设置选中状态为true。用于显示，应该只被订阅的变更事件和初始化调用</para> 
        /// </summary>
        /// <param name="name"></param>
        public void AddSelected(string name)
        {
            if (SelectedList.Contains(name)) return;
            if (!tagSelectorControl.AllowInput && !AllList.Any(x => x.Name == name))
            {
                if (!tagSelectorControl.AllowCustom)
                    tagSelectorControl.Result.Remove(name);
                return;
            }
            SelectedList.Add(name);

            var source = AllList.FirstOrDefault(x => x.Name == name);
            if (source != null)
                source.IsSelected = true;
        }
        /// <summary>
        /// Remove SelectedList item and set selected state false if exist in source.
        /// For displaying, should be only called by CollectionChange.
        /// <para>删除SelectedList项目，如果AllList中存在则将选中状态设置为false。用于显示，应该只被订阅的变更事件调用</para> 
        /// </summary>
        /// <param name="name"></param>
        public void RemoveSelected(string name)
        {
            if (!SelectedList.Contains(name)) return;
            SelectedList.Remove(name);

            foreach (var item in AllList.Where(x => x.Name == name))
                item.IsSelected = false;
        }
    }
}
