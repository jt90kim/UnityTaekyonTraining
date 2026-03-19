using UnityEngine;
using Taekyon;

public class TechniqueController : MonoBehaviour
{
    public MotionPlayer motionPlayer;

    public TextAsset kickMotion;
    public TextAsset stepMotion;

    public void PlayKick()
    {
        MotionClip clip = MotionLoader.LoadFromJson(kickMotion.text);
        motionPlayer.Load(clip);
        motionPlayer.Start();
    }

    public void PlayStep()
    {
        MotionClip clip = MotionLoader.LoadFromJson(stepMotion.text);
        motionPlayer.Load(clip);
        motionPlayer.Start();
    }
}