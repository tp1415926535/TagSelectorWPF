# TagSelectorWPF [标签选择器]
* Support for bindings and attributes
  * A list of candidate sources: **Source** - ObserverableCollection&lt;string&gt;
  * Preset or get result list: **Result** - ObserverableCollectio&lt;string&gt;
* Enter text return to generate labels
  * Whether to allow input: **AllowInput** - bool 
  * Tip words when no input is entered: **Tip** - string
  * Whether to allow content other than candidate words to generate labels: **AllowCustom** - bool
  * Auto Associative Completion: **AutoComplete** - bool
     
* 支持绑定和属性
  * 候选源列表： **Source** - ObserverableCollection&lt;string&gt;
  * 预设或获取结果列表： **Result** - ObserverableCollectio&lt;string&gt;
* 输入文字回车生成标签
  * 是否允许输入：**AllowInput** - bool 
  * 未输入时提示词： **Tip** - string
  * 是否允许候选词以外的内容生成标签：**AllowCustom** - bool
  * 自动联想补全： **AutoComplete** - bool

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
