using Windows.Kinect;
using Assets.Scripts.KinectManagers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Assets
{
    public class Manager : NetworkBehaviour
    {
        [SerializeField]
        private GameObject _armPrefab;

        [SerializeField]
        private GameObject _headPrefab;

        [SerializeField]
        private Text _pointsText;

        private BodySourceManager _bodySourceManager;

        private GameObject _leftHand;
        private GameObject _righttHand;
        private GameObject _head;

        [SerializeField]
        private float _offset = 10;

        [SerializeField]
        private GameObject _targets;

        private GameObject _cameraGameObject;
        private void Start()
        {
            if (isServer)
            {
                _bodySourceManager = GetComponent<BodySourceManager>();
                Debug.Log(_bodySourceManager);
                _cameraGameObject = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        public override void OnStartServer() {

            _leftHand = Instantiate(_armPrefab, transform.position, transform.rotation);
            _leftHand.GetComponent<SpriteRenderer>().flipX = true;
            Debug.Log(_leftHand.GetComponent<SpriteRenderer>().flipX);
            _leftHand.name = "Left Hand";
            NetworkServer.Spawn(_leftHand);
            _righttHand = Instantiate(_armPrefab, transform.position, transform.rotation);
            _righttHand.name = "Rigth hand";
            NetworkServer.Spawn(_righttHand);
            _head = Instantiate(_headPrefab, transform.position, transform.rotation);
            _head.name = "Head";
            NetworkServer.Spawn(_head);
        }
        

        // Update is called once per frame
        void Update () {

            if (!isServer) return;
            if (Input.GetKeyDown(KeyCode.A))
            {
                FindObjectOfType<Player>().RpcSetPoints();
            }

            if (_bodySourceManager == null) return;
            var bodies = _bodySourceManager.GetData();
            if (bodies == null) return;
            if (bodies[_bodySourceManager.CurrentBody].IsTracked)
            {
                var body = bodies[_bodySourceManager.CurrentBody];
                Debug.Log(body);
                Debug.Log(_bodySourceManager.CurrentBody);
                var pos = body.Joints[JointType.HandLeft].Position;
                _leftHand.transform.localPosition = new Vector3(pos.X, pos.Y, -pos.Z) * _offset;
                pos = body.Joints[JointType.HandRight].Position;
                _righttHand.transform.localPosition = new Vector3(pos.X, pos.Y, -pos.Z) * _offset;
                pos = body.Joints[JointType.Head].Position;
                _head.transform.localPosition = new Vector3(pos.X, pos.Y, -pos.Z) * _offset;
                _cameraGameObject.transform.position = _head.transform.position;
            }
            else
            {
                for(var i = 0; i < bodies.Length; i++)
                {
                    if (bodies[i].IsTracked)
                    {
                        _bodySourceManager.CurrentBody = i;
                    }
                }
            }
            _targets.transform.position = new Vector3(_targets.transform.position.x, _head.transform.position.y, _head.transform.position.z);
        }
    }
}
