using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class DashBar : MonoBehaviour
{

    [SerializeField] private PlayerController _player;
    [SerializeField] private Image _dashBar;

    private void Start()
    {
        _player.DashCooldown.Subscribe(value =>
        {
            _dashBar.fillAmount = _player.MaxDashCooldown - value / _player.MaxDashCooldown - (_player.MaxDashCooldown - 1f); // scorr math
        }).AddTo(this);
    }
}
