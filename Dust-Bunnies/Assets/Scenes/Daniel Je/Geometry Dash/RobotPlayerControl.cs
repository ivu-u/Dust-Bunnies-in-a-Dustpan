using UnityEngine;

public class RobotPlayerControl : MonoBehaviour
{
    [SerializeField]
    private float upwardAcceleration = 100;
    [SerializeField]
    private float forwardVelocity = 10;
    [SerializeField]
    private float airTime = 0.5f;
    [SerializeField]
    private float gravity = 15;
    private bool _isHolding = false;
    private float _airTimer = 0;
    private Rigidbody _rb;
    private bool _alive = true;
    private bool _touchingGround = true;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity = new Vector3(forwardVelocity, 0, 0);
    }

    void FixedUpdate()
    {
        if (_alive)
        {
            if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)) && _airTimer <= airTime)
            {
                if (_touchingGround)
                {
                    if (_isHolding == false)
                        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, upwardAcceleration/10, 0);
                    _isHolding = true;
                }
                _rb.AddForce(transform.up * upwardAcceleration);
                _touchingGround = false;
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
            if (_touchingGround)
            {
                _airTimer = 0;
            }
            _airTimer += Time.fixedDeltaTime;
        }
        Debug.Log(_rb.linearVelocity);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Hazard>() != null)
        {
            Die();
        }
    }
    void Die()
    {
        // Add death logic here
        _rb.linearVelocity = new Vector3(0, 0, 0);
        _alive = false;
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Floor>() != null)
        {
            _touchingGround = true;
        }
    }
}
