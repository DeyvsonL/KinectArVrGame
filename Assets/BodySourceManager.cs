using Windows.Kinect;
using UnityEngine;

namespace Assets.Scripts.KinectManagers
{
    public class BodySourceManager : MonoBehaviour
    {

        private KinectSensor _sensor;
        private BodyFrameReader _reader;
        private Body[] _data;

        public Body[] GetData()
        {
            return _data;
        }

        public int CurrentBody { get; set; }

        void Start()
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor == null) return;
            _reader = _sensor.BodyFrameSource.OpenReader();

            if (!_sensor.IsOpen)
            {
                _sensor.Open();
            }

        }

        void Update()
        {
            if (_reader == null) return;
            using (var frame = _reader.AcquireLatestFrame())
            {
                if (frame == null) return;
                if (_data == null)
                {
                    _data = new Body[_sensor.BodyFrameSource.BodyCount];
                }
                frame.GetAndRefreshBodyData(_data);
            }
        }

        void OnApplicationQuit()
        {
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }

            if (_sensor == null) return;
            if (_sensor.IsOpen)
            {
                _sensor.Close();
            }
            _sensor = null;
        }
    }
}
