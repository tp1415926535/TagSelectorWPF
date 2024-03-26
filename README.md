# TagSelectorWPF
Tag selector, support for binding, setting up preset lists, adding custom values.        
标签选择器，支持绑定，设置预设列表，添加自定义值。

[![release](https://img.shields.io/github/v/release/tp1415926535/TagSelectorWPF?color=green&logo=github)](https://github.com/tp1415926535/TagSelectorWPF/releases) 
[![nuget](https://img.shields.io/nuget/v/TagSelectorWPF?color=lightblue&logo=nuget)](https://www.nuget.org/packages/TagSelectorWPF)     
![language](https://img.shields.io/github/languages/top/tp1415926535/TagSelectorWPF)

## Example
![image](https://github.com/tp1415926535/TagSelectorWPF/assets/58326584/78997e6c-d84a-441a-a0c2-cbc3c9695ba9)

xaml:
```xml
xmlns:tsc="clr-namespace:TagSelectorWPF;assembly=TagSelectorWPF"

<tsc:TagSelector Source="{Binding SourceList}" Result="{Binding ResultList}"
                 Tip="Press Enter to add Tag"  AllowInput="True" AllowCustom="True"/>
```

ViewModel:
```c#
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
    MessageBox.Show(string.Join(",", ResultList));
}
```
