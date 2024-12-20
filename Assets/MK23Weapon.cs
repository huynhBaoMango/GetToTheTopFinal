﻿using DG.Tweening;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class MK23Weapon : APlayerWeapon
{
    float currentDelayBullet = 0;
    bool isReloading;
    [SerializeField] private GameObject explosionImpactPref;

    [Header("Sounds")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    Coroutine lastRoutine = null;

    private void Awake()
    {
        currentAmmo = maxAmmo;
        ammoText = GameObject.FindWithTag("AmmoText").GetComponent<TextMeshProUGUI>();
        ammoText.text = null;
        UpdateAmmoDisplay();
    }
    public override void AnimateWeapon()
    {
        //anim luc ban sung
        AnimateWeaponServer();
        Instantiate(muzzleFlash, muzzleTransform.position, transform.rotation);
        transform.DOLocalMove(new Vector3(transform.localPosition.x, transform.localPosition.y - 0.01f, transform.localPosition.z - 0.07f), 0.001f).OnComplete(() =>
        {
            transform.DOLocalMove(new Vector3(transform.localPosition.x, transform.localPosition.y + 0.01f, transform.localPosition.z + 0.07f), 0.1f).SetEase(Ease.OutBack);
        });
    }

    [ServerRpc(RequireOwnership = false)]
    void AnimateWeaponServer()
    {
        AnimateWeaponObserver();
    }

    [ObserversRpc(ExcludeOwner = true)]
    void AnimateWeaponObserver()
    {
        transform.DOLocalMove(new Vector3(transform.localPosition.x, transform.localPosition.y - 0.01f, transform.localPosition.z - 0.07f), 0.001f).OnComplete(() =>
        {
            Instantiate(muzzleFlash, muzzleTransform.position, transform.rotation);
            transform.DOLocalMove(new Vector3(transform.localPosition.x, transform.localPosition.y + 0.01f, transform.localPosition.z + 0.07f), 0.1f).SetEase(Ease.OutBack);
        });
    }

    public void KeepMagInHand()
    {
        MagPref.transform.position = LeftHandIKTarget.position;
    }

    public override void Reload()
    {
        if (!isReloading)
        {
            gameObject.GetComponent<AudioSource>().PlayOneShot(reloadSound);
            isReloading = true;
            ReloadServer();
            //xu li tay
            LeftHandIKTarget.rotation = magHoldPos.rotation;
            LeftHandIKTarget.DOLocalMove(magHoldPos.transform.localPosition, 0.5f).OnComplete(() =>
            {
                InvokeRepeating("KeepMagInHand", 0f, 0.001f);
                LeftHandIKTarget.rotation = reloadPos.rotation;
                LeftHandIKTarget.DOLocalMove(reloadPos.transform.localPosition, 1f).OnComplete(() =>
                {
                    LeftHandIKTarget.rotation = magHoldPos.rotation;
                    LeftHandIKTarget.DOLocalMove(magHoldPos.localPosition, 0.5f).OnComplete(() =>
                    {
                        MagPref.transform.position = MagPos.position;
                        CancelInvoke("KeepMagInHand");
                        LeftHandIKTarget.DOLocalMove(tempLeftHandIK.localPosition, 1f);
                        LeftHandIKTarget.rotation = tempLeftHandIK.rotation;
                        if (maxAmmo + currentAmmo >= 9)
                        {
                            maxAmmo = maxAmmo + currentAmmo - 9;
                            currentAmmo = 9;
                        }
                        else
                        {
                            currentAmmo = currentAmmo + maxAmmo;
                            maxAmmo = 0;
                        }

                        UpdateAmmoDisplay();
                        isReloading = false;
                    });
                });
            });

            //xu li sung
            transform.DOLocalRotate(new Vector3(transform.localRotation.x - 75f, transform.localRotation.y, transform.localRotation.z), 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                transform.DOLocalRotate(new Vector3(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z), 1f).SetEase(Ease.OutBack).SetDelay(1.5f);
            });
            transform.DOLocalMove(new Vector3(transform.localPosition.x, transform.localPosition.y + 0.2f, transform.localPosition.z - 0.07f), 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                transform.DOLocalMove(new Vector3(transform.localPosition.x, transform.localPosition.y - 0.2f, transform.localPosition.z + 0.07f), 1f).SetEase(Ease.OutBack).SetDelay(1.5f);
            });
        }

    }

    [ServerRpc(RequireOwnership = false)]
    void ReloadServer()
    {
        ReloadObserver();
    }

    [ObserversRpc(ExcludeOwner = true)]
    void ReloadObserver()
    {
        LeftHandIKTarget.rotation = magHoldPos.rotation;
        LeftHandIKTarget.DOLocalMove(magHoldPos.transform.localPosition, 0.5f).OnComplete(() =>
        {
            InvokeRepeating("KeepMagInHand", 0f, 0.001f);
            LeftHandIKTarget.rotation = reloadPos.rotation;
            LeftHandIKTarget.DOLocalMove(reloadPos.transform.localPosition, 1f).OnComplete(() =>
            {
                LeftHandIKTarget.rotation = magHoldPos.rotation;
                LeftHandIKTarget.DOLocalMove(magHoldPos.localPosition, 0.5f).OnComplete(() =>
                {
                    MagPref.transform.position = MagPos.position;
                    CancelInvoke("KeepMagInHand");
                    LeftHandIKTarget.DOLocalMove(tempLeftHandIK.localPosition, 1f);
                    LeftHandIKTarget.rotation = tempLeftHandIK.rotation;
                });
            });
        });

        //xu li sung
        transform.DOLocalRotate(new Vector3(transform.localRotation.x - 75f, transform.localRotation.y, transform.localRotation.z), 0.5f).SetEase(Ease.InBack).OnComplete(() =>
        {
            transform.DOLocalRotate(new Vector3(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z), 1f).SetEase(Ease.OutBack).SetDelay(1.5f);
        });
        transform.DOLocalMove(new Vector3(transform.localPosition.x, transform.localPosition.y + 0.2f, transform.localPosition.z - 0.07f), 0.5f).SetEase(Ease.InBack).OnComplete(() =>
        {
            transform.DOLocalMove(new Vector3(transform.localPosition.x, transform.localPosition.y - 0.2f, transform.localPosition.z + 0.07f), 1f).SetEase(Ease.OutBack).SetDelay(1.5f);
        });
    }

    public override void Fire()
    {
        if (IsOwner)
        {
            currentDelayBullet -= Time.deltaTime;
            if (currentDelayBullet <= 0 && currentAmmo > 0 && !isReloading)
            {
                AnimateWeapon();
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Ray ray = Camera.main.ScreenPointToRay(screenCenter);
                gameObject.GetComponent<AudioSource>().PlayOneShot(fireSound);

                if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
                {

                    if (hit.collider.TryGetComponent<ZombieHealth>(out ZombieHealth zombieHealth))
                    {
                        zombieHealth.TakeDamage(damage);
                        SpawnImpactEffect(hit.point, hit.normal, bloodImpactPref);

                        if (zombieHealth._currentHealth <= damage && zombieHealth.isAlive)
                        {
                            transform.root.GetComponent<PlayerMoney>().ChangeCurrentMoney(20);
                        }
                    }
                    else if (hit.collider.TryGetComponent<GasTank>(out GasTank gastank)) { gastank.TakeDamage(damage); SpawnImpactEffect(hit.point, hit.normal, explosionImpactPref); }
                    else
                    {
                        Debug.Log($"Hit: {hit.collider.gameObject.name}");
                        SpawnImpactEffect(hit.point, hit.normal, norImpactPref);
                    }
                }
                currentAmmo -= 1;
                UpdateAmmoDisplay();
                currentDelayBullet = delayBulletTime;
            }
            if (currentAmmo <= 0)
            {
                Reload();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnImpactEffect(Vector3 hitPoint, Vector3 hitNormal, GameObject impactEffectPrefab)
    {
        SpawnImpactEffectObserver(hitPoint, hitNormal, impactEffectPrefab);
    }

    [ObserversRpc]
    private void SpawnImpactEffectObserver(Vector3 hitPoint, Vector3 hitNormal, GameObject impactEffectPrefab)
    {
        if (impactEffectPrefab != null)
        {
            GameObject impactEffect = Instantiate(impactEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            ServerManager.Spawn(impactEffect);
            Destroy(impactEffect, 2f);
        }
    }
}
