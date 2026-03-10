using System.Collections.Generic;

namespace Taekyon
{
    public static class SkeletonDefinition
    {
        public static readonly List<string> JointNames = new List<string>
        {
            "hip",
            "knee_l",
            "ankle_l",
            "knee_r",
            "ankle_r"
        };

        public static readonly List<(string, string)> Bones = new List<(string, string)>
        {
            ("hip", "knee_l"),
            ("knee_l", "ankle_l"),
            ("hip", "knee_r"),
            ("knee_r", "ankle_r")
        };
    }
}