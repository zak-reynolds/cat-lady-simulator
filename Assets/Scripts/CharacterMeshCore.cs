using MathExtensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

public class CharacterMeshCore : MonoBehaviour
{

    public enum VelocityMethod { Rigidbody, NavMeshAgent }

    [Header("Configuration")]
    [SerializeField]
    private VelocityMethod velocityMethod;

    [Header("Feel")]
    [SerializeField]
    private float rotationDamping = 10f;
    [SerializeField]
    private float velocityDeadZone = 0.1f;
    [SerializeField]
    private float tiltSpeedThreshold = 5f;
    [SerializeField]
    private float tiltDamping = 8f;

    [Header("Debug")]
    [SerializeField]
    private bool drawGizmos = false;

    private Rigidbody rb;
    private NavMeshAgent nma;
    private Animator animator;

    private enum CharacterParam
    {
        Idle, Walk, Run,
        Pass, Reach, PassMirror, ReachMirror
    }
    private enum CharacterLayer
    {
        LowerOverride, UpperOverride
    }

    // TODO: INJECT
    private Dictionary<CharacterParam, int> animatorParameterMap;

    private float[] splineSamples;

    private void InitializeStaticData()
    {
        if (animatorParameterMap == null)
        {
            animatorParameterMap = new Dictionary<CharacterParam, int>()
            {
                // Locomotion modes
                { CharacterParam.Idle, Animator.StringToHash("Idle") },
                { CharacterParam.Walk, Animator.StringToHash("Walk") },
                { CharacterParam.Run, Animator.StringToHash("Run") },

                // Locomotion state
                { CharacterParam.Pass, Animator.StringToHash("Pass") },
                { CharacterParam.Reach, Animator.StringToHash("Reach") },
                { CharacterParam.PassMirror, Animator.StringToHash("Pass-Mirror") },
                { CharacterParam.ReachMirror, Animator.StringToHash("Reach-Mirror") }
            };
        }
    }

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        nma = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        InitializeStaticData();
        lastFrame = animatorParameterMap[CharacterParam.Idle];
        nextFrame = animatorParameterMap[CharacterParam.Idle];
        splineSamples = Spline.GenerateSamples(new[] {
            Vector3.zero,
            new Vector3(0.5f, 0f),
            new Vector3(0.5f, 1f),
            new Vector3(1f, 1f)
        });
    }


    private Vector3 velocity;

    void Update()
    {
        velocity = velocityMethod == VelocityMethod.Rigidbody ? rb.velocity : nma.velocity;
        var flattenedVelocity = velocity.Flatten().normalized;
        if (velocity.Flatten().magnitude > velocityDeadZone)
        {
            var velocityRotation = Quaternion.LookRotation(flattenedVelocity);
            var targetRotation = Quaternion.Slerp(velocityRotation, velocityRotation * GetTiltRotation(), Time.deltaTime * tiltDamping);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationDamping);
        }
        if (!animator || animatorParameterMap == null) return;
        radius = Mathf.Clamp(velocity.Flatten().magnitude, 0.1f, 1f);
        speedOffset += velocity.Flatten().magnitude / radius * Time.deltaTime;
        if (flattenedVelocity.magnitude < velocityDeadZone)
        {
            animator.SetFloat(animatorParameterMap[CharacterParam.Idle], 1f);
            animator.SetFloat(animatorParameterMap[CharacterParam.Walk], 0f);
            animator.SetFloat(animatorParameterMap[CharacterParam.Run], 0f);
        }
        else
        {
            UpdateAnimator();
        }
    }

    private int nextFrame;
    private int lastFrame;

    private void UpdateAnimator()
    {
        animator.SetFloat(animatorParameterMap[CharacterParam.Idle], 0f);
        float r = radius == 1f ? 1f : 0f;
        animator.SetFloat(animatorParameterMap[CharacterParam.Walk], splineSamples.Sample(1 - r));
        animator.SetFloat(animatorParameterMap[CharacterParam.Run], splineSamples.Sample(r));
        int quantized = Mathf.FloorToInt((Mathf.Rad2Deg * speedOffset) / 360f * 8f);
        int targetFrame = 0;
        switch (quantized % 4)
        {
            case 0: targetFrame = animatorParameterMap[CharacterParam.Pass]; break;
            case 1: targetFrame = animatorParameterMap[CharacterParam.Reach]; break;
            case 2: targetFrame = animatorParameterMap[CharacterParam.PassMirror]; break;
            case 3: targetFrame = animatorParameterMap[CharacterParam.ReachMirror]; break;
        }
        if (targetFrame != nextFrame)
        {
            animator.SetFloat(lastFrame, 0f);
            lastFrame = nextFrame;
            nextFrame = targetFrame;
        }
        float t = ((Mathf.Rad2Deg * speedOffset) / 360f * 8f) - quantized;
        animator.SetFloat(lastFrame, splineSamples.Sample(1 - t));
        animator.SetFloat(nextFrame, splineSamples.Sample(t));
    }

    private Quaternion GetTiltRotation()
    {
        var flatVelocity = new Vector3(velocity.x, velocity.z, 0).normalized;
        if (velocity.magnitude < tiltSpeedThreshold)
        {
            return Quaternion.identity;
        }
        var direction = new Vector3(transform.forward.x, transform.forward.z, 0).normalized;
        var cross = Vector3.Cross(direction, flatVelocity);
        var dot = Vector3.Dot(direction, flatVelocity);
        if (dot > 0.999999f)
        {
            return Quaternion.identity;
        }
        if (dot < -0.999999f)
        {
            return Quaternion.Inverse(Quaternion.identity);
        }
        return new Quaternion(
            cross.x,
            cross.y,
            cross.z,
            Mathf.Sqrt((direction.magnitude * direction.magnitude) * (flatVelocity.magnitude * flatVelocity.magnitude)) + dot
        );
    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!drawGizmos || !rb) return;
        DrawTilt();
        DrawVelocityWheel();
    }

    private void DrawTilt()
    {
        var start = transform.position + Vector3.up * 3f;
        var endUnit = Vector3.up;
        var endTilt = start + (GetTiltRotation() * Vector3.up);
        Debug.DrawLine(start, start + endUnit, Color.cyan);
        Debug.DrawLine(start, endTilt, Color.green);
    }

    private float speedOffset = 0f; // radians around the wheel
    private float radius = 0.5f;

    private void DrawVelocityWheel()
    {
        var spokes = 8;
        var origin = transform.position + new Vector3(0, radius, 0);
        if (speedOffset > Mathf.Deg2Rad * 360) speedOffset -= Mathf.Deg2Rad * 360;
        for (int i = 0; i < spokes; ++i)
        {
            var rotation = Quaternion.AngleAxis(i * (360f / spokes) + (Mathf.Rad2Deg * speedOffset), transform.right);
            Debug.DrawLine(origin, origin + (rotation * transform.forward * radius), Color.magenta);
        }
    }
    #endregion
}
