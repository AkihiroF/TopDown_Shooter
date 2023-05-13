using _Source.Core;
using _Source.FireSystem.SOs;
using _Source.FireSystem.Weapons;
using _Source.Player;
using _Source.Services;
using _Source.SignalsEvents.CoreEvents;
using _Source.SignalsEvents.WeaponsEvents;
using UnityEngine;

namespace _Source.FireSystem.Player
{
    public class PlayerFireSystem : MonoBehaviour
    {
        [SerializeField] private Transform pointPositionGun;
        [SerializeField] private PlayerGunSo firstGun;

        private PlayerGunSo _currentGunSo;
        private GameObject _gunObj;
        private ABaseGunController _currentGun;
        private ClipSo _currentClip;
        private int _currentCountAmmo;

        private void Start()
        {
            Signals.Get<OnRestart>().AddListener(UnSubscribe);
            Signals.Get<OnFinishReloadWeapon>().AddListener(PrintAmmo);
            if (_currentGunSo == null)
            {
                _currentGunSo = firstGun;
            }
            CreateWeapon();
        }

        private void CreateWeapon()
        {
            _gunObj = Instantiate(_currentGunSo.GunObjectObject, pointPositionGun);
            _currentGun = _gunObj.GetComponent<ABaseGunController>();
            _currentGun.OnFireFromWeapon += UpdateCurrentCountAmmoInGun;
            _currentClip = _currentGunSo.ClipInfo;
            if (InventoryPlayer.GetWeapon(_currentGun.GetType()) == null)
            {
                InventoryPlayer.AddWeapon(_currentGun.GetType(), _currentGunSo);
                InventoryPlayer.AddItem(_currentClip, _currentClip.CountBullet);
            }
            Signals.Get<OnUpdateIconWeapon>().Dispatch(_currentGunSo.IconGun);
            SetParamInGun();
        }

        private void UnSubscribe()
        {
            _currentGun.OnFireFromWeapon -= UpdateCurrentCountAmmoInGun;
            Signals.Get<OnFinishReloadWeapon>().RemoveListener(PrintAmmo);
            Signals.Get<OnRestart>().RemoveListener(UnSubscribe);
        }

        public void SetSavedParameters(PlayerGunSo savedGun, int currentAmmo)
        {
            _currentGunSo = savedGun;
            _currentCountAmmo = currentAmmo;
        }

        public PlayerGunSo GetCurrentGun
        {
            get
            {
                return _currentGunSo;
            }
        }

        public int CurrentCountAmmoInGun
        {
            get
            {
                return _currentCountAmmo;
            }
        }

        private void SetParamInGun()
        {
            _currentGun.SetParameters(_currentClip,_currentCountAmmo);
        }

        private void UpdateCurrentCountAmmoInGun(int count)
        {
            _currentCountAmmo = count;
            PrintAmmo();
        }


        public void PrintAmmo()
        {
            Signals.Get<OnPrintInfoAboutFire>().Dispatch(
                ($"{_currentCountAmmo} / {InventoryPlayer.GetCountItem(_currentClip) / _currentClip.CountBullet}"));
        }

        public void Fire()
        {
            _currentGun.Fire();
        }

        public void SwitchWeapon(int id)
        {
            PlayerGunSo weapon;
            switch (id)
            {
                case 1:
                    weapon = InventoryPlayer.GetWeapon(typeof(KnifeController));
                    break;
                case 2:
                    weapon = InventoryPlayer.GetWeapon(typeof(PistolController));
                    break;
                case 3:
                    weapon = InventoryPlayer.GetWeapon(typeof(ShortGunController));
                    break;
                default:
                    weapon = null;
                    Debug.Log("Idi nahui");
                    break;
            }
            if (weapon is not null)
            {
                if(_currentGunSo == weapon)
                    return;
                SwitchingOnNewWeapon(weapon);
            }
        }

        private void SwitchingOnNewWeapon(PlayerGunSo weapon)
        {
            Signals.Get<OnFinishReloadWeapon>().Dispatch();
            _currentGun.OnFireFromWeapon -= UpdateCurrentCountAmmoInGun;
            Destroy(_currentGun.gameObject);
            _currentGunSo = weapon;
            InventoryPlayer.AddItem(_currentClip,_currentCountAmmo);
            _currentCountAmmo = 0;
            CreateWeapon();
        }

        public void ReloadWeapon()
        {
            _currentGun.StartReloadWeapon();
        }
        
    }
}
