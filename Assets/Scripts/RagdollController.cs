using UnityEngine;
using System.Collections.Generic;

public class RagdollController : MonoBehaviour
{
    public Animator animator;
    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
            
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        SetRagdoll(false); // start in animated mode
    }

    public void SetRagdoll(bool enabled)
    {
        if (animator != null)
            animator.enabled = !enabled;

        foreach (var rb in ragdollBodies)
        {
            if (!enabled && rb.gameObject == this.gameObject) continue; // skip root if disabling ragdoll (Awake)
            
            rb.isKinematic = !enabled;
            if (enabled) 
            {
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.None;
            }
        }
        foreach (var col in ragdollColliders)
        {
            if (!enabled && col.gameObject == this.gameObject) continue;
            
            col.enabled = enabled;
            if (enabled) 
            {
                col.isTrigger = false;
            }
        }
    }

    public void Die()
    {
        SetRagdoll(true);

        // Break joints and unparent to dislocate pieces and let them fall
        if (ragdollBodies != null)
        {
            foreach (var rb in ragdollBodies)
            {
                if (rb == null || rb.gameObject == this.gameObject) continue;

                bool keepJoint = false;
                if (animator != null && animator.avatar != null && animator.avatar.isHuman)
                {
                    if (rb.transform == animator.GetBoneTransform(HumanBodyBones.LeftLowerArm) ||
                        rb.transform == animator.GetBoneTransform(HumanBodyBones.RightLowerArm) ||
                        rb.transform == animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg) ||
                        rb.transform == animator.GetBoneTransform(HumanBodyBones.RightLowerLeg) ||
                        rb.transform == animator.GetBoneTransform(HumanBodyBones.LeftHand) ||
                        rb.transform == animator.GetBoneTransform(HumanBodyBones.RightHand) ||
                        rb.transform == animator.GetBoneTransform(HumanBodyBones.LeftFoot) ||
                        rb.transform == animator.GetBoneTransform(HumanBodyBones.RightFoot))
                    {
                        keepJoint = true;
                    }
                }
                
                string n = rb.gameObject.name.ToLower();
                if (n.Contains("calf") || n.Contains("knee") || n.Contains("lowerleg") || n.Contains("lower_leg") ||
                    n.Contains("forearm") || n.Contains("elbow") || n.Contains("lowerarm") || n.Contains("lower_arm") ||
                    n.Contains("hand") || n.Contains("foot"))
                {
                    keepJoint = true;
                }

                if (!keepJoint)
                {
                    // Destroy joints to dislocate the pieces
                    Joint[] joints = rb.GetComponents<Joint>();
                    foreach (var j in joints)
                    {
                        Destroy(j);
                    }

                    // Unparent to ensure no animator or root script affects them
                    rb.transform.SetParent(null);
                    
                    // Since they are unparented, ensure they get destroyed after 5 seconds
                    Destroy(rb.gameObject, 5f);
                }
            }
        }

        // Add a shatter/death impulse
        try
        {
            if (animator != null && animator.avatar != null && animator.avatar.isHuman)
            {
                Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
                Rigidbody spineRb = spine != null ? spine.GetComponent<Rigidbody>() : null;
                if (spineRb != null)
                    spineRb.AddForce(transform.forward * -3f + Vector3.up * 2f, ForceMode.Impulse);
            }
            else if (ragdollBodies != null && ragdollBodies.Length > 0)
            {
                // Fallback: apply explosion to all child rigidbodies to make them scatter
                Vector3 explosionPos = transform.position + Vector3.down * 0.5f;
                foreach (var rb in ragdollBodies)
                {
                    if (rb != null && rb.gameObject != this.gameObject)
                    {
                        rb.AddExplosionForce(150f, explosionPos, 5f, 1f, ForceMode.Impulse);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Ragdoll impulse failed: " + e.Message);
        }
    }
}