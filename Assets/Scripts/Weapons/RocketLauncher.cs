using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : WeaponBase
{
    [Header("Rocket Settings")]
    public GameObject rocketPrefab;
    public float rocketSpeed = 30f;

    protected override void Awake()
    {
        base.Awake();

        // Weapon identity
        isAutomatic = false;

        damage = 100f;
        fireRate = 1.2f;
        range = 200f;

        magazineSize = 1;
        reserveAmmo = 10;
        reloadTime = 2.5f;

        recoilMin = new Vector3(-2f, -100f, 0f);
        recoilMax = new Vector3(2f, -120f, 0f);

        kickbackAmount = 0.4f;
        recoilKickAngle = 11f;
        maxRecoilAngle = 30f;
        recoilAngleRecoverySpeed = 12f;
    }

    protected override void Fire()
    {
        Camera camera = Camera.main;
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 direction = ray.direction;

        Debug.Log("Rocket spawn: " + firePoint.position);

        GameObject rocket = Instantiate(
            rocketPrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        RocketProjectile projectile = rocket.GetComponent<RocketProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction, rocketSpeed, damage);
        }
    }
}
