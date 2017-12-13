using System.Collections.Generic;
using System.Linq;
using Windows.Kinect;
using Assets.Scripts.KinectManagers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.KinectUIModule
{
    [AddComponentMenu("Kinect/Kinect Input Module")]
    [RequireComponent(typeof(EventSystem))]
    public class KinectInputModule : StandaloneInputModule
    {
        [Space(20)]
        public KinectInputData[] _inputData = new KinectInputData[0];
        [SerializeField]
        private float _scrollTreshold = .5f;
        [SerializeField]
        private float _scrollSpeed = 3.5f;
        [SerializeField]
        private float _waitOverTime = 2f;

        PointerEventData _handPointerData;

        [SerializeField]
        private BodySourceManager _bodyManager;

        private int _currentBody;

        private bool _userTracked = false;

        [SerializeField] private GameObject _userNotTrackedText;


        public void TrackBody()
        {
            var data = _bodyManager.GetData();
            if (data == null) return;
            var currentBody = data[_currentBody];
            _bodyManager.CurrentBody = _currentBody;
            if (currentBody != null && currentBody.IsTracked)
            {
                if (!_userTracked) UserTrackedFeedback();
                foreach (var kinectInputData in _inputData)
                {
                    kinectInputData.UpdateComponent(currentBody);
                }
                return;
            }

            for (var i = 0; i < data.Length; i++)
            {
                if (!data[i].IsTracked) continue;
                _currentBody = i;
                if (!_userTracked) UserTrackedFeedback();
                foreach (var kinectInputData in _inputData)
                {
                    kinectInputData.UpdateComponent(data[i]);
                }
                return;
            }

            UserNotTrackedFeedback();

        }
    
        private void UserTrackedFeedback()
        {
            //_userNotTrackedText.SetActive(false);
            foreach (var kinectInputData in _inputData)
            {
                //kinectInputData.KinectUiCursor.gameObject.SetActive(true);
            }
        }

        private void UserNotTrackedFeedback()
        {
            //_userNotTrackedText.SetActive(true);
            foreach (var kinectInputData in _inputData)
            {
                //kinectInputData.KinectUiCursor.gameObject.SetActive(false);
            }
        }

        // get a pointer event data for a screen position
        private PointerEventData GetLookPointerEventData(Vector3 componentPosition)
        {
            if (_handPointerData == null)
            {
                _handPointerData = new PointerEventData(eventSystem);
            }
            _handPointerData.Reset();
            _handPointerData.delta = Vector2.zero;
            _handPointerData.position = componentPosition;
            _handPointerData.scrollDelta = Vector2.zero;
            eventSystem.RaycastAll(_handPointerData, m_RaycastResultCache);
            _handPointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();
            return _handPointerData;
        }
   
        public override void Process()
        {
            TrackBody();
            ProcessHover();
            ProcessPress();
            ProcessDrag();
            ProcessWaitOver();
            /*var lookData = GetLookPointerEventData(Input.mousePosition);
            var go = lookData.pointerCurrentRaycast.gameObject;
            if (go != null && go.GetComponent<KinectUIWaitOverButton>() != null && Input.GetMouseButtonDown(0)) return;*/
            base.Process();
        }
        /// <summary>
        /// Processes waitint over componens, if hovererd buttons click type is waitover, process it!
        /// </summary>
        private void ProcessWaitOver()
        {
            foreach (var kinectInputData in _inputData)
            {
                if (!kinectInputData.IsHovering || kinectInputData.ClickGesture != KinectUIClickGesture.WaitOver) continue;
                kinectInputData.WaitOverAmount = (Time.time - kinectInputData.HoverTime) / _waitOverTime;
                if (!(Time.time >= kinectInputData.HoverTime + _waitOverTime)) continue;
                var lookData = GetLookPointerEventData(kinectInputData.GetHandScreenPosition());
                var go = lookData.pointerCurrentRaycast.gameObject;
                ExecuteEvents.ExecuteHierarchy(go, lookData, ExecuteEvents.submitHandler);
                // reset time
                kinectInputData.HoverTime = Time.time;
            }
        }

        private void ProcessDrag()
        {
            foreach (var kinectInputData in _inputData)
            {
                // if not pressing we can't drag
                if (!kinectInputData.IsPressing) continue;
                //Debug.Log("drag " + Mathf.Abs(_inputData[i].TempHandPosition.x - _inputData[i].HandPosition.x));
                // Check if we reach drag treshold for any axis, temporary position set when we press an object
                if (Mathf.Abs(kinectInputData.TempHandPosition.x - kinectInputData.HandPosition.x) > _scrollTreshold || Mathf.Abs(kinectInputData.TempHandPosition.y - kinectInputData.HandPosition.y) > _scrollTreshold)
                {
                    kinectInputData.IsDraging = true;
                }
                else
                {
                    kinectInputData.IsDraging = false;
                }
                //Debug.Log("drag " + _inputData[i].IsDraging + " press " + _inputData[i].IsPressing);
                // If dragging use unit's eventhandler to send an event to a scrollview like component
                if (!kinectInputData.IsDraging) continue;
                var lookData = GetLookPointerEventData(kinectInputData.GetHandScreenPosition());
                eventSystem.SetSelectedGameObject(null);
                //Debug.Log("drag");
                var go = lookData.pointerCurrentRaycast.gameObject;
                var pEvent = new PointerEventData(eventSystem)
                {
                    dragging = true,
                    scrollDelta = (kinectInputData.TempHandPosition - kinectInputData.HandPosition) * _scrollSpeed,
                    useDragThreshold = true
                };
                ExecuteEvents.ExecuteHierarchy(go, pEvent, ExecuteEvents.scrollHandler);
            }
        }
        /// <summary>
        ///  Process pressing, event click trigered on button by closing and opening hand,sends submit event to gameobject
        /// </summary>
        private void ProcessPress()
        {
            foreach (var kinectInputData in _inputData)
            {
                //Check if we are tracking hand state not wait over
                if (!kinectInputData.IsHovering || kinectInputData.ClickGesture != KinectUIClickGesture.HandState) continue;
                // If hand state is not tracked reset properties
                if (kinectInputData.CurrentHandState == HandState.NotTracked)
                {
                    kinectInputData.IsPressing = false;
                    kinectInputData.IsDraging = false;
                }
                // When we close hand and we are not pressing set property as pressed
                if (!kinectInputData.IsPressing && kinectInputData.CurrentHandState == HandState.Closed)
                {
                    kinectInputData.IsPressing = true;
                    //PointerEventData lookData = GetLookPointerEventData(_inputData[i].GetHandScreenPosition());
                    //eventSystem.SetSelectedGameObject(null);
                    //if (lookData.pointerCurrentRaycast.gameObject != null && !_inputData[i].IsDraging)
                    //{
                    //    GameObject go = lookData.pointerCurrentRaycast.gameObject;
                    //    ExecuteEvents.ExecuteHierarchy(go, lookData, ExecuteEvents.pointerDownHandler);
                    //}
                }
                // If hand state is opened and is pressed, make click action
                else if (kinectInputData.IsPressing && (kinectInputData.CurrentHandState == HandState.Open))//|| _inputData[i].CurrentHandState == HandState.Unknown))
                {
                    //_inputData[i].IsDraging = false;
                    var lookData = GetLookPointerEventData(kinectInputData.GetHandScreenPosition());
                    eventSystem.SetSelectedGameObject(null);
                    if (lookData.pointerCurrentRaycast.gameObject != null && !kinectInputData.IsDraging)
                    {
                        var go = lookData.pointerCurrentRaycast.gameObject;
                        ExecuteEvents.ExecuteHierarchy(go, lookData, ExecuteEvents.submitHandler);
                        //ExecuteEvents.ExecuteHierarchy(go, lookData, ExecuteEvents.pointerUpHandler);
                    }
                    kinectInputData.IsPressing = false;
                }
            }
        }
        /// <summary>
        /// Process hovering over component, sends pointer enter exit event to gameobject
        /// </summary>
        private void ProcessHover()
        {
            List<GameObject> objects = new List<GameObject>();
            foreach (var inputData in _inputData)
            {
                var pointer = GetLookPointerEventData(inputData.GetHandScreenPosition());
                var obj = _handPointerData.pointerCurrentRaycast.gameObject;

                if (inputData.HoveringObject == null && obj == null)
                {
                    continue;
                }
                if (obj != null)
                {
                    var pointerEnterHandler = obj.GetComponent<IPointerEnterHandler>();
                    if (pointerEnterHandler!=null)
                    {
                        objects.Add(obj);
                        pointerEnterHandler.OnPointerEnter(null);
                    }
                }

                if (inputData.HoveringObject !=null && !objects.Contains(inputData.HoveringObject))
                {
                    var exitHandler = inputData.HoveringObject.GetComponent<IPointerExitHandler>();
                    if (exitHandler != null)
                    {
                        exitHandler.OnPointerExit(null);
                    }
                }

                inputData.IsHovering = obj != null;
                inputData.HoveringObject = obj;
            }

            foreach (var inputData in _inputData)
            {
                if (inputData.HoveringObject == null || objects.Contains(inputData.HoveringObject)) continue;
                var exitHandler = inputData.HoveringObject.GetComponent<IPointerExitHandler>();
                if (exitHandler != null)
                {
                    exitHandler.OnPointerExit(null);
                }
            }

        }
        /// <summary>
        /// Used from UI hand cursor components
        /// </summary>
        /// <param name="handType"></param>
        /// <returns></returns>
        public KinectInputData GetHandData(KinectUIHandType handType)
        {
            return _inputData.FirstOrDefault(kinectInputData => kinectInputData.trackingHandType == handType);
        }
    }
    [System.Serializable]
    public class KinectInputData
    {
        //public KinectUICursor KinectUiCursor;
        // Which hand we are tracking
        public KinectUIHandType trackingHandType = KinectUIHandType.Right;
        // We can normalize camera z position with this
        public float handScreenPositionMultiplier = 5f;
        // Is hand in pressing condition
        private bool _isPressing;//, _isHovering;
        // Hovering Gameobject, needed for WaitOver like clicking detection
        private GameObject _hoveringObject;


        // Joint type, we need it for getting body's hand world position
        public JointType handType
        {
            get { return trackingHandType == KinectUIHandType.Right ? JointType.HandRight : JointType.HandLeft; }
        }
        // Hovering Gameobject getter setter, needed for WaitOver like clicking detection
        public GameObject HoveringObject
        {
            get { return _hoveringObject; }
            set
            {
                if (value == _hoveringObject) return;
                HoverTime = Time.time;
                _hoveringObject = value;
                if (_hoveringObject == null) return;
                //ClickGesture = _hoveringObject.GetComponent<KinectUIWaitOverButton>() ? KinectUIClickGesture.WaitOver : KinectUIClickGesture.HandState;
                WaitOverAmount = 0f;
            }
        }
        public HandState CurrentHandState { get; private set; }
        // Click gesture of button
        public KinectUIClickGesture ClickGesture { get; private set; }
        // Is this hand tracking started
        public bool IsTracking { get; private set; }
        // Is this hand over a UI component
        public bool IsHovering { get; set; }
        // Is hand dragging a component
        public bool IsDraging { get; set; }
        // Is hand pressing a button
        public bool IsPressing
        {
            get { return _isPressing; }
            set
            {
                _isPressing = value;
                if (_isPressing)
                    TempHandPosition = HandPosition;
            }
        }
        // Global position of tracked hand
        public Vector3 HandPosition { get; private set; }

        public Vector3 ElbowLeftPosition { get; private set; }
        public Vector3 ElbowRightPosition { get; private set; }
        public Vector3 HeadPosition { get; private set; }


        // Temporary hand position of hand, used for draging check
        public Vector3 TempHandPosition { get; private set; }
        // Hover start time, used for waitover type buttons
        public float HoverTime { get; set; }
        // Amout of wait over , between 1 - 0 , when reaches 1 button is clicked
        public float WaitOverAmount { get; set; }

        // Must be called for each hand 
        public void UpdateComponent(Body body)
        {
            HandPosition = GetVector3FromJoint(body.Joints[handType]);
            CurrentHandState = GetStateFromJointType(body, handType);
            IsTracking = true;
            //KinectUiCursor.Setup(this);
            ElbowLeftPosition = GetVector3FromJoint(body.Joints[JointType.ElbowLeft]);
            ElbowRightPosition = GetVector3FromJoint(body.Joints[JointType.ElbowRight]);
            HeadPosition = GetVector3FromJoint(body.Joints[JointType.Head]);

        }
        // Converts hand position to screen coordinates
        public Vector3 GetHandScreenPosition()
        {
            //var multiplier = SceneManager.GetActiveScene().name.Equals(Constants.Scenes.Training.Flexibility) && !GameManager.Instance.GamePaused ? 0.4f : 1;
            return Camera.main.WorldToScreenPoint(new Vector3(HandPosition.x , HandPosition.y , HandPosition.z - handScreenPositionMultiplier));
        }

        public Vector3 GetElbowLeftScreenPosition()
        {
            return Camera.main.WorldToScreenPoint(new Vector3(ElbowLeftPosition.x, ElbowLeftPosition.y, ElbowLeftPosition.z - handScreenPositionMultiplier));
        }

        public Vector3 GetElbowRightScreenPosition()
        {
            return Camera.main.WorldToScreenPoint(new Vector3(ElbowRightPosition.x, ElbowRightPosition.y, ElbowRightPosition.z - handScreenPositionMultiplier));
        }

        public Vector3 GetHeadScreenPosition()
        {
            return Camera.main.WorldToScreenPoint(new Vector3(HeadPosition.x, HeadPosition.y, HeadPosition.z - handScreenPositionMultiplier));
        }

        // Get hand state data from kinect body
        private HandState GetStateFromJointType(Body body, JointType type)
        {
            switch (type)
            {
                case JointType.HandLeft:
                    return body.HandLeftState;
                case JointType.HandRight:
                    return body.HandRightState;
                default:
                    Debug.LogWarning("Please select a hand joint, by default right hand will be used!");
                    return body.HandRightState;
            }
        }

        // Get Vector3 position from Joint position
        private Vector3 GetVector3FromJoint(Windows.Kinect.Joint joint)
        {
            return new Vector3(joint.Position.X * 30, joint.Position.Y * 30, 25);
        }
    }

    public enum KinectUIClickGesture
    {
        HandState, Push, WaitOver
    }
    public enum KinectUIHandType
    {
        Right,Left
    }
}