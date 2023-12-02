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
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Admin;
using System.Drawing;

public static class Lib
{
    // TODO: i dont think there is a builtin func for this...
    static public void print_centre_all(String str)
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.is_valid())
            {
                continue;
            }

            player.PrintToCenter(str);
        }
    }

    static public void print_console_all(String str)
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(!player.is_valid())
            {
                continue;
            }

            player.PrintToConsole(str);
        }
    }

    static public void slay(this CCSPlayerController? player)
    {
        if(player != null && player.is_valid_alive())
        {
            player.PlayerPawn.Value?.CommitSuicide(true, true);
        }
    }

    // Cheers Kill for suggesting method extenstions
    static public bool is_valid(this CCSPlayerController? player)
    {
        return player != null && player.IsValid &&  player.PlayerPawn.IsValid;
    }

    static public bool is_t(this CCSPlayerController? player)
    {
        return player != null && is_valid(player) && player.TeamNum == TEAM_T;
    }

    static public bool is_ct(this CCSPlayerController? player)
    {
        return player != null && is_valid(player) && player.TeamNum == TEAM_CT;
    }

    // yes i know the null check is redundant but C# is dumb
    static public bool is_valid_alive(this CCSPlayerController? player)
    {
        return player != null && player.is_valid() && player.PawnIsAlive && player.get_health() > 0;
    }

    static public CCSPlayerPawn? pawn(this CCSPlayerController? player)
    {
        if(player == null || !player.is_valid())
        {
            return null;
        }

        CCSPlayerPawn? pawn = player.PlayerPawn.Value;

        return pawn;
    }

    static public void set_health(this CCSPlayerController? player, int hp)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null)
        {
            pawn.Health = hp;
        }
    }

    static public bool is_windows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    static public int get_health(this CCSPlayerController? player)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn == null)
        {
            return 100;
        }

        return pawn.Health;
    }

    static public void set_movetype(this CCSPlayerController? player, MoveType_t type)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null)
        {
            pawn.MoveType = type;
        }
    }

    static public void set_gravity(this CCSPlayerController? player, float value)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null)
        {
            pawn.GravityScale = value;
        }
    }

    static public void set_velocity(this CCSPlayerController? player, float value)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null)
        {
            pawn.Speed = value;
        }
    }


    static public void set_armour(this CCSPlayerController? player, int hp)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null)
        {
            pawn.ArmorValue = hp;
        }
    }

    static public void strip_weapons(this CCSPlayerController? player, bool remove_knife = false)
    {
        // only care if player is valid
        if(player == null || !player.is_valid_alive())
        {
            return;
        }

        player.RemoveWeapons();
    
        // dont remove knife its buggy
        if(!remove_knife)
        {
            player.GiveNamedItem("weapon_knife");
        }
    }

    static public void set_colour(this CCSPlayerController? player, Color colour)
    {
        CCSPlayerPawn? pawn = player.pawn();

        if(pawn != null)
        {
            pawn.RenderMode = RenderMode_t.kRenderTransColor;
            pawn.Render = colour;
        }
    }

    static public bool is_generic_admin(this CCSPlayerController? player)
    {
        if(player == null || !player.is_valid())
        {
            return false;
        }

        return AdminManager.PlayerHasPermissions(player,new String[] {"@css/generic"});
    }

    static public void play_sound(this CCSPlayerController? player, String sound)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        player.ExecuteClientCommand($"play {sound}");
    }

    static public CCSPlayerController? player(this CEntityInstance? instance)
    {
        if(instance == null)
        {
            return null;
        }

        // grab the pawn index
        int player_index = (int)instance.Index;

        // grab player controller from pawn
        CCSPlayerPawn? player_pawn =  Utilities.GetEntityFromIndex<CCSPlayerPawn>(player_index);

        // pawn valid
        if(player_pawn == null || !player_pawn.IsValid)
        {
            return null;
        }

        // controller valid
        if(player_pawn.OriginalController == null || !player_pawn.OriginalController.IsValid)
        {
            return null;
        }

        // any further validity is up to the caller
        return player_pawn.OriginalController.Value;
    }

    static public CCSPlayerController? player(this CHandle<CBaseEntity> handle)
    {
        if(handle.IsValid)
        {
            CBaseEntity? ent = handle.Value;

            if(ent != null)
            {
                return handle.Value.player();
            }
        }

        return null;
    }

    static public void mute(this CCSPlayerController? player)
    {
        // admins cannot be muted by the plugin
        if(!player.is_generic_admin())
        {

        }
    }

    // TODO: this needs to be hooked into the ban system that becomes used
    static public void unmute(this CCSPlayerController? player)
    {

    }

    static public void mute_all()
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.is_valid())
            {
                player.mute();
            }
        }
    }



    static public void kill_timer(ref CSTimer.Timer? timer)
    {
        if(timer != null)
        {
            timer.Kill();
            timer = null;
        }
    }

    static public void unmute_all()
    {
        foreach(CCSPlayerController player in Utilities.GetPlayers())
        {
            if(player.is_valid())
            {
                player.unmute();
            }
        }
    }

    static public bool is_valid(this CBasePlayerWeapon? weapon)
    {
        return weapon != null && weapon.IsValid;
    }

    static public CBasePlayerWeapon? find_weapon(this CCSPlayerController? player, String name)
    {
        // only care if player is alive
        if(!player.is_valid_alive())
        {
            return null;
        }

        CCSPlayerPawn? pawn = player.pawn();

        if(pawn == null)
        {
            return null;
        }

        var weapons = pawn.WeaponServices?.MyWeapons;

        if(weapons == null)
        {
            return null;
        }

        foreach (var weapon_opt in weapons)
        {
            CBasePlayerWeapon? weapon = weapon_opt.Value;

            if(weapon == null)
            {
                continue;
            }
         
            if(weapon.DesignerName.Contains(name))
            {
                return weapon;
            }
        }

        return null;
    }

    static public void set_ammo(this CBasePlayerWeapon? weapon, int clip, int reserve)
    {
        if(weapon == null || !weapon.is_valid())
        {
            return;
        }

        weapon.Clip1 = clip;
        weapon.ReserveAmmo[0] = reserve;
    }

    public static void restore_hp(CCSPlayerController? player, int damage, int health)
    {
        if(player == null || !player.is_valid())
        {
            return;
        }

        // TODO: why does this sometimes mess up?
        if(health < 100)
        {
            player.set_health(Math.Min(health + damage,100));
        }

        else
        {
            player.set_health(health + damage);
        }
    }

    static void give_menu_weapon(CCSPlayerController player, ChatMenuOption option)
    {
        if(!player.is_valid())
        {
            return;
        }

        strip_weapons(player);

        player.GiveNamedItem("weapon_" + option.Text);
        player.GiveNamedItem("weapon_deagle");

        player.GiveNamedItem("item_assaultsuit");
    }

    static String[] GUN_LIST =
    {	
        "ak47", "m4a1_silencer","nova",
        "p90", "m249", "mp5sd",
        "galilar", "sg556","bizon", "aug",
        "famas", "xm1014","ssg08","awp"
        
    };
    
    static public void gun_menu_internal(this CCSPlayerController? player, bool no_awp, Action<CCSPlayerController, ChatMenuOption> callback)
    {
        // player must be alive and active!
        if(player == null || !player.is_valid_alive())
        {
            return;
        } 

    
        var gun_menu = new ChatMenu("Gun Menu");

        foreach(var weapon_name in GUN_LIST)
        {
            if(no_awp && weapon_name == "awp")
            {
                continue;
            }

            gun_menu.AddMenuOption(weapon_name, callback);
        }

        ChatMenus.OpenMenu(player, gun_menu);
    }

    static public void gun_menu(this CCSPlayerController? player, bool no_awp)
    {
        gun_menu_internal(player,no_awp,give_menu_weapon);
    }

    // chat + centre text print
    static public void announce(String prefix,String str)
    {
        Server.PrintToChatAll(prefix + str);
        print_centre_all(str);
    }

    static public void announce(this CCSPlayerController? player,String prefix,String str)
    {
        if(player != null && player.is_valid())
        {
            player.PrintToChat(prefix + str);
            player.PrintToCenter(str);
        }
    }

    static public void enable_friendly_fire()
    {
        if(ff != null)
        {
            ff.SetValue(true);
        }
    }

    static public void disable_friendly_fire()
    {
        if(ff != null)
        {
            ff.SetValue(false);
        }
    }

    static public List<CCSPlayerController> get_alive_ct()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        return players.FindAll(player => player.is_valid_alive() && player.is_ct());
    }

    static public int ct_count()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        return players.FindAll(player => player.is_valid() && player.is_ct()).Count;        
    }

    static public int t_count()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        return players.FindAll(player => player.is_valid() && player.is_t()).Count;        
    }

    static public int alive_ct_count()
    {
        return get_alive_ct().Count;
    }

    static public List<CCSPlayerController> get_alive_t()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        return players.FindAll(player => player.is_valid_alive() && player.is_t());;
    }

    static public int alive_t_count()
    {
        return get_alive_t().Count;
    }

    static public bool block_enabled()
    {
        if(block_cvar != null)
        {
            return block_cvar.GetPrimitiveValue<int>() == 1;
        }

        return true;
    }

    static public void block_all()
    {
        if(block_cvar != null)
        {
            block_cvar.SetValue(1);
        }
    }

    static public void unblock_all()
    {
        if(block_cvar != null)
        {
            block_cvar.SetValue(0);
        }
    }

    
    static public void set_cvar_str(String name, String value)
    {
        // why doesn't this work lol
        
        ConVar? cvar = ConVar.Find(name);

        if(cvar != null)
        {
            cvar.StringValue = value;
        }
    }

    public static int? to_slot(int? user_id)
    {
        if(user_id == null)
        {
            return null;
        }

        return user_id & 0xff;
    }

    public static int? slot(this CCSPlayerController? player)
    {
        if(player == null)
        {
            return null;
        }

        return to_slot(player.UserId);
    }

    public static void force_open()
    {
        announce("[Door control]: ","Forcing open all doors!");

        /*
        // search for door entitys and open all of them!
        var doors = Utilities.FindAllEntitiesByDesignerName<CBaseDoor>("func_door");

        */
    }

    static public bool active_team(int team)
    {
        return (team == Lib.TEAM_T || team == Lib.TEAM_CT);
    }

    public static readonly Color CYAN = Color.FromArgb(255, 153, 255, 255);


    static ConVar? block_cvar = ConVar.Find("mp_solid_teammates");
    static ConVar? ff = ConVar.Find("mp_teammates_are_enemies");

    // CONST DEFS
    public const int TEAM_SPEC = 1;
    public const int TEAM_T = 2;
    public const int TEAM_CT = 3;

    public const int HITGROUP_HEAD = 0x1;
}