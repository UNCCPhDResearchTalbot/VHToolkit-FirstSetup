using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AudioEvents : GenericEvents
{
    #region Functions
    public override string GetEventType() { return GenericEventNames.Audio; }
    #endregion

    #region Events
    public class AudioEvent_Base : ICutsceneEventInterface { }

    public class AudioEvent_PlaySound : AudioEvent_Base
    {
        #region Functions
        public void PlaySound(AudioSource source, AudioClip clip)
        {
            source.PlayOneShot(clip);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<AudioSource>(ce, 0);
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Cast<AudioSource>(ce, 0).Stop();
        }

        public override string GetLengthParameterName() { return "clip"; }
        #endregion
    }

    public class AudioEvent_StopSound : AudioEvent_Base
    {
        #region Functions
        public void StopSound(AudioSource source)
        {
            source.Stop();
        }
        #endregion
    }

    public class AudioEvent_SetVolume : AudioEvent_Base
    {
        #region Functions
        public void SetVolume(AudioSource source, float volume)
        {
            source.volume = volume;
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<AudioSource>(ce, 0).volume;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            SetVolume(Cast<AudioSource>(ce, 0), (float)rData);
        }
        #endregion
    }

    public class AudioEvent_SetPitch : AudioEvent_Base
    {
        #region Functions
        public void SetPitch(AudioSource source, float pitch)
        {
            source.pitch = pitch;
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<AudioSource>(ce, 0).pitch;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            SetPitch(Cast<AudioSource>(ce, 0), (float)rData);
        }
        #endregion
    }

    public class AudioEvent_SetPriority : AudioEvent_Base
    {
        #region Functions
        public void SetPriority(AudioSource source, int priority)
        {
            source.priority = priority;
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<AudioSource>(ce, 0).priority;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            SetPriority(Cast<AudioSource>(ce, 0), (int)rData);
        }
        #endregion
    }
    #endregion
}

