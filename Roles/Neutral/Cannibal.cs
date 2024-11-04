using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using static TOHE.Translator;
using TOHE.Roles.Core;
using InnerNet;

namespace TOHE.Roles.Neutral;

internal class Cannibal : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 15700;
    private static readonly HashSet<byte> PlayerIds = [];
    public static bool HasEnabled => PlayerIds.Any();
    
    public override CustomRoles ThisRoleBase => CustomRoles.Imposter;
    public override Custom_RoleType ThisRoleType => Custom_RoleType.NeutralKilling;
    //==================================================================\\

    private static OptionItem CannibalKillCooldown;
    private static OptionItem CanVent;
    private static OptionItem ImpostorVision;

    public static readonly HashSet<byte> KilledPlayersId = [];

    public override void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Cannibal);
        CannibalKillCooldown = FloatOptionItem.Create(Id + 10, GeneralOption.KillCooldown, new(5f, 180f, 2.5f), 40f, TabGroup.NeutralRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Cannibal])
            .SetValueFormat(OptionFormat.Seconds);
        CanVent = BooleanOptionItem.Create(Id + 11, GeneralOption.CanVent, true, TabGroup.NeutralRoles, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Cannibal]);
        ImpostorVision = BooleanOptionItem.Create(Id + 12, GeneralOption.ImpostorVision, true, TabGroup.NeutralRoles, false)
            .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Cannibal]);
    }
    public override bool CanUseImpostorVentButton(PlayerControl pc) => CanVent.GetBool();
    public override void Init()
    {
        PlayerIds.Clear();
        KilledPlayersId.Clear();
    }
    public override void Add(byte playerId)
    {
        PlayerIds.Add(playerId);
    }

    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = CannibalKillCooldown.GetFloat();

    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        target.RpcTeleport(ExtendedPlayerControl.GetBlackRoomPosition());
        if (target.Is(CustomRoles.Bait)) return true;
        KilledPlayersId.Add(target.PlayerId);

        _ = new LateTask(
            () =>
            {
                target.RpcMurderPlayer(target);
                target.SetRealKiller(killer);
                RPC.PlaySoundRPC(killer.PlayerId, Sounds.KillSound);
                target.Notify(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Cannibal), Translator.GetString("KilledByCannibal")), time: 8f);
            },
            0.5f, "Cannibal Kill");
        
        killer.SetKillCooldown();
        return false;
    }

    public override bool OnCheckReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo deadBody, PlayerControl killer)
        => !killer.Is(CustomRoles.Cannibal);

    public override void SetAbilityButtonText(HudManager hud, byte playerId)
    {
        hud.KillButton.OverrideText(GetString("CannibalButtonText"));
    }
    public override Sprite GetKillButtonSprite(PlayerControl player, bool shapeshifting) => CustomButton.Get("Vulture");
}
