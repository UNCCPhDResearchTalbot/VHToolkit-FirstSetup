using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationEvents : GenericEvents
{
    #region Functions
    public override string GetEventType() { return GenericEventNames.Animation; }
    #endregion 

    #region Events
    public class AnimationEvent_Base : ICutsceneEventInterface { }

    public class AnimationEvent_PlayAnimation : AnimationEvent_Base
    {
        #region Functions
        public void PlayAnimation(Animation animPlayer, AnimationClip anim)
        {
            animPlayer.clip = anim;
            animPlayer[animPlayer.clip.name].time = 0;
            animPlayer.Play();
        }

        public void PlayAnimation(Animation animPlayer, string anim)
        {
            animPlayer[anim].time = 0;
            animPlayer.Play(anim);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return SaveTransformHierarchy(Cast<Animation>(ce, 0).transform);
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            LoadTransformHierarchy((List<TransformData>)rData, Cast<Animation>(ce, 0).transform);
        }

        public override string GetLengthParameterName() { return "anim"; }
        #endregion
    }

    public class AnimationEvent_PlayAnimationQueued : AnimationEvent_Base
    {
        #region Functions
        public void PlayAnimationQueued(Animation animPlayer, string anim)
        {
            animPlayer.PlayQueued(anim);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return SaveTransformHierarchy(Cast<Animation>(ce, 0).transform);
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            LoadTransformHierarchy((List<TransformData>)rData, Cast<Animation>(ce, 0).transform);
        }
        #endregion
    }

    public class AnimationEvent_StopAnimation : AnimationEvent_Base
    {
        #region Functions
        public void StopAnimation(Animation animPlayer)
        {
            animPlayer.Stop();
        }
        #endregion
    }

    public class AnimationEvent_SetSpeed : AnimationEvent_Base
    {
        #region Functions
        public void SetSpeed(Animation animPlayer, float speed, AnimationClip anim)
        {
            animPlayer[anim.name].speed = speed;
        }

        public void SetSpeed(Animation animPlayer, float speed, string anim)
        {
            animPlayer[anim].speed = speed;
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            Animation animPlayer = Cast<Animation>(ce, 0);
            float retVal = 0;
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    retVal = animPlayer[Cast<AnimationClip>(ce, 2).name].speed;
                    break;

                case 1:
                    retVal = animPlayer[Param(ce, 2).stringData].speed;
                    break;
            }
            return retVal;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Animation animPlayer = Cast<Animation>(ce, 0);
            switch (ce.FunctionOverloadIndex)
            {
                case 0:
                    SetSpeed(animPlayer, (float)rData, Cast<AnimationClip>(ce, 2));
                    break;

                case 1:
                    SetSpeed(animPlayer, (float)rData, Param(ce, 2).stringData);
                    break;
            }
        }
        #endregion
    }
    #endregion
}