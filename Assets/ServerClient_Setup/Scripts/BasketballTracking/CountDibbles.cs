using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Training
{
    public class CountDibbles : MonoBehaviour
    {

        private int _countDibbles;

        private Vector3 _lastPosition;
        private bool _first;
        private bool _falling;
        [SerializeField]
        private double OFFSET = 20;

        private Player _player;

        void Start ()
        {
            _lastPosition = transform.position;
            _first = true;
            _player = FindObjectOfType<Player>();
        }
	
        // Update is called once per frame
        void Update ()
        {
            var down = _lastPosition.y - transform.position.y < OFFSET;
            _lastPosition = transform.position;
            if (_first)
            {
                _first = false;
                _falling = false;
                return;
            }

            if (!down)
            {
                _falling = false;
                return;
            }
            if (_falling) return;
            Debug.Log("Drible");
            _player.RpcAddDribbles();
            _falling = true;
        }
    }
}
