using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimation : NetworkBehaviour
{
    #region Variables

    [Header("Animator")]
    [SerializeField] Animator _animator;

    [Header("Foot IK")]
    [SerializeField] private Transform _leftFootTarget;
    [SerializeField] private Transform _rightFootTarget;
    [SerializeField] private TwoBoneIKConstraint _leftFootIKConstraint;
    [SerializeField] private TwoBoneIKConstraint _rightFootIKConstraint;

    [SerializeField] private Vector2 _feetOffsetHeight = new Vector2(0.5f, 0.0f);

    [SerializeField] private float _ikBlendDuration = 0.25f;
    
    [Serializable]
    public struct WallIKData : IEquatable<WallIKData>
    {
        public bool  active;
        public bool  isLeftSide;
        public Vector3 leftFootPos;
        public Vector3 rightFootPos;

        public bool Equals(WallIKData other) =>
            active == other.active &&
            isLeftSide == other.isLeftSide &&
            leftFootPos == other.leftFootPos &&
            rightFootPos == other.rightFootPos;
    }

    private readonly SyncVar<WallIKData> _wallIKData = new SyncVar<WallIKData>();

    private Coroutine _blendCoroutine;

    #endregion

    #region Lifecycle

    private void Awake()
    {
        _wallIKData.OnChange += OnWallIKDataChanged;
    }

    #endregion

    #region Animator

    public void SetMovingAnim(bool isMoving)  => _animator.SetBool("Move", isMoving);
    public void SetJumpAnim(bool isJumping)   => _animator.SetBool("Jump", isJumping);
    public void SetFallingAnim(bool isFalling) => _animator.SetBool("Falling", isFalling);
    public void SetGroundedAnim(bool isGrounded) => _animator.SetBool("Grounded", isGrounded);
    public void SetDeadAnim(bool isDead)      => _animator.SetBool("Dead", isDead);

    public void ChangeAirState(bool isGrounded)
    {
        _animator.SetBool("Falling", !isGrounded);
        _animator.SetBool("Grounded", isGrounded);
    }

    #endregion

    #region IK Feet
    
    [ServerRpc(RequireOwnership = true)]
    public void SetWallIKServerRpc(bool isLeftSide, Vector3 wallContactPoint, Vector3 wallNormal)
    {
        Vector3 up = Vector3.up;

        Vector3 highFoot = wallContactPoint + up * _feetOffsetHeight.x;
        Vector3 lowFoot = wallContactPoint + up * _feetOffsetHeight.y;

        Vector3 footSpread = Vector3.Cross(wallNormal, up).normalized * 0.15f;

        WallIKData data = new WallIKData
        {
            active = true,
            isLeftSide = isLeftSide,
            leftFootPos = isLeftSide ? highFoot - footSpread : lowFoot + footSpread,
            rightFootPos = isLeftSide ? lowFoot  + footSpread : highFoot - footSpread,
        };

        _wallIKData.Value = data;
    }
    
    [ServerRpc(RequireOwnership = true)]
    public void ResetWallIKServerRpc()
    {
        _wallIKData.Value = new WallIKData { active = false };
    }

    #endregion

    #region IK Feet — réaction au changement (tous les clients)

    private void OnWallIKDataChanged(WallIKData prev, WallIKData next, bool asServer)
    {
        if (asServer) return;

        if (_blendCoroutine != null) StopCoroutine(_blendCoroutine);

        if (next.active)
        {
            _leftFootTarget.position  = next.leftFootPos;
            _rightFootTarget.position = next.rightFootPos;
            _blendCoroutine = StartCoroutine(BlendIKWeight(0f, 1f));
        }
        else
        {
            _blendCoroutine = StartCoroutine(BlendIKWeight(
                _leftFootIKConstraint.weight, 0f));
        }
    }

    private IEnumerator BlendIKWeight(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < _ikBlendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _ikBlendDuration);

            float w = Mathf.Lerp(from, to, t);
            _leftFootIKConstraint.weight  = w;
            _rightFootIKConstraint.weight = w;

            yield return null;
        }

        _leftFootIKConstraint.weight  = to;
        _rightFootIKConstraint.weight = to;
    }

    #endregion
}