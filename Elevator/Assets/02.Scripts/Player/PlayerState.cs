using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idle,        // 아무것도 안 함
    Looking,     // 시선 이동 중
    UsingPhone,  // 폰 보는 중
    PressingBtn, // 버튼 누르는 중
    Panicked     // 멘탈 흔들림
}
