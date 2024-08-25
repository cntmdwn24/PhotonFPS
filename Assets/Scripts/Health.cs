using UnityEngine;
using Photon.Pun;
using TMPro;

public class Health : MonoBehaviour
{
    public int health;
    public bool isLocalPlayer;

    [Header("UI")]
    public TMP_Text healthText;

    [PunRPC]
    public void TakeDamage(int _damage)
    {
        health -= _damage;

        healthText.text = _damage.ToString();

        if (health <= 0)
        {
            if (isLocalPlayer)
            {
                RoomManager.instance.SpawnPlayer();
            }

            RoomManager.instance.deaths++;
            RoomManager.instance.SetHashes();

            Destroy(gameObject);
        }
    }
}
