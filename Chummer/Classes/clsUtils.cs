/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
﻿using System.IO;
﻿using System.Linq;
﻿using System.Net;
﻿using System.Reflection;
﻿using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    static class Utils
    {
        public static void BreakIfDebug()
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }

        public static bool IsRunningInVisualStudio => Process.GetCurrentProcess().ProcessName == "devenv";

        private static Version s_VersionCachedGitVersion = null;
        public static Version CachedGitVersion
        {
            get
            {
                return s_VersionCachedGitVersion;
            }
            set
            {
                s_VersionCachedGitVersion = value;
            }
        }

        public static int GitUpdateAvailable()
        {
            Version verCurrentversion = Assembly.GetExecutingAssembly().GetName().Version;
            int intResult = CachedGitVersion?.CompareTo(verCurrentversion) ?? 0;
            return intResult;
        }

        /// <summary>
        /// Restarts Chummer5a.
        /// </summary>
        /// <param name="strText">Text to display in the prompt to restart. If empty, no prompt is displayed.</param>
        public static void RestartApplication(string strText = "Message_Options_Restart")
        {
            if (!string.IsNullOrEmpty(strText))
            {
                string text = LanguageManager.GetString(strText);
                string caption = LanguageManager.GetString("MessageTitle_Options_CloseForms");

                if (MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }
            // Need to do this here in case filenames are changed while closing forms (because a character who previously did not have a filename was saved when prompted)
            // Cannot use foreach because saving a character as created removes the current form and adds a new one
            for (int i = 0; i < Program.MainForm.OpenCharacterForms.Count; ++i)
            {
                CharacterShared objOpenCharacterForm = Program.MainForm.OpenCharacterForms[i];
                if (objOpenCharacterForm.IsDirty)
                {
                    string strCharacterName = objOpenCharacterForm.CharacterObject.CharacterName;
                    DialogResult objResult = MessageBox.Show(LanguageManager.GetString("Message_UnsavedChanges").Replace("{0}", strCharacterName), LanguageManager.GetString("MessageTitle_UnsavedChanges"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (objResult == DialogResult.Yes)
                    {
                        // Attempt to save the Character. If the user cancels the Save As dialogue that may open, cancel the closing event so that changes are not lost.
                        bool blnResult = objOpenCharacterForm.SaveCharacter();
                        if (!blnResult)
                            return;
                        // We saved a character as created, which closed the current form and added a new one
                        // This works regardless of dispose, because dispose would just set the objOpenCharacterForm pointer to null, so OpenCharacterForms would never contain it
                        else if (!Program.MainForm.OpenCharacterForms.Contains(objOpenCharacterForm))
                            i -= 1;
                    }
                    else if (objResult == DialogResult.Cancel)
                    {
                        return;
                    }
                }
            }
            Log.Info("Restart Chummer");
            Program.MainForm.Cursor = Cursors.WaitCursor;
            // Get the parameters/arguments passed to program if any
            string arguments = string.Empty;
            foreach (CharacterShared objOpenCharacterForm in Program.MainForm.OpenCharacterForms)
            {
                arguments += "\"" + objOpenCharacterForm.CharacterObject.FileName + "\" ";
            }
            arguments = arguments.Trim();
            // Restart current application, with same arguments/parameters
            foreach (Form objForm in Program.MainForm.MdiChildren)
            {
                objForm.Close();
            }
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Application.StartupPath + Path.DirectorySeparatorChar + AppDomain.CurrentDomain.FriendlyName,
                Arguments = arguments
            };
            Application.Exit();
            Process.Start(startInfo);
        }
    }
}
