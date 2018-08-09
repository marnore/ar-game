using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Shims;
using UnityEngine;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Json Converter for Vector2 and Vector3 - Coordinates. Serializes x, y and (z) properties to lat, lng and (alt).
    /// </summary>
    [Preserve]
    public class CoordinatesToVectorConverter : JsonConverter
    {
        private static readonly Type V2 = typeof(Vector2);
        private static readonly Type V3 = typeof(Vector3);

        public bool EnableVector2 { get; set; }
        public bool EnableVector3 { get; set; }

        /// <summary>
        /// Default Constructor - All Vector types enabled by default
        /// </summary>
        public CoordinatesToVectorConverter()
        {
            EnableVector2 = true;
            EnableVector3 = true;
        }

        /// <summary>
        /// Selectively enable Vector types
        /// </summary>
        /// <param name="enableVector2">Use for Vector2 objects</param>
        /// <param name="enableVector3">Use for Vector3 objects</param>
        public CoordinatesToVectorConverter(bool enableVector2, bool enableVector3) : this()
        {
            EnableVector2 = enableVector2;
            EnableVector3 = enableVector3;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var targetType = value.GetType();

            if (targetType == V2)
            {
                var targetVal = (Vector2)value;
                WriteVector(writer, targetVal.x, targetVal.y, null);
            }
            else if (targetType == V3)
            {
                var targetVal = (Vector3)value;
                WriteVector(writer, targetVal.x, targetVal.y, targetVal.z);
            }
            else
            {
                writer.WriteNull();
            }

        }

        private static void WriteVector(JsonWriter writer, float x, float y, float? z)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("lat");
            writer.WriteValue(x);
            writer.WritePropertyName("lng");
            writer.WriteValue(y);

            if (z.HasValue)
            {
                writer.WritePropertyName("alt");
                writer.WriteValue(z.Value);
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == V2)
                return PopulateVector2(reader);
            else
                return PopulateVector3(reader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return (EnableVector2 && objectType == V2) || (EnableVector3 && objectType == V3);
        }

        private static Vector2 PopulateVector2(JsonReader reader)
        {
            var result = new Vector2();

            if (reader.TokenType != JsonToken.Null)
            {
                var jo = JObject.Load(reader);
                result.x = jo["lat"].Value<float>();
                result.y = jo["lng"].Value<float>();
            }

            return result;
        }

        private static Vector3 PopulateVector3(JsonReader reader)
        {
            var result = new Vector3();

            if (reader.TokenType != JsonToken.Null)
            {
                var jo = JObject.Load(reader);
                result.x = jo["lat"].Value<float>();
                result.y = jo["lng"].Value<float>();
                result.z = jo["alt"].Value<float>();
            }

            return result;
        }
    }
}
