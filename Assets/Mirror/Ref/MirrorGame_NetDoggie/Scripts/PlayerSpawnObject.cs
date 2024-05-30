using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class PlayerSpawnObject : NetworkBehaviour
{
    [Header("Components")]
    public NavMeshAgent NavAgent_Player;
    public Animator Animator_Player;
    public TextMesh TextMesh_HealthBar;
    public TextMesh TextMesh_NetType;
    public Transform Transform_Player;
    [Header("Movement")]
    public float _ratationSpeed = 100.0f;  // _ 붙으면 전역으로 살아있는 애 

    [Header("Attack")]
    public KeyCode _atkKey = KeyCode.Space;
    public GameObject Prefab_AtkObject;
    public Transform Transform_AtkSpawnPos;

    [Header("Stats Server")]
    [SyncVar] public int _health = 4;

    public void Update()
    {
        SetHealthBarOnUpdate(_health);
        if (CheckIsFocusedOnUpdate() == false)
            return;
        CheckIsLocalPlayerOnUpdate();
    }
    private void SetHealthBarOnUpdate(int health)
    {
        TextMesh_HealthBar.text = new string('-', health);//원래는 게터세터로 한다고 함 
    }
    bool CheckIsFocusedOnUpdate()
    {
        return Application.isFocused;
    }
    void CheckIsLocalPlayerOnUpdate()
    {

    }
    //클라에서 서버로 호출은 하지만 로직의 동작은 서버사이트 온리
    [Command]

    private void CommandAtk()
    {

    }

    [ClientRpc]
    private void RpcOnAtk()
    {

    }
    //클라에서 다음함수가 실행되지 않도록 ServerCallback을 달아줌
    [ServerCallback]
    //클라에서 다음함수가 실행되지 않도록 ServerCallback을 달아줌
    private void OnTriggerEnter(Collider other) //서버 클라 둘다 불러질 수 있어서 클라에서는 실행되지 않도록 하기 위함
    {
        
    }


}
