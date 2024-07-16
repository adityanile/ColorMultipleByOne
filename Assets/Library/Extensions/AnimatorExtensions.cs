using JetBrains.Annotations;
using UnityEngine;

namespace Library.Extensions
{
    public static class AnimatorExtensions
    {
        [UsedImplicitly]
        public static bool IsPlaying(this Animator animator, string stateName, int layerIndex)
        {
            var stateHash = Animator.StringToHash(stateName);
            return IsPlaying(animator, stateHash, layerIndex);
        }

        [UsedImplicitly]
        public static bool IsPlaying(this Animator animator, int stateHash, int layerIndex)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            return stateInfo.shortNameHash == stateHash;
        }
    }
}