
using UnityEngine;
using ToolbarControl_NS;

namespace Tac
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(TacAtomicClockMain.MODID, TacAtomicClockMain.MODNAME);
        }
    }
}