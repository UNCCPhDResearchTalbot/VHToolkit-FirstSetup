using UnityEngine;
using System.Collections;

public class LocomotionData
{
    public float rps = 0.75f;
    public int x_flag = 0;
    public int z_flag = 0;
    public int rps_flag = 0;
    public float spd = 0;
    public float x_spd = 7;
    public float z_spd = 70;
    public string t_direction;//char t_direction[200];
    public string character;//char character[100];
    public int char_index = 0;
    public int kmode = 0;
    public float height_disp = 0.0f;
    public float height_disp_delta = 1.0f;
    public bool height_disp_inc = false;
    public bool height_disp_dec = false;
    public bool upkey = false;
    public bool downkey = false;
    public bool leftkey = false;
    public bool rightkey = false;
    public bool a_key = false;
    public bool d_key = false;
    public float off_height_comp = 0.0f;
}
