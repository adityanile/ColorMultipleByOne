using UnityEngine;

namespace AkshanshKanojia.Animations
{
    public class EffectsManger2D : MonoBehaviour
    {
        #region public properties
        //holds all available 2d effects
        public enum AvailableEffects { FlipPage}
        public AvailableEffects CurrentEffect;

        //flip properties
        public enum FlipObjectTypes { UI,Sprite}
        [HideInInspector] public FlipObjectTypes FlipObject;
        [HideInInspector] public float FlipSpeed = 4f;
        [HideInInspector] public GameObject RightFlipTrigger, LeftFlipTrigger,BookCover,BookPagePref;
        [HideInInspector] public Vector3 BookCenter;
        #endregion

    }
}
