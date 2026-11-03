using Odyssey.Models.Localization;
using Odyssey.Settings;
using System;
using System.Diagnostics;

namespace Odyssey.Models.Documents
{
    public class EresseaDocument
    {
        /// <summary>
        /// Lines number of the document..
        /// </summary>
        public int LinesNumber { get; protected set; }

        /// <summary>
        /// Language defined in the Eressea document.
        /// </summary>
        public GameLanguage Locale { get; protected set; }

        /// <summary>
        /// Returns true if the document has been modified; otherwise, false.
        /// </summary>
        public bool IsModified { get; protected set; }

        public event EventHandler? ModifiedChanged;

        /// <summary>
        /// Returns true if document is read only; otherwise false.
        /// </summary>
        public bool IsReadOnly { get; protected set; }

        /// <summary>
        /// Content of the document.
        /// </summary>
        public string? Text { get; protected set; }
        public EresseaDocument()
        {
            IsReadOnly = GlobalSettings.Get<bool>(GlobalSettings.IS_READONLY_MODE);
            IsModified = false;
            Text = "";
            Locale = GameLanguage.Unknown;
        }
        /// <summary>
        /// Mark the document as the specified modified status.
        /// If readonly, the document can not be marked as modified.
        /// </summary>
        /// <param name="isModified"></param>
        public void SetModified(bool isModified)
        {
            if (IsReadOnly && isModified)
            {
                Debug.WriteLine("[ERESSEA-DOC] !!! a readonly doc can not be modified");
                return ;
            }

            if (IsModified == isModified)
            {
                return;
            }

            IsModified = isModified;
            ModifiedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
