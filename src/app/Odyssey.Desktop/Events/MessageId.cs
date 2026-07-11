
namespace Odyssey.Events
{
    public enum MessageId
    {
        ID_NONE,
        ID_HIDE,            // ID_HIDE+FALSE
        ID_SHOW,            // ID_HIDE+TRUE
        ID_TOGGLESHOWN,
        ID_LOWER,
        ID_RAISE,
        ID_DELETE,
        ID_DISABLE,         // ID_DISABLE+FALSE
        ID_ENABLE,          // ID_DISABLE+TRUE
        ID_TOGGLEENABLED,
        ID_UNCHECK,         // ID_UNCHECK+FALSE
        ID_CHECK,           // ID_UNCHECK+TRUE
        ID_UNKNOWN,         // ID_UNCHECK+MAYBE
        ID_UPDATE,
        ID_AUTOSCROLL,
        ID_TIPTIMER,
        ID_HSCROLLED,
        ID_VSCROLLED,
        ID_SETVALUE,
        ID_SETINTVALUE,
        ID_SETREALVALUE,
        ID_SETSTRINGVALUE,
        ID_SETICONVALUE,
        ID_SETINTRANGE,
        ID_SETREALRANGE,
        ID_GETINTVALUE,
        ID_GETREALVALUE,
        ID_GETSTRINGVALUE,
        ID_GETICONVALUE,
        ID_GETINTRANGE,
        ID_GETREALRANGE,
        ID_SETHELPSTRING,
        ID_GETHELPSTRING,
        ID_SETTIPSTRING,
        ID_GETTIPSTRING,
        ID_QUERY_MENU,
        ID_HOTKEY,
        ID_ACCEL,
        ID_UNPOST,
        ID_POST,
        ID_MDI_TILEHORIZONTAL,
        ID_MDI_TILEVERTICAL,
        ID_MDI_CASCADE,
        ID_MDI_MAXIMIZE,
        ID_MDI_MINIMIZE,
        ID_MDI_RESTORE,
        ID_MDI_CLOSE,
        ID_MDI_WINDOW,
        ID_MDI_MENUWINDOW,
        ID_MDI_MENUMINIMIZE,
        ID_MDI_MENURESTORE,
        ID_MDI_MENUCLOSE,
        ID_MDI_NEXT,
        ID_MDI_PREV,
        ID_LAST
    }

}
