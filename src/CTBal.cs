
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

using System.Text.Json.Serialization;
public class BalConfig : BasePluginConfig
{
    // how many cts for every t
    [JsonPropertyName("ratio")]
    public double ratio {get; set;} = 0.5;
}


public class BalPlugin : BasePlugin, IPluginConfig<BalConfig>
{
    public override string ModuleName => "CS2 ct ratio plugin - destoer";

    public override string ModuleVersion => "v0.0.1";

    public BalConfig Config  { get; set; } = new BalConfig();

    public override void Load(bool hotReload)
    {
        register_commands();
    }

    public void OnConfigParsed(BalConfig config)
    {
        this.Config = config;
    }

    void register_commands()
    {
        // reg team hook
        AddCommand("jointeam","boop",join_team);
    }

    static readonly String TEAM_PREFIX = $" {ChatColors.Green}[TEAM]: {ChatColors.White}";

    void respawn_callback(int? slot)
    {
        if(slot != null)
        {
            var player = Utilities.GetPlayerFromSlot(slot.Value);

            if(player != null && player.is_valid())
            {
                player.Respawn();
            }
        }   
    }

    public void respawn_delay(CCSPlayerController? player, float delay)
    {
        AddTimer(delay,() => respawn_callback(player.slot()),CSTimer.TimerFlags.STOP_ON_MAPCHANGE);
    }

    public void join_team(CCSPlayerController? invoke, CommandInfo command)
    {
        if(invoke == null || !invoke.is_valid())
        {
            return;
        }

        if(command.ArgCount != 3)
        {
            invoke.SwitchTeam(CsTeam.Terrorist);
            invoke.announce(TEAM_PREFIX,"You cannot join that team");
            return;
        }

        CCSPlayerPawn? pawn = invoke.pawn(); 

        int old_team = -1;

        if(pawn != null)
        {
            old_team = pawn.TeamNum;
        }


        if(!Int32.TryParse(command.ArgByIndex(1),out int team))
        {
            return;
        }

        if(old_team == team)
        {
            //Server.PrintToChatAll("no switch");
            invoke.play_sound("sounds/ui/counter_beep.vsnd");
            return;
        }


        double ct_count = Lib.ct_count();
        double t_count = Lib.t_count();
        double ratio = Config.ratio;

        bool empty = ct_count == 0 || t_count == 0 || ratio == 0.0;

        switch(team)
        {
            // T  CT
            // numbers: 1, 1
            // raito:   5, 4
            //  T's for every ct
            // 5 < 4 1?

            case Lib.TEAM_CT:
            {
                //Server.PrintToChatAll($"({ct_count}:{t_count}) {ct_count} < {t_count} * {ratio} = {t_count * ratio}");

                // check CT aint full 
                // i.e at a suitable raito or either team is empty
                if(ct_count < (t_count * ratio) || empty)
                {
                    invoke.SwitchTeam(CsTeam.CounterTerrorist);
                }

                // switch to T
                else
                {
                    invoke.SwitchTeam(CsTeam.Terrorist);
                    invoke.announce(TEAM_PREFIX,$"Sorry, CT has too many players {ratio} CT for every T");
                    invoke.play_sound("sounds/ui/counter_beep.vsnd");

                    // update to actual switch
                    team = Lib.TEAM_T;
                }
                
                break;
            }

            case Lib.TEAM_T:
            {
                //Server.PrintToChatAll($"({ct_count}:{t_count}) {t_count} < {ct_count} * { 1 / ratio} = {ct_count *  (1 / ratio)}");

                // check T aint full 
                // i.e at a suitable raito or either team is empty
                if(t_count < (ct_count * ( 1 / ratio)) || empty)
                {
                    invoke.SwitchTeam(CsTeam.Terrorist);
                }

                // switch to CT
                else
                {
                    invoke.SwitchTeam(CsTeam.CounterTerrorist);
                    invoke.announce(TEAM_PREFIX,$"Sorry, T has too many players {ratio} T for every CT");
                    invoke.play_sound("sounds/ui/counter_beep.vsnd");

                    // update to actual switch
                    team = Lib.TEAM_CT;
                }
                break;
            }

            // spec
            case Lib.TEAM_SPEC:
            {
                invoke.SwitchTeam(CsTeam.Spectator);
                break;
            }

            default:
            {
                invoke.announce(TEAM_PREFIX,"You cannot join that team");
                invoke.play_sound("sounds/ui/counter_beep.vsnd");
                break;
            }
        }

        bool alive = invoke.is_valid_alive();

        // team has changed between active
        // make sure the player cannot just switch teams in a spawn
        if(Lib.active_team(old_team) && Lib.active_team(team))
        {
            Server.ExecuteCommand("mp_autokick 0");
            invoke.slay();

            respawn_delay(invoke,1.0f);
        }
    }
}
