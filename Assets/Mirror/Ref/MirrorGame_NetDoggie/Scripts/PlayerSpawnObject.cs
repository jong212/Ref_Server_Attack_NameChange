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
    [SyncVar(hook = nameof(OnMyNameChanged))] public string _myName = "string_empty";

    public void Update()
    {
        //    string netTypeStr = isClient ? "클라" : "클라아님";

        //    TextMesh_NetType.text = this.isLocalPlayer ? $"[로컬/{netTypeStr}]{this.netId}" 
        //        : $"[로컬아님/{netTypeStr}]{this.netId}";

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
    void CheckIsLocalPlayerOnUpdate() //리무트 플레이어는 여기 안 들어옴
    {
        // 1.이거 빼면 내 화면의 다른 참가자 플레이어도 같이 움직임....
        // 그래서 이 함수는 자기 자신만 움직일 수 있게끔 해야하기 때문에 isLocalPlayer가 아니면 false를 시킴 
        // 왜 로컬에서만 수행되어야 하냐면 이걸 빼면 유니티 화면에서 다른 플레이어도 같이 움직이기 때문... 
        // 여기서 드는 의문이 창1에서의 A라는 로컬플레이어가 움직일 때 창2 에서의 A는 로컬이 아니라 리무트 플레이어일텐데 어떻게 움직이는거지 했는데 Network Transform 컴포넌트 덕분이였음!!
        Debug.Log(isLocalPlayer);
        if (this.isLocalPlayer == false)
            return;


        // 로컬 플레이어의 회전
        float horizontal = Input.GetAxis("Horizontal");
        transform.Rotate(0, horizontal * _ratationSpeed * Time.deltaTime, 0);

        // 포컬 플레이어의 이동
        float vetrial = Input.GetAxis("Vertical");
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        NavAgent_Player.velocity = forward * Mathf.Max(vetrial, 0) * NavAgent_Player.speed;
        Animator_Player.SetBool("Moving", NavAgent_Player.velocity != Vector3.zero);

        // 공격
        if (Input.GetKeyDown(_atkKey))
        {
            ChangeNameChilsoon("칠순이");
            CommandAtk();
        }
        RotateLocalPlayer();
    }
    [Command]
    void ChangeNameChilsoon(string name)
    {
        _myName = name;
       //RpcNameChanged(); 240530 동기화 타이밍 문제로 빌드된 창에서 이름이 변경되지 않는 이슈가 있어서 훅으로 처리함
    }


    /*[ClientRpc]
    void RpcNameChanged()
    {
        TextMesh_NetType.text = _myName;
    }*/
    void OnMyNameChanged(string oldName, string newName)
    {
        TextMesh_NetType.text = newName;
    }

    void RotateLocalPlayer()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            Debug.DrawLine(ray.origin, hit.point);
            Vector3 lookRotate = new Vector3(hit.point.x, Transform_Player.position.y, hit.point.z);
            Transform_Player.LookAt(lookRotate);
        }
    }
    //클라에서 서버로 호출은 하지만 로직의 동작은 서버사이트 온리
    [Command]
    //서버사이드에서(네트워크 서버의 스폰) 스폰을 해주자..
    private void CommandAtk()
    {
        GameObject atkObjectForSpawn = Instantiate(Prefab_AtkObject, Transform_AtkSpawnPos.transform.position, Transform_AtkSpawnPos.transform.rotation);
        NetworkServer.Spawn(atkObjectForSpawn); // 공격한 오브젝트가 서버에 동기화가 되어야 하는데 클라 뿐만 아니라 서버
        RpcOnAtk();
    }

    [ClientRpc]//언제 들어오는지 파악해야함 > 어택을 한 애의 RPC함수를 호출해서 쏴 줌
    private void RpcOnAtk()
    {
        Debug.LogWarning($"{this.netId}가 RPC 옴");
        Animator_Player.SetTrigger("Atk");
        // TODO
    }
    //클라에서 다음함수가 실행되지 않도록 ServerCallback을 달아줌
    [ServerCallback]
    //클라에서 다음함수가 실행되지 않도록 ServerCallback을 달아줌
    private void OnTriggerEnter(Collider other) //서버 클라 둘다 불러질 수 있어서 클라에서는 실행되지 않도록 하기 위함
    {
        var atkGenObject = other.GetComponent<AttackSpawnObject>();
        if (atkGenObject == null)
            return;

        _health--;
        if (_health == 0)
        {
            NetworkServer.Destroy(this.gameObject);
        }
        // TODO
    }


}
