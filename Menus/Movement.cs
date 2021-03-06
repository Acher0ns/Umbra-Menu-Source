﻿using System;
using System.Collections.Generic;
using UnityEngine;
using RoR2;


namespace UmbraMenu.Menus
{
    public sealed class Movement : Menu
    {
        private static readonly IMenu movement = new NormalMenu(2, new Rect(374, 560, 20, 20), "MOVEMENT MENU");

        public static bool jumpPackToggle, flightToggle, alwaysSprintToggle;
        public static int jumpPackMul = 1;

        public Button toggleAlwaysSprint;
        public Button toggleFlight;
        public Button toggleJumpPack;

        public Movement() : base(movement)
        {
            if (UmbraMenu.characterCollected)
            {
                toggleAlwaysSprint = new Button(new TogglableButton(this, 1, "ALWAYS SPRINT : OFF", "ALWAYS SPRINT : ON", ToggleSprint, ToggleSprint));
                toggleFlight = new Button(new TogglableButton(this, 2, "FLIGHT : OFF", "FLIGHT : ON", ToggleFlight, ToggleFlight));
                toggleJumpPack = new Button(new TogglableButton(this, 3, "JUMP PACK : OFF", "JUMP PACK : ON", ToggleJump, ToggleJump));

                AddButtons(new List<Button>()
                {
                    toggleAlwaysSprint,
                    toggleFlight,
                    toggleJumpPack
                });
                SetActivatingButton(Utility.FindButtonById(0, 2));
                SetPrevMenuId(0);
            }
        }

        public override void Draw()
        {
            if (IsEnabled())
            {
                SetWindow();
                base.Draw();
            }
        }

        public override void Reset()
        {
            jumpPackToggle = false;
            flightToggle = false;
            alwaysSprintToggle = false;
            jumpPackMul = 1;
            base.Reset();
        }

        private static void ToggleFlight()
        {
            flightToggle = !flightToggle;
        }

        private static void ToggleSprint()
        {
            alwaysSprintToggle = !alwaysSprintToggle;
        }

        private static void ToggleJump()
        {
            jumpPackToggle = !jumpPackToggle;
        }

        public static void AlwaysSprint()
        {
            var isMoving = UmbraMenu.LocalNetworkUser.inputPlayer.GetAxis("MoveVertical") != 0f || UmbraMenu.LocalNetworkUser.inputPlayer.GetAxis("MoveHorizontal") != 0f;
            var localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser == null || localUser.cachedMasterController == null || localUser.cachedMasterController.master == null) return;

            var controller = localUser.cachedMasterController;
            var body = controller.master.GetBody();
            if (body && !body.isSprinting && !localUser.inputPlayer.GetButton("Sprint"))
            {
                if (isMoving)
                {
                    body.isSprinting = true;
                }
            }
        }

        public static void Flight()
        {
            try
            {
                if (Utility.GetCurrentCharacter().ToString() != "Loader")
                {
                    UmbraMenu.LocalPlayerBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                }

                var forwardDirection = UmbraMenu.LocalPlayerBody.GetComponent<InputBankTest>().moveVector.normalized;
                var aimDirection = UmbraMenu.LocalPlayerBody.GetComponent<InputBankTest>().aimDirection.normalized;
                var upDirection = UmbraMenu.LocalPlayerBody.GetComponent<InputBankTest>().moveVector.y + 1;
                var downDirection = UmbraMenu.LocalPlayerBody.GetComponent<InputBankTest>().moveVector.y - 1;
                var isForward = Vector3.Dot(forwardDirection, aimDirection) > 0f;

                var isSprinting = alwaysSprintToggle ? UmbraMenu.LocalPlayerBody.isSprinting : UmbraMenu.LocalNetworkUser.inputPlayer.GetButton("Sprint");
                var isJumping = UmbraMenu.LocalNetworkUser.inputPlayer.GetButton("Jump");
                var isGoingDown = Input.GetKey(KeyCode.X);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                var isStrafing = UmbraMenu.LocalNetworkUser.inputPlayer.GetAxis("MoveVertical") != 0f;

                if (isSprinting)
                {
                    if (!alwaysSprintToggle && !UmbraMenu.LocalNetworkUser.inputPlayer.GetButton("Sprint"))
                    {
                        UmbraMenu.LocalPlayerBody.isSprinting = false;
                    }

                    UmbraMenu.LocalPlayerBody.characterMotor.velocity = forwardDirection * 100f;
                    UmbraMenu.LocalPlayerBody.characterMotor.velocity.y = upDirection * 0.510005f;
                    if (isStrafing)
                    {
                        if (isForward)
                        {
                            UmbraMenu.LocalPlayerBody.characterMotor.velocity.y = aimDirection.y * 100f;
                        }
                        else
                        {
                            UmbraMenu.LocalPlayerBody.characterMotor.velocity.y = aimDirection.y * -100f;
                        }
                    }
                }
                else
                {
                    UmbraMenu.LocalPlayerBody.characterMotor.velocity = forwardDirection * 50;
                    UmbraMenu.LocalPlayerBody.characterMotor.velocity.y = upDirection * 0.510005f;
                    if (isStrafing)
                    {
                        if (isForward)
                        {
                            UmbraMenu.LocalPlayerBody.characterMotor.velocity.y = aimDirection.y * 50;
                        }
                        else
                        {
                            UmbraMenu.LocalPlayerBody.characterMotor.velocity.y = aimDirection.y * -50;
                        }
                    }
                }
                if (isJumping)
                {
                    UmbraMenu.LocalPlayerBody.characterMotor.velocity.y = upDirection * 100;
                }
                if (isGoingDown)
                {
                    UmbraMenu.LocalPlayerBody.characterMotor.velocity.y = downDirection * 100;
                }
            }
            catch (NullReferenceException) {

                Debug.Log("Movement1");
            }
        }

        public static void JumpPack()
        {
            try
            {
                if (Utility.GetCurrentCharacter().ToString() != "Loader")
                {
                    UmbraMenu.LocalPlayerBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                }

                var forwardDirection = UmbraMenu.LocalPlayerBody.GetComponent<InputBankTest>().moveVector.normalized;
                var aimDirection = UmbraMenu.LocalPlayerBody.GetComponent<InputBankTest>().aimDirection.normalized;
                var upDirection = UmbraMenu.LocalPlayerBody.GetComponent<InputBankTest>().moveVector.y + 1;
                var downDirection = UmbraMenu.LocalPlayerBody.GetComponent<InputBankTest>().moveVector.y - 1;
                var isForward = Vector3.Dot(forwardDirection, aimDirection) > 0f;

                var isJumping = UmbraMenu.LocalNetworkUser.inputPlayer.GetButton("Jump");
                // ReSharper disable once CompareOfFloatsByEqualityOperator

                if (isJumping)
                {
                    UmbraMenu.LocalPlayerBody.characterMotor.velocity.y = upDirection += 0.75f * jumpPackMul;
                    jumpPackMul++;

                    if (jumpPackMul > 200)
                    {
                        jumpPackMul = 200;
                    }
                }
                else
                {
                    jumpPackMul = 1;
                }
            }
            catch (NullReferenceException) 
            {
                Debug.Log("Jump - Pack is throwing a NullReferenceException");
            }
        }
    }
}
