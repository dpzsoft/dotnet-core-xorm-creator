using System;
using System.Collections.Generic;
using dpz2;

namespace Creator {
    public class Program {

        /// <summary>
        /// 字段集合
        /// </summary>
        public static List<string> Fields;

        /// <summary>
        /// 输出内容
        /// </summary>
        /// <param name="content"></param>
        public static void Say(string content, string sign = null) {
            Console.ForegroundColor = ConsoleColor.Green;
            if (sign.IsNone()) {
                Console.Write($"-");
            } else {
                Console.Write($"[{sign}]");
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" > ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(content);
        }

        private static void SayHelp() {
            Console.WriteLine($"参数说明：");
            Console.WriteLine($"    -namespace 代码所属命名空间定义");
            Console.WriteLine($"    -url XML定义在线地址");
            Console.WriteLine($"    -setting 主定义XML文件定义相对地址");
            Console.WriteLine($"    -path 代码输出目录");
        }

        static void Main(string[] args) {

            // 初始化字段集合
            Program.Fields = new List<string>();

            // 存储参数
            dpz2.KeyValues<string> pairs = new dpz2.KeyValues<string>();
            string sign = "";

            // 获取参数
            for (int i = 0; i < args.Length; i++) {
                if (sign.IsNone()) {
                    string argSign = args[i];
                    if (argSign.StartsWith("-") || argSign.StartsWith("/")) {
                        sign = argSign.Substring(1);
                    } else {
                        Say($"不支持的参数定义:{argSign}");
                        return;
                    }
                } else {
                    string argValue = args[i];
                    pairs[sign] = argValue;
                    sign = "";
                }
            }

            string nameSpace = pairs["namespace"];
            if (nameSpace.IsNone()) {
                Say("未发现命名空间定义地址!");
                Console.WriteLine();
                SayHelp();
                return;
            }

            string url = pairs["url"];
            if (url.IsNone()) {
                Say("未发现Xml定义地址!");
                Console.WriteLine();
                SayHelp();
                return;
            }

            string setting = pairs["setting"];
            if (setting.IsNone()) {
                Say("未发现主setting定义相对地址!");
                Console.WriteLine();
                SayHelp();
                return;
            }

            string path = pairs["path"];
            if (path.IsNone()) {
                Say("未发现存储路径!");
                return;
            }

            // 处理存储路径
            path = path.Replace("\\", "/");
            if (!path.EndsWith("/")) path += "/";
            //string sourcePath = path + "Tables";
            //Say($"建立存储文件夹 {sourcePath} ...");
            //if (!System.IO.Directory.Exists(sourcePath)) System.IO.Directory.CreateDirectory(sourcePath);

            // 获取定义xml文本
            Say($"正在从 {url}{setting} 下载XML定义数据 ...");
            string xmlStr = dpz2.Net.Https.Get(url + setting);
            Say($"下载完成");

            // 解析定义
            using (dpz2.Xml.XmlRoot xml = new dpz2.Xml.XmlRoot(xmlStr)) {
                Say($"当前XML定义版本 {xml["database"].Attr["version"]}");

                // 判断并新建Tables类文件
                string tablesPath = path + "Tables.cs";
                if (!System.IO.File.Exists(tablesPath)) {
                    using (dpz2.File.UTF8File file = new dpz2.File.UTF8File(tablesPath, System.IO.FileMode.Create)) {
                        file.WriteLine("");
                    }
                }

                // 创建一个新的文件
                using (dpz2.File.UTF8File file = new dpz2.File.UTF8File(tablesPath, System.IO.FileMode.Truncate)) {

                    // 初始化Tables类文件
                    Say($"Tables类文件初始定义 ...");
                    file.AppendLine($"using System;");
                    file.AppendLine($"using System.Collections.Generic;");
                    file.AppendLine($"using System.Text;");
                    file.AppendLine($"");

                    // 输出命名空间
                    file.AppendLine($"namespace {nameSpace} {{");
                    file.AppendLine($"");

                    // 输出类信息
                    file.AppendLine($"    /// <summary>");
                    file.AppendLine($"    /// Orm表格信息集合");
                    file.AppendLine($"    /// </summary>");
                    file.AppendLine($"    public class Tables : dpz2.db.OrmTables {{");
                    file.AppendLine($"");

                    // 获取所有的platform
                    var platforms = xml["database"].GetNodesByName("platform", false);
                    foreach (var platform in platforms) {

                        // platform.Name
                        var platformName = platform.Attr["name"];

                        Say($"处理中 ...", platformName);

                        // 获取所有的table
                        var tables = platform.GetNodesByName("table", false);
                        foreach (var table in tables) {

                            string tableName = table.Attr["name"];

                            // 初始化Tables类文件
                            Say($"输出Tables类文件中的 {tableName} 表定义 ...");
                            file.AppendLine($"        // {tableName}表对象缓存");
                            file.AppendLine($"        private {platformName}.{tableName} _cache{tableName} = null;");
                            file.AppendLine($"");
                            file.AppendLine($"        /// <summary>");
                            file.AppendLine($"        /// 获取 {tableName} 表定义");
                            file.AppendLine($"        /// </summary>");
                            file.AppendLine($"        public {platformName}.{tableName} {tableName} {{");
                            file.AppendLine($"            get {{");
                            file.AppendLine($"                if (_cache{tableName} == null) _cache{tableName} = new {platformName}.{tableName}();");
                            file.AppendLine($"                return _cache{tableName};");
                            file.AppendLine($"            }}");
                            file.AppendLine($"        }}");
                            file.AppendLine($"");

                            //创建类文件
                            Say($"处理类文件 {tableName} ...", platformName);
                            OrmCreator.CreateTableClass(url, path, platformName, nameSpace, table);
                        }
                    }

                    Say($"Tables类名称查找定义 ...");
                    file.AppendLine($"        /// <summary>");
                    file.AppendLine($"        /// 根据名称获取表格定义");
                    file.AppendLine($"        /// </summary>");
                    file.AppendLine($"        protected override dpz2.db.OrmTable OnGetTable(string name) {{");
                    file.AppendLine($"            switch (name) {{");
                    file.AppendLine($"");
                    foreach (var platform in platforms) {
                        // 获取所有的table
                        var tables = platform.GetNodesByName("table", false);
                        foreach (var table in tables) {
                            string tableName = table.Attr["name"];
                            file.AppendLine($"                case \"{tableName}\": return this.{tableName};");
                        }

                    }
                    file.AppendLine($"                default: throw new Exception(\"未找到对应表定义\");");
                    file.AppendLine($"            }}");
                    file.AppendLine($"        }}");
                    file.AppendLine($"");

                    Say($"Tables类释放资源定义 ...");
                    file.AppendLine($"        /// <summary>");
                    file.AppendLine($"        /// 释放资源");
                    file.AppendLine($"        /// </summary>");
                    file.AppendLine($"        protected override void OnDispose() {{");
                    file.AppendLine($"            base.OnDispose();");
                    file.AppendLine($"");
                    foreach (var platform in platforms) {
                        // 获取所有的table
                        var tables = platform.GetNodesByName("table", false);
                        foreach (var table in tables) {
                            string tableName = table.Attr["name"];
                            file.AppendLine($"            if (_cache{tableName} != null) {{");
                            file.AppendLine($"                _cache{tableName}.Dispose();");
                            file.AppendLine($"                _cache{tableName} = null;");
                            file.AppendLine($"            }}");
                            file.AppendLine($"");
                        }

                    }
                    file.AppendLine($"        }}");
                    file.AppendLine($"");

                    Say($"Tables类文件结尾定义 ...");
                    file.AppendLine($"");
                    file.AppendLine($"    }}");

                    file.AppendLine($"");
                    file.AppendLine($"}}");

                }

                // 判断并新建fields类文件
                string fieldsPath = path + "Fields.cs";
                if (!System.IO.File.Exists(fieldsPath)) {
                    using (dpz2.File.UTF8File file = new dpz2.File.UTF8File(fieldsPath, System.IO.FileMode.Create)) {
                        file.WriteLine("");
                    }
                }

                // 创建一个新的文件
                using (dpz2.File.UTF8File file = new dpz2.File.UTF8File(fieldsPath, System.IO.FileMode.Truncate)) {

                    // 初始化Tables类文件
                    Say($"Fields ...");
                    file.AppendLine($"using System;");
                    file.AppendLine($"using System.Collections.Generic;");
                    file.AppendLine($"using System.Text;");
                    file.AppendLine($"");

                    // 输出命名空间
                    file.AppendLine($"namespace {nameSpace} {{");
                    file.AppendLine($"");

                    // 输出类信息
                    file.AppendLine($"    /// <summary>");
                    file.AppendLine($"    /// Orm表格信息集合");
                    file.AppendLine($"    /// </summary>");
                    file.AppendLine($"    public class Fields : dpz2.db.OrmFields {{");
                    file.AppendLine($"");
                    file.AppendLine($"        // 空表对象");
                    file.AppendLine($"        private dpz2.db.SqlUnits.Table _table = new dpz2.db.SqlUnits.Table(\"\");");
                    file.AppendLine($"");

                    foreach (var field in Program.Fields) {

                        // 初始化Tables类文件
                        Say($"输出Fields类文件中的 {field} 字段定义 ...");
                        file.AppendLine($"        // {field} 字段对象缓存");
                        file.AppendLine($"        private dpz2.db.SqlUnits.TableField _cache{field} = null;");
                        file.AppendLine($"");
                        file.AppendLine($"        /// <summary>");
                        file.AppendLine($"        /// 获取 {field} 字段定义");
                        file.AppendLine($"        /// </summary>");
                        file.AppendLine($"        public dpz2.db.SqlUnits.TableField {field} {{");
                        file.AppendLine($"            get {{");
                        file.AppendLine($"                if (Equals(_cache{field}, null)) _cache{field} = new dpz2.db.SqlUnits.TableField(_table,\"{field}\");");
                        file.AppendLine($"                return _cache{field};");
                        file.AppendLine($"            }}");
                        file.AppendLine($"        }}");
                        file.AppendLine($"");

                    }

                    Say($"Fields类释放资源定义 ...");
                    file.AppendLine($"        /// <summary>");
                    file.AppendLine($"        /// 释放资源");
                    file.AppendLine($"        /// </summary>");
                    file.AppendLine($"        protected override void OnDispose() {{");
                    file.AppendLine($"            base.OnDispose();");
                    file.AppendLine($"");

                    foreach (var field in Program.Fields) {
                        file.AppendLine($"            if (!Equals(_cache{field}, null)) {{");
                        file.AppendLine($"                _cache{field}.Dispose();");
                        file.AppendLine($"                _cache{field} = null;");
                        file.AppendLine($"            }}");
                        file.AppendLine($"");
                    }

                    file.AppendLine($"        }}");
                    file.AppendLine($"");

                    Say($"Fields类文件结尾定义 ...");
                    file.AppendLine($"");
                    file.AppendLine($"    }}");

                    file.AppendLine($"");
                    file.AppendLine($"}}");

                }

            }
        }
    }
}
