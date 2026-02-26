using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Taekyon
{
    public static class MotionLoader
    {
        public static MotionClip Load(string path)
        {
            var json = File.ReadAllText(path);
            var settings = new JsonSerializerSettings { Converters = { new Vector3ArrayConverter() } };
            var clip = JsonConvert.DeserializeObject<MotionClip>(json, settings);

            if (clip?.frames != null)
            {
                var jointNames = new HashSet<string>();
                foreach (var frame in clip.frames)
                {
                    if (frame?.joints == null) continue;
                    foreach (var key in frame.joints.Keys)
                        jointNames.Add(key);
                }
                Debug.Log($"fps={clip.fps}\nframes={clip.frames.Length}\njoints=[{string.Join(", ", jointNames)}]");
            }

            return clip;
        }
    }

    public class Vector3ArrayConverter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var arr = JArray.Load(reader);
            if (arr.Count < 3) return default;
            return new Vector3(arr[0].Value<float>(), arr[1].Value<float>(), arr[2].Value<float>());
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.x);
            writer.WriteValue(value.y);
            writer.WriteValue(value.z);
            writer.WriteEndArray();
        }
    }
}
