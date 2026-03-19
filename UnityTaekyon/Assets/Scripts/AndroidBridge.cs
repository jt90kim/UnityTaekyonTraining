using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Taekyon
{

    public class AndroidBridge : MonoBehaviour
    {
        public SkeletonMapper skeletonMapper;

        // Technique JSONs (assign in inspector)
        public TextAsset kickMotion;
        public TextAsset stepMotion;

        private MotionPlayer motionPlayer;
        private MotionTimeController timeController;

        private Queue<System.Action> techniqueQueue = new Queue<System.Action>();
        private bool isPlayingTechnique = false;

        void Awake()
        {
            motionPlayer = new MotionPlayer(skeletonMapper);
            timeController = new MotionTimeController(motionPlayer);
        }

        public void ReceiveMessage(string message)
        {
            Debug.Log("[AndroidBridge] Received message: " + message);

            // ---------------------------
            // LOAD FROM FILE (existing)
            // ---------------------------
            if (message.StartsWith("LOAD_MOTION:"))
            {
                string path = message.Substring("LOAD_MOTION:".Length);

                var clip = MotionLoader.Load(path);

                if (!motionPlayer.Load(clip))
                {
                    Debug.LogError("Motion load failed");
                }

                return;
            }

            // ---------------------------
            // LOAD FROM RAW JSON (future-ready)
            // ---------------------------
            if (message.StartsWith("LOAD_JSON:"))
            {
                string json = message.Substring("LOAD_JSON:".Length);

                var clip = MotionLoader.LoadFromJson(json);

                if (!motionPlayer.Load(clip))
                {
                    Debug.LogError("Motion load failed (JSON)");
                }

                return;
            }

            // ---------------------------
            // TECHNIQUES (NEW)
            // ---------------------------
            if (message == "KICK")
            {
                EnqueueTechnique(() => PlayFromTextAsset(kickMotion));
                return;
            }

            if (message == "STEP")
            {
                EnqueueTechnique(() => PlayFromTextAsset(stepMotion));
                return;
            }

            // ---------------------------
            // SESSION CONTROL (existing)
            // ---------------------------
            if (message == "START_SESSION")
            {
                motionPlayer.SetTimingProfile(
                    new DrillTimingProfile(triggerFrame: 1, holdAtTrigger: true)
                );

                motionPlayer.Start();
                timeController.Start(motionPlayer.GetFps());
                return;
            }

            if (message == "STOP_SESSION")
            {
                motionPlayer.Stop();
                return;
            }

            if (message == "PAUSE_SESSION")
            {
                motionPlayer.Pause();
                return;
            }

            if (message == "RESUME_SESSION")
            {
                motionPlayer.Resume();
                return;
            }

            // ---------------------------
            // FRAME CONTROL (existing)
            // ---------------------------
            if (message == "NEXT_FRAME")
            {
                motionPlayer.NextFrame();
                return;
            }

            if (message == "PREVIOUS_FRAME")
            {
                motionPlayer.PreviousFrame();
                return;
            }

            if (message.StartsWith("SEEK_FRAME:"))
            {
                string value = message.Substring("SEEK_FRAME:".Length);

                if (int.TryParse(value, out int frameIndex))
                {
                    motionPlayer.SeekToFrame(frameIndex);
                }
                else
                {
                    Debug.LogWarning("[AndroidBridge] SEEK_FRAME parse failed");
                }

                return;
            }

            if (message == "RELEASE_HOLD")
            {
                motionPlayer.ReleaseHold();
                return;
            }

            Debug.LogWarning("[AndroidBridge] Unknown message: " + message);
        }

        // ---------------------------
        // Helper: play TextAsset motion
        // ---------------------------
        void PlayFromTextAsset(TextAsset asset)
        {
            if (asset == null)
            {
                Debug.LogError("[AndroidBridge] Motion asset is null");
                return;
            }

            var clip = MotionLoader.LoadFromJson(asset.text);

            if (!motionPlayer.Load(clip))
            {
                Debug.LogError("[AndroidBridge] Failed to load clip");
                return;
            }

            motionPlayer.Start();
            timeController.Start(motionPlayer.GetFps());
        }

        void Update()
        {
            timeController.Update(Time.deltaTime);

            // Check if motion finished
            if (isPlayingTechnique && motionPlayer.IsFinished())
            {
                Debug.Log("[Queue] Technique finished");
                PlayNextTechnique();
            }
        }
        void EnqueueTechnique(System.Action action)
        {
            techniqueQueue.Enqueue(action);

            if (!isPlayingTechnique)
            {
                PlayNextTechnique();
            }
        }

        void PlayNextTechnique()
        {
            if (techniqueQueue.Count == 0)
            {
                isPlayingTechnique = false;
                return;
            }

            isPlayingTechnique = true;

            var action = techniqueQueue.Dequeue();
            action.Invoke();
        }
    }

    //public class AndroidBridge : MonoBehaviour
    //{
    //    public SkeletonMapper skeletonMapper;
    //    private MotionClip currentClip;
    //    private int currentFrameIndex = 0;

    //    private bool isPlaying = false;
    //    private float playbackTimer = 0f;
    //    private float frameDuration = 0f;


    //    public void ReceiveMessage(string message)
    //    {
    //        Debug.Log("[AndroidBridge] Received message: " + message);

    //        if (message == "START_SESSION")
    //        {
    //            StartPlayback();
    //            return;
    //        }

    //        if (message.StartsWith("LOAD_MOTION:"))
    //        {
    //            string path = message.Substring("LOAD_MOTION:".Length);

    //            currentClip = MotionLoader.Load(path);

    //            if (currentClip == null)
    //            {
    //                Debug.LogError("Motion loading failed");
    //                return;
    //            }

    //            currentFrameIndex = 0;

    //            ApplyCurrentFrame();
    //            return;
    //        }

    //        if (message == "NEXT_FRAME")
    //        {
    //            NextFrame();
    //            return;
    //        }

    //        if (message == "PREVIOUS_FRAME")
    //        {
    //            PreviousFrame();
    //            return;
    //        }

    //    }

    //    private void StartPlayback()
    //    {
    //        if (currentClip == null || currentClip.frames == null || currentClip.frames.Length == 0)
    //        {
    //            Debug.LogWarning("Cannot start playback — no motion loaded");
    //            return;
    //        }

    //        frameDuration = 1f / currentClip.fps;
    //        playbackTimer = 0f;
    //        currentFrameIndex = 0;
    //        isPlaying = true;

    //        ApplyCurrentFrame();

    //        Debug.Log("Playback started");
    //    }


    //    private void ValidateMotionAgainstSkeleton(MotionClip clip)
    //    {

    //        if (clip.frames == null || clip.frames.Length == 0)
    //        {
    //            Debug.LogWarning("Motion clip has no frames");
    //            return;
    //        }

    //        var firstFrame = clip.frames[0];

    //        skeletonMapper.ApplyFrame(firstFrame);

    //        // var firstFrame = clip.frames[0];

    //        // foreach (var jointName in firstFrame.joints.Keys)
    //        // {
    //        //     var joint = skeletonMapper.GetJoint(jointName);
    //        //     if (joint != null)
    //        //     {
    //        //         Debug.Log($"Joint OK: {jointName}");
    //        //     }
    //        //     else
    //        //     {
    //        //         Debug.LogError($"Joint MISSING in skeleton: {jointName}");
    //        //     }
    //        // }
    //    }

    //    private void ApplyCurrentFrame()
    //    {
    //        if (currentClip == null || currentClip.frames == null || currentClip.frames.Length == 0)
    //        {
    //            Debug.LogWarning("No motion loaded");
    //            return;
    //        }

    //        if (currentFrameIndex < 0 || currentFrameIndex >= currentClip.frames.Length)
    //        {
    //            Debug.LogWarning("Frame index out of bounds");
    //            return;
    //        }

    //        var frame = currentClip.frames[currentFrameIndex];
    //        skeletonMapper.ApplyFrame(frame);

    //        Debug.Log($"Applied frame index: {currentFrameIndex}");
    //    }

    //    public void NextFrame()
    //    {
    //        if (currentClip == null) return;

    //        currentFrameIndex++;

    //        if (currentFrameIndex >= currentClip.frames.Length)
    //            currentFrameIndex = currentClip.frames.Length - 1;

    //        ApplyCurrentFrame();
    //    }

    //    public void PreviousFrame()
    //    {
    //        if (currentClip == null) return;

    //        currentFrameIndex--;

    //        if (currentFrameIndex < 0)
    //            currentFrameIndex = 0;

    //        ApplyCurrentFrame();
    //    }

    //    void Update()
    //    {
    //        if (!isPlaying || currentClip == null)
    //            return;

    //        playbackTimer += Time.deltaTime;

    //        if (playbackTimer >= frameDuration)
    //        {
    //            playbackTimer -= frameDuration;

    //            currentFrameIndex++;

    //            if (currentFrameIndex >= currentClip.frames.Length)
    //            {
    //                isPlaying = false;
    //                Debug.Log("Playback finished");
    //                return;
    //            }

    //            ApplyCurrentFrame();
    //        }
    //    }


    //    //void Update()
    //    //{
    //    //    if (Keyboard.current == null)
    //    //        return;

    //    //    if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
    //    //    {
    //    //        NextFrame();
    //    //    }

    //    //    if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
    //    //    {
    //    //        PreviousFrame();
    //    //    }
    //    //}
    //}
}
