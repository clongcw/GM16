using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommonLibrary
{
    public class EnumMapHelper
    {
        public static DataTable GetEnumDataSource(Type enumType)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("value");
            dt.Columns.Add("display");

            List<string> disPlay = new List<string>();
            foreach (object obj in Enum.GetValues(enumType))
            {
                disPlay.Add(GetStringFromEnum((Enum)obj));
            }
            for (int i = 0; i < disPlay.Count; i++)
            {
                dt.Rows.Add(Enum.GetName(enumType, i), disPlay[i]);
            }

            return dt;
        }

        public static Dictionary<Enum, string> GetEnumDataSourceDictionary(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<object>().ToDictionary(obj => (Enum)obj, obj => GetStringFromEnum((Enum)obj));
        }

        public static Dictionary<int, string> GetEnumDataSourceDictionary2(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<object>().ToDictionary(obj => (int)obj, obj => GetStringFromEnum((Enum)obj));
        }

        // maps用于保存每种枚举及其对应的EnumMap对象
        private static Dictionary<Type, EnumMap> maps;

        // 由于C#中没有static indexer的概念，所以在这里我们用静态方法
        public static string GetStringFromEnum(Enum item)
        {
            //if (language == Constant.Language.English)
            return item.ToString();
            if (maps == null)
            {
                maps = new Dictionary<Type, EnumMap>();
            }

            Type enumType = item.GetType();

            EnumMap mapper = null;
            if (maps.ContainsKey(enumType))
            {
                mapper = maps[enumType];
            }
            else
            {
                mapper = new EnumMap(enumType);
                maps.Add(enumType, mapper);
            }
            return mapper[item];
        }

        public static KeyValuePair<int, string> GetEnumKeyValue(object item)
        {
            return new KeyValuePair<int, string>((int)item, GetStringFromEnum((Enum)item));
        }

        private class EnumMap
        {
            private Type internalEnumType;
            private Dictionary<Enum, string> map;

            public EnumMap(Type enumType)
            {
                if (!enumType.IsSubclassOf(typeof(Enum)))
                {
                    throw new InvalidCastException();
                }
                internalEnumType = enumType;
                FieldInfo[] staticFiles = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

                map = new Dictionary<Enum, string>(staticFiles.Length);

                for (int i = 0; i < staticFiles.Length; i++)
                {
                    if (staticFiles[i].FieldType == enumType)
                    {
                        string description = "";
                        object[] attrs = staticFiles[i].GetCustomAttributes(typeof(DescriptionAttribute), true);
                        description = attrs.Length > 0 ?
                            ((DescriptionAttribute)attrs[0]).Description :
                            //若没找到EnumItemDescription标记，则使用该枚举值的名字
                            description = staticFiles[i].Name;

                        map.Add((Enum)staticFiles[i].GetValue(enumType), description);
                    }
                }
            }

            public string this[Enum item]
            {
                get
                {
                    if (item.GetType() != internalEnumType)
                    {
                        throw new ArgumentException();
                    }
                    return map[item];
                }
            }
        }
    }
}
