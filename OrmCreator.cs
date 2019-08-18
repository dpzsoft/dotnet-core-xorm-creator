using System;
using System.Collections.Generic;
using System.Text;

namespace Creator {
    public class OrmCreator {

        /// <summary>
        /// 生成表类文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="platform"></param>
        /// <param name="ns"></param>
        /// <param name="table"></param>
        public static void CreateTableClass(string url, string path, string platform, string ns, dpz2.Xml.XmlNode table) {

            // 建立平台文件夹
            string platformPath = path + platform;
            if (!System.IO.Directory.Exists(platformPath)) System.IO.Directory.CreateDirectory(platformPath);

            // 获取表名称
            string tableName = table.Attr["name"];
            string sign = $"{platform}.{tableName}";
            string tableVersion = table.Attr["version"];
            Program.Say($"定义版本 {tableVersion}", sign);

            // 下载表配置文件
            string xmlPath = table.Attr["path"];
            Program.Say($"正在从 {url}{xmlPath} 下载表格XML定义数据 ...", sign);
            string xmlStr = dpz2.Net.Https.Get(url + xmlPath);
            Program.Say($"下载完成", sign);

            // 解析定义
            using (dpz2.Xml.XmlRoot xml = new dpz2.Xml.XmlRoot(xmlStr)) {

                var xmlTable = xml["table"];
                var fields = xmlTable.GetNodesByName("field", false);

                // 定义类存储路径
                string tablePath = platformPath + "/" + tableName + ".cs";

                // 判断并新建文件
                if (!System.IO.File.Exists(tablePath)) {
                    using (dpz2.File.UTF8File file = new dpz2.File.UTF8File(tablePath, System.IO.FileMode.Create)) {
                        file.WriteLine("");
                    }
                }

                // 创建一个新的文件
                using (dpz2.File.UTF8File file = new dpz2.File.UTF8File(tablePath, System.IO.FileMode.Truncate)) {
                    file.AppendLine($"using System;");
                    file.AppendLine($"using System.Collections.Generic;");
                    file.AppendLine($"using System.Text;");
                    file.AppendLine($"using dpz2;");
                    file.AppendLine($"");

                    // 输出命名空间
                    file.AppendLine($"namespace {ns}.{platform} {{");
                    file.AppendLine($"");

                    // 输出类信息
                    file.AppendLine($"    /// <summary>");
                    file.AppendLine($"    /// {xmlTable.Attr["title"]}");
                    file.AppendLine($"    /// {xmlTable.Attr["description"]}");
                    file.AppendLine($"    /// Version {tableVersion}");
                    file.AppendLine($"    /// </summary>");
                    file.AppendLine($"    public class {tableName} : dpz2.db.SqlUnits.Table {{");
                    file.AppendLine($"");

                    // 输出行操作类信息
                    file.AppendLine($"        /// <summary>");
                    file.AppendLine($"        /// {xmlTable.Attr["title"]} 行数据操作器");
                    file.AppendLine($"        /// </summary>");
                    file.AppendLine($"        public class RowOperator : dpz2.db.RowOperator {{");
                    file.AppendLine($"");
                    file.AppendLine($"            /// <summary>");
                    file.AppendLine($"            /// 对象实例化");
                    file.AppendLine($"            /// </summary>");
                    file.AppendLine($"            /// <param name=\"row\">行数据对象</param>");
                    file.AppendLine($"            public RowOperator(dpz2.db.Row row) : base(row) {{ }}");
                    file.AppendLine($"");
                    file.AppendLine($"            /// <summary>");
                    file.AppendLine($"            /// 设置ID");
                    file.AppendLine($"            /// </summary>");
                    file.AppendLine($"            /// <param name=\"value\">值</param>");
                    file.AppendLine($"            public RowOperator SetID(string value) {{");
                    file.AppendLine($"                base.Row[\"ID\"] = value;");
                    file.AppendLine($"                return this;");
                    file.AppendLine($"            }}");
                    file.AppendLine($"");
                    file.AppendLine($"            /// <summary>");
                    file.AppendLine($"            /// 设置ID");
                    file.AppendLine($"            /// </summary>");
                    file.AppendLine($"            /// <param name=\"value\">值</param>");
                    file.AppendLine($"            public RowOperator SetID(long value) {{");
                    file.AppendLine($"                base.Row[\"ID\"] = \"\" + value;");
                    file.AppendLine($"                return this;");
                    file.AppendLine($"            }}");
                    file.AppendLine($"");
                    file.AppendLine($"            /// <summary>");
                    file.AppendLine($"            /// 获取或设置ID");
                    file.AppendLine($"            /// </summary>");
                    file.AppendLine($"            /// <param name=\"value\">值</param>");
                    file.AppendLine($"            public long ID {{ get {{ return base.Row[\"ID\"].ToLong(); }} set {{ base.Row[\"ID\"] = \"\" + value; }} }}");
                    foreach (var field in fields) {
                        string fieldName = field.Attr["name"];
                        var fieldData = field["data"];
                        // 添加到字段集合
                        if (!Program.Fields.Contains(fieldName)) Program.Fields.Add(fieldName);
                        file.AppendLine($"");
                        file.AppendLine($"            /// <summary>");
                        file.AppendLine($"            /// 设置{field.Attr["title"]}");
                        file.AppendLine($"            /// </summary>");
                        file.AppendLine($"            /// <param name=\"value\">值</param>");
                        file.AppendLine($"            public RowOperator Set{fieldName}(string value) {{");
                        file.AppendLine($"                base.Row[\"{fieldName}\"] = value;");
                        file.AppendLine($"                return this;");
                        file.AppendLine($"            }}");

                        // 添加对类型的特殊支持
                        switch (fieldData.Attr["type"].ToLower()) {
                            case "numeric":
                                if (fieldData.Attr["float"].ToLower() == "0") {
                                    // 长整型
                                    file.AppendLine($"");
                                    file.AppendLine($"            /// <summary>");
                                    file.AppendLine($"            /// 设置{field.Attr["title"]}");
                                    file.AppendLine($"            /// </summary>");
                                    file.AppendLine($"            /// <param name=\"value\">值</param>");
                                    file.AppendLine($"            public RowOperator Set{fieldName}(long value) {{");
                                    file.AppendLine($"                base.Row[\"{fieldName}\"] = \"\" + value;");
                                    file.AppendLine($"                return this;");
                                    file.AppendLine($"            }}");
                                    file.AppendLine($"");
                                    file.AppendLine($"            /// <summary>");
                                    file.AppendLine($"            /// 获取或设置{field.Attr["title"]}");
                                    file.AppendLine($"            /// </summary>");
                                    file.AppendLine($"            /// <param name=\"value\">值</param>");
                                    file.AppendLine($"            public long {fieldName} {{ get {{ return base.Row[\"{fieldName}\"].ToLong(); }} set {{ base.Row[\"{fieldName}\"] = \"\" + value; }} }}");
                                } else {
                                    // 浮点类型
                                    file.AppendLine($"");
                                    file.AppendLine($"            /// <summary>");
                                    file.AppendLine($"            /// 设置{field.Attr["title"]}");
                                    file.AppendLine($"            /// </summary>");
                                    file.AppendLine($"            /// <param name=\"value\">值</param>");
                                    file.AppendLine($"            public RowOperator Set{fieldName}(double value) {{");
                                    file.AppendLine($"                base.Row[\"{fieldName}\"] = \"\" + value;");
                                    file.AppendLine($"                return this;");
                                    file.AppendLine($"            }}");
                                    file.AppendLine($"");
                                    file.AppendLine($"            /// <summary>");
                                    file.AppendLine($"            /// 获取或设置{field.Attr["title"]}");
                                    file.AppendLine($"            /// </summary>");
                                    file.AppendLine($"            /// <param name=\"value\">值</param>");
                                    file.AppendLine($"            public double {fieldName} {{ get {{ return base.Row[\"{fieldName}\"].ToDouble(); }} set {{ base.Row[\"{fieldName}\"] = \"\" + value; }} }}");
                                }
                                break;
                            case "int":
                                // 浮点类型
                                file.AppendLine($"");
                                file.AppendLine($"            /// <summary>");
                                file.AppendLine($"            /// 设置{field.Attr["title"]}");
                                file.AppendLine($"            /// </summary>");
                                file.AppendLine($"            /// <param name=\"value\">值</param>");
                                file.AppendLine($"            public RowOperator Set{fieldName}(int value) {{");
                                file.AppendLine($"                base.Row[\"{fieldName}\"] = \"\" + value;");
                                file.AppendLine($"                return this;");
                                file.AppendLine($"            }}");
                                file.AppendLine($"");
                                file.AppendLine($"            /// <summary>");
                                file.AppendLine($"            /// 获取或设置{field.Attr["title"]}");
                                file.AppendLine($"            /// </summary>");
                                file.AppendLine($"            /// <param name=\"value\">值</param>");
                                file.AppendLine($"            public int {fieldName} {{ get {{ return base.Row[\"{fieldName}\"].ToInteger(); }} set {{ base.Row[\"{fieldName}\"] = \"\" + value; }} }}");
                                break;
                            default:
                                break;
                        }
                    }
                    file.AppendLine($"");

                    file.AppendLine($"        }}");
                    file.AppendLine($"");
                    file.AppendLine($"        /// <summary>");
                    file.AppendLine($"        /// 获取*字段定义");
                    file.AppendLine($"        /// </summary>");
                    file.AppendLine($"        public dpz2.db.SqlUnits.TableField All {{ get; private set; }}");
                    file.AppendLine($"");
                    file.AppendLine($"        /// <summary>");
                    file.AppendLine($"        /// 获取ID字段定义");
                    file.AppendLine($"        /// </summary>");
                    file.AppendLine($"        public dpz2.db.SqlUnits.TableField ID {{ get; private set; }}");
                    foreach (var field in fields) {
                        string fieldName = field.Attr["name"];
                        file.AppendLine($"");
                        file.AppendLine($"        /// <summary>");
                        file.AppendLine($"        /// 获取{field.Attr["title"]}字段定义");
                        file.AppendLine($"        /// </summary>");
                        file.AppendLine($"        public dpz2.db.SqlUnits.TableField {fieldName} {{ get; private set; }}");
                    }
                    file.AppendLine($"");
                    file.AppendLine($"        /// <summary>");
                    file.AppendLine($"        /// 创建一个行数据操作器");
                    file.AppendLine($"        /// </summary>");
                    file.AppendLine($"        /// <param name=\"row\">行数据对象</param>");
                    file.AppendLine($"        protected override RowOperator OnRowerCreate(dpz2.db.Row row) {{");
                    file.AppendLine($"            return new RowOperator(row);");
                    file.AppendLine($"        }}");
                    file.AppendLine($"");
                    file.AppendLine($"        /// <summary>");
                    file.AppendLine($"        /// 获取字段定义");
                    file.AppendLine($"        /// </summary>");
                    file.AppendLine($"        /// <param name=\"name\">字段名称</param>");
                    file.AppendLine($"        protected override dpz2.db.SqlUnits.TableField OnGetField(string name) {{");
                    file.AppendLine($"            switch (name) {{");
                    file.AppendLine($"                case \"ID\": return this.ID;");
                    foreach (var field in fields) {
                        string fieldName = field.Attr["name"];
                        file.AppendLine($"                case \"{fieldName}\": return this.{fieldName};");
                    }
                    file.AppendLine($"                default: return base.OnGetField(name);");
                    file.AppendLine($"            }}");
                    file.AppendLine($"        }}");
                    file.AppendLine($"");
                    file.AppendLine($"        /// <summary>");
                    file.AppendLine($"        /// 对象实例化");
                    file.AppendLine($"        /// </summary>");
                    file.AppendLine($"        public {tableName}() : base(\"{tableName}\") {{");
                    file.AppendLine($"            this.All = new dpz2.db.SqlUnits.TableField(this, \"*\");");
                    file.AppendLine($"            this.ID = new dpz2.db.SqlUnits.TableField(this, \"ID\");");
                    foreach (var field in fields) {
                        string fieldName = field.Attr["name"];
                        file.AppendLine($"            this.{fieldName} = new dpz2.db.SqlUnits.TableField(this, \"{fieldName}\");");
                    }
                    file.AppendLine($"        }}");

                    file.AppendLine($"");
                    file.AppendLine($"    }}");

                    file.AppendLine($"");
                    file.AppendLine($"}}");
                }

            }

        }

    }
}
