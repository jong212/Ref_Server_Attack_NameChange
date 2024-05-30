using UnityEngine;
using Mirror;
using JetBrains.Annotations;
using System.Security.Cryptography;
public class AttackSpawnObject : NetworkBehaviour
{
    public float _destoryAfter = 10.0f;
    public float _force = 1000;
    public Rigidbody RigidBody_AtkObject;

    //스타트인데 서버에서만 불려지는 
    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), _destoryAfter);
    }
    private void Start()
    {
        RigidBody_AtkObject.AddForce(transform.forward * _force);
    }
    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(this.gameObject);
    }
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        DestroySelf();
        
    }
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        
    }
}
