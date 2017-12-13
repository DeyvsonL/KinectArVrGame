using Assets.Scripts.KinectManagers;
using Assets.Scripts.KinectUIModule;
using OpenCvSharp;
using UnityEngine;

namespace Assets.Scripts.Training
{
    [RequireComponent(typeof(BodyIndexSourceManager))]
    public class BascketBallTracking : MonoBehaviour
    {

        //initial min and max HSV filter values.
        //these will be changed using trackbars

        const int FRAME_WIDTH = 640;
        const int FRAME_HEIGHT = 480;

        //names that will appear at the top of each window
        const string windowName = "Original Image";
        const string windowName2 = "Thresholded Image";

        [Space(20)]
        [Header("Hough circles params")]
        [SerializeField]
        private float _dp = 2;
        [SerializeField]
        private int _minDist = 120;
        [SerializeField]
        private int _param1 = 50;
        [SerializeField]
        private int _param2 = 60;
        [SerializeField]
        private int _minRadius = 10;
        [SerializeField]
        private int _maxRadius = 100;

        [Header("Multipliers")]
        [SerializeField]
        private float _multiplyHeight = 2;

        [SerializeField]
        private float _multiplyWidth = 1.5f;

        [SerializeField]
        private float _offsetHeight = 0.3f;

        [SerializeField]
        private float _offsetWidth = -0.284f;

        [SerializeField]
        private bool _showFrame = false;


        //initial
        private Mat cameraFeed;
        //matrix storage for HSV image
        private Mat HSV;
        //matrix storage for binary threshold image
        public bool trackObjects = false;
        public bool useMorphOps = false;

        //matrix storage for binary threshold image
        //x and y values for the location of the object
        private int x = 0, y = 0;

        private BodyIndexSourceManager _bodyManager;

        private KinectInputData _inputData;

        private Rigidbody _rb;

        private void Awake()
        {
            _bodyManager = GetComponent<BodyIndexSourceManager>();
            _inputData = FindObjectOfType<KinectInputModule>()._inputData[0];
            _rb = GetComponent<Rigidbody>();
        }

        [SerializeField] private float _divider = 5;

        // Update is called once per frame
        private void Update()
        {
            var m = new Mat(_bodyManager.BodyIndexrHeight, _bodyManager.BodyIndexWidth, MatType.CV_8UC4, _bodyManager.RawData);

            using (cameraFeed = m)
            {
                var grayScaleMat = new Mat();
                Cv2.CvtColor(cameraFeed, grayScaleMat, ColorConversionCodes.BGR2GRAY);
                Cv2.GaussianBlur(grayScaleMat, grayScaleMat, new Size(9,9), 0);
                var circles = Cv2.HoughCircles(grayScaleMat, HoughMethods.Gradient, _dp, _minDist, _param1, _param2, _minRadius, _maxRadius);

                if (useMorphOps)
                    MorphOps(grayScaleMat);
                CircleSegment? definitiveCircle = null;
                var height = 0f;
                foreach (var circle in circles)
                {
                    var ballCenter = circle.Center.X / grayScaleMat.Width;
                    var leftElbow = Camera.main.WorldToScreenPoint(new Vector3(_inputData.ElbowLeftPosition.x/2,
                    _inputData.ElbowLeftPosition.y / 2, _inputData.ElbowLeftPosition.z)).x / Camera.main.pixelWidth;
                    var rightElbow = Camera.main.WorldToScreenPoint(new Vector3(_inputData.ElbowRightPosition.x / 2,
                                         _inputData.ElbowRightPosition.y / 2, _inputData.ElbowRightPosition.z)).x / Camera.main.pixelWidth;

                    /*cameraFeed.Circle((int)(leftElbow * grayScaleMat.Width),
                        grayScaleMat.Height / 2, 15, Scalar.AliceBlue, -1);*/

                    //if (leftElbow < ballCenter && rightElbow > ballCenter) continue;
                    height = (grayScaleMat.Height - circle.Center.Y) / grayScaleMat.Height * _multiplyHeight;
                    height -=_offsetHeight;
                    //transform.position = new Vector3( (circle.Center.X-grayScaleMat.Width/2f )/grayScaleMat.Width* _multiplyWidth + _offsetWidth,
                    //    height , transform.position.z );
                    if (definitiveCircle == null || circle.Radius > definitiveCircle.Value.Radius)
                    {
                        definitiveCircle = circle;
                    }
                }

                if (definitiveCircle != null)
                {
                    _rb.MovePosition(new Vector3((definitiveCircle.Value.Center.X - grayScaleMat.Width / 2f) / grayScaleMat.Width * _multiplyWidth + _offsetWidth,
                        height, transform.position.z));
                    cameraFeed.Circle(definitiveCircle.Value.Center, (int)definitiveCircle.Value.Radius, Scalar.AliceBlue, -1);
                }

                //show frames 
                if (_showFrame)
                {
                    Cv2.ImShow(windowName2, grayScaleMat);
                    Cv2.ImShow(windowName, cameraFeed);
                }
                //Cv2.ImShow(windowName, cameraFeed);
                
                //delay 30ms so that screen can refresh.
                //image will not appear without this waitKey() command
                Cv2.WaitKey(30);
            }
        }

        private static void MorphOps(Mat thresh)
        {

            //create structuring element that will be used to "dilate" and "erode" image.
            //the element chosen here is a 3px by 3px rectangle
            var erodeElement = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
            //dilate with larger element so make sure object is nicely visible
            var dilateElement = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(8, 8));

            Cv2.Erode(thresh, thresh, erodeElement);
            Cv2.Erode(thresh, thresh, erodeElement);
            
            Cv2.Dilate(thresh, thresh, dilateElement);
            Cv2.Dilate(thresh, thresh, dilateElement);
        }
        
        private void OnDestroy()
        {
            Cv2.DestroyAllWindows();
        }
    }
}
