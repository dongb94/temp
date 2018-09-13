﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;


public class Psyous : Champion {
    #region <Enums>
    private enum Info{
        speed,
        damage,
        max_collider,
        life_time
    }

    #endregion </Enums>

    #region <Consts>

    // cached collider size
    private static readonly Vector3 FireBall_Range = new Vector3(1.0f, 2.0f, 0.015f);  // (horizon wide, height, vertical wide) Box type
    private static readonly Vector3 ForceHammer_Range = new Vector3(2.5f, 0.0f, 0.0f);  // (horizon wide, height, vertical wide) Sphere type
    private static readonly Vector3 MagmaBall_Range = new Vector3(0.5f, 2.0f, 0.015f);  // (horizon wide, height, vertical wide) Box type
    private static readonly Vector3 FireRain_Range = new Vector3(3.0f, 0.0f, 0.0f);  // (horizon wide, height, vertical wide) Sphere type

    /// cached skill information [speed, (int)damage, (int)max_collider, life_time]
    private static readonly float[] NormalAction_Info = { 30.0f, 3.0f, 1f, 0.5f };
    private static readonly float[] FireBall_Info =     { 20.0f, 5.0f, 5f, 5.0f };
    private static readonly float[] ForceHammer_Info =  { 0.0f, 10.0f, 10f, 1.0f };
    private static readonly float[] MagmaBall_Info =    { 5.0f, 1.0f, 5f, 5.0f };
    private static readonly float[] FireRain_Info =     { 0.0f, 2.0f, 10f, 3.0f };

    #endregion </Consts>

    #region <Unity/Callbacks>

    protected override void Awake()
    {
        base.Awake();

        ChampionType = K514SfxStorage.ChampionType.Psyous;

        NormalActionsEventGroup = new[]
        {
            NormalAction01(HUDManager.GetInstance.ActionTriggerGroup[(int)ActionTrigger.TriggerType.MainTrigger]),
            NormalAction02(HUDManager.GetInstance.ActionTriggerGroup[(int)ActionTrigger.TriggerType.MainTrigger]),
            NormalAction03(HUDManager.GetInstance.ActionTriggerGroup[(int)ActionTrigger.TriggerType.MainTrigger])
        };

        PrimaryActionEventGroup = Spell02(HUDManager.GetInstance.ActionTriggerGroup[(int)ActionTrigger.TriggerType.SubTriggerLeft]);
        SecondaryActionEventGroup = Spell03(HUDManager.GetInstance.ActionTriggerGroup[(int)ActionTrigger.TriggerType.SubTriggerRight]);
    }

    #endregion </Unity/Callbacks>

    #region <NormalAction/Methods>

    private Action<CustomEventArgs.CommomActionArgs>[] NormalAction01(ActionTrigger pTargetTrigger)
    {

        #region <NormalAction/Methods/Init>
        var eventGroup = new Action<CustomEventArgs.CommomActionArgs>[(int)EventInfo.Count];
        #endregion

        #region <NormalAction/Methods/Clicked>
        eventGroup[(int)EventInfo.Clicked] = other =>
        {
            pTargetTrigger.Processtype = ActionTrigger.ProcessType.Simple;
            pTargetTrigger.Countertype = ActionTrigger.CounterType.TimeLerp;
        };
        #endregion

        #region <NormalAction/Methods/Birth>   
        eventGroup[(int)EventInfo.Birth] = (other) =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.UpdateTension();
            SetCurrentEventState(other.SetCandidate(lChampion.DetectAndChaseEnemyInRange(8.5f, 0f, 0f)));
            UnitBoneAnimator.SetCast("Normal-Action", 0, lChampion.CastSpeed);
            SoundManager.GetInstance
                .CastSfx(SoundManager.AudioMixerType.VOICE, lChampion.ChampionType, K514SfxStorage.ActivityType.Attack).SetTrigger();
        };
        #endregion

        #region <NormalAction/Methods/Exit>
        eventGroup[(int)EventInfo.Exit] = (other) =>
        {
            var lChampion = (Champion)other.Caster;

            var triggerPosition = lChampion.AttachPoint[(int) AttachPointType.RightHandIndex1].position;
            var direction = other.Candidate == null ?
                    lChampion._Transform.TransformDirection(Vector3.forward).normalized
                    : (new Vector3(other.Candidate._Transform.position.x, triggerPosition.y, other.Candidate._Transform.position.z) - triggerPosition).normalized;

            ProjectileManager.GetInstance.CreateProjectile(ProjectileManager.Type.PsyousMagicMissile, lChampion, NormalAction_Info[(int)Info.life_time], triggerPosition)
                .SetVelocity(direction * NormalAction_Info[(int)Info.speed])
                .SetDirection()
                .SetMaxColliderNumber((int)NormalAction_Info[(int)Info.max_collider])
                .SetRemoveDelay(0.1f)
                .SetArrowLeave(false)
                .SetProjectileType((int)Projectile.ProjectileType.Point)
                .SetCollideUnitAction(args =>
                {
                    var proj = (Projectile)args.MorphObject;
                    foreach (var collidedUnit in proj.CollidedUnitGroup)
                    {
                        collidedUnit.Hurt(proj.GetCaster(), (int)NormalAction_Info[(int)Info.damage], TextureType.Magic, proj.Direction,
                            (trigger, subject, forceDirection) =>
                            {
                                subject.AddForce(forceDirection * 2.0f);
                            });
                    }
                    proj.Remove();
                })
                .SetActive(true);

            lChampion.ResetSpellColliderActive();
        };
        #endregion

        #region <NormalAction/Methods/Terminate>
        eventGroup[(int)EventInfo.Terminate] = other =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.ResetFromCast();
        };
        #endregion

        #region <NormalAction/Methods/Released>
        eventGroup[(int)EventInfo.Released] = other =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.ProcessNormalAttackSequence();
        };
        #endregion

        return eventGroup;
    } //Normal Action 01

    private Action<CustomEventArgs.CommomActionArgs>[] NormalAction02(ActionTrigger pTargetTrigger)
    {

        #region <NormalAction/Methods/Init>
        var eventGroup = new Action<CustomEventArgs.CommomActionArgs>[(int)EventInfo.Count];
        #endregion

        #region <NormalAction/Methods/Clicked>
        eventGroup[(int)EventInfo.Clicked] = other =>
        {
            pTargetTrigger.Processtype = ActionTrigger.ProcessType.Simple;
            pTargetTrigger.Countertype = ActionTrigger.CounterType.TimeLerp;
        };
        #endregion

        #region <NormalAction/Methods/Birth>   
        eventGroup[(int)EventInfo.Birth] = (other) =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.UpdateTension();
            SetCurrentEventState(other.SetCandidate(lChampion.DetectAndChaseEnemyInRange(8.5f, 0f, 0f)));
            UnitBoneAnimator.SetCast("Normal-Action", 1, lChampion.CastSpeed);
            SoundManager.GetInstance
                .CastSfx(SoundManager.AudioMixerType.VOICE, lChampion.ChampionType, K514SfxStorage.ActivityType.Attack).SetTrigger();
        };
        #endregion

        #region <NormalAction/Methods/Exit>
        eventGroup[(int)EventInfo.Exit] = (other) =>
        {
            var lChampion = (Champion)other.Caster;
            
            var triggerPosition = lChampion.AttachPoint[(int)AttachPointType.RightHandIndex1].position;
            var direction = other.Candidate == null ?
                    lChampion._Transform.TransformDirection(Vector3.forward).normalized
                    : (new Vector3(other.Candidate._Transform.position.x, triggerPosition.y, other.Candidate._Transform.position.z) - triggerPosition).normalized;

            ProjectileManager.GetInstance.CreateProjectile(ProjectileManager.Type.PsyousMagicMissile, lChampion, NormalAction_Info[(int)Info.life_time], triggerPosition)
                .SetVelocity(direction * NormalAction_Info[(int)Info.speed])
                .SetDirection()
                .SetMaxColliderNumber((int)NormalAction_Info[(int)Info.max_collider])
                .SetRemoveDelay(0.1f)
                .SetArrowLeave(false)
                .SetProjectileType((int)Projectile.ProjectileType.Point)
                .SetCollideUnitAction(args =>
                {
                    var proj = (Projectile)args.MorphObject;
                    foreach (var collidedUnit in proj.CollidedUnitGroup)
                    {
                        collidedUnit.Hurt(proj.GetCaster(), (int)NormalAction_Info[(int)Info.damage], TextureType.Magic, proj.Direction,
                            (trigger, subject, forceDirection) =>
                            {
                                subject.AddForce(forceDirection * 2.0f);
                            });
                    }
                    proj.Remove();
                })
                .SetActive(true);

            lChampion.ResetSpellColliderActive();
        };
        #endregion

        #region <NormalAction/Methods/Terminate>
        eventGroup[(int)EventInfo.Terminate] = other =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.ResetFromCast();
        };
        #endregion

        #region <NormalAction/Methods/Released>
        eventGroup[(int)EventInfo.Released] = other =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.ProcessNormalAttackSequence();
        };
        #endregion

        return eventGroup;
    } //Normal Action 02

    private Action<CustomEventArgs.CommomActionArgs>[] NormalAction03(ActionTrigger pTargetTrigger)
    {
        #region <NormalAction/Methods/Init>
        var eventGroup = new Action<CustomEventArgs.CommomActionArgs>[(int)EventInfo.Count];
        #endregion

        #region <NormalAction/Methods/Clicked>
        eventGroup[(int)EventInfo.Clicked] = other =>
        {
            pTargetTrigger.Processtype = ActionTrigger.ProcessType.Simple;
            pTargetTrigger.Countertype = ActionTrigger.CounterType.TimeLerp;
            pTargetTrigger.UpdateCooldown(4, resettable: true);
        };
        #endregion

        #region <NormalAction/Methods/Birth>   
        eventGroup[(int)EventInfo.Birth] = (other) =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.UpdateTension();
            SetCurrentEventState(other.SetCandidate(lChampion.DetectAndChaseEnemyInRange(8.5f, 0f, 0f)));
            UnitBoneAnimator.SetCast("Normal-Action", 2, lChampion.CastSpeed);
            SoundManager.GetInstance
                .CastSfx(SoundManager.AudioMixerType.VOICE, lChampion.ChampionType, K514SfxStorage.ActivityType.Attack).SetTrigger();
        };
        #endregion

        #region <NormalAction/Methods/Exit>
        eventGroup[(int)EventInfo.Exit] = (other) =>
        {
            var lChampion = (Champion)other.Caster;

            var triggerPosition = lChampion.AttachPoint[(int)AttachPointType.RightHandIndex1].position;
            var direction = other.Candidate == null ?
                    lChampion._Transform.TransformDirection(Vector3.forward).normalized
                    : (new Vector3(other.Candidate._Transform.position.x, triggerPosition.y, other.Candidate._Transform.position.z) - triggerPosition).normalized;

            ProjectileManager.GetInstance.CreateProjectile(ProjectileManager.Type.PsyousMagicMissile, lChampion, NormalAction_Info[(int)Info.life_time], triggerPosition)
                .SetVelocity(direction * NormalAction_Info[(int)Info.speed])
                .SetDirection()
                .SetMaxColliderNumber((int)NormalAction_Info[(int)Info.max_collider])
                .SetRemoveDelay(0.1f)
                .SetArrowLeave(false)
                .SetProjectileType((int)Projectile.ProjectileType.Point)
                .SetCollideUnitAction(args =>
                {
                    var proj = (Projectile)args.MorphObject;
                    foreach (var collidedUnit in proj.CollidedUnitGroup)
                    {
                        collidedUnit.Hurt(proj.GetCaster(), (int)NormalAction_Info[(int)Info.damage], TextureType.Magic, proj.Direction,
                            (trigger, subject, forceDirection) =>
                            {
                                subject.AddForce(forceDirection * 2.0f);
                            });
                    }
                    proj.Remove();
                })
                .SetActive(true);

            lChampion.ResetSpellColliderActive();
        };
        #endregion

        #region <NormalAction/Methods/Terminate>
        eventGroup[(int)EventInfo.Terminate] = other =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.ResetFromCast();
        };
        #endregion

        #region <NormalAction/Methods/Released>
        eventGroup[(int)EventInfo.Released] = other =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.ProcessNormalAttackSequence();
        };
        #endregion

        return eventGroup;
    } //Normal Action 03

    #endregion </NormalAction/Methods>

    #region <Spell/Methods>      

    private Action<CustomEventArgs.CommomActionArgs>[] Spell01(ActionTrigger pTargetTrigger)
    {
        #region <Spell01/Methods/Init>

        var eventGroup = new Action<CustomEventArgs.CommomActionArgs>[(int)EventInfo.Count];

        #endregion

        #region <Spell01/Methods/Clicked>

        eventGroup[(int)EventInfo.Clicked] = other =>
        {
            pTargetTrigger.Processtype = ActionTrigger.ProcessType.Simple;
            pTargetTrigger.Countertype = ActionTrigger.CounterType.TimeLerp;
            pTargetTrigger.UpdateCooldown(5, resettable: true);
        };

        #endregion

        #region <Spell01/Methods/Birth>

        eventGroup[(int)EventInfo.Birth] = other =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.UpdateTension();
            SetCurrentEventState(other.SetCandidate(lChampion.DetectAndChaseEnemyInRange(8.5f, 0f, 0f)));
            lChampion.UnitBoneAnimator.SetCast("Spell", 0, lChampion.CastSpeed);
            SoundManager.GetInstance
                .CastSfx(SoundManager.AudioMixerType.VOICE, lChampion.ChampionType, K514SfxStorage.ActivityType.Serifu).SetTrigger();
        };

        #endregion

        #region <Spell01/Methods/Enter>

        eventGroup[(int)EventInfo.Enter] = other =>
        {
        };

        #endregion

        #region <Spell01/Methods/Exit>
        eventGroup[(int)EventInfo.Exit] = (other) =>
        {
            var lChampion = (Champion)other.Caster;

            var triggerPosition = lChampion.AttachPoint[(int)AttachPointType.LeftHandIndex1].position;
            var direction = other.Candidate == null ?
                    lChampion._Transform.TransformDirection(Vector3.forward).normalized
                    : (new Vector3(other.Candidate._Transform.position.x, triggerPosition.y, other.Candidate._Transform.position.z) - triggerPosition).normalized;

            K514VfxManager.GetInstance.CastVfx(K514VfxManager.ParticleType.PFire, triggerPosition)
                    .SetLifeSpan(.3f)
                    .SetForward(triggerPosition)
                    .SetScale(1f)
                    .SetTrigger();
            K514VfxManager.GetInstance.CastVfx(K514VfxManager.ParticleType.PEmber, triggerPosition)
                    .SetLifeSpan(.3f)
                    .SetForward(triggerPosition)
                    .SetScale(1f)
                    .SetTrigger();

            ProjectileManager.GetInstance.CreateProjectile(ProjectileManager.Type.PsyousFireBall, lChampion, FireBall_Info[(int)Info.life_time], triggerPosition)
                .SetVelocity(direction * FireBall_Info[(int)Info.speed])
                .SetDirection()
                .SetMaxColliderNumber((int)FireBall_Info[(int)Info.max_collider])
                .SetRemoveDelay(5.0f)
                .SetArrowLeave(false)
                .SetProjectileType((int)Projectile.ProjectileType.Box)
                .SetColliderBox(FireBall_Range)
                .SetCollideUnitAction(args =>
                {
                    var proj = (Projectile)args.MorphObject;
                    foreach (var collidedUnit in proj.CollidedUnitGroup)
                    {
                        collidedUnit.Hurt(proj.GetCaster(), (int)FireBall_Info[(int)Info.damage], TextureType.Magic, proj.Direction,
                            (trigger, subject, forceDirection) =>
                            {
                                subject.AddForce(forceDirection * 10.0f);
                            });
                    }
                })
                .SetActive(true);

            lChampion.ResetSpellColliderActive();
        };
        #endregion

        #region <Spell01/Methods/Terminate>
        eventGroup[(int)EventInfo.Terminate] = (other) =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.ResetFromCast();
        };
        #endregion

        return eventGroup;
    } // Fire_Ball

    private Action<CustomEventArgs.CommomActionArgs>[] Spell02(ActionTrigger pTargetTrigger)
    {
        #region <Spell02/Methods/Init>

        var eventGroup = new Action<CustomEventArgs.CommomActionArgs>[(int)EventInfo.Count];

        #endregion

        #region <Spell02/Methods/Clicked>

        eventGroup[(int)EventInfo.Clicked] = other =>
        {
            pTargetTrigger.Processtype = ActionTrigger.ProcessType.Simple;
            pTargetTrigger.Countertype = ActionTrigger.CounterType.TimeLerp;
            pTargetTrigger.UpdateCooldown(5, resettable: true);
        };

        #endregion

        #region <Spell02/Methods/Birth>
        eventGroup[(int)EventInfo.Birth] = other =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.UpdateTension();
            SetCurrentEventState(other.SetCandidate(lChampion.DetectAndChaseEnemyInRange(7.5f, 0.0f, .0f)));
            lChampion.UnitBoneAnimator.SetCast("Spell", 1, lChampion.CastSpeed);
            SoundManager.GetInstance
                .CastSfx(SoundManager.AudioMixerType.VOICE, lChampion.ChampionType, K514SfxStorage.ActivityType.Serifu).SetTrigger();
        };
        #endregion

        #region <Spell02/Methods/Enter>
        eventGroup[(int)EventInfo.Enter] = other =>
        {
            
        };
        #endregion

        #region <Spell02/Methods/Exit>
        eventGroup[(int)EventInfo.Exit] = (other) =>
        {
            var lChampion = (Champion)other.Caster;
            var triggerPosition = other.Candidate==null?
                lChampion.AttachPoint[(int)AttachPointType.LeftHandIndex1].position
                : other.Candidate._Transform.position;

            K514VfxManager.GetInstance.CastVfx(K514VfxManager.ParticleType.PBlast, triggerPosition)
                    .SetLifeSpan(.3f)
                    .SetForward(triggerPosition)
                    .SetScale(0.3f)
                    .SetTrigger();

            ProjectileManager.GetInstance.CreateProjectile(ProjectileManager.Type.PsyousForceHammer, lChampion, ForceHammer_Info[(int)Info.life_time], triggerPosition)
                .SetMaxColliderNumber((int)ForceHammer_Info[(int)Info.max_collider])
                .SetRemoveDelay(5.0f)
                .SetArrowLeave(false)
                .SetProjectileType((int)Projectile.ProjectileType.Sphere)
                .SetColliderBox(ForceHammer_Range)
                .SetCollideUnitAction(args =>
                {
                    var proj = (Projectile)args.MorphObject;
                    foreach (var collidedUnit in proj.CollidedUnitGroup)
                    {
                        collidedUnit.Hurt(proj.GetCaster(), (int)ForceHammer_Info[(int)Info.damage], TextureType.Magic, proj.Direction,
                            (trigger, subject, forceDirection) =>
                            {
                                subject.AddForce(forceDirection * 2.0f);
                            });
                    }
                    proj.Remove();
                })
                .SetActive(true);

            lChampion.ResetSpellColliderActive();
        };
        #endregion

        #region <Spell02/Methods/Terminate>
        eventGroup[(int)EventInfo.Terminate] = (other) =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.ResetFromCast();
        };
        #endregion

        return eventGroup;
    } // Force_Hammer

    private Action<CustomEventArgs.CommomActionArgs>[] Spell03(ActionTrigger pTargetTrigger)
    {
        #region <Spell03/Methods/Init>

        var eventGroup = new Action<CustomEventArgs.CommomActionArgs>[(int)EventInfo.Count];

        #endregion

        #region <Spell03/Methods/Clicked>

        eventGroup[(int)EventInfo.Clicked] = other =>
        {
            pTargetTrigger.Processtype = ActionTrigger.ProcessType.Simple;
            pTargetTrigger.Countertype = ActionTrigger.CounterType.TimeLerp;
            pTargetTrigger.UpdateCooldown(5, resettable: true);
        };

        #endregion

        #region <Spell03/Methods/Birth>

        eventGroup[(int)EventInfo.Birth] = other =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.UpdateTension();
            SetCurrentEventState(other.SetCandidate(lChampion.DetectAndChaseEnemyInRange(8.5f, 0f, 0f)));
            lChampion.UnitBoneAnimator.SetCast("Spell", 0, lChampion.CastSpeed);
            SoundManager.GetInstance
                .CastSfx(SoundManager.AudioMixerType.VOICE, lChampion.ChampionType, K514SfxStorage.ActivityType.Serifu).SetTrigger();
        };

        #endregion

        #region <Spell03/Methods/Enter>

        eventGroup[(int)EventInfo.Enter] = other =>
        {
        };

        #endregion

        #region <Spell03/Methods/Exit>
        eventGroup[(int)EventInfo.Exit] = (other) =>
        {
            var lChampion = (Champion)other.Caster;

            var triggerPosition = lChampion.AttachPoint[(int)AttachPointType.LeftHandIndex1].position;
            var direction = other.Candidate == null ?
                    lChampion._Transform.TransformDirection(Vector3.forward).normalized
                    : (new Vector3(other.Candidate._Transform.position.x, triggerPosition.y, other.Candidate._Transform.position.z) - triggerPosition).normalized;
            var fallDirection = direction + new Vector3(0, -4f, 0);

            K514VfxManager.GetInstance.CastVfx(K514VfxManager.ParticleType.PFire, triggerPosition)
                    .SetLifeSpan(.3f)
                    .SetForward(triggerPosition)
                    .SetScale(1f)
                    .SetTrigger();
            K514VfxManager.GetInstance.CastVfx(K514VfxManager.ParticleType.PEmber, triggerPosition)
                    .SetLifeSpan(.3f)
                    .SetForward(triggerPosition)
                    .SetScale(1f)
                    .SetTrigger();

            ProjectileManager.GetInstance.CreateProjectile(ProjectileManager.Type.PsyousMagmaBall, lChampion, MagmaBall_Info[(int)Info.life_time], triggerPosition- (fallDirection * 5))
                .SetVelocity(fallDirection * 20)
                .SetDirection()
                .SetCollidedObstacleAction(arg =>
                {
                    var proj = (Projectile)arg.MorphObject;
                    ProjectileManager.GetInstance.CreateProjectile(ProjectileManager.Type.PsyousMagmaBall, lChampion, MagmaBall_Info[(int)Info.life_time], proj._Transform.position)
                        .SetVelocity(direction * MagmaBall_Info[(int)Info.speed])
                        .SetDirection()
                        .SetMaxColliderNumber((int)MagmaBall_Info[(int)Info.max_collider])
                        .SetArrowLeave(false)
                        .SetProjectileType((int)Projectile.ProjectileType.Box)
                        .SetColliderBox(MagmaBall_Range)
                        .SetNumberOfHit(15)
                        .SetCollideUnitAction(args =>
                        {
                            var subProj = (Projectile)args.MorphObject;
                            foreach (var collidedUnit in subProj.CollidedUnitGroup)
                            {
                                collidedUnit.Hurt(subProj.GetCaster(), (int)MagmaBall_Info[(int)Info.damage], TextureType.Heavy, subProj.Direction,
                                    (trigger, subject, forceDirection) =>
                                    {
                                        subject.AddForce(forceDirection * 6.0f);
                                    });
                            }
                        })
                        .SetActive(true);
                    proj.Remove();
                })
                .SetActive(true);

                
            

            lChampion.ResetSpellColliderActive();
        };
        #endregion

        #region <Spell03/Methods/Terminate>
        eventGroup[(int)EventInfo.Terminate] = (other) =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.ResetFromCast();
        };
        #endregion

        return eventGroup;
    } // Magma_Ball

    private Action<CustomEventArgs.CommomActionArgs>[] Spell04(ActionTrigger pTargetTrigger)
    {
        #region <Spell04/Methods/Init>

        var eventGroup = new Action<CustomEventArgs.CommomActionArgs>[(int)EventInfo.Count];

        #endregion

        #region <Spell04/Methods/Clicked>

        eventGroup[(int)EventInfo.Clicked] = other =>
        {
            pTargetTrigger.Processtype = ActionTrigger.ProcessType.Simple;
            pTargetTrigger.Countertype = ActionTrigger.CounterType.TimeLerp;
            pTargetTrigger.UpdateCooldown(5, resettable: true);
        };

        #endregion

        #region <Spell04/Methods/Birth>
        eventGroup[(int)EventInfo.Birth] = other =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.UpdateTension();
            SetCurrentEventState(other.SetCandidate(lChampion.DetectAndChaseEnemyInRange(7.5f, 0.0f, .0f)));
            lChampion.UnitBoneAnimator.SetCast("Spell", 1, lChampion.CastSpeed);
            SoundManager.GetInstance
                .CastSfx(SoundManager.AudioMixerType.VOICE, lChampion.ChampionType, K514SfxStorage.ActivityType.Serifu).SetTrigger();
        };
        #endregion

        #region <Spell04/Methods/Enter>
        eventGroup[(int)EventInfo.Enter] = other =>
        {

        };
        #endregion

        #region <Spell04/Methods/Exit>
        eventGroup[(int)EventInfo.Exit] = (other) =>
        {
            var lChampion = (Champion)other.Caster;
            var triggerPosition = other.Candidate == null ?
                lChampion.AttachPoint[(int)AttachPointType.LeftHandIndex1].position
                : other.Candidate._Transform.position;

            ProjectileManager.GetInstance.CreateProjectile(ProjectileManager.Type.PsyousFireBall, lChampion, ForceHammer_Info[(int)Info.life_time], triggerPosition)
                .SetMaxColliderNumber((int)ForceHammer_Info[(int)Info.max_collider])
                .SetRemoveDelay(5.0f)
                .SetArrowLeave(false)
                .SetProjectileType((int)Projectile.ProjectileType.Sphere)
                .SetColliderBox(FireRain_Range)
                .SetCollideUnitAction(args =>
                {
                    var proj = (Projectile)args.MorphObject;
                    foreach (var collidedUnit in proj.CollidedUnitGroup)
                    {
                        collidedUnit.Hurt(proj.GetCaster(), (int)ForceHammer_Info[(int)Info.damage], TextureType.Magic, proj.Direction,
                            (trigger, subject, forceDirection) =>
                            {
                                subject.AddForce(forceDirection * 2.0f);
                            });
                    }
                    proj.Remove();
                })
                .SetActive(true);

            lChampion.ResetSpellColliderActive();
        };
        #endregion

        #region <Spell04/Methods/Terminate>
        eventGroup[(int)EventInfo.Terminate] = (other) =>
        {
            var lChampion = (Champion)other.Caster;
            lChampion.ResetFromCast();
        };
        #endregion

        return eventGroup;
    } // Fire_Rain


    #endregion </Spell/Methods>

}
