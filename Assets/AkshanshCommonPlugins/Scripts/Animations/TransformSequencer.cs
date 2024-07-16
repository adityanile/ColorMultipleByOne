using AkshanshKanojia.Controllers.ObjectManager;
using System.Collections.Generic;
using UnityEngine;

namespace AkshanshKanojia.Animations
{
    public class TransformSequencer : MonoBehaviour
    {
        //events
        public delegate void SequenceFinished(int _sequenceIndex);
        public event SequenceFinished OnSequenceEnd;
        public delegate void SequenceListFinished();
        public event SequenceListFinished OnSequenceListEnd;

        [System.Serializable]
        public class SequenceDataHolder
        {
            public GameObject TargetObject;
            public Vector3 TargetPos, TargetRot, TargetScale = Vector3.one;
            [Tooltip("Use this to get id to identify if specific event is finished.")]
            public int SequenceID = 0;
            public float SequenceStartDelay = 0, TrackSpeed = 3f;
            public bool MoveOnLocalAxis = false;
        }
        [SerializeField] SequenceDataHolder[] SequenceInArray;
        [SerializeField] bool playOnAwake = true, loopSequence = false;

        SequenceDataHolder tempTargetSequence;
        Queue<SequenceDataHolder> CurrentSequences;

        [SerializeField] ObjectController objCont;
        bool isActive = true, posReached, rotReached, scaleReached,isPaused;

        private void Start()
        {
            isActive = playOnAwake;
            Initialize();
        }
        //checks if object controller is present and resets animation stack
        private void Initialize()
        {
            if (!objCont)
                objCont = FindObjectOfType<ObjectController>();
            if (!objCont)
            {
                Debug.LogWarning("Can not find object controller, transform sequencer will not work!");
                isActive = false;
            }
            if (isActive)
            {
                CurrentSequences = new Queue<SequenceDataHolder>();
                for (int i = 0; i < SequenceInArray.Length; i++)
                {
                    CurrentSequences.Enqueue(SequenceInArray[i]);
                }
                if (CurrentSequences.Count != 0)
                {
                    objCont.OnMovementEnd += OnPosReached;
                    objCont.OnRotationEnd += OnRotReached;
                    objCont.OnScaleEnd += OnScaleReached;
                }
                if (playOnAwake)
                {
                    UpdateCurrentSequence();
                }
            }
        }
        //checks if specified target position has been reached in current active sequence
        void OnPosReached(GameObject _go)
        {
            if (_go != tempTargetSequence.TargetObject)
                return;
            posReached = true;
            CheckSequenceStatus();
        }
        void OnRotReached(GameObject _go)
        {
            if (_go != tempTargetSequence.TargetObject)
                return;
            rotReached = true;
            CheckSequenceStatus();
        }
        void OnScaleReached(GameObject _go)
        {
            if (_go != tempTargetSequence.TargetObject)
                return;
            scaleReached = true;
            CheckSequenceStatus();
        }

        //check if all conditions in sequence has been fullfilled
        void CheckSequenceStatus()
        {
            if (isPaused)
                return;
            if (posReached && rotReached && scaleReached)
            {
                OnSequenceEnd?.Invoke(tempTargetSequence.SequenceID);
                UpdateCurrentSequence();
            }
        }
        private void OnDisable()
        {
            objCont.OnMovementEnd -= OnPosReached;
            objCont.OnRotationEnd -= OnRotReached;
            objCont.OnScaleEnd -= OnScaleReached;
        }
        //add next sequence as target and stops/repeats it if all sequences are finished based on parameters
        void UpdateCurrentSequence()
        {
            if (CurrentSequences.Count <= 0)
            {

                objCont.OnMovementEnd -= OnPosReached;
                objCont.OnRotationEnd -= OnRotReached;
                objCont.OnScaleEnd -= OnScaleReached;
                if (loopSequence)
                {
                    Initialize();
                    return;
                }
                isActive = false;
                OnSequenceListEnd?.Invoke();
                return;
            }
            tempTargetSequence = CurrentSequences.Dequeue();
            posReached = false;
            rotReached = false;
            scaleReached = false;
            StartCoroutine(StartSequenceQueue(tempTargetSequence.SequenceStartDelay));
        }
        IEnumerator<WaitForSeconds> StartSequenceQueue(float _tempWaitDuration)
        {
            yield return new WaitForSeconds(_tempWaitDuration);
            objCont.AddEvent(tempTargetSequence.TargetObject, tempTargetSequence.TargetPos, Quaternion.Euler(tempTargetSequence.TargetRot),
                tempTargetSequence.TargetScale, tempTargetSequence.TrackSpeed, tempTargetSequence.MoveOnLocalAxis);
        }
        public void PauseSequence()
        {
            isPaused = true;
            CheckSequenceStatus();
        }
        public void ResumeSequecnce()
        {
            isPaused = false;
            CheckSequenceStatus();
        }
        public void Play()
        {
            isActive = true;
            Initialize();
            UpdateCurrentSequence();
        }
    }
}
