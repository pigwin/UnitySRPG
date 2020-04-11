using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*******************************************************************/
/*カメラの位置や、向きに関する関数や処理を行うクラス               */
/*******************************************************************/
public class CameraAngle : MonoBehaviour {

    private static Vector3 goal_vector;
    private static Vector3 start_vector;
    private static bool _moveFlag = false;
    public float movetime = 0.2f;
    private static float now_movetime = 0;
    public static bool moveFlag
    {
        get { return _moveFlag; }
    }

    public int zoom_distance = 1;

    public int ANGLE = 30;
    public static GameObject _CamParent;

    private float temp;
    private float tempZ;
    
    private int rotflag = 0;
    public float rottime = 20f;
    private float now_rottime = 0;
    private float goal_rot = 0;


    private int zoomflag = 0;
    private int zoomZflag = 0;
    public float zoomtime = 20f;
    private float now_zoom = 0;
    private float now_zoomtime = 0;

    public float zoomZtime = 20f;
    public int zoomZ_distanse = 10;
    private float now_zoomZ = 0;
    private float now_zoomZtime = 0;

    private static int rotatemode = 0; //0->1->2->3->0->... //0,3の時はキー入力上下が反転、1,2の時はキー入力左右が反転

    private int _zoom_Min_Max = 10;

    private int zoom_Min_Max
    {
        get { return _zoom_Min_Max; }
        set { _zoom_Min_Max = Mathf.Clamp(value, 4, 13); }
    }

    private int _zoomZ_Min_Max = 0;

    private int zoomZ_Min_Max
    {
        get { return _zoomZ_Min_Max; }
        set { _zoomZ_Min_Max = Mathf.Clamp(value, -30, 30); }
    }

    public float Radian(float angle)
    {
        return angle * Mathf.PI / 180;
    }

    private Vector3 CameraPosition(float far)
    {
        Vector3 data = new Vector3(0, far * Mathf.Sin(Radian(90-ANGLE)), -far * Mathf.Cos(Radian(90-ANGLE)));
        return data;
    }

	// Use this for initialization
	void Start () {
        CamParent = transform.root.gameObject;
        CamParent.transform.Rotate(new Vector3(0,60,0));
        Camera.main.transform.localPosition = CameraPosition(_zoom_Min_Max);
        now_zoom = _zoom_Min_Max;
        now_zoomZ = _zoomZ_Min_Max;
        rotatemode = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //キー入力を一部状態に限定する
        if(BattleVal.status == STATUS.SETUP || BattleVal.status == STATUS.PLAYER_UNIT_SELECT 
          || BattleVal.status == STATUS.PLAYER_UNIT_MOVE || BattleVal.status == STATUS.PLAYER_UNIT_ATTACK
          || BattleVal.status == STATUS.PLAYER_UNIT_SKILL || BattleVal.status == STATUS.ENEMY_UNIT_SELECT)
        {
            if (Input.GetAxisRaw("RotateHorizontal") == -1 && rotflag == 0)
            {
                rotflag = 1;
                temp = 90 / (float)rottime * (float)rotflag;
                //注意:QuanternionとVector3の座標計算は異なる
                goal_rot = _CamParent.transform.rotation.eulerAngles.y + 90;
                rotatemode = (rotatemode + 1) % 4;
            }
            else if (Input.GetAxisRaw("RotateHorizontal") == 1 && rotflag == 0)
            {
                rotflag = -1;
                temp = 90 / (float)rottime * (float)rotflag;
                goal_rot = _CamParent.transform.rotation.eulerAngles.y - 90;
                rotatemode = (rotatemode + 3) % 4;
            }
            else if (Input.GetAxisRaw("RotateVertical") == 1 && zoomflag == 0)
            {
                int zoom_times = zoom_Min_Max;
                zoom_Min_Max -= zoom_distance;
                if (zoom_times != zoom_Min_Max)
                {
                    zoomflag = -1;
                    temp = zoom_distance / (float)zoomtime * (float)zoomflag;
                }
            }
            else if (Input.GetAxisRaw("RotateVertical") == -1 && zoomflag == 0)
            {
                int zoom_times = zoom_Min_Max;
                zoom_Min_Max += zoom_distance;
                if (zoom_times != zoom_Min_Max)
                {
                    zoomflag = 1;
                    temp = zoom_distance / (float)zoomtime * (float)zoomflag;
                }
            }
            else if (Input.GetAxisRaw("Lookup") == 1 && zoomZflag == 0)
            {
                int zoom_times = zoomZ_Min_Max;
                zoomZ_Min_Max += zoomZ_distanse;
                if (zoom_times != zoomZ_Min_Max)
                {
                    zoomZflag = 1;
                    tempZ = zoomZ_distanse / (float)zoomtime * (float)zoomZflag;
                }
            }
            else if (Input.GetAxisRaw("Lookup") == -1 && zoomZflag == 0)
            {
                int zoom_times = zoomZ_Min_Max;
                zoomZ_Min_Max -= zoomZ_distanse;
                if (zoom_times != zoomZ_Min_Max)
                {
                    zoomZflag = -1;
                    tempZ = zoomZ_distanse / (float)zoomtime * (float)zoomZflag;
                }
            }
        }
        
        if (rotflag != 0)
        {
            if (now_rottime >= rottime - Time.deltaTime)
            {
                transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, goal_rot, 0));
                //Debug.Log(goal_rot);
                rotflag = 0;
                now_rottime = 0;
            }
            else
            {
                now_rottime += Time.deltaTime;
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + temp*Time.deltaTime, 0);
            }
        }
        if(zoomflag != 0)
        {
            if(now_zoomtime >= zoomtime - Time.deltaTime)
            {
                Camera.main.transform.localPosition=(new Vector3(0, zoom_Min_Max * Mathf.Sin(Radian(90-ANGLE)), -zoom_Min_Max * Mathf.Cos(Radian(90-ANGLE))));
                zoomflag = 0;
                now_zoomtime = 0;
                now_zoom = zoom_Min_Max;
            }
            else
            {
                now_zoomtime += Time.deltaTime;
                now_zoom += temp * Time.deltaTime;
                Camera.main.transform.localPosition = (new Vector3(0, now_zoom * Mathf.Sin(Radian(90-ANGLE)), -now_zoom * Mathf.Cos(Radian(90-ANGLE))));
            }
        }
        if (zoomZflag != 0)
        {
            if (now_zoomZtime >= zoomtime - Time.deltaTime)
            {
                CamParent.transform.rotation = Quaternion.Euler(zoomZ_Min_Max, transform.rotation.eulerAngles.y, 0);
                zoomZflag = 0;
                now_zoomZtime = 0;
                now_zoomZ = zoomZ_Min_Max;
            }
            else
            {
                now_zoomZtime += Time.deltaTime;
                now_zoomZ += tempZ * Time.deltaTime;
                CamParent.transform.rotation = Quaternion.Euler(now_zoomZ, transform.rotation.eulerAngles.y, 0);
            }
        }

        if (_moveFlag)
        {
            if(now_movetime >= movetime - Time.deltaTime)
            {
                _CamParent.transform.position = goal_vector;
                _moveFlag = false;
                now_movetime = 0;
            }
            else
            {
                now_movetime += Time.deltaTime;
                _CamParent.transform.position = start_vector + ((goal_vector - start_vector) / movetime * now_movetime);
            }
        }
    }
    public static void CameraPoint(Vector3 vector)
    {
        //まだカメラが動いている途中の場合
        if (_moveFlag)
        {
            now_movetime = 0;

        }
        start_vector = _CamParent.transform.position;
        goal_vector = vector;
        _moveFlag = true;
    }
    public GameObject CamParent
    {
        get { return _CamParent; }
        set { _CamParent = value; }
    }

    //外部から今のカメラの見てる空間座標を取得
    public static Vector3 NowCameraLook
    {
        get { return goal_vector; }
    }

    //キー入力とカメラ視点の反転補正
    public static int is_inverse_keyx
    {
        get
        {
            switch(rotatemode)
            {
                case 0:
                case 3:
                    return 1;

                case 1:
                case 2:

                    return -1;

                default:
                    // error handling
                    return 0;
            }
        }
    }

    public static int is_inverse_keyy
    {
        get
        {
            switch (rotatemode)
            {
                case 0:
                case 3:
                    return -1;

                case 1:
                case 2:

                    return 1;

                default:
                    // error handling
                    return 0;
            }
        }
    }

    public static Vector2 keypadxinput_vector2
    {
        get
        {
            switch (rotatemode)
            {
                case 0:
                    return new Vector2(0, 1);

                case 1:
                    return new Vector2(-1, 0);

                case 2:
                    return new Vector2(0, -1);

                case 3:
                    return new Vector2(1, 0);

                default:
                    // error handling
                    return new Vector2(0, 0);
            }
        }
    }

    public static Vector2 keypadyinput_vector2
    {
        get
        {
            switch (rotatemode)
            {
                case 0:
                    return new Vector2(1, 0);

                case 1:
                    return new Vector2(0, 1);

                case 2:
                    return new Vector2(-1, 0);

                case 3:
                    return new Vector2(0, -1);

                default:
                    // error handling
                    return new Vector2(0, 0);
            }
        }
    }
}
