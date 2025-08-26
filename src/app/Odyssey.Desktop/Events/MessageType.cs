
namespace Odyssey.Events
{
    // FOX System Defined Selector Types
    public enum MessageType
    {
        SEL_NONE,
        SEL_KEYPRESS,                         /// Key pressed
        SEL_KEYRELEASE,                       /// Key released
        SEL_LEFTBUTTONPRESS,                  /// Left mouse button pressed
        SEL_LEFTBUTTONRELEASE,                /// Left mouse button released
        SEL_MIDDLEBUTTONPRESS,                /// Middle mouse button pressed
        SEL_MIDDLEBUTTONRELEASE,              /// Middle mouse button released
        SEL_RIGHTBUTTONPRESS,                 /// Right mouse button pressed
        SEL_RIGHTBUTTONRELEASE,               /// Right mouse button released
        SEL_MOTION,                           /// Mouse motion
        SEL_ENTER,                            /// Mouse entered window
        SEL_LEAVE,                            /// Mouse left window
        SEL_FOCUSIN,                          /// Focus into window
        SEL_FOCUSOUT,                         /// Focus out of window
        SEL_KEYMAP,
        SEL_UNGRABBED,                        /// Lost the grab (Windows)
        SEL_PAINT,                            /// Must repaint window
        SEL_CREATE,
        SEL_DESTROY,
        SEL_UNMAP,                            /// Window was hidden
        SEL_MAP,                              /// Window was shown
        SEL_CONFIGURE,                        /// Resize
        SEL_SELECTION_LOST,                   /// Widget lost selection
        SEL_SELECTION_GAINED,                 /// Widget gained selection
        SEL_SELECTION_REQUEST,                /// Inquire selection data
        SEL_RAISED,                           /// Window to top of stack
        SEL_LOWERED,                          /// Window to bottom of stack
        SEL_CLOSE,                            /// Close window
        SEL_DELETE,                           /// Delete window
        SEL_MINIMIZE,                         /// Iconified
        SEL_RESTORE,                          /// No longer iconified or maximized
        SEL_MAXIMIZE,                         /// Maximized
        SEL_UPDATE,                           /// GUI update
        SEL_COMMAND,                          /// GUI command
        SEL_CLICKED,                          /// Clicked
        SEL_DOUBLECLICKED,                    /// Double-clicked
        SEL_TRIPLECLICKED,                    /// Triple-clicked
        SEL_MOUSEWHEEL,                       /// Mouse wheel
        SEL_CHANGED,                          /// GUI has changed
        SEL_VERIFY,                           /// Verify change
        SEL_DESELECTED,                       /// Deselected
        SEL_SELECTED,                         /// Selected
        SEL_INSERTED,                         /// Inserted
        SEL_REPLACED,                         /// Replaced
        SEL_DELETED,                          /// Deleted
        SEL_OPENED,                           /// Opened
        SEL_CLOSED,                           /// Closed
        SEL_EXPANDED,                         /// Expanded
        SEL_COLLAPSED,                        /// Collapsed
        SEL_BEGINDRAG,                        /// Start a drag
        SEL_ENDDRAG,                          /// End a drag
        SEL_DRAGGED,                          /// Dragged
        SEL_LASSOED,                          /// Lassoed
        SEL_TIMEOUT,                          /// Timeout occurred
        SEL_SIGNAL,                           /// Signal received
        SEL_CLIPBOARD_LOST,                   /// Widget lost clipboard
        SEL_CLIPBOARD_GAINED,                 /// Widget gained clipboard
        SEL_CLIPBOARD_REQUEST,                /// Inquire clipboard data
        SEL_CHORE,                            /// Background chore
        SEL_FOCUS_SELF,                       /// Focus on widget itself
        SEL_FOCUS_RIGHT,                      /// Focus moved right
        SEL_FOCUS_LEFT,                       /// Focus moved left
        SEL_FOCUS_DOWN,                       /// Focus moved down
        SEL_FOCUS_UP,                         /// Focus moved up
        SEL_FOCUS_NEXT,                       /// Focus moved to next widget
        SEL_FOCUS_PREV,                       /// Focus moved to previous widget
        SEL_DND_ENTER,                        /// Drag action entering potential drop target
        SEL_DND_LEAVE,                        /// Drag action leaving potential drop target
        SEL_DND_DROP,                         /// Drop on drop target
        SEL_DND_MOTION,                       /// Drag position changed over potential drop target
        SEL_DND_REQUEST,                      /// Inquire drag and drop data
        SEL_IO_READ,                          /// Read activity on a pipe
        SEL_IO_WRITE,                         /// Write activity on a pipe
        SEL_IO_EXCEPT,                        /// Except activity on a pipe
        SEL_PICKED,                           /// Picked some location
        SEL_QUERY_TIP,                        /// Message inquiring about tooltip
        SEL_QUERY_HELP,                       /// Message inquiring about statusline help
        SEL_DOCKED,                           /// Toolbar docked
        SEL_FLOATED,                          /// Toolbar floated
        SEL_SESSION_NOTIFY,                   /// Session is about to close
        SEL_SESSION_CLOSED,                   /// Session is closed
        SEL_LAST
    };
}
