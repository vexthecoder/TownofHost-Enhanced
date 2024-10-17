using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using TOHE.Modules;
using TOHE.Roles.Core;
using UnityEngine;
using System.Collections.Generic;

namespace TOHE.Roles.Neutral
{
    internal class Cannibal : RoleBase
    {
        //===========================SETUP================================\\
        private const int Id = 15700;
        public static bool HasEnabled => CustomRoleManager.HasEnabled(CustomRoles.Cannibal);
        public override bool IsDesyncRole => true;
        public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
        public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
        //==================================================================\\

        private static OptionItem KillCooldown;
        private static OptionItem HasImpostorVision;
        private static OptionItem CanVent;

        private static readonly Dictionary<byte, HashSet<byte>> eatenList = new();
        private static readonly Dictionary<byte, float> originalSpeed = new();

        public override void SetupCustomOption()
        {
            Options.SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Cannibal, 1, zeroOne: false);
            KillCooldown = FloatOptionItem.Create(Id + 10, "CannibalKillCooldown", new(0f, 180f, 2.5f), 25f, TabGroup.NeutralRoles, false)
                .SetValueFormat(OptionFormat.Seconds);
            CanVent = BooleanOptionItem.Create(Id + 11, "CanVent", false, TabGroup.NeutralRoles, false);
            HasImpostorVision = BooleanOptionItem.Create(Id + 12, "ImpostorVision", true, TabGroup.NeutralRoles, false);
        }

        public override void Init()
        {
            eatenList.Clear();
            originalSpeed.Clear();
        }

        public override void Remove(byte playerId)
        {
            ReturnEatenPlayerBack(Utils.GetPlayerById(playerId));
        }

        public override void ApplyGameOptions(IGameOptions opt, byte id)
        {
            opt.SetVision(HasImpostorVision.GetBool());
        }

        public override bool CanUseKillButton(PlayerControl pc) => true;

        public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();

        public override void SetAbilityButtonText(HudManager hud, byte playerId)
        {
            hud.KillButton.OverrideText("Eat");
        }

        public override Sprite GetKillButtonSprite(PlayerControl player, bool shapeshifting) => CustomButton.Get("Eat");

        public static bool CanEat(PlayerControl cannibal, byte targetId)
        {
            if (!cannibal.Is(CustomRoles.Cannibal) || GameStates.IsMeeting) return false;
            var target = Utils.GetPlayerById(targetId);
            return target != null && target.CanBeTeleported() && !IsEaten(cannibal, targetId);
        }

        private void EatPlayer(PlayerControl cannibal, PlayerControl target)
        {
            if (cannibal == null || target == null) return;
            if (!eatenList.ContainsKey(cannibal.PlayerId)) eatenList.Add(cannibal.PlayerId, new HashSet<byte>());
            eatenList[cannibal.PlayerId].Add(target.PlayerId);
            originalSpeed[target.PlayerId] = Main.AllPlayerSpeed[target.PlayerId];
            target.RpcTeleport(GetBlackRoomPosition());
            KillEatenPlayer(cannibal, target);
        }

        private void KillEatenPlayer(PlayerControl cannibal, PlayerControl target)
        {
            Main.AllPlayerSpeed[target.PlayerId] = originalSpeed[target.PlayerId];
            target.MurderPlayer();
            Logger.Info($"{cannibal.GetRealName()} ate and killed {target.GetRealName()}");
        }

        public static Vector2 GetBlackRoomPosition()
        {
            return Utils.GetActiveMapId() switch
            {
                0 => new Vector2(-54f, 6.6f), // The Skeld
                1 => new Vector2(-22.8f, 16.4f), // MIRA HQ
                2 => new Vector2(85.2f, -39.8f), // Polus
                3 => new Vector2(54f, 6.6f), // dlekS ehT
                4 => new Vector2(-33.6f, -12.4f), // Airship
                5 => new Vector2(19.2f, 46.4f), // The Fungle
                _ => throw new System.NotImplementedException(),
            };
        }

        public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
        {
            if (CanEat(killer, target.PlayerId))
            {
                EatPlayer(killer, target);
                killer.SetKillCooldown();
                return false;
            }
            return true;
        }
    }
}
