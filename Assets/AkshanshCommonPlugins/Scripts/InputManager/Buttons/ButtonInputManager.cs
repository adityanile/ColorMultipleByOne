using AkshanshKanojia.Inputs.Mobile;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AkshanshKanojia.Inputs.Button
{
    public class ButtonInputManager : MobileInputs, IPointerUpHandler, IPointerClickHandler,IPointerDownHandler
    {
        [SerializeField] enum AvailableObjectTypes { UIObject, Object2d, Object3d }
        [SerializeField] AvailableObjectTypes CurtType;
        [SerializeField] LayerMask RaycastLayer;

        //events
        public delegate void OnButtonTapped(GameObject _obj);
        public delegate void OnButtonHeld(GameObject _obj);
        public delegate void OnButtonTapEnd(GameObject _obj);
        public event OnButtonTapped OnTap;
        public event OnButtonHeld OnHeld;
        public event OnButtonTapEnd OnLeft;
        //inspector events
        public UnityEvent OnClickBegin;
        public UnityEvent OnClickStay;
        public UnityEvent OnClickLeft;

        bool isTapped = false,isHeld = false;

        private void OnDisable()
        {
            isTapped = false;
            isHeld = false;
        }
        #region Inputs

        private void Update()
        {
            if(isHeld)
            {
                OnHeld?.Invoke(gameObject);
            }
        }
        public override void OnTapEnd(MobileInputManager.TouchData _data)
        {
            switch (CurtType)
            {
                case AvailableObjectTypes.Object2d:
                case AvailableObjectTypes.Object3d:
                    if (!isTapped)
                        return;
                    isTapped = false;
                    OnLeft?.Invoke(gameObject);
                    OnClickLeft?.Invoke();
                    break;
                default:
                    break;
            }
        }

        public override void OnTapMove(MobileInputManager.TouchData _data)
        {
        }

        public override void OnTapped(MobileInputManager.TouchData _data)
        {
            switch (CurtType)
            {
                case AvailableObjectTypes.Object2d:
                    RaycastHit2D _hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(_data.TouchPosition),Mathf.Infinity,RaycastLayer);
                    if (_hit)
                    {
                        if (_hit.collider == GetComponent<Collider2D>())
                        {
                            isTapped = true;
                            OnTap?.Invoke(gameObject);
                            OnClickBegin?.Invoke();
                        }
                    }
                    break;
                case AvailableObjectTypes.Object3d:
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(_data.TouchPosition), out RaycastHit _tempHit,Mathf.Infinity,RaycastLayer))
                    {
                        if (_tempHit.collider == GetComponent<Collider>())
                        {
                            isTapped = true;
                            OnTap?.Invoke(gameObject);
                            OnClickBegin?.Invoke();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public override void OnTapStay(MobileInputManager.TouchData _data)
        {
            switch (CurtType)
            {
                case AvailableObjectTypes.Object2d:
                    if (!isTapped)
                        return;
                    RaycastHit2D _hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(_data.TouchPosition),Mathf.Infinity,RaycastLayer);
                    if (_hit)
                    {
                        if (_hit.collider == GetComponent<Collider2D>())
                        {
                            OnHeld?.Invoke(gameObject);
                            OnClickStay?.Invoke();
                        }
                        else
                        {
                            isTapped = false;
                        }
                    }
                    else
                    {
                        isTapped = false;
                    }
                    break;
                case AvailableObjectTypes.Object3d:
                    if (!isTapped)
                        return;
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(_data.TouchPosition), out RaycastHit _tempHit,Mathf.Infinity,RaycastLayer))
                    {
                        if (_tempHit.collider == GetComponent<Collider>())
                        {
                            OnHeld?.Invoke(gameObject);
                            OnClickStay?.Invoke();
                        }
                        else
                        {
                            isTapped = false;
                        }
                    }
                    else
                    {
                        isTapped = false;
                    }
                    break;
                default:
                    break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (CurtType == AvailableObjectTypes.UIObject)
            {
                OnLeft?.Invoke(gameObject);
                OnClickLeft?.Invoke();
                isHeld = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (CurtType == AvailableObjectTypes.UIObject)
            {
                isHeld = true;
                OnClickStay?.Invoke();
                OnHeld?.Invoke(gameObject);
            }
        }

        public override void OnPinchBegin(MobileInputManager.PinchData _pinchData)
        {
        }

        public override void OnPinchMove(MobileInputManager.PinchData _pinchData)
        {
        }

        public override void OnPinchEnd(MobileInputManager.PinchData _pinchData)
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (CurtType == AvailableObjectTypes.UIObject)
            {
                isHeld = true;
                OnTap?.Invoke(gameObject);
                OnClickBegin?.Invoke();
            }
        }
        #endregion
    }
}
