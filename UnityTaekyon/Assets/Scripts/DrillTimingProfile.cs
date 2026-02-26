using UnityEngine;

namespace Taekyon
{
    public class DrillTimingProfile
    {
        public int TriggerFrame { get; private set; }
        public bool HoldAtTrigger { get; private set; }

        public DrillTimingProfile(int triggerFrame, bool holdAtTrigger)
        {
            TriggerFrame = triggerFrame;
            HoldAtTrigger = holdAtTrigger;
        }
    }
}