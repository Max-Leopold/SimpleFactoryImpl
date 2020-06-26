using SimpleFactoryServerLib.Network.Messages.Players;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class NetworkPlayer : MonoBehaviour
{

    public string username;
    public int userId;
    public NetworkController networkController;

    private bool _isRemotePlayer = false;
    private float networkThreshold = .15f; // 100 ms
    private float networkUpdateSlowdown = 0;
    

    public bool IsRemotePlayer
    {
        get => _isRemotePlayer;
        set
        {
            if (value)
            {
                GetComponent<FirstPersonController>().enabled = false;
                transform.Find("FirstPersonCharacter").gameObject.SetActive(false);
                GetComponent<InventoryManager>().enabled = false;
                GetComponent<AudioSource>().enabled = false;
                GetComponent<CharacterController>().enabled = false;
                Destroy(GetComponent<Rigidbody>());
            }else{
                // Disable the eyes
                transform.Find("Eyes").gameObject.SetActive(false);
            }
            _isRemotePlayer = value;
        }
    }
    private Vector3 lastPos = Vector3.zero;
    private Quaternion lastRot = Quaternion.identity;
    
    private Vector3 remoteTargetPos = Vector3.zero;
    
    public void updatePosition(PlayerPositionUpdate update)
    {
        if (_isRemotePlayer && update.userID == userId)
        {
            remoteTargetPos = new Vector3(update.position.x, update.position.y, update.position.z);
            transform.rotation = Quaternion.Euler(new Vector3(update.rotation.x, update.rotation.y, update.rotation.z));
        }
    }

    private void Update()
    {
        if (!_isRemotePlayer)
        {
            bool updated = false;
            if (lastPos != transform.position)
            {
                lastPos = transform.position;
                updated = true;
            }
            if (lastRot != transform.rotation)
            {
                lastRot = transform.rotation;
                updated = true;
            }

            if (networkUpdateSlowdown < networkThreshold)
            {
                networkUpdateSlowdown += Time.deltaTime;
            }

            if (updated && networkUpdateSlowdown >= networkThreshold && networkController != null)
            {
                networkUpdateSlowdown = 0;
                networkController.sendToServerUdp(new PlayerPositionUpdate(networkController.convertVec3ToPosition(lastPos), networkController.convertRotToQuaternion(lastRot)));
                Debug.Log("Send To Server is called");
            }   
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, remoteTargetPos, Time.deltaTime * 10);
        }
    }
}
