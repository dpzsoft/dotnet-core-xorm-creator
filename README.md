# ������Xml Orm��ܵ�C#�Զ����ɴ��빤��

�˹��������Զ����ɷ�����Xml Orm��ܵ�C#���룬���ɵĴ�����Ҫ����dotnet-core-dpz2��dotnet-core-dpz2-db���������ɴ�Nuget���أ���

## XML�ļ�����ʾ��

Xml Orm���(���Xorm���)��Ҫ��XML�������ļ���XML�����ļ���ɡ�

������XML����ƽ̨���ࡢ���ʶ������汾�ͱ����ַ��

������XML�ļ���ʽ(ʾ����Activityƽ̨�����ActivityNew��Aosƽ̨��AosObjects��AosAuthorize��)��

```
<database version="1.01.1711.002">
	<platform name="Activity" title="���Ϣ">
		<table name="ActivityNew" version="1.0.1810.2" path="/Activity/ActivityNew.xml"/>
	</platform>
	<platform name="Aos" title="������ݿ�">
		<table name="AosObjects" version="2016.1102" path="/Aos/AosObjects.xml"/>
		<table name="AosAuthorize" version="1.5.1904.16" path="/Aos/AosAuthorize.xml"/>
	</platform>
</database>
```

����XML���������ơ���������Ϣ�����ֶεĶ���

����XML�ļ���ʽ(ʾ��ΪActivityƽ̨�����ActivityNew��)��

```
<table name="ActivityNew" title="���»" description="���»">
	<field name="Title" title="����">
		<data type="string" size="50" float="0"/>
	</field>
	<field name="ImgPath" title="ͼƬ��ַ">
		<data type="string" size="500" float="0"/>
	</field>
	<field name="Description" title="����">
		<data type="text" size="0" float="0"/>
	</field>
	<field name="Index" title="��������">
		<data type="int" size="0" float="0"/>
	</field>
</table>
```