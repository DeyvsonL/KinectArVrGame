using Windows.Kinect;
using UnityEngine;

namespace Assets.Scripts.KinectManagers
{
    public class BodyIndexSourceManager : MonoBehaviour
    {
        private KinectSensor _sensor;
        private BodyIndexFrameReader _reader;

        public byte[] RawData { get; private set; }
        private byte[] _data;

        public int BodyIndexWidth { get; private set; }
        public int BodyIndexrHeight { get; private set; }

        [SerializeField]
        private BodySourceManager _bodySourceManager;

        private Texture2D _texture;

        public Texture2D GetBodyIndexTexture()
        {
            return _texture;
        }

        void Start()
        {
            _sensor = KinectSensor.GetDefault();
            if (_sensor == null) return;
            _reader = _sensor.BodyIndexFrameSource.OpenReader();
            var frameDesc = _sensor.BodyIndexFrameSource.FrameDescription;
            BodyIndexWidth = frameDesc.Width;
            BodyIndexrHeight = frameDesc.Height;
            _data = new byte[frameDesc.BytesPerPixel * frameDesc.Width * frameDesc.Height];
            RawData = new byte[_data.Length * 4];
            _texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false);
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
                frame.CopyFrameDataToArray(_data);

                var index = 0;
                Debug.Log(_bodySourceManager.CurrentBody);
                foreach (var ir in _data)
                {
                    if (ir != _bodySourceManager.CurrentBody)
                    {
                        RawData[index++] = 225;
                        RawData[index++] = 225;
                        RawData[index++] = 225;
                        RawData[index++] = 255;
                    }
                    else
                    {
                        RawData[index++] = 0;
                        //(byte) (255 - ir)
                        RawData[index++] = 0;
                        RawData[index++] = 0;
                        RawData[index++] = 255;
                    }
                }

                _texture.LoadRawTextureData(RawData);
                _texture.Apply();

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
