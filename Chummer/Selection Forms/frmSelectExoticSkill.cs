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
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectExoticSkill : Form
    {
        private readonly Character _objCharacter;

        #region Control Events
        public frmSelectExoticSkill(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void frmSelectExoticSkill_Load(object sender, EventArgs e)
        {
            XmlDocument objXmlDocument = XmlManager.Load("skills.xml");

            List<ListItem> lstSkills = new List<ListItem>();
            

            // Build the list of Exotic Active Skills from the Skills file.
            XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/skills/skill[exotic = \"Yes\"]");
            foreach (XmlNode objXmlSkill in objXmlSkillList)
            {
                string strName = objXmlSkill["name"].InnerText;
                lstSkills.Add(new ListItem(strName, objXmlSkill["translate"]?.InnerText ?? strName));
            }
            SortListItem objSort = new SortListItem();
            lstSkills.Sort(objSort.Compare);
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = lstSkills;

            // Select the first Skill in the list.
            cboCategory.SelectedIndex = 0;

            cboCategory.EndUpdate();

            BuildList();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildList();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Skill that was selected in the dialogue.
        /// </summary>
        public string SelectedExoticSkill
        {
            get
            {
                return cboCategory.SelectedValue.ToString();
            }
        }
        /// <summary>
        /// Skill specialisation that was selected in the dialogue.
        /// </summary>
        public string SelectedExoticSkillSpecialisation
        {
            get
            {
                if (cboSkillSpecialisations.SelectedValue == null)
                {
                    return cboSkillSpecialisations.Text;
                }
                else
                {
                    return cboSkillSpecialisations.SelectedValue.ToString();
                }
            }
        }


        #endregion

        private void BuildList()
        {
            List<ListItem> lstSkillSpecialisations = new List<ListItem>();
            XmlDocument objXmlDocument = XmlManager.Load("skills.xml");

            XmlNodeList objXmlSelectedSkill =
                objXmlDocument.SelectNodes("/chummer/skills/skill[name = \"" + cboCategory.SelectedValue.ToString() + "\" and (" + _objCharacter.Options.BookXPath() + ")]/specs/spec");
            XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");
            XmlNodeList objXmlWeaponList =
                objXmlWeaponDocument.SelectNodes("/chummer/weapons/weapon[(category = \"" + cboCategory.SelectedValue.ToString() +
                                                 "s\" or useskill = \"" + cboCategory.SelectedValue.ToString() + "\") and (" + _objCharacter.Options.BookXPath() + ")]");
            foreach (XmlNode objXmlWeapon in objXmlWeaponList)
            {
                string strName = objXmlWeapon["name"].InnerText;
                lstSkillSpecialisations.Add(new ListItem(strName, objXmlWeapon["translate"]?.InnerText ?? strName));
            }
            foreach (XmlNode objXmlSpecialization in objXmlSelectedSkill)
            {
                string strInnerText = objXmlSpecialization.InnerText;
                lstSkillSpecialisations.Add(new ListItem(strInnerText, objXmlSpecialization.Attributes?["translate"]?.InnerText ?? strInnerText));
            }
            cboSkillSpecialisations.BeginUpdate();
            cboSkillSpecialisations.ValueMember = "Value";
            cboSkillSpecialisations.DisplayMember = "Name";
            cboSkillSpecialisations.DataSource = lstSkillSpecialisations;

            // Select the first Skill in the list.
            cboSkillSpecialisations.SelectedIndex = 0;
            cboSkillSpecialisations.EndUpdate();
        }
    }
}
