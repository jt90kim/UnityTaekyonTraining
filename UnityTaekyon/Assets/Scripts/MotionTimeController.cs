using Taekyon;
using UnityEngine;

public class MotionTimeController
{
    private MotionPlayer player;
    private float frameDuration;
    private float timer = 0f;

    public MotionTimeController(MotionPlayer motionPlayer)
    {
        player = motionPlayer;
    }

    public void Start(float fps)
    {
        frameDuration = 1f / fps;
        timer = 0f;
    }

    public void Update(float deltaTime)
    {
        timer += deltaTime;

        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            player.AdvanceOneFrame();
        }
    }
}