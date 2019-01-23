using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceFabric.Serialization.V2.Extensions;
using ServiceFabric.Serialization.V2.Interfaces;
using ServiceStack;
using ServiceStack.Text;

namespace ServiceFabric.Serialization.V2.Serialization
{
    public static class SerializationExtensions
    {
        static SerializationExtensions()
        {
            JsConfig.TypeAttr = "$type";
            JsConfig.IncludePublicFields = true;
            JsConfig.IncludeTypeInfo = true;
            JsConfig.ThrowOnError = true;
            JsConfig.PreferInterfaces = false;
            JsConfig.AssumeUtc = true;
            JsConfig.IncludeNullValues = true;
            JsConfig.IncludeNullValuesInDictionaries = true;
            JsConfig.IncludeDefaultEnums = true;
            JsConfig.AppendUtcOffset = true;
            JsConfig.DateHandler = DateHandler.ISO8601;
            JsConfig.TimeSpanHandler = TimeSpanHandler.StandardFormat;
            JsConfig.PropertyConvention = PropertyConvention.Lenient;
        }

        #region Json

        public static string IndentJson(this string json)
        {
            return TypeSerializer.IndentJson(json);
        }

        public static string CleanJson(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return "";
            }

            var items = json
                .Trim()
                .Split('\n')
                .Select(x => x.TrimEnd())
                .Where(x => x.Length > 0);

            return string.Join("\r\n", items);
        }

        public static string SerializeObject(this object item, bool indent = true)
        {
            if (item == null)
            {
                return "";
            }

            return SerializeData(item, data =>
            {
                return new[]
                    {
                        data.GetType().SerializeToString(indent),
                        data.SerializeToString(indent)
                    }
                    .SerializeToString(indent);
            });
        }

        public static string SerializeToString<T>(this T item, bool indent = true)
        {
            if (item == null)
            {
                return "";
            }

            return SerializeData(item, data =>
            {
                var json = data.ToJson();

                if (indent)
                {
                    json = json.IndentJson();
                }

                return json;
            });
        }

        public static string SerializeToString(this object item, Type type, bool indent = true)
        {
            if (item == null)
            {
                return "";
            }

            return SerializeData(item, data =>
            {
                var json = JsonSerializer.SerializeToString(data, type);

                if (indent)
                {
                    json = json.IndentJson();
                }

                return json;
            });
        }

        public static byte[] Serialize<T>(this T item, bool indent = true)
        {
            if (item == null)
            {
                return new byte[0];
            }

            return SerializeData(item, data =>
            {
                var json = data
                    .ToJson();

                if (indent)
                {
                    json = json
                        .IndentJson();
                }

                return json.CompressText();
            });
        }

        public static byte[] Serialize(this object item, Type type, bool indent = true)
        {
            if (item == null)
            {
                return new byte[0];
            }

            return SerializeData(item, data =>
            {
                var json = JsonSerializer.SerializeToString(data, type);

                if (indent)
                {
                    json = json
                        .IndentJson();
                }

                return json?.CompressText();
            });
        }

        public static object DeserializeObject(this string item)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                return null;
            }

            var parts = item.Deserialize<string[]>();
            var type = parts[0].Deserialize<Type>();
            var result = parts[1].Deserialize(type);

            return RestoreItem(result);
        }

        public static T Deserialize<T>(this byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return default(T);
            }

            var json = data.DecompressText();
            var result = json.FromJson<T>();

            return RestoreItem(result);
        }

        public static object Deserialize(this byte[] data, Type type)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }

            var json = data?.DecompressText();
            var result = JsonSerializer.DeserializeFromString(json, type);

            return RestoreItem(result);
        }

        public static T Deserialize<T>(this string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return default(T);
            }

            var result = JsonSerializer.DeserializeFromString<T>(data);
            return RestoreItem(result);
        }

        public static object Deserialize(this string data, Type type)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            var result = JsonSerializer.DeserializeFromString(data, type);
            return RestoreItem(result);
        }

        #endregion

        #region Xml

        public static string SerializeObjectToXml(this object item)
        {
            if (item == null)
            {
                return "";
            }

            return new[]
                {
                    item.GetType().SerializeToXmlString(),
                    item.SerializeToXmlString()
                }
                .SerializeToXmlString();
        }

        public static string SerializeToXmlString<T>(this T item)
        {
            if (item == null)
            {
                return "";
            }

            return item
                .ToXml();
        }

        public static string SerializeToXmlString(this object item)
        {
            if (item == null)
            {
                return "";
            }

            return XmlSerializer.SerializeToString(item);
        }

        public static byte[] SerializeToXml<T>(this T item)
        {
            if (item == null)
            {
                return new byte[0];
            }

            return item
                .ToXml()
                .CompressText();
        }

        public static byte[] SerializeToXml(this object item)
        {
            if (item == null)
            {
                return new byte[0];
            }

            return XmlSerializer.SerializeToString(item)
                .CompressText();
        }

        public static object DeserializeXmlObject(this string item)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                return null;
            }

            var parts = item.DeserializeXml<string[]>();
            var type = parts[0].DeserializeXml<Type>();
            var result = parts[1].DeserializeXml(type);

            return RestoreItem(result);
        }

        public static T DeserializeXml<T>(this byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return default(T);
            }

            var json = data.DecompressText();
            var result = json.FromXml<T>();

            return RestoreItem(result);
        }

        public static object DeserializeXml(this byte[] data, Type type)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }

            var json = data?.DecompressText();
            var result = XmlSerializer.DeserializeFromString(json, type);

            return RestoreItem(result);
        }

        public static T DeserializeXml<T>(this string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return default(T);
            }

            var result = XmlSerializer.DeserializeFromString<T>(data);
            return RestoreItem(result);
        }

        public static object DeserializeXml(this string data, Type type)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            var result = XmlSerializer.DeserializeFromString(data, type);

            return RestoreItem(result);
        }

        #endregion

        #region Csv

        public static string SerializeObjectToCsv(this object item)
        {
            if (item == null)
            {
                return "";
            }

            return new[]
                {
                    item.GetType().SerializeToCsvString(),
                    item.SerializeToCsvString()
                }
                .SerializeToCsvString();
        }

        public static string SerializeToCsvString<T>(this T item)
        {
            if (item == null)
            {
                return "";
            }

            return item
                .ToCsv();
        }

        public static string SerializeToCsvString(this object item)
        {
            if (item == null)
            {
                return "";
            }

            return CsvSerializer.SerializeToString(item);
        }

        public static byte[] SerializeToCsv<T>(this T item)
        {
            if (item == null)
            {
                return new byte[0];
            }

            return item
                .ToCsv()
                .CompressText();
        }

        public static byte[] SerializeToCsv(this object item)
        {
            if (item == null)
            {
                return new byte[0];
            }

            return CsvSerializer.SerializeToString(item)
                .CompressText();
        }

        public static object DeserializeCsvObject(this string item)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                return null;
            }

            var parts = item.DeserializeCsv<string[]>();
            var type = parts[0].DeserializeCsv<Type>();
            var result = parts[1].DeserializeCsv(type);

            return RestoreItem(result);
        }

        public static T DeserializeCsv<T>(this byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return default(T);
            }

            var json = data.DecompressText();
            var result = json.FromCsv<T>();

            return RestoreItem(result);
        }

        public static object DeserializeCsv(this byte[] data, Type type)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }

            var json = data?.DecompressText();
            var result = CsvSerializer.DeserializeFromString(type, json);

            return RestoreItem(result);
        }

        public static T DeserializeCsv<T>(this string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return default(T);
            }

            var result = CsvSerializer.DeserializeFromString<T>(data);
            return RestoreItem(result);
        }

        public static object DeserializeCsv(this string data, Type type)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            var result = CsvSerializer.DeserializeFromString(type, data);
            return RestoreItem(result);
        }

        #endregion

        #region Private Methods

        private static T RestoreItem<T>(T item)
        {
            if (item is IRestorable restorable)
            {
                restorable.Restore();
            }
            else if (item is IEnumerable enumerable)
            {
                foreach (var restorableItem in enumerable)
                {
                    if (restorableItem is IRestorable listItem)
                    {
                        listItem.Restore();
                    }
                }
            }

            return item;
        }

        private static TResult SerializeData<TItem, TResult>(TItem item, Func<TItem, TResult> serializer)
        {
            try
            {
                return serializer(item);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to serialize '{item.GetType().Name}':\r\n{item.Dump()}", ex);
            }
        }

        #endregion

    }
}
