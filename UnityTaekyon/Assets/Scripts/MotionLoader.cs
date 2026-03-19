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

            var clip = JsonConvert.DeserializeObject<MotionClip>(json);
            Debug.Log("[MotionLoader] Raw JSON: " + json);

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

        public static MotionClip LoadFromJson(string json)
        {
            var clip = JsonConvert.DeserializeObject<MotionClip>(json);

            Debug.Log("[MotionLoader] Loaded from TEXT");

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
}
    

