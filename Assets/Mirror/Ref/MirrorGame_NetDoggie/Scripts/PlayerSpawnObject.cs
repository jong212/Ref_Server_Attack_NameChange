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
    public float _ratationSpeed = 100.0f;  // _ ������ �������� ����ִ� �� 

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
        TextMesh_HealthBar.text = new string('-', health);//������ ���ͼ��ͷ� �Ѵٰ� �� 
    }
    bool CheckIsFocusedOnUpdate()
    {
        return Application.isFocused;
    }
    void CheckIsLocalPlayerOnUpdate() //����Ʈ �÷��̾�� ���� �� ����
    {
        //�̰� ���� �ٸ� �÷��̾ �� ȸ����...
        if (this.isLocalPlayer == false)
            return;

        // ���� �÷��̾��� ȸ��
        float horizontal = Input.GetAxis("Horizontal");
        transform.Rotate(0, horizontal * _ratationSpeed * Time.deltaTime, 0);

        // ���� �÷��̾��� �̵�
        float vetrial = Input.GetAxis("Vertical");
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        NavAgent_Player.velocity = forward * Mathf.Max(vetrial, 0) * NavAgent_Player.speed;
        Animator_Player.SetBool("Moving", NavAgent_Player.velocity != Vector3.zero);

        // ����
        if (Input.GetKeyDown(_atkKey))
        {
            CommandAtk();
        }
        RotateLocalPlayer();
    }

    void RotateLocalPlayer()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray,out RaycastHit hit, 100))
        {
            Debug.DrawLine(ray.origin, hit.point);
            Vector3 lookRotate = new Vector3(hit.point.x, Transform_Player.position.y, hit.point.z);
            Transform_Player.LookAt(lookRotate);
        }
    }
    //Ŭ�󿡼� ������ ȣ���� ������ ������ ������ ��������Ʈ �¸�
    [Command]
    //�������̵忡��(��Ʈ��ũ ������ ����) ������ ������..
    private void CommandAtk()
    {
        GameObject atkObjectForSpawn = Instantiate(Prefab_AtkObject, Transform_AtkSpawnPos.transform.position, Transform_AtkSpawnPos.transform.rotation);
        NetworkServer.Spawn(atkObjectForSpawn); // ������ ������Ʈ�� ������ ����ȭ�� �Ǿ�� �ϴµ� Ŭ�� �Ӹ� �ƴ϶� ����
    }

    [ClientRpc]//���� �������� �ľ��ؾ��� > ������ �� ���� RPC�Լ��� ȣ���ؼ� �� ��
    private void RpcOnAtk() 
    {
        Debug.LogWarning($"{this.netId}�� RPC ��");
        Animator_Player.SetTrigger("Atk");
        // TODO
    }
    //Ŭ�󿡼� �����Լ��� ������� �ʵ��� ServerCallback�� �޾���
    [ServerCallback]
    //Ŭ�󿡼� �����Լ��� ������� �ʵ��� ServerCallback�� �޾���
    private void OnTriggerEnter(Collider other) //���� Ŭ�� �Ѵ� �ҷ��� �� �־ Ŭ�󿡼��� ������� �ʵ��� �ϱ� ����
    {
        var atkGenObject = other.GetComponent<>();
        if (atkGenObject == null)
            return;

        _health--;
        if(_health == 0)
        {
            NetworkServer.Destroy(this.gameObject);
        }
        // TODO
    }


}