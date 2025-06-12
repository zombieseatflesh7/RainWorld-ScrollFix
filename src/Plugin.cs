using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Security.Permissions;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace ScrollFix;

[BepInPlugin("zombieseatflesh7.ScrollFix", "Scroll Fix", "1.0.1")]
sealed class Plugin : BaseUnityPlugin
{
    public static new BepInEx.Logging.ManualLogSource Logger;

    public void OnEnable()
    {
        Logger = base.Logger;

        On.MainLoopProcess.RawUpdate += MainLoopProcess_RawUpdate;
        IL.Menu.Menu.Update += Menu_UpdateIL;
    }

    private void MainLoopProcess_RawUpdate(On.MainLoopProcess.orig_RawUpdate orig, MainLoopProcess self, float dt)
    {
        // update scroll delta
        if (self.manager.currentMainLoop == self && self.manager.menuesMouseMode)
        {
            if (self is Menu.Menu)
                (self as Menu.Menu).floatScrollWheel += Input.GetAxis("Mouse ScrollWheel");
            else if (self is RainWorldGame && (self as RainWorldGame).pauseMenu != null)
                (self as RainWorldGame).pauseMenu.floatScrollWheel += Input.GetAxis("Mouse ScrollWheel");
        }
        orig(self, dt);
    }

    private void Menu_UpdateIL(ILContext il)
    {
        ILCursor c = new(il);

        try
        {
            // remove scroll delta update, because it is handled in RawUpdate now
            c.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld("Menu.Menu", "floatScrollWheel")
                );
            c.RemoveRange(7);

            // number edit from 15f to 10f
            c.GotoNext(i => i.MatchLdcR4(15f));
            c.Remove();
            c.Emit(OpCodes.Ldc_R4, 10f);
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }
}
