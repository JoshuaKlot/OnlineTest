using UnityEngine;
using Unity.Netcode;
public class AudioManager : NetworkBehaviour
{
    public static AudioManager Instance;


    [SerializeField] private AudioSource CursorPhase;
    [SerializeField] private AudioSource PlayerPhase;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayWaiting()
    {
        PlayWaitingClientRpc();
    }

    public void PlayPlaying()
    {
        PlayPlayingClientRpc();
    }

    public void StopPlaying()
    {
        QuietClientRpc();
    }
    [ClientRpc]

    void PlayWaitingClientRpc()
    {
        MuteAll();
        CursorPhase.Play();
    }

    [ClientRpc]

    void PlayPlayingClientRpc()
    {
        MuteAll();
        PlayerPhase.Play();
    }
    [ClientRpc]
    void QuietClientRpc()
    {
        MuteAll();
    }
    private void MuteAll()
    {
        CursorPhase.Stop();
        PlayerPhase.Stop();
    }
}
