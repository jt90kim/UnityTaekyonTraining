using System.Collections.Generic;

namespace Taekyon
{
    public static class SkeletonDefinition
    {
        public static readonly List<string> JointNames = new List<string>
    {
        "hip",

        // legs
        "knee_l",
        "ankle_l",
        "knee_r",
        "ankle_r",

        // torso
        "spine",
        "neck",
        "head",

        // arms
        "shoulder_l",
        "elbow_l",
        "wrist_l",

        "shoulder_r",
        "elbow_r",
        "wrist_r",
    };

        public static readonly List<(string, string)> Bones = new List<(string, string)>
    {
        // legs
        ("hip", "knee_l"),
        ("knee_l", "ankle_l"),
        ("hip", "knee_r"),
        ("knee_r", "ankle_r"),

        // torso
        ("hip", "spine"),
        ("spine", "neck"),
        ("neck", "head"),

        // arms
        ("spine", "shoulder_l"),
        ("shoulder_l", "elbow_l"),
        ("elbow_l", "wrist_l"),

        ("spine", "shoulder_r"),
        ("shoulder_r", "elbow_r"),
        ("elbow_r", "wrist_r"),
    };
    }
}