
using UnityEngine;

public class MyPlayer : MonoBehaviour
{
    public Vector2 Position;
    public Vector2 PreviousPosition;

    private void Update()
    {
        PreviousPosition = Position;

        //Vars
        {
            // idle timer
            idleTimer += Time.deltaTime;
            if (level != null && level.InCutscene)
                idleTimer = -5;
            else if (Speed.x != 0 || Speed.y != 0)
                idleTimer = 0;

            //Underwater music
            // if (!Dead)
            //     Audio.MusicUnderwater = UnderwaterMusicCheck();

            //Just respawned
            if (JustRespawned && Speed != Vector2.zero)
                JustRespawned = false;

            //Get ground
            if (StateMachine.State == StDreamDash)
                onGround = OnSafeGround = false;
            else if (Speed.y >= 0)
            {
                Platform first = CollideFirst<Solid>(Position + Vector2.up);
                if (first == null)
                    first = CollideFirstOutside<JumpThru>(Position + Vector2.up);

                if (first != null)
                {
                    onGround = true;
                    OnSafeGround = first.Safe;
                }
                else
                    onGround = OnSafeGround = false;
            }
            else
                onGround = OnSafeGround = false;

            if (StateMachine.State == StSwim)
                OnSafeGround = true;

            //Safe Ground Blocked?
            if (OnSafeGround)
            {
                foreach (SafeGroundBlocker blocker in Scene.Tracker.GetComponents<SafeGroundBlocker>())
                {
                    if (blocker.Check(this))
                    {
                        OnSafeGround = false;
                        break;
                    }
                }
            }

            playFootstepOnLand -= Time.deltaTime;

            //Highest Air Y
            if (onGround)
                highestAirY = Y;
            else
                highestAirY = Math.Min(Y, highestAirY);

            //Flashing
            if (Scene.OnInterval(.05f))
                flash = !flash;

            //Wall Slide
            if (wallSlideDir != 0)
            {
                wallSlideTimer = Math.Max(wallSlideTimer - Time.deltaTime, 0);
                wallSlideDir = 0;
            }

            //Wall Boost
            if (wallBoostTimer > 0)
            {
                wallBoostTimer -= Time.deltaTime;
                if (moveX == wallBoostDir)
                {
                    Speed.x = WallJumpHSpeed * moveX;
                    Stamina += ClimbJumpCost;
                    wallBoostTimer = 0;
                    sweatSprite.Play("idle");
                }
            }

            //After Dash
            if (onGround && StateMachine.State != StClimb)
            {
                AutoJump = false;
                Stamina = ClimbMaxStamina;
                wallSlideTimer = WallSlideTime;
            }

            //Dash Attack
            if (dashAttackTimer > 0)
                dashAttackTimer -= Time.deltaTime;

            //Jump Grace
            if (onGround)
            {
                dreamJump = false;
                jumpGraceTimer = JumpGraceTime;
            }
            else if (jumpGraceTimer > 0)
                jumpGraceTimer -= Time.deltaTime;

            //Dashes
            {
                if (dashCooldownTimer > 0)
                    dashCooldownTimer -= Time.deltaTime;

                if (dashRefillCooldownTimer > 0)
                    dashRefillCooldownTimer -= Time.deltaTime;
                else if (SaveData.Instance.AssistMode &&
                         SaveData.Instance.Assists.DashMode == Assists.DashModes.Infinite && !level.InCutscene)
                    RefillDash();
                else if (!Inventory.NoRefills)
                {
                    if (StateMachine.State == StSwim)
                        RefillDash();
                    else if (onGround)
                        if (CollideCheck<Solid, NegaBlock>(Position + Vector2.up) ||
                            CollideCheckOutside<JumpThru>(Position + Vector2.up))
                            if (!CollideCheck<Spikes>(Position) ||
                                (SaveData.Instance.AssistMode && SaveData.Instance.Assists.Invincible))
                                RefillDash();
                }
            }

            //Var Jump
            if (varJumpTimer > 0)
                varJumpTimer -= Time.deltaTime;

            //Auto Jump
            if (AutoJumpTimer > 0)
            {
                if (AutoJump)
                {
                    AutoJumpTimer -= Time.deltaTime;
                    if (AutoJumpTimer <= 0)
                        AutoJump = false;
                }
                else
                    AutoJumpTimer = 0;
            }

            //Force Move X
            if (forceMoveXTimer > 0)
            {
                forceMoveXTimer -= Time.deltaTime;
                moveX = forceMoveX;
            }
            else
            {
                moveX = InputMoveX;
                climbHopSolid = null;
            }

            //Climb Hop Solid Movement
            if (climbHopSolid != null && !climbHopSolid.Collidable)
                climbHopSolid = null;
            else if (climbHopSolid != null && climbHopSolid.Position != climbHopSolidPosition)
            {
                var move = climbHopSolid.Position - climbHopSolidPosition;
                climbHopSolidPosition = climbHopSolid.Position;
                MoveHExact((int) move.x);
                MoveVExact((int) move.y);
            }

            //Wind
            if (noWindTimer > 0)
                noWindTimer -= Time.deltaTime;

            //Facing
            if (moveX != 0 && InControl
                           && StateMachine.State != StClimb && StateMachine.State != StPickup &&
                           StateMachine.State != StRedDash && StateMachine.State != StHitSquash)
            {
                var to = (Facings) moveX;
                if (to != Facing && Ducking)
                    Sprite.Scale = new Vector2(0.8f, 1.2f);
                Facing = to;
            }

            //Aiming
            lastAim = Input.GetAimVector(Facing);

            //Wall Speed Retention
            if (wallSpeedRetentionTimer > 0)
            {
                if (Math.Sign(Speed.x) == -Math.Sign(wallSpeedRetained))
                    wallSpeedRetentionTimer = 0;
                else if (!CollideCheck<Solid>(Position + Vector2.right * Math.Sign(wallSpeedRetained)))
                {
                    Speed.x = wallSpeedRetained;
                    wallSpeedRetentionTimer = 0;
                }
                else
                    wallSpeedRetentionTimer -= Time.deltaTime;
            }

            //Hop Wait X
            if (hopWaitX != 0)
            {
                if (Math.Sign(Speed.x) == -hopWaitX || Speed.y > 0)
                    hopWaitX = 0;
                else if (!CollideCheck<Solid>(Position + Vector2.right * hopWaitX))
                {
                    Speed.x = hopWaitXSpeed;
                    hopWaitX = 0;
                }
            }

            // Wind Timeout
            if (windTimeout > 0)
                windTimeout -= Time.deltaTime;

            // Hair
            {
                var windDir = windDirection;
                if (ForceStrongWindHair.magnitude > 0)
                    windDir = ForceStrongWindHair;

                if (windTimeout > 0 && windDir.x != 0)
                {
                    windHairTimer += Time.deltaTime * 8f;

                    Hair.StepPerSegment = new Vector2(windDir.x * 5f, (float) Math.Sin(windHairTimer));
                    Hair.StepInFacingPerSegment = 0f;
                    Hair.StepApproach = 128f;
                    Hair.StepYSinePerSegment = 0;
                }
                else if (Dashes > 1)
                {
                    Hair.StepPerSegment = new Vector2((float) Math.Sin(Scene.TimeActive * 2) * 0.7f - (int) Facing * 3,
                        (float) Math.Sin(Scene.TimeActive * 1f));
                    Hair.StepInFacingPerSegment = 0f;
                    Hair.StepApproach = 90f;
                    Hair.StepYSinePerSegment = 1f;

                    Hair.StepPerSegment.y += windDir.y * 2f;
                }
                else
                {
                    Hair.StepPerSegment = new Vector2(0, 2f);
                    Hair.StepInFacingPerSegment = 0.5f;
                    Hair.StepApproach = 64f;
                    Hair.StepYSinePerSegment = 0;

                    Hair.StepPerSegment.y += windDir.y * 0.5f;
                }
            }

            if (StateMachine.State == StRedDash)
                Sprite.HairCount = 1;
            else if (StateMachine.State != StStarFly)
                Sprite.HairCount = (Dashes > 1 ? 5 : startHairCount);

            //Min Hold Time
            if (minHoldTimer > 0)
                minHoldTimer -= Time.deltaTime;

            //Launch Particles
            if (launched)
            {
                var sq = Speed.LengthSquared();
                if (sq < LaunchedMinSpeedSq)
                    launched = false;
                else
                {
                    var was = launchedTimer;
                    launchedTimer += Time.deltaTime;

                    if (launchedTimer >= .5f)
                    {
                        launched = false;
                        launchedTimer = 0;
                    }
                    else if (Calc.OnInterval(launchedTimer, was, .15f))
                        level.Add(Engine.Pooler.Create<SpeedRing>().Init(Center, Speed.Angle(), Color.white));
                }
            }
            else
                launchedTimer = 0;
        }

        if (IsTired)
        {
            // Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
            if (!wasTired)
            {
                wasTired = true;
            }
        }
        else
            wasTired = false;

        base.Update();

        //Light Offset
        if (Ducking)
            Light.Position = duckingLightOffset;
        else
            Light.Position = normalLightOffset;

        //Jump Thru Assist
        if (!onGround && Speed.y <= 0 && (StateMachine.State != StClimb || lastClimbMove == -1) &&
            CollideCheck<JumpThru>() && !JumpThruBoostBlockedCheck())
            MoveV(JumpThruAssistSpeed * Time.deltaTime);

        //Dash Floor Snapping
        if (!onGround && DashAttacking && DashDir.y == 0)
        {
            if (CollideCheck<Solid>(Position + Vector2.up * DashVFloorSnapDist) ||
                CollideCheckOutside<JumpThru>(Position + Vector2.up * DashVFloorSnapDist))
                MoveVExact(DashVFloorSnapDist);
        }

        //Falling unducking
        if (Speed.y > 0 && CanUnDuck && Collider != starFlyHitbox && !onGround)
            Ducking = false;

        //Physics
        if (StateMachine.State != StDreamDash && StateMachine.State != StAttract)
            MoveH(Speed.x * Time.deltaTime, onCollideH);
        if (StateMachine.State != StDreamDash && StateMachine.State != StAttract)
            MoveV(Speed.y * Time.deltaTime, onCollideV);

        //Swimming
        if (StateMachine.State == StSwim)
        {
            //Stay at water surface
            if (Speed.y < 0 && Speed.y >= SwimMaxRise)
            {
                while (!SwimCheck())
                {
                    Speed.y = 0;
                    if (MoveVExact(1))
                        break;
                }
            }
        }
        else if (StateMachine.State == StNormal && SwimCheck())
            StateMachine.State = StSwim;
        else if (StateMachine.State == StClimb && SwimCheck())
        {
            var water = CollideFirst<Water>(Position);
            if (water != null && Center.y < water.Center.y)
            {
                while (SwimCheck())
                    if (MoveVExact(-1))
                        break;
                if (SwimCheck())
                    StateMachine.State = StSwim;
            }
            else
                StateMachine.State = StSwim;
        }

        // wall slide SFX
        {
            var isSliding = Sprite.CurrentAnimationID != null &&
                            Sprite.CurrentAnimationID.Equals(PlayerSprite.WallSlide) && Speed.y > 0;
            if (isSliding)
            {
                if (!wallSlideSfx.Playing)
                    Loop(wallSlideSfx, Sfxs.char_mad_wallslide);

                var platform =
                    SurfaceIndex.GetPlatformByPriority(CollideAll<Solid>(Center + Vector2.right * (int) Facing, temp));
                if (platform != null)
                    wallSlideSfx.Param(SurfaceIndex.Param, platform.GetWallSoundIndex(this, (int) Facing));
            }
            else
                Stop(wallSlideSfx);
        }

        // update sprite
        UpdateSprite();

        //Carry held item
        UpdateCarry();

        //Triggers
        if (StateMachine.State != StReflectionFall)
        {
            foreach (Trigger trigger in Scene.Tracker.GetEntities<Trigger>())
            {
                if (CollideCheck(trigger))
                {
                    if (!trigger.Triggered)
                    {
                        trigger.Triggered = true;
                        triggersInside.Add(trigger);
                        trigger.OnEnter(this);
                    }

                    trigger.OnStay(this);
                }
                else if (trigger.Triggered)
                {
                    triggersInside.Remove(trigger);
                    trigger.Triggered = false;
                    trigger.OnLeave(this);
                }
            }
        }

        //Strawberry Block
        StrawberriesBlocked = CollideCheck<BlockField>();

        // Camera (lerp by distance using delta-time)
        if (InControl || ForceCameraUpdate)
        {
            if (StateMachine.State == StReflectionFall)
            {
                level.Camera.Position = CameraTarget;
            }
            else
            {
                var from = level.Camera.Position;
                var target = CameraTarget;
                var multiplier = StateMachine.State == StTempleFall ? 8 : 1f;

                level.Camera.Position =
                    from + (target - from) * (1f - (float) Math.Pow(0.01f / multiplier, Time.deltaTime));
            }
        }

        //Player Colliders
        if (!Dead && StateMachine.State != StCassetteFly)
        {
            Collider was = Collider;
            Collider = hurtbox;

            foreach (PlayerCollider pc in Scene.Tracker.GetComponents<PlayerCollider>())
            {
                if (pc.Check(this) && Dead)
                {
                    Collider = was;
                    return;
                }
            }

            // If the current collider is not the hurtbox we set it to, that means a collision callback changed it. Keep the new one!
            bool keepNew = (Collider != hurtbox);

            if (!keepNew)
                Collider = was;
        }

        //Bounds
        if (InControl && !Dead && StateMachine.State != StDreamDash)
            level.EnforceBounds(this);

        UpdateChaserStates();
        UpdateHair(true);

        //Sounds on ducking state change
        if (wasDucking != Ducking)
        {
            wasDucking = Ducking;
            if (wasDucking)
            {
                // Play(Sfxs.char_mad_duck);
            }
            else if (onGround)
            {
                // Play(Sfxs.char_mad_stand);
            }
        }

        // shallow swim sfx
        if (Speed.x != 0 && ((StateMachine.State == StSwim && !SwimUnderwaterCheck()) ||
                             (StateMachine.State == StNormal && CollideCheck<Water>(Position))))
        {
            if (!swimSurfaceLoopSfx.Playing)
                swimSurfaceLoopSfx.Play(Sfxs.char_mad_water_move_shallow);
        }
        else
            swimSurfaceLoopSfx.Stop();

        wasOnGround = onGround;
    }
}