using UnityEngine;

/// <summary>
/// Rotaciona o torso (Cabeca) na direção do mouse e dá o "soluço" de recuo ao
/// acertar. É a fonte única de duas informações que os braços leem:
///   • AngularSpeed → o quão rápido o torso está girando (a serra usa).
///   • AimForward   → para onde o torso aponta (a arma usa).
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Rotação do Torso")]
    [SerializeField] private Transform body;          // Cabeca
    [SerializeField] private float rotateSpeed = 15f;
    [SerializeField] private float bodyHeightOffset = 1f;

    [Header("Feedback de Hit")]
    [SerializeField] private float rotationFreezeOnHit = 0.15f; // segundos que a rotação para ao acertar
    [SerializeField] private float hitBumpSpeed = 60f;          // graus/s de "soluço" ao acertar

    private Camera _cam;
    private float _freezeUntil;
    private bool _bumpActive;
    private float _bumpSign = 1f;
    private Quaternion _lastBodyRot;

    public float AngularSpeed { get; private set; }
    public Vector3 AimForward => body != null ? body.forward : transform.forward;

    public void OnSawHit()
    {
        _freezeUntil = Time.time + rotationFreezeOnHit;
        _bumpActive = true;
        _bumpSign = Random.value > 0.5f ? 1f : -1f;
    }

    private void Awake() => _cam = Camera.main;

    private void Start()
    {
        if (body != null) _lastBodyRot = body.rotation;
    }

    private void Update()
    {
        if (Time.time < _freezeUntil) ApplyHitBump();
        else { _bumpActive = false; RotateBodyTowardsMouse(); }

        if (body != null)
        {
            AngularSpeed = Quaternion.Angle(_lastBodyRot, body.rotation) / Mathf.Max(Time.deltaTime, 1e-5f);
            _lastBodyRot = body.rotation;
        }
    }

    private void RotateBodyTowardsMouse()
    {
        if (body == null || _cam == null) return;

        Plane bodyPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y + bodyHeightOffset, 0f));
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        if (bodyPlane.Raycast(ray, out float dist))
        {
            Vector3 worldPoint = ray.GetPoint(dist);
            Vector3 direction = worldPoint - body.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) return;

            Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
            body.rotation = Quaternion.Slerp(body.rotation, target, rotateSpeed * Time.deltaTime);
        }
    }

    private void ApplyHitBump()
    {
        if (!_bumpActive || body == null) return;
        body.Rotate(Vector3.up, _bumpSign * hitBumpSpeed * Time.deltaTime, Space.World);
    }
}