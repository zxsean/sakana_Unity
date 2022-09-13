using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class Sakana : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    #region Characters

    private enum ECHARType
    {
        CHISATO = 0,
        TAKINA = 1,
    }

    public class Characters
    {
        public Sprite m_sprite;
        public Color m_lineColor;
        public AudioClip m_audioClip;
        public float m_inertia;

        /// <summary>
        /// 角度
        /// </summary>
        public float m_r;

        /// <summary>
        /// 高度
        /// </summary>
        public float m_y;

        /// <summary>
        /// 垂直速度
        /// </summary>
        public float m_t;

        /// <summary>
        /// 横向速度
        /// </summary>
        public float m_w;

        public float m_decay;
    }

    #endregion Characters

    #region const

    private const float __STICKY = 0.1f;

    private const float __MAX_R = 60;

    private const float __MAX_Y = 110;

    private const float __THRESHOLD = 0.1f;

    private float __DEFAULT_FRAME = 60;

    #endregion const

    private float _originRotate = 0;

    private float m_inertia = float.MaxValue;

    public Sprite[] m_sprites = null;
    public AudioClip[] m_audioClips = null;

    public Image m_image = null;
    public UILineRenderer m_line;
    public RectTransform m_canvas;

    public AudioSource m_audioSource;

    private ECHARType m_cueType;

    private Characters[] m_characters = new Characters[2];

    private Characters m_curChar = null;

    private bool m_isRunning = false;

    private float m_downPageY = 0;

    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        ColorUtility.TryParseHtmlString("#C63C47", out var _color1);
        ColorUtility.TryParseHtmlString("#2C3B5D", out var _color2);

        m_characters[0] = new Characters
        {
            m_sprite = m_sprites[0],
            m_inertia = 0.1f,
            m_lineColor = _color1,
            m_audioClip = m_audioClips[0],

            m_r = 0, //角度
            m_y = 40, //高度
            m_t = 0, //垂直速度
            m_w = 0, //横向速度
            m_decay = 0.99f, //衰减
        };

        m_characters[1] = new Characters
        {
            m_sprite = m_sprites[1],
            m_inertia = 0.01f,
            m_lineColor = _color2,
            m_audioClip = m_audioClips[1],

            m_r = 12,
            m_y = 2,
            m_t = 0,
            m_w = 0,
            m_decay = 0.99f,
        };

        m_curChar = m_characters[(int)m_cueType];
        m_line.color = m_curChar.m_lineColor;
        m_image.sprite = m_curChar.m_sprite;

        m_inertia = m_curChar.m_inertia;
        m_inertia = Mathf.Min(0.5f, Mathf.Max(0, m_inertia));
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        Init();
    }

    private void Update()
    {
        Draw();
        Run();
    }

    private Vector2 RotatePoint(float _cx, float _cy, float _x, float _y, float _angle)
    {
        var _radians = (Mathf.PI / 180) * _angle;
        var _cos = Mathf.Cos(_radians);
        var _sin = Mathf.Sin(_radians);
        var _nx = (_cos * (_x - _cx)) + (_sin * (_y - _cy)) + _cx;
        var _ny = (_cos * (_y - _cy)) - (_sin * (_x - _cx)) + _cy;

        Vector2 _v2 = new Vector2(_nx, _ny);

        return _v2;
    }

    private Vector3 Bezier(Vector3 _p0, Vector3 _p1, Vector3 _p2, float _t)
    {
        Vector3 _p0p1 = (1 - _t) * _p0 + _t * _p1;
        Vector3 _p1p2 = (1 - _t) * _p1 + _t * _p2;
        Vector3 _pos = (1 - _t) * _p0p1 + _t * _p1p2;

        return _pos;
    }

    public void Move(float _x, float _y)
    {
        var _r = _x * __STICKY;

        _r = Mathf.Max(-__MAX_R, _r);
        _r = Mathf.Min(__MAX_R, _r);

        _y = _y * __STICKY * 2;

        _y = Mathf.Max(-__MAX_Y, _y);
        _y = Mathf.Min(__MAX_Y, _y);

        m_curChar.m_r = _r;
        m_curChar.m_y = _y;
        m_curChar.m_w = 0;
        m_curChar.m_t = 0;

        Draw();
    }

    public void Run()
    {
        if (!m_isRunning)
        {
            return;
        }

        var _inertia = m_inertia;

        //var _timeDelta = Time.deltaTime;   //上一帧到当前帧的时间，单位毫秒

        //if (_timeDelta < 16) //两次的时间差大于16毫秒则不处理
        //{
        //    _inertia = m_inertia / (1000 / __DEFAULT_FRAME * _timeDelta);
        //}

        var _r = m_curChar.m_r;
        var _y = m_curChar.m_y;
        var _t = m_curChar.m_t;
        var _w = m_curChar.m_w;
        var _d = m_curChar.m_decay;

        _w = _w - _r * 2 - _originRotate;
        _r = _r + _w * _inertia * 1.2f;
        m_curChar.m_w = _w * _d;
        m_curChar.m_r = _r;

        _t = _t - _y * 2;
        _y = _y + _t * _inertia * 2;
        m_curChar.m_t = _t * _d;
        m_curChar.m_y = _y;

        //小于一定动作时停止绘制
        if (Mathf.Max(Mathf.Abs(m_curChar.m_w),
            Mathf.Abs(m_curChar.m_r),
            Mathf.Abs(m_curChar.m_t),
            Mathf.Abs(m_curChar.m_y)) < __THRESHOLD)
        {
            m_isRunning = false;
            return;
        }

        Draw();
    }

    private void Draw()
    {
        var r = m_curChar.m_r;
        var y = m_curChar.m_y;
        //var t = m_curChar.m_t;
        //var w = m_curChar.m_w;
        //var d = m_curChar.m_decay;

        float x = r * 1;
        //var _y = y;

        m_image.transform.localEulerAngles = new Vector3(0, 0, -r);

        var cx = 0;
        var cy = -100;

        var n = RotatePoint(cx, cy, x, -y, r);

        var nx = n.x;
        var ny = -n.y - 100;

        m_image.GetComponent<RectTransform>().anchoredPosition = new Vector2(nx, -ny);

        var _pointList = new List<Vector2>();

        for (float i = 0; i <= 1; i += 0.1f)
        {
            var _v = Bezier(new Vector2(0, -140), new Vector2(0, -75), m_image.GetComponent<RectTransform>().anchoredPosition, i);

            _pointList.Add(new Vector2(_v.x, _v.y));
        }

        m_line.Points = _pointList.ToArray();
    }

    public void ChangeCharacter()
    {
        if (m_cueType == ECHARType.CHISATO)
        {
            m_cueType = ECHARType.TAKINA;
        }
        else if (m_cueType == ECHARType.TAKINA)
        {
            m_cueType = ECHARType.CHISATO;
        }

        Init();
    }

    public void OnDrag(PointerEventData _eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvas, _eventData.position, _eventData.enterEventCamera, out var _pos);

        m_isRunning = false;

        var _X = _pos.x - m_image.rectTransform.anchoredPosition.x;
        var _Y = _pos.y - m_downPageY;

        Move(_X, -_Y);
    }

    public void OnPointerDown(PointerEventData _eventData)
    {
        m_isRunning = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvas, _eventData.position, _eventData.enterEventCamera, out var _pos);

        m_downPageY = _pos.y;
    }

    public void OnPointerUp(PointerEventData _)
    {
        m_isRunning = true;

        PlayVoice();
    }

    private void PlayVoice()
    {
        if (m_cueType == ECHARType.CHISATO)
        {
            if (Mathf.Abs(m_curChar.m_r) <= 4 &&
                Mathf.Abs(m_curChar.m_y) >= 20)
            {
                _PlayVoice();
            };
        }
        else
        {
            if (m_curChar.m_r >= m_characters[(int)ECHARType.TAKINA].m_r &&
                (Mathf.Abs(m_curChar.m_y) <= 12 ||
                 m_curChar.m_r >= 3 * Mathf.Abs(m_curChar.m_y)))
            {
                _PlayVoice();
            };
        };
    }

    private void _PlayVoice()
    {
        if (m_audioSource.isPlaying)
        {
            m_audioSource.Stop();
        }

        AudioClip _clip = m_curChar.m_audioClip;
        m_audioSource.clip = _clip;
        m_audioSource.Play();
    }
}