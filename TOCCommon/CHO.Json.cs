using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;


namespace CHO.Json
{
    /// <summary>
    /// 表示Json数据类型异常
    /// </summary>
    public class JsonDataTypeException : Exception
    {
        private readonly string message;
        public JsonDataTypeException(string message)
        {
            this.message = message;
        }
        public override string Message => message;
    }

    /// <summary>
    /// 表示分析Json数据时的非法字符异常
    /// </summary>
    public class InvalidCharParseException : Exception
    {
        private readonly string message;
        private readonly int index;
        public InvalidCharParseException(string message, int index)
        {
            this.message = message;
            this.index = index;
        }
        public override string Message => message;
        public int Index => index;
    }

    /// <summary>
    /// 表示分析Json数据时某个数据未结束导致的异常
    /// </summary>
    public class NotClosedParseException : Exception
    {
        private readonly string message;
        private readonly int index;
        public NotClosedParseException(string message, int index)
        {
            this.message = message;
            this.index = index;
        }
        public override string Message => message;
        public int Index => index;
    }

    /// <summary>
    /// 表示函数错误调用错误 (此异常一般不会出现)
    /// </summary>
    public class ParseCallError : Exception
    {
        private readonly string message;
        private readonly int index;
        public ParseCallError(string message, int index)
        {
            this.message = message;
            this.index = index;
        }
        public override string Message => message;
        public int Index => index;
    }

    /// <summary>
    /// 表示分析时的未知错误 (此异常一般不会出现)
    /// </summary>
    public class ParseUnknownError : Exception
    {
        private readonly string message;
        private readonly int index;
        public ParseUnknownError(string message, int index)
        {
            this.message = message;
            this.index = index;
        }
        public override string Message => message;
        public int Index => index;
    }

    /// <summary>
    /// 表示分析Json数据时的Json格式异常
    /// </summary>
    public class JsonFormatParseException : Exception
    {
        private readonly string message;
        private readonly int index;
        public JsonFormatParseException(string message, int index)
        {
            this.message = message;
            this.index = index;
        }
        public override string Message => message;
        public int Index => index;
    }
    /// <summary>
    /// Json数据类型, 表示Json数据中包含数据的类型
    /// </summary>
    public enum JsonDataType
    {
        Null,
        Object,
        Array,
        String,
        Int32,
        Float,
        Double,
        Boolean
    }
    public class JsonSerializer
    {
        /// <summary>
        /// 将JsonData实例中的数据反序列化为指定类型的实例
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="jsonData">要进行操作的JsonData实例</param>
        /// <returns></returns>
        public static T ConvertToInstance<T>(JsonData jsonData)
        {
            return ConvertToInstance<T>(jsonData, null);
        }
        protected static object ConvertToInstance(JsonData jsonData, Type resultType)
        {
            if (jsonData.DataType == JsonDataType.Null || jsonData.content == null)
            {
                return null;
            }
            else if (resultType == typeof(JsonData))
            {
                return jsonData;
            }
            else if (resultType.IsAssignableFrom(jsonData.content.GetType()))
            {
                return Convert.ChangeType(jsonData.content, resultType);
            }
            else
            {
                object result = Activator.CreateInstance(resultType);
                if (jsonData.DataType == JsonDataType.Object)
                {
                    if (CheckInterface(resultType, typeof(IDictionary), out Type[] _) && CheckInterface(resultType, typeof(IDictionary<,>), out Type[] geneticArgs))
                    {
                        foreach (JsonData i in (jsonData.content as IDictionary<JsonData, JsonData>).Keys)
                        {
                            (result as IDictionary)[ConvertToInstance(i, geneticArgs[0])] = ConvertToInstance((jsonData.content as IDictionary<JsonData, JsonData>)[i], geneticArgs[1]);
                        }
                        return result;
                    }
                    else
                    {
                        foreach (JsonData i in (jsonData.content as IDictionary<JsonData, JsonData>).Keys)
                        {
                            if (i.DataType == JsonDataType.String)
                            {
                                FieldInfo field = resultType.GetField(i.content as string);
                                if (field != null)
                                {
                                    JsonData value = (jsonData.content as IDictionary<JsonData, JsonData>)[i];
                                    field.SetValue(result, ConvertToInstance(value, field.FieldType));
                                }
                                else
                                {
                                    // 表示遍历JsonData时, 未找到用户指定类中的对应字段
                                }
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException("Object类型的JsonData在反序列化为自定义类型时, 要求JsonData的键必须是String类型");
                            }
                        }
                        return result;
                    }
                }
                else if (jsonData.DataType == JsonDataType.Array)
                {
                    if (CheckInterface(resultType, typeof(IList), out Type[] _) && CheckInterface(resultType, typeof(IList<>), out Type[] geneticTypes))
                    {
                        foreach (JsonData i in jsonData.content as IList<JsonData>)
                        {
                            (result as IList).Add(ConvertToInstance(i, geneticTypes[0]));
                        }
                        return result;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("Array类型的JsonData在反序列化为类实例时, 要求类必须继承IList与IList<T>接口");
                    }
                }
                else
                {
                    throw new JsonDataTypeException("未知类型的JsonData数据");
                }
            }
        }
        public static T ConvertToInstance<T>(JsonData jsonData, object useless = default)
        {
            return (T)ConvertToInstance(jsonData, typeof(T));
        }
        /// <summary>
        /// 将JsonData实例中的数据转换成Json文本
        /// 注意: CHO.Json可以读取, 但其他Json序列化库可能无法读取它
        /// </summary>
        /// <returns>String类型的Json文本</returns>
        public static string ConvertToText(JsonData jsonData)
        {
            switch (jsonData.DataType)
            {
                case JsonDataType.Object:
                    List<string> pairs = new List<string>();
                    foreach (JsonData key in (jsonData.content as Dictionary<JsonData, JsonData>).Keys)
                    {
                        pairs.Add(string.Format("{0}: {1}", ConvertToText(key), ConvertToText((jsonData.content as Dictionary<JsonData, JsonData>)[key])));
                    }
                    return string.Format("{0}{1}{2}", "{", string.Join(", ", pairs), "}");
                case JsonDataType.Array:
                    List<string> elements = new List<string>();
                    foreach (JsonData element in (jsonData.content as List<JsonData>))
                    {
                        elements.Add(ConvertToText(element));
                    }
                    return string.Format("[{0}]", string.Join(", ", elements));
                case JsonDataType.String:
                    return string.Format("\"{0}\"", ((string)jsonData.content).Replace("\\", "\\\\").Replace("\'", "\\\'").Replace("\"", "\\\"").Replace("\0", "\\0").Replace("\a", "\\a").Replace("\b", "\\b").Replace("\f", "\\f").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t").Replace("\v", "\\v"));
                case JsonDataType.Int32:
                    return ((int)jsonData.content).ToString();
                case JsonDataType.Float:
                    return ((float)jsonData.content).ToString();
                case JsonDataType.Double:
                    return ((double)jsonData.content).ToString();
                case JsonDataType.Boolean:
                    return (bool)jsonData.content ? "true" : "false";
                case JsonDataType.Null:
                    return "null";
                default:
                    throw new JsonDataTypeException("所访问数据类型未知");
            }
        }
        public static bool TryConvertToInstance<T>(JsonData jsonData, out T result) where T : new()
        {
            try
            {
                result = ConvertToInstance<T>(jsonData);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
        public static bool TryConvertToText(JsonData jsonData, out string result)
        {
            try
            {
                result = ConvertToText(jsonData);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        private static bool IsEmptyChar(char c)
        {
            return c == ' ' || c == '\n' || c == '\r' || c == '\t' || c == '\0';
        }
        private static bool IsLetterChar(char c)
        {
            return c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
        }
        private static bool IsNumberStartChar(char c)
        {
            return (c >= '0' && c <= '9') || c == '-';
        }
        private static bool IsNumberParsingChar(char c)
        {
            return IsNumberStartChar(c) || c == '.' || c == '-' || IsLetterChar(c);
        }
        private static bool IsWordParseChar(char c)
        {
            return IsLetterChar(c) || c == '_';
        }
        private static bool IsEndChar(char c)
        {
            return IsEmptyChar(c) || c == '"' || c == ':' || c == ',' || c == ']' || c == '}';
        }
        protected static bool CheckInterface(Type targetType, Type interfaceType, out Type[] geneticTypes)
        {
            foreach (Type i in targetType.GetInterfaces())
            {
                if (i.IsGenericType)
                {
                    if (i.GetGenericTypeDefinition().Equals(interfaceType))
                    {
                        geneticTypes = i.GetGenericArguments();
                        return true;
                    }
                }
                else
                {
                    if (i.Equals(interfaceType))
                    {
                        geneticTypes = null;
                        return true;
                    }
                }
            }
            geneticTypes = null;
            return false;
        }

        private enum ArrayParseState
        {
            NotStart,
            ElementStart,
            ElementEnd
        }
        private enum ObjectParseState
        {
            NotStart,
            KeyStart,
            KeyEnd,
            ValueStart,
            ValueEnd
        }
        protected static JsonData ParseData(ref char[] source, ref int offset)
        {
            for (; offset < source.Length; offset++)
            {
                if (IsEmptyChar(source[offset]))
                {
                    continue;
                }
                else
                {
                    if (source[offset] == '{')
                    {
                        return ParseObject(ref source, ref offset);
                    }
                    else if (source[offset] == '[')
                    {
                        return ParseArray(ref source, ref offset);
                    }
                    else if (source[offset] == '"')
                    {
                        return ParseString(ref source, ref offset);
                    }
                    else if (IsNumberStartChar(source[offset]))
                    {
                        return ParseNumber(ref source, ref offset);
                    }
                    else if (IsWordParseChar(source[offset]))
                    {
                        return ParseWord(ref source, ref offset);
                    }
                    else if (source[offset] == '}' || source[offset] == ']')
                    {
                        offset--;
                        return default;
                    }
                    else
                    {
                        throw new InvalidCharParseException(string.Format("非法字符'{0}'", source[offset]), offset);
                    }
                }
            }
            return default;
        }
        protected static JsonData ParseString(ref char[] source, ref int offset)
        {
            bool parsing = false;
            bool escape = false;
            StringBuilder content = new StringBuilder();

            for (; offset < source.Length; offset++)
            {
                if (escape)
                {
                    switch (source[offset])
                    {
                        case 'a':
                            content.Append('\a');
                            break;
                        case 'b':
                            content.Append('\b');
                            break;
                        case 'f':
                            content.Append('\f');
                            break;
                        case 'n':
                            content.Append('\n');
                            break;
                        case 'r':
                            content.Append('\r');
                            break;
                        case 't':
                            content.Append('\t');
                            break;
                        case 'v':
                            content.Append('\v');
                            break;
                        default:
                            content.Append(source[offset]);
                            break;
                    }
                    escape = false;
                }
                else if (parsing)
                {
                    if (source[offset] == '"')
                    {
                        return Create(content.ToString());
                    }
                    else if (source[offset] == '\\')
                    {
                        escape = true;
                    }
                    else
                    {
                        content.Append(source[offset]);
                    }
                }
                else
                {
                    if (IsEmptyChar(source[offset]))
                    {
                        continue;
                    }
                    else if (source[offset] == '"')
                    {
                        parsing = true;
                    }
                    else
                    {
                        throw new InvalidCharParseException(string.Format("非法字符'{0}'", source[offset]), offset);
                    }
                }
            }
            throw new NotClosedParseException("String无结束符号", offset);
        }
        protected static JsonData ParseNumber(ref char[] source, ref int offset)
        {
            bool parsing = false;
            StringBuilder content = new StringBuilder();

            for (; offset < source.Length; offset++)
            {
                if (parsing)
                {
                    if (IsNumberParsingChar(source[offset]))
                    {
                        content.Append(source[offset]);
                    }
                    else if (IsEndChar(source[offset]))
                    {
                        offset--;
                        break;
                    }
                    else
                    {
                        throw new InvalidCharParseException(string.Format("非法字符'{0}'", source[offset]), offset);
                    }
                }
                else
                {
                    if (IsEmptyChar(source[offset]))
                    {
                        continue;
                    }
                    else if (IsNumberStartChar(source[offset]))
                    {
                        parsing = true;
                        content.Append(source[offset]);
                    }
                    else
                    {
                        throw new ParseCallError("未知错误! 一般情况下,此错误不会被触发", offset);
                    }
                }
            }
            try
            {
                double template = double.Parse(content.ToString());

                if (template == (int)template)
                {
                    return Create((int)template);
                }
                else if (template == (float)template)
                {
                    return Create((float)template);
                }
                else
                {
                    return Create(template);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        protected static JsonData ParseWord(ref char[] source, ref int offset)
        {
            bool parsing = false;
            StringBuilder content = new StringBuilder();

            for (; offset < source.Length; offset++)
            {
                if (parsing)
                {
                    if (IsWordParseChar(source[offset]))
                    {
                        content.Append(source[offset]);
                    }
                    else if (IsEndChar(source[offset]))
                    {
                        offset--;
                        break;
                    }
                }
                else
                {
                    if (IsEmptyChar(source[offset]))
                    {
                        continue;
                    }
                    else if (IsWordParseChar(source[offset]))
                    {
                        parsing = true;
                        content.Append(source[offset]);
                    }
                    else
                    {
                        throw new InvalidCharParseException(string.Format("非法字符'{0}'", source[offset]), offset);
                    }
                }
            }
            switch (content.ToString())
            {
                case "true":
                    return Create(true);
                case "false":
                    return Create(false);
                case "null":
                    return default;
                default:
                    throw new JsonDataTypeException(string.Format("未知的关键词'{0}'", content.ToString()));
            }
        }
        protected static JsonData ParseArray(ref char[] source, ref int offset)
        {
            ArrayParseState parseState = ArrayParseState.NotStart;
            List<JsonData> resultContainer = new List<JsonData>();

            for (; offset < source.Length; offset++)
            {
                switch (parseState)
                {
                    case ArrayParseState.ElementStart:
                        JsonData tempJson = ParseData(ref source, ref offset);
                        if (tempJson != default(JsonData))
                        {
                            resultContainer.Add(tempJson);
                        }
                        parseState = ArrayParseState.ElementEnd;
                        break;
                    case ArrayParseState.ElementEnd:
                        if (IsEmptyChar(source[offset]))
                        {
                            continue;
                        }
                        else if (source[offset] == ',')
                        {
                            parseState = ArrayParseState.ElementStart;
                        }
                        else if (source[offset] == ']')
                        {
                            JsonData result = new JsonData
                            {
                                DataType = JsonDataType.Array,
                                content = resultContainer
                            };
                            return result;
                        }
                        else
                        {
                            throw new InvalidCharParseException(string.Format("非法字符'{0}'", source[offset]), offset);
                        }
                        break;
                    case ArrayParseState.NotStart:
                        if (IsEmptyChar(source[offset]))
                        {
                            continue;
                        }
                        else if (source[offset] == '[')
                        {
                            parseState = ArrayParseState.ElementStart;
                        }
                        else
                        {
                            throw new InvalidCharParseException(string.Format("非法字符'{0}'", source[offset]), offset);
                        }
                        break;

                }
            }

            throw new NotClosedParseException("Array无结束符号", offset);
        }
        protected static JsonData ParseObject(ref char[] source, ref int offset)
        {
            ObjectParseState parseState = ObjectParseState.NotStart;
            Dictionary<JsonData, JsonData> resultContainer = new Dictionary<JsonData, JsonData>();
            JsonData tempKey = default;

            for (; offset < source.Length; offset++)
            {
                switch (parseState)
                {
                    case ObjectParseState.KeyStart:
                        tempKey = ParseData(ref source, ref offset);
                        if (tempKey != default(JsonData))
                        {
                            parseState = ObjectParseState.KeyEnd;
                        }
                        else
                        {
                            parseState = ObjectParseState.ValueEnd;
                        }
                        break;
                    case ObjectParseState.KeyEnd:
                        if (IsEmptyChar(source[offset]))
                        {
                            continue;
                        }
                        else if (source[offset] == ':')
                        {
                            parseState = ObjectParseState.ValueStart;
                        }
                        else
                        {
                            throw new InvalidCharParseException(string.Format("非法字符'{0}'", source[offset]), offset);
                        }
                        break;
                    case ObjectParseState.ValueStart:
                        resultContainer[tempKey] = ParseData(ref source, ref offset);
                        parseState = ObjectParseState.ValueEnd;
                        break;
                    case ObjectParseState.ValueEnd:
                        if (IsEmptyChar(source[offset]))
                        {
                            continue;
                        }
                        else if (source[offset] == ',')
                        {
                            parseState = ObjectParseState.KeyStart;
                        }
                        else if (source[offset] == '}')
                        {
                            JsonData result = new JsonData
                            {
                                DataType = JsonDataType.Object,
                                content = resultContainer
                            };
                            return result;
                        }
                        else
                        {
                            throw new InvalidCharParseException(string.Format("非法字符'{0}'", source[offset]), offset);
                        }
                        break;
                    case ObjectParseState.NotStart:
                        if (IsEmptyChar(source[offset]))
                        {
                            continue;
                        }
                        else if (source[offset] == '{')
                        {
                            parseState = ObjectParseState.KeyStart;
                        }
                        else
                        {
                            throw new InvalidCharParseException(string.Format("非法字符'{0}'", source[offset]), offset);
                        }
                        break;
                }
            }

            throw new NotClosedParseException("Object无结束符号", offset);
        }

        /// <summary>
        /// 从包含Json文本的字符数组中分析Json数据
        /// </summary>
        /// <param name="jsonText">Json文本</param>
        /// <returns>包含Json数据的JsonData实例</returns>
        public static JsonData Parse(char[] jsonText)
        {
            char[] source = jsonText;
            int offset = 0;

            JsonData result = ParseData(ref source, ref offset);

            for (offset++; offset < source.Length; offset++)
            {
                if (!IsEmptyChar(source[offset]))
                {
                    throw new JsonFormatParseException("一个Json文本中不能出现两个元素并列的情况", offset);
                }
            }

            return result;
        }
        /// <summary>
        /// 从Json文本中分析Json数据
        /// </summary>
        /// <param name="jsonText">Json文本</param>
        /// <returns>包含Json数据的JsonData实例</returns>
        public static JsonData Parse(string jsonText)
        {
            char[] source = jsonText.ToCharArray();
            int offset = 0;

            JsonData result = ParseData(ref source, ref offset);

            for (offset++; offset < source.Length; offset++)
            {
                if (!IsEmptyChar(source[offset]))
                {
                    throw new JsonFormatParseException("一个Json文本中不能出现两个元素并列的情况", offset);
                }
            }

            return result;
        }
        public static JsonData[] ParseStream(char[] jsonText)
        {
            char[] source = jsonText;
            int offset = 0;

            List<JsonData> result = new List<JsonData>();

            for (; offset < source.Length; offset++)
            {
                JsonData currentJson = ParseData(ref source, ref offset);
                result.Add(currentJson);
            }

            return result.ToArray();
        }
        public static JsonData[] ParseStream(string jsonText)
        {
            char[] source = jsonText.ToCharArray();
            int offset = 0;

            List<JsonData> result = new List<JsonData>();

            for (; offset < source.Length; offset++)
            {
                JsonData currentJson = ParseData(ref source, ref offset);
                result.Add(currentJson);
            }

            return result.ToArray();
        }
        /// <summary>
        /// 尝试从包含Json文本的字符数组中分析Json数据
        /// </summary>
        /// <param name="jsonText">包含Json数据的文本</param>
        /// <param name="jsonObj">返回的结果</param>
        /// <returns>是否分析成功</returns>
        public static bool TryParse(char[] jsonText, out JsonData jsonObj)
        {
            try
            {
                jsonObj = Parse(jsonText);
                return true;
            }
            catch
            {
                jsonObj = default;
                return false;
            }
        }
        /// <summary>
        /// 尝试从Json文本中分析Json数据
        /// </summary>
        /// <param name="jsonText">包含Json数据的文本</param>
        /// <param name="jsonObj">返回的结果</param>
        /// <returns>是否分析成功</returns>
        public static bool TryParse(string jsonText, out JsonData jsonObj)
        {
            try
            {
                jsonObj = Parse(jsonText);
                return true;
            }
            catch
            {
                jsonObj = default;
                return false;
            }
        }
        public static bool TryParseStream(char[] jsonText, out JsonData[] jsonObjs)
        {
            try
            {
                jsonObjs = ParseStream(jsonText);
                return true;
            }
            catch
            {
                jsonObjs = null;
                return false;
            }
        }
        public static bool TryParseStream(string jsonText, out JsonData[] jsonObjs)
        {
            try
            {
                jsonObjs = ParseStream(jsonText);
                return true;
            }
            catch
            {
                jsonObjs = null;
                return false;
            }
        }
        /// <summary>
        /// 根据参数创建一个包含相应类型Json数据的JsonData实例
        /// </summary>
        /// <param name="obj">参数</param>
        /// <returns>对应JsonData实例</returns>
        public static JsonData Create(object obj)
        {
            if (obj == null)
            {
                return default;
            }
            else
            {

                JsonData result = new JsonData();
                if (obj is string)
                {
                    result.DataType = JsonDataType.String;
                    result.content = obj;
                    return result;
                }
                else if (obj is bool)
                {
                    result.DataType = JsonDataType.Boolean;
                    result.content = obj;
                    return result;
                }
                else if (obj is int)
                {
                    result.DataType = JsonDataType.Int32;
                    result.content = obj;
                    return result;
                }
                else if (obj is float)
                {
                    result.DataType = JsonDataType.Float;
                    result.content = obj;
                    return result;
                }
                else if (obj is double)
                {
                    result.DataType = JsonDataType.Double;
                    result.content = obj; 
                    return result;
                }
                else if (obj is IDictionary)
                {
                    Dictionary<JsonData, JsonData> objcntnt = new Dictionary<JsonData, JsonData>();
                    foreach (object i in (obj as IDictionary).Keys)
                    {
                        objcntnt[Create(i)] = Create((obj as IDictionary)[i]);
                    }

                    result.DataType = JsonDataType.Object;
                    result.content = objcntnt;
                    return result;
                }
                else if (obj is IList)
                {
                    List<JsonData> objcntnt = new List<JsonData>();
                    foreach (object i in obj as IList)
                    {
                        objcntnt.Add(Create(i));
                    }

                    result.DataType = JsonDataType.Array;
                    result.content = objcntnt;
                    return result;
                }
                else
                {
                    Dictionary<JsonData, JsonData> objData = new Dictionary<JsonData, JsonData>();
                    FieldInfo[] fs = obj.GetType().GetFields();

                    foreach (FieldInfo i in fs)
                    {
                        objData[Create(i.Name)] = Create(i.GetValue(obj));
                    }

                    result.DataType = JsonDataType.Object;
                    result.content = objData;
                    return result;
                }
            }
        }

    }

    /// <summary>
    /// 单个Json数据
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public struct JsonData : IList<JsonData>,ICollection<JsonData>,IList,ICollection,IEnumerable,IDictionary,IReadOnlyDictionary<JsonData,JsonData>,IDictionary<JsonData,JsonData>
    {
        internal object content;

        public object this[int index] { get => ((IList)Array)[index]; set => ((IList)Array)[index] = value; }
        public object this[object key] { get => ((IDictionary)dictionary)[key]; set => ((IDictionary)dictionary)[key] = value; }

        public JsonData this[JsonData key] => ((IReadOnlyDictionary<JsonData, JsonData>)dictionary)[key];

        JsonData IList<JsonData>.this[int index] { get => ((IList<JsonData>)Array)[index]; set => ((IList<JsonData>)Array)[index] = value; }
        JsonData IDictionary<JsonData, JsonData>.this[JsonData key] { get => ((IDictionary<JsonData, JsonData>)dictionary)[key]; set => ((IDictionary<JsonData, JsonData>)dictionary)[key] = value; }

        public JsonDataType DataType { get ; internal set; }
        /// <summary>
        /// 从包含Array类型Json数据的JsonData实例中获取所包含的数据
        /// </summary>
        /// <returns>List<JsonData>实例</returns>
        public List<JsonData> Array => DataType == JsonDataType.Array ? (List<JsonData>)content : throw new JsonDataTypeException("所访问数据不是Array类型");
        /// <summary>
        /// 从包含Object类型Json数据的JsonData实例中获取所包含的数据
        /// </summary>
        public Dictionary<JsonData,JsonData> dictionary => DataType == JsonDataType.Object ? (Dictionary<JsonData,JsonData>)content : throw new JsonDataTypeException("所访问数据不是Object类型");
        /// <summary>
        /// 从包含Json数据的JsonData实例中获取所包含的数据
        /// </summary>
        /// <returns>对应数据的实例</returns>
        public object Content => content;

        public bool IsReadOnly => ((IList)Array).IsReadOnly;

        public bool IsFixedSize => ((IList)Array).IsFixedSize;

        public int Count => ((ICollection)Array).Count;

        public object SyncRoot => ((ICollection)Array).SyncRoot;

        public bool IsSynchronized => ((ICollection)Array).IsSynchronized;

        public ICollection Keys => ((IDictionary)dictionary).Keys;

        public ICollection Values => ((IDictionary)dictionary).Values;

        IEnumerable<JsonData> IReadOnlyDictionary<JsonData, JsonData>.Keys => ((IReadOnlyDictionary<JsonData, JsonData>)dictionary).Keys;

        ICollection<JsonData> IDictionary<JsonData, JsonData>.Keys => ((IDictionary<JsonData, JsonData>)dictionary).Keys;

        IEnumerable<JsonData> IReadOnlyDictionary<JsonData, JsonData>.Values => ((IReadOnlyDictionary<JsonData, JsonData>)dictionary).Values;

        ICollection<JsonData> IDictionary<JsonData, JsonData>.Values => ((IDictionary<JsonData, JsonData>)dictionary).Values;

        public int Add(object value)
        => ((IList)Array).Add(value);

        public void Add(JsonData item)
        {
            ((ICollection<JsonData>)Array).Add(item);
        }

        public void Add(object key, object value)
        {
            ((IDictionary)dictionary).Add(key, value);
        }

        public void Add(JsonData key, JsonData value)
        {
            ((IDictionary<JsonData, JsonData>)dictionary).Add(key, value);
        }

        public void Add(KeyValuePair<JsonData, JsonData> item)
        {
            ((ICollection<KeyValuePair<JsonData, JsonData>>)dictionary).Add(item);
        }

        public void Clear()
        {
            ((IList)Array).Clear();
        }

        public bool Contains(object value)
        => ((IList)Array).Contains(value);

        public bool Contains(JsonData item)
        => ((ICollection<JsonData>)Array).Contains(item);

        public bool Contains(KeyValuePair<JsonData, JsonData> item)
        => ((ICollection<KeyValuePair<JsonData, JsonData>>)dictionary).Contains(item);

        public bool ContainsKey(JsonData key)
        => ((IReadOnlyDictionary<JsonData, JsonData>)dictionary).ContainsKey(key);

        public void CopyTo(Array array, int index)
        {
            ((ICollection)Array).CopyTo(array, index);
        }

        public void CopyTo(JsonData[] array, int arrayIndex)
        {
            ((ICollection<JsonData>)Array).CopyTo(array, arrayIndex);
        }
        public void CopyTo(KeyValuePair<JsonData, JsonData>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<JsonData, JsonData>>)dictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator GetEnumerator()
        => ((IEnumerable)Array).GetEnumerator();

        /// <summary>
        /// 获取JsonData实例所包含数据的HashCode
        /// </summary>
        /// <returns>int类型的HashCode值</returns>
        public override int GetHashCode() => content.GetHashCode();

        public int IndexOf(object value)
        => ((IList)Array).IndexOf(value);

        public int IndexOf(JsonData item)
        => ((IList<JsonData>)Array).IndexOf(item);

        public void Insert(int index, object value)
        {
            ((IList)Array).Insert(index, value);
        }

        public void Insert(int index, JsonData item)
        {
            ((IList<JsonData>)Array).Insert(index, item);
        }

        public void Remove(object value)
        {
            ((IList)Array).Remove(value);
        }

        public bool Remove(JsonData item)
        => ((ICollection<JsonData>)Array).Remove(item);

        public bool Remove(KeyValuePair<JsonData, JsonData> item)
        => ((ICollection<KeyValuePair<JsonData, JsonData>>)dictionary).Remove(item);

        public void RemoveAt(int index)
        {
            ((IList)Array).RemoveAt(index);
        }

        public bool TryGetValue(JsonData key, out JsonData value)
        => ((IReadOnlyDictionary<JsonData, JsonData>)dictionary).TryGetValue(key, out value);
        public override string ToString() => JsonSerializer.ConvertToText(this);
        private string GetDebuggerDisplay() => ToString();

        IEnumerator<JsonData> IEnumerable<JsonData>.GetEnumerator() => ((IEnumerable<JsonData>)Array).GetEnumerator();
        IDictionaryEnumerator IDictionary.GetEnumerator()
        => ((IDictionary)dictionary).GetEnumerator();

        IEnumerator<KeyValuePair<JsonData, JsonData>> IEnumerable<KeyValuePair<JsonData, JsonData>>.GetEnumerator()
        => ((IEnumerable<KeyValuePair<JsonData, JsonData>>)dictionary).GetEnumerator();
        public static bool operator ==(JsonData a,JsonData b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(JsonData a, JsonData b)
        {
            return !a.Equals(b);
        }
    }
}
