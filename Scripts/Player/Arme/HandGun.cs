﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using VHS;

namespace DitzelGames.FastIK
{
    public class HandGun : AArme
    {
        [Header("Arme Setting")]
        [SerializeField] private Animator m_animator = null;
        [SerializeField] private int currentMunition = 0;
        [SerializeField] private int chargeurMunition = 0;
        [SerializeField] private int maxMunition = 0;
        [SerializeField] private float shootRate = 0.8f;
        [SerializeField] private float reloadTime = 0.8f;
        [SerializeField] private float magnitudeShakeShoot = 0.1f;
        [SerializeField] private float timeShakeShoot = 0.5f;
        [Header("Arme Raycast")]
        [SerializeField] private LayerMask layerImpactDegat = 0;
        [SerializeField] private float hitForceTire = 0;
        [SerializeField] private float degatParBalle = 10.0f;
        [SerializeField] private float Z = 10;
        [SerializeField] private float scale = 0.1f;
        [SerializeField] private GameObject targetCamera = null;
        private RaycastHit hit;
        private Ray ray;
        private bool isShoot = false;
        private bool isReload = false;

        public override IEnumerator shoot()
        {
            if (this.currentMunition > 0)
            {
                if (!isShoot && !isReload && Input.GetButtonDown("Fire1"))
                {
                    isShoot = true;
                    base.netAnim.SetTrigger("shootOneShot");
                    base.wM.CmdTire();
                    base.wM.StartcShake(this.magnitudeShakeShoot, this.timeShakeShoot);
                    yield return new WaitForSeconds(this.shootRate);
                    isShoot = false;
                }
            }
            else if (Input.GetButtonDown("Fire1"))
            {
                base.netAnim.SetTrigger("noAmmo");
            }
            yield return null;
        }

        public override IEnumerator reload()
        {
            if (this.chargeurMunition > 0 && this.currentMunition < this.maxMunition && !isReload)
            {
                isReload = true;
                m_animator.SetBool("reload", true);
                base.wM.CmdReload();
            }
            yield return null;
        }

        public override void CmdSendTire()
        {
            if (currentMunition > 0 && !isReload)
            {
                Vector3 direction = Random.insideUnitCircle * this.scale;
                direction.z = this.Z;
                direction = this.targetCamera.transform.TransformDirection(direction.normalized);
                this.ray = new Ray(this.targetCamera.transform.position, direction);
                if (Physics.Raycast(this.ray, out hit, Mathf.Infinity, this.layerImpactDegat))
                {
                    GameObject io = Instantiate(base.wM.getImpactByTag(hit.transform.tag), hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                    if (hit.rigidbody)
                    {
                        hit.rigidbody.AddForce(this.ray.direction * hitForceTire);
                    }
                    NetworkServer.Spawn(io);
                    io.GetComponent<ImpactGlue>().AplyGlue(hit.transform.gameObject);
                    if (hit.transform.gameObject.GetComponent<EnemieMembre>())
                    {
                        hit.transform.gameObject.GetComponent<EnemieMembre>().ReceiveDamageMembre(degatParBalle, base.wM.gameObject);
                    }
                }
                this.currentMunition--;
                base.wM.RpcSendMunition(base.idArme, this.currentMunition, this.chargeurMunition);
            }
        }

        public override IEnumerator CmdSendReload()
        {
            isReload = true;
            for (int i = 0; i < this.maxMunition && this.chargeurMunition > 0 && this.currentMunition < this.maxMunition; i++)
            {                
                this.currentMunition++;
                this.chargeurMunition--;
                yield return null;                
            }
            yield return new WaitForSeconds(reloadTime);
            base.wM.RpcSendMunition(base.idArme, this.currentMunition, this.chargeurMunition);
            isReload = false;
        }

        public override void OnChangeCM(int mun, int charg, bool draw)
        {
            this.chargeurMunition = charg;
            this.currentMunition = mun;
            if (draw)
            {
                base.wM.SetTextMun(this.currentMunition.ToString() + "/" + this.chargeurMunition.ToString());
            }
            if (this.chargeurMunition == 0 || this.currentMunition == this.maxMunition)
            {
                this.isReload = false;
                m_animator.SetBool("reload", false);
            }
        }

        public override void OnChangeWeapon()
        {
            base.wM.SetTextMun("");
        }

        public override void OnSelectWeapon()
        {
            base.wM.SetTextMun(this.currentMunition.ToString() + "/" + this.chargeurMunition.ToString());
        }
    }
}
