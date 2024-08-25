using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Photon.Pun.UtilityScripts;

public class Weapon : MonoBehaviour
{
    public int damage;
    public Camera camera;
    public float fireRate;

    [Header("VFX")]
    public GameObject hitVFX;

    private float nextFire;

    [Header("Ammo")]
    public int mag = 5;
    public int ammo = 30;
    public int magAmmo = 30;

    [Header("UI")]
    public TMP_Text magText;
    public TMP_Text ammoText;

    [Header("Animation")]
    public Animation animation;
    public AnimationClip reload;

    [Header("Recoil Settings")]
    public float recoilUp = 1f;
    public float recoilBack = 0f;
    public float recoilDuration = 0.1f;
    public float recoverDuration = 0.2f;

    private Vector3 originalPosition;
    private Vector3 recoilVelocity = Vector3.zero;

    private float recoilLength;
    private float recoverLength;

    private bool recoiling;
    private bool recovering;

    void Start()
    {
        if (fireRate <= 0)
        {
            fireRate = 1f;
            Debug.LogWarning("fireRate was set to a non-positive value. Reset to 1f.");
        }

        recoilLength = recoilDuration;
        recoverLength = recoverDuration;

        originalPosition = transform.localPosition;

        UpdateUI();
    }

    void Update()
    {
        if (nextFire > 0)
        {
            nextFire -= Time.deltaTime;
        }

        if (Input.GetButton("Fire1") && nextFire <= 0 && ammo > 0 && (animation == null || !animation.isPlaying))
        {
            nextFire = 1 / fireRate;
            ammo--;
            UpdateUI();
            Fire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }

        if (recoiling)
        {
            Recoil();
        }

        if (recovering)
        {
            Recovering();
        }
    }

    void Reload()
    {
        if (ammo == magAmmo || (animation != null && animation.isPlaying))
        {
            return;
        }

        if (animation != null && reload != null)
        {
            animation.Play(reload.name);

            if (mag > 0)
            {
                mag--;
                ammo = magAmmo;
                UpdateUI();
            }
        }
        else
        {
            Debug.LogWarning("Animation or reload clip is not assigned!");
        }
    }

    void Fire()
    {
        recoiling = true;
        recovering = false;

        Ray ray = new Ray(camera.transform.position, camera.transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            PhotonNetwork.Instantiate(hitVFX.name, hit.point, Quaternion.identity);

            Health health = hit.transform.gameObject.GetComponent<Health>();
            PhotonView photonView = hit.transform.gameObject.GetComponent<PhotonView>();

            if (health != null && photonView != null)
            {

                int remainingHealth = health.health - damage;

                if (remainingHealth <= 0)
                {
                    RoomManager.instance.kills++;
                    RoomManager.instance.SetHashes();

                    PhotonNetwork.LocalPlayer.AddScore(100);
                }

                photonView.RPC("TakeDamage", RpcTarget.All, damage);
            }
        }
    }


    void Recoil()
    {
        Vector3 finalPosition = new Vector3(originalPosition.x, originalPosition.y + recoilUp, originalPosition.z - recoilBack);

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoilLength);

        if (Vector3.Distance(transform.localPosition, finalPosition) < 0.05f)
        {
            recoiling = false;
            recovering = true;
        }
    }

    void Recovering()
    {
        Vector3 finalPosition = originalPosition;

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoverLength);

        if (Vector3.Distance(transform.localPosition, finalPosition) < 0.05f)
        {
            recovering = false;
        }
    }

    void UpdateUI()
    {
        if (magText != null)
        {
            magText.text = mag.ToString();
        }
        if (ammoText != null)
        {
            ammoText.text = ammo + "/" + magAmmo;
        }
    }
}