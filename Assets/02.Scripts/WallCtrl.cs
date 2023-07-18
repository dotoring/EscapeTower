using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCtrl : MonoBehaviour
{
    //스파크 파티클 프리팹 연결할 변수
    public GameObject sparkEffect;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //충돌이 시작할 때 발생하는 이벤트
    void OnCollisionEnter(Collision coll)
    {
        //충돌한 게임오브젝트의 태그값 비교
        if(coll.collider.tag == "BULLET")
        {
            //스파크 파티클을 동적으로 생성
            GameObject spark = (GameObject)Instantiate(sparkEffect, coll.transform.position, Quaternion.identity);

            //ParticleSystem 컴포넌트의 수행시간(duration)이 지난 후 삭제 처리
            Destroy(spark, spark.GetComponent<ParticleSystem>().main.duration + 0.2f);

            //충돌한 게임오브젝트 삭제
            Destroy(coll.gameObject);
        }
    }
}
