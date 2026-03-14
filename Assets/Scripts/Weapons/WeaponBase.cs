using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Behavior")]
    public bool usesAmmo = true;

    [Header("Weapon Stats")]
    public float damage = 25;
    public float fireRate = 0.5f;
    public float range = 100f;
    public bool isAutomatic = false;

    [Header("Ammo Settings")]
    public int magazineSize = 12;
    public int reserveAmmo = 36;
    public float reloadTime = 1.5f;

    [Header("Recoil Settings")]
    public Vector3 recoilMin = new Vector3(-0.5f, -15f, 0f);
    public Vector3 recoilMax = new Vector3(0.5f, -20f, 0f);

    [Header("Motion Type")]
    public WeaponMotionType motionType = WeaponMotionType.Gun;

    [Header("Gun Kickback")]
    public float kickbackAmount = 0.15f;
    public float kickbackRecoverySpeed = 8f;
    public Transform recoilPivot;
    public float recoilKickAngle = 3.5f;
    public float maxRecoilAngle = 12f;
    public float recoilAngleRecoverySpeed = 16f;
    Vector3 originalLocalPosition;
    Quaternion originalRecoilPivotLocalRotation;
    float currentRecoilAngle;

    [Header("Melee Swing")]
    public float swingAngle = 60f;
    public float swingSpeed = 12f;
    public Vector3 meleeWindupEuler = new Vector3(0f, 0f, 25f);
    public Vector3 meleeSwingEuler = new Vector3(0f, 0f, -85f);
    public float meleeWindupTime = 0.05f;
    public float meleeAttackTime = 0.06f;
    public float meleeRecoverTime = 0.14f;
    public float meleeLungeDistance = 0.18f;
    Quaternion originalLocalRotation;
    bool isSwinging = false;

    [Header("References")]
    public Transform firePoint;
    public CameraRecoil cameraRecoil;

    protected float lastShotTime = -999f;
    protected int currentAmmo;
    protected bool isReloading = false;

    protected bool reloadCancelledThisFrame = false;
    protected Coroutine reloadCoroutine;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        if (usesAmmo)
        {
            currentAmmo = magazineSize;
        }

        if (recoilPivot == null)
        {
            recoilPivot = transform;
        }

        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalRecoilPivotLocalRotation = recoilPivot.localRotation;
    }

    void Update()
    {
        HandleInput();

        if (motionType == WeaponMotionType.Gun)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalLocalPosition,
                kickbackRecoverySpeed * Time.deltaTime
            );

            currentRecoilAngle = Mathf.MoveTowards(
                currentRecoilAngle,
                0f,
                recoilAngleRecoverySpeed * Time.deltaTime
            );

            Quaternion targetRecoilRotation =
                originalRecoilPivotLocalRotation *
                Quaternion.Euler(-currentRecoilAngle, 0f, 0f);

            recoilPivot.localRotation = Quaternion.Slerp(
                recoilPivot.localRotation,
                targetRecoilRotation,
                recoilAngleRecoverySpeed * Time.deltaTime
            );
        }
    }

    protected virtual void HandleInput()
    {
        if (usesAmmo && Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            reloadCoroutine = StartCoroutine(Reload());
            return;
        }

        if (isAutomatic)
        {
            if (Input.GetMouseButton(0))
            {
                TryFire();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryFire();
            }
        }
    }

    protected void TryFire()
    {
        if (isReloading)
        {
            CancelReload();
        }

        if (usesAmmo && currentAmmo <= 0)
        {
            if (reserveAmmo > 0 && !reloadCancelledThisFrame)
            {
                reloadCoroutine = StartCoroutine(Reload());
            }
            else
            {
                Debug.Log("Click! (Dry Fire)");
            }

            reloadCancelledThisFrame = false;
            return;
        }

        if (Time.time >= lastShotTime + fireRate)
        {
            lastShotTime = Time.time;
            if (usesAmmo)
            {
                currentAmmo--;
            }

            // Weapon attack animation
            if (motionType == WeaponMotionType.Gun)
            {
                if (cameraRecoil != null)
                {
                    Vector3 recoil = new Vector3(
                        Random.Range(recoilMin.x, recoilMax.x),
                        Random.Range(recoilMin.y, recoilMax.y),
                        0f
                    );

                    cameraRecoil.AddRecoil(recoil);
                }

                transform.localPosition -= Vector3.forward * kickbackAmount;
                currentRecoilAngle = Mathf.Clamp(
                    currentRecoilAngle + recoilKickAngle,
                    0f,
                    maxRecoilAngle
                );
            }
            else if (motionType == WeaponMotionType.Melee)
            {
                if (!isSwinging)
                {
                    StartCoroutine(PerformSwing());
                }
            }


            Fire();
        }
    }

    protected IEnumerator Reload()
    {
        if (!usesAmmo)
        {
            yield break; // This weapon doesn't use ammo
        }

        if (currentAmmo == magazineSize)
        {
            yield break; // Magazine is already full
        }

        if (reserveAmmo <= 0)
        {
            yield break; // No reserve ammo to reload
        }

        isReloading = true;
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = magazineSize - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);

        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;

        isReloading = false;
        Debug.Log("Reloaded. Ammo: " + currentAmmo + "/" + reserveAmmo);
    }

    protected void CancelReload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        isReloading = false;
        reloadCancelledThisFrame = true;

        Debug.Log("Reload Cancelled");
    }

    IEnumerator PerformSwing()
    {
        isSwinging = true;

        Quaternion startRot = originalLocalRotation;

        Quaternion windupRot =
            originalLocalRotation *
            Quaternion.Euler(meleeWindupEuler);
        Quaternion swingRot =
            originalLocalRotation *
            Quaternion.Euler(meleeSwingEuler);

        float t = 0;

        // WINDUP (raise knife)
        while (t < meleeWindupTime)
        {
            t += Time.deltaTime;
            float progress = t / meleeWindupTime;

            transform.localPosition = originalLocalPosition;
            transform.localRotation =
                Quaternion.Slerp(startRot, windupRot, progress);

            yield return null;
        }

        t = 0;

        // ATTACK (downward slash)
        while (t < meleeAttackTime)
        {
            t += Time.deltaTime;
            float progress = t / meleeAttackTime;

            transform.localPosition =
                originalLocalPosition + Vector3.forward * meleeLungeDistance;

            transform.localRotation =
                Quaternion.Slerp(windupRot, swingRot, progress);

            yield return null;
        }

        t = 0;

        // RECOVERY (slower)
        while (t < meleeRecoverTime)
        {
            t += Time.deltaTime;
            float progress = t / meleeRecoverTime;

            transform.localPosition = originalLocalPosition;

            transform.localRotation =
                Quaternion.Slerp(swingRot, startRot, progress);

            yield return null;
        }

        transform.localRotation = startRot;
        isSwinging = false;
    }

    protected abstract void Fire();
}
