using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Shooter : NetworkBehaviour {

    [SerializeField]
    private float _interval = 2;

    [SerializeField]
    private float _timeToTarget = 1.5f;

    private const float Delay = .5f;

    [SerializeField]
    private GameObject _ball;

    [SerializeField]
    private GameObjectColumn[] _columns;

    private Position[] Positions = new Position[] {Position.Left, Position.Middle, Position.Right
        };

    // Use this for initialization
    public override void OnStartServer()
    {
            InvokeRepeating("Shoot", _interval, _interval);
    }

    private void Shoot() {
        var instance = Instantiate(_ball, gameObject.transform).GetComponent<Rigidbody>();
        var auxColumns = _columns.Where(column => Positions.Contains(column.Position)).ToArray();
        var auxColumn = auxColumns[Random.Range(0, auxColumns.Length)];
        var target = auxColumn.Targets[Random.Range(0, auxColumn.Targets.Length)].transform.position;
        instance.velocity = calculateBestThrowSpeed(instance.transform.position, target, _timeToTarget);
        NetworkServer.Spawn(instance.gameObject);
        Destroy(instance.gameObject, _timeToTarget + Delay);
    }

    private Vector3 calculateBestThrowSpeed(Vector3 origin, Vector3 target, float timeToTarget) {
        // calculate vectors
        var toTarget = target - origin;
        var toTargetXz = toTarget;
        toTargetXz.y = 0;

        // calculate xz and y
        var y = toTarget.y;
        var xz = toTargetXz.magnitude;

        // calculate starting speeds for xz and y. Physics forumulase deltaX = v0 * t + 1/2 * a * t * t
        // where a is "-gravity" but only on the y plane, and a is 0 in xz plane.
        // so xz = v0xz * t => v0xz = xz / t
        // and y = v0y * t - 1/2 * gravity * t * t => v0y * t = y + 1/2 * gravity * t * t => v0y = y / t + 1/2 * gravity * t
        var t = timeToTarget;
        var v0y = y / t + 0.5f * Physics.gravity.magnitude * t;
        var v0xz = xz / t;

        // create result vector for calculated starting speeds
        var result = toTargetXz.normalized;        // get direction of xz but with magnitude 1
        result *= v0xz;                                // set magnitude of xz to v0xz (starting speed in xz plane)
        result.y = v0y;                                // set y to v0y (starting speed of y plane)

        return result;
    }

}

#region ArrayHelper
[System.Serializable]
public class GameObjectColumn {
    [SerializeField]
    private Position _position;

    [SerializeField]
    private GameObject[] _targets;

    public GameObject[] Targets {
        get {
            return _targets;
        }

        set {
            _targets = value;
        }
    }

    public Position Position {
        get {
            return _position;
        }

        set {
            _position = value;
        }
    }
}

[System.Serializable]
public enum Position {
    Left, Middle, Right
}
#endregion