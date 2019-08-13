# 服务于Xml Orm框架的C#自动生成代码工具

此工具用于自动生成服务于Xml Orm框架的C#代码，生成的代码需要引用dotnet-core-dpz2及dotnet-core-dpz2-db两个包（可从Nuget下载）。

## XML文件定义示例

Xml Orm框架(简称Xorm框架)主要由XML主定义文件和XML表定义文件组成。

主定义XML包括平台分类、表标识符、表版本和表定义地址。

主定义XML文件格式(示例含Activity平台分类的ActivityNew表，Aos平台的AosObjects、AosAuthorize表)：

```
<database version="1.01.1711.002">
	<platform name="Activity" title="活动信息">
		<table name="ActivityNew" version="1.0.1810.2" path="/Activity/ActivityNew.xml"/>
	</platform>
	<platform name="Aos" title="框架数据库">
		<table name="AosObjects" version="2016.1102" path="/Aos/AosObjects.xml"/>
		<table name="AosAuthorize" version="1.5.1904.16" path="/Aos/AosAuthorize.xml"/>
	</platform>
</database>
```

表定义XML包括表名称、表描述信息、表字段的定义

表定义XML文件格式(示例为Activity平台分类的ActivityNew表)：

```
<table name="ActivityNew" title="最新活动" description="最新活动">
	<field name="Title" title="标题">
		<data type="string" size="50" float="0"/>
	</field>
	<field name="ImgPath" title="图片地址">
		<data type="string" size="500" float="0"/>
	</field>
	<field name="Description" title="描述">
		<data type="text" size="0" float="0"/>
	</field>
	<field name="Index" title="排序索引">
		<data type="int" size="0" float="0"/>
	</field>
</table>
```