using UnityEngine;

public class RobotPlayerControl : MonoBehaviour
{
    [SerializeField]
    private float upwardAcceleration = 100;
    [SerializeField]
    private float forwardAcceleration = 10;
    [SerializeField]
    private float airTime = 0.5f;
    [SerializeField]
    private float gravity = 15;
    private bool _isHolding = false;
    private float _airTimer = 0;
    private Rigidbody _rb;
    private float _startingY;
    private bool alive = true;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _startingY = transform.position.y;
        _rb.linearVelocity = new Vector3(forwardAcceleration, 0, 0);
    }

    void FixedUpdate()
    {
        if (alive) {
            if (Input.GetMouseButton(0) && _airTimer <= airTime)
            {
                if (transform.position.y - (_startingY) <= 0.01)
                {
                    _isHolding = true;
                }
                _rb.AddForce(transform.up * upwardAcceleration);
            }
            else
            {
                if (_isHolding)
                {
                    _airTimer += 50000;
                    _isHolding = false;
                }
                _rb.AddForce(Physics.gravity * gravity);
            }
            if (transform.position.y - (_startingY) <= 0.01)
            {
                _airTimer = 0;
            }
            _airTimer += Time.fixedDeltaTime;
            _rb.AddForce(Vector3.right * forwardAcceleration);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("dead");
        Die();
    }
    void Die()
    {
        // Add death logic here
        _rb.linearVelocity = new Vector3(0, 0, 0);
        alive = false;
    }
}
