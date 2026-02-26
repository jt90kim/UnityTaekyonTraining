using UnityEngine;

namespace Taekyon
{
    public enum PlaybackState
    {
        Idle,
        Loaded,
        Playing,
        Paused,
        Finished
    }

    public class MotionPlayer
    {
        private SkeletonMapper skeletonMapper;

        private MotionClip currentClip;
        private int currentFrameIndex = 0;
        private PlaybackState state = PlaybackState.Idle;
        private float playbackTimer = 0f;
        private float frameDuration = 0f;
        private bool isHeld = false;
        private DrillTimingProfile timingProfile;

        public MotionPlayer(SkeletonMapper mapper)
        {
            skeletonMapper = mapper;
        }

        // =============================
        // LOAD
        // =============================
        public bool Load(MotionClip clip)
        {
            if (clip == null || clip.frames == null || clip.frames.Length == 0)
            {
                Debug.LogWarning("[MotionPlayer] Load failed");
                return false;
            }

            currentClip = clip;
            currentFrameIndex = 0;
            playbackTimer = 0f;
            frameDuration = 0f;

            state = PlaybackState.Loaded;

            Debug.Log("[MotionPlayer] State → Loaded");

            ApplyCurrentFrame();

            return true;
        }

        // =============================
        // PLAYBACK CONTROL
        // =============================
        public void Start()
        {
            if (state != PlaybackState.Loaded && state != PlaybackState.Finished)
            {
                Debug.LogWarning($"[MotionPlayer] Start rejected — current state: {state}");
                return;
            }

            frameDuration = 1f / currentClip.fps;
            playbackTimer = 0f;
            currentFrameIndex = 0;

            state = PlaybackState.Playing;

            Debug.Log($"[MotionPlayer] State → Playing (fps={currentClip.fps})");

            ApplyCurrentFrame();
        }


        public void Stop()
        {
            if (state != PlaybackState.Playing)
                return;

            state = PlaybackState.Loaded;
            playbackTimer = 0f;

            Debug.Log("[MotionPlayer] State → Loaded (stopped)");
        }

        public void Update(float deltaTime)
        {
            if (state != PlaybackState.Playing || currentClip == null)
                return;

            // Intentionally empty for now.
            // Time authority will be external.
        }
        public float GetFps()
        {
            return currentClip != null ? currentClip.fps : 1f;
        }
        public void AdvanceOneFrame()
        {
            if (state != PlaybackState.Playing)
                return;

            if (isHeld)
            {
                Debug.Log("[MotionPlayer] Advance blocked — held");
                return;
            }

            int nextIndex = currentFrameIndex + 1;

            // Trigger logic
            if (timingProfile != null &&
                timingProfile.HoldAtTrigger &&
                nextIndex == timingProfile.TriggerFrame)
            {
                currentFrameIndex = nextIndex;
                ApplyCurrentFrame();

                isHeld = true;
                Debug.Log($"[MotionPlayer] Drill trigger hold at frame {currentFrameIndex}");
                return;
            }

            currentFrameIndex = nextIndex;

            if (currentFrameIndex >= currentClip.frames.Length)
            {
                state = PlaybackState.Finished;
                Debug.Log("[MotionPlayer] State → Finished");
                return;
            }

            Debug.Log($"[MotionPlayer] Advance → frame {currentFrameIndex}");
            ApplyCurrentFrame();
        }


        // =============================
        // MANUAL FRAME STEPPING
        // =============================
        public void NextFrame()
        {
            if (currentClip == null)
                return;

            currentFrameIndex++;

            if (currentFrameIndex >= currentClip.frames.Length)
                currentFrameIndex = currentClip.frames.Length - 1;

            ApplyCurrentFrame();
        }

        public void PreviousFrame()
        {
            if (currentClip == null)
                return;

            currentFrameIndex--;

            if (currentFrameIndex < 0)
                currentFrameIndex = 0;

            ApplyCurrentFrame();
        }

        public void Pause()
        {
            if (state != PlaybackState.Playing)
            {
                Debug.LogWarning($"[MotionPlayer] Pause rejected — state: {state}");
                return;
            }

            state = PlaybackState.Paused;
            Debug.Log("[MotionPlayer] State → Paused");
        }

        public void SetTimingProfile(DrillTimingProfile profile)
        {
            timingProfile = profile;
        }

        public void Resume()
        {
            if (state != PlaybackState.Paused)
            {
                Debug.LogWarning($"[MotionPlayer] Resume rejected — state: {state}");
                return;
            }

            state = PlaybackState.Playing;
            Debug.Log("[MotionPlayer] State → Playing (resumed)");
        }

        public void SeekToFrame(int index)
        {
            if (state == PlaybackState.Idle)
            {
                Debug.LogWarning("[MotionPlayer] Seek rejected — state: Idle");
                return;
            }

            if (currentClip == null || currentClip.frames == null)
            {
                Debug.LogWarning("[MotionPlayer] Seek rejected — no clip loaded");
                return;
            }

            if (index < 0 || index >= currentClip.frames.Length)
            {
                Debug.LogWarning($"[MotionPlayer] Seek rejected — index out of bounds: {index}");
                return;
            }

            currentFrameIndex = index;
            playbackTimer = 0f;

            Debug.Log($"[MotionPlayer] Seek → frame {currentFrameIndex}");

            ApplyCurrentFrame();
        }

        public void Hold()
        {
            if (state == PlaybackState.Playing)
            {
                isHeld = true;
                Debug.Log("[MotionPlayer] Hold engaged");
            }
        }

        public void ReleaseHold()
        {
            if (state == PlaybackState.Playing)
            {
                isHeld = false;
                Debug.Log("[MotionPlayer] Hold released");
            }
        }
        private void ApplyCurrentFrame()
        {
            if (currentClip == null)
            {
                Debug.LogWarning("[MotionPlayer] ApplyCurrentFrame called with null clip");
                return;
            }

            if (currentFrameIndex < 0 || currentFrameIndex >= currentClip.frames.Length)
            {
                Debug.LogWarning($"[MotionPlayer] Frame index out of bounds: {currentFrameIndex}");
                return;
            }

            var frame = currentClip.frames[currentFrameIndex];

            Debug.Log($"[MotionPlayer] Applying frame {currentFrameIndex} / {currentClip.frames.Length - 1}");

            skeletonMapper.ApplyFrame(frame);
        }

    }
}
