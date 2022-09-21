# BindTool

用于绑定组件数据的工具

* 全自动绑定的方式
* 自动绑定规则可以自定义，配置后自动绑定将更符合需求
* 在按照自己所定义规则创建物体的可以精准的绑定，间接省去绑定的步骤
* 提供自定义的脚本生成，自定义生成变量，属性，方法
* 自定义生成名称，可以在生成时按照一定的规范进行生成

## 快速构建

1、右键需要绑定物体，选择BindWindown

2、在Build界面中设置好保存的路径

3、点击Build按钮进行构建（如果绑定没有数据请在Bind界面将自定义绑定取消勾选）

这样绑定是将子物体下所有组件进行绑定，需要更灵活的绑定在Bind界面中打开自定义绑定选项

## 自定义绑定使用

1、打开自定义绑定选项

2、点击全部自动绑定

3、点击Build按钮进行构建

## 自定义绑定界面介绍

第一栏用于设置生成的数据会有两种模式显示
1、生成，该模式下将会生成新的脚本，显示输入栏输入生成脚本的名称
2、附加，该模式下会将绑定的数据生成到附加的脚本上，显示绑定的组件且必须继承Monobehaviour
点击最右边按钮可以在两种模式间切换

全部自动绑定：根据设置好的绑定规则对子物体进行绑定

绑定区域：选择一个或多个组件拖拽到绑定区域，点击想要绑定的组件即可

### Hierarchy界面

绑定：在Hierarchy界面选择要绑定的物体点击绑定即可
查看绑定：点击需要查看的组件后在绑定列表中查看对应的数据即可
五角星表示：红色为主物体，黄色为该物体绑定了组件，白色为该物体绑定了组件但并不是主物体的子物体

### 绑定列表

可以更改绑定的组件，更改变量名
注意该界面下的变量名为初始名称，最终变量名会根据设置进行变化

## 设置界面介绍

可以将当前所有的设置保存起来。然后根据不同的情况来选择对应的设置
如果导入到不同目录下请修改路径找到Core-Data-Resources-DataContainer

### 脚本设置

#### 通用生成

创建配置：

* 是否生成新脚本：如果勾选则在保存路径下生成新的脚本，如果不勾选则为附加模式
* 是否分文件生成：如果勾选了则将绑定数据生成在子文件
* 继承的类：生成脚本时继承的类
* 是否使用命名空间：勾选则创建命名空间
* 是否保存旧脚本：当生成新的脚本或改变脚本时是否将旧脚本保存起来

名称绑定：

* 绑定时自动生成变量名：如果勾选则每次绑定新的组件时自动设置变量名为物体名称，如果不勾选则为空
* 是否创建属性：如果勾选则会创建一个对应变量的属性
* 属性类型：选择创建set与get
* 是否添加类名：如果勾选了则会在生成时添加类型名(类型名添加后才会添加前缀和后缀)
* 脚本命名处理：处理初始名称
* 名称重复处理规则：1.不处理；2.添加数字（后缀）

#### 方法生成

提供两种方法生成

* 为指定类型的字段生成获取与设置的方法
* 为指定类型生成一个模板脚本(可编辑)，并在生成的时候读取这个模板脚本

字段访问列表：

* 选择对应的类型进行添加
* 选择字段
* 选择访问类型

模板方法列表：

* 选择对应的类型进行添加，添加时会创建脚本
* 选择模板脚本继承的类
* 打开脚本进行编辑并且只在下面所示中添加需要的模板方法

```csharp
#region Template Method
#endregion
```

### 自动绑定设置

在绑定组件时选择自动绑定时的绑定规则

* 根据名称来绑定对应的类型
* 根据名称忽略对应的物体
* 按照类型的顺序进行绑定

### 创建名称设置

当绑定组件或数据时自动为它们创建名称，默认为物体名称

* 名称替换列表，将默认名称中匹配的字符替换成指定字符

## 其他

绑定数据后会将数据生成到region里面，不要去修改他，每次生成只会替换region里面的数据，尽量代码中不要出现
重复的region。

```csharp
#region Auto Generate
#endregion
```

## 待开发功能

* 添加对Lua的支持

## 问题

有建议或者问题请联系
1076995595@qq.com
