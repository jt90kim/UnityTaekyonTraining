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

        private float delayTimer = 0f;
        private bool isWaiting = false;
        Queue<System.Action> triggerQueue = new Queue<System.Action>();
        bool waitingForTrigger = false;
        class QueuedAction
        {
            public System.Action action;
            public float delay;
        }

        private Queue<QueuedAction> techniqueQueue = new Queue<QueuedAction>();
        private bool isPlayingTechnique = false;
        private System.Action pendingAction;
        void Awake()
        {
            motionPlayer = new MotionPlayer(skeletonMapper);
            timeController = new MotionTimeController(motionPlayer);
        }

        public void ReceiveMessage(string message)
        {
            Debug.Log("[AndroidBridge] Received message: " + message);

            if (message.StartsWith("{"))
            {
                HandleJson(message);
                return;
            }

            if (message == "TRIGGER")
            {
                Trigger();
                return;
            }

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
                EnqueueTechnique(() => PlayFromTextAsset(kickMotion), 0.5f);
                return;
            }

            if (message == "STEP")
            {
                EnqueueTechnique(() => PlayFromTextAsset(stepMotion), 0.3f);
                return;
            }

            // ---------------------------
            // SESSION CONTROL (existing)
            // ---------------------------
            if (message == "START_SESSION")
            {
                //motionPlayer.SetTimingProfile(
                //    new DrillTimingProfile(triggerFrame: 1, holdAtTrigger: true)
                //);

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

        void HandleJson(string json)
        {
            var drill = JsonUtility.FromJson<DrillSequence>(json);

            if (drill.type == "DRILL")
            {
                RunDrill(drill);
            }
        }

        void RunDrill(DrillSequence drill)
        {
            foreach (var step in drill.sequence)
            {
                float delay = ResolveDelay(step);

                TextAsset motion = null;

                // PRIORITY 1: weighted
                if (step.weights != null && step.weights.Length > 0)
                {
                    motion = PickWeighted(step.weights);
                }
                // PRIORITY 2: random flag
                else
                {
                    motion = ResolveTechnique(step.action, step.random);
                }

                if (motion == null)
                    continue;

                if (step.waitForTrigger)
                {
                    EnqueueWaitForTrigger(() => PlayFromTextAsset(motion));
                }
                else
                {
                    EnqueueTechnique(() => PlayFromTextAsset(motion), delay);
                }
            }
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
        TextAsset PickWeighted(WeightedOption[] options)
        {
            float total = 0f;

            foreach (var o in options)
                total += o.weight;

            float r = Random.Range(0f, total);
            float cumulative = 0f;

            foreach (var o in options)
            {
                cumulative += o.weight;

                if (r <= cumulative)
                {
                    Debug.Log($"[Weighted] Picked {o.name}");
                    return ResolveTechnique(o.name, false);
                }
            }

            return null;
        }
        void Update()
        {
            if (waitingForTrigger)
            {
                return;
            }

            timeController.Update(Time.deltaTime);

            // Handle delay
            if (isWaiting)
            {
                delayTimer -= Time.deltaTime;

                if (delayTimer <= 0f)
                {
                    isWaiting = false;
                    isPlayingTechnique = true;

                    pendingAction?.Invoke();
                    pendingAction = null;
                }

                return;
            }

            // Handle motion completion
            if (isPlayingTechnique && motionPlayer.IsFinished())
            {
                Debug.Log("[Queue] Technique finished");
                isPlayingTechnique = false;
                PlayNextTechnique();
            }
        }
        float ResolveDelay(DrillStep step)
        {
            if (step.delay == 0 && step.delayMin == 0 && step.delayMax == 0)
            {
                return 0.3f; // default rhythm
            }
            // Use random range if provided
            if (step.delayMax > step.delayMin)
            {
                float d = Random.Range(step.delayMin, step.delayMax);
                Debug.Log($"[Delay] Random delay: {d:F2}");
                return d;
            }

            // fallback to fixed delay
            return step.delay;
        }
        void EnqueueTechnique(System.Action action, float delay = 0f)
        {
            techniqueQueue.Enqueue(new QueuedAction
            {
                action = action,
                delay = delay
            });

            if (!isPlayingTechnique && !isWaiting)
            {
                PlayNextTechnique();
            }
        }

        void EnqueueWaitForTrigger(System.Action action)
        {
            triggerQueue.Enqueue(action);
            waitingForTrigger = true;

            Debug.Log("[Trigger] Queued trigger step");
        }

        public void Trigger()
        {
            if (triggerQueue.Count == 0)
                return;

            Debug.Log("[Trigger] Activated");

            var action = triggerQueue.Dequeue();
            action.Invoke();

            waitingForTrigger = triggerQueue.Count > 0;
        }

        void PlayNextTechnique()
        {
            if (techniqueQueue.Count == 0)
            {
                isPlayingTechnique = false;
                return;
            }

            var next = techniqueQueue.Dequeue();

            if (next.delay > 0f)
            {
                delayTimer = next.delay;
                isWaiting = true;

                // store action to run after delay
                pendingAction = next.action;
                return;
            }

            isPlayingTechnique = true;
            next.action.Invoke();
        }

        TextAsset ResolveTechnique(string action, bool random)
        {
            if (!random)
            {
                // deterministic
                if (action == "STEP") return stepMotion;
                if (action == "KICK") return kickMotion;

                Debug.LogWarning("[Technique] Unknown action: " + action);
                return null;
            }

            // RANDOM selection
            if (action == "KICK")
            {
                // later you’ll expand this list
                TextAsset[] kicks = new TextAsset[]
                {
            kickMotion
                    // future: lowKickMotion, roundKickMotion, etc.
                };

                int index = Random.Range(0, kicks.Length);
                Debug.Log("[Technique] Random KICK → index " + index);

                return kicks[index];
            }

            if (action == "STEP")
            {
                // you could add variations later
                return stepMotion;
            }

            Debug.LogWarning("[Technique] Random unknown action: " + action);
            return null;
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
