using System;
using System.Threading;
using Rug.Osc;
using System.Timers;
using System.Collections.Generic;
using System.Text;

namespace VRCTTS
{
    class Menu_Azure
    {
        private Azure_Speech azure;
        private Writer writer;
        private OSC osc;
        private Globals globals;
        private Voice_Manager voice_Manager;

        private ConsoleKeyInfo keyPressed;

        private string[] menuItems = { "Listen         ", "Change voice   ", "Back           " };
        private string[] subMenuVoiceSettings = { "Language  ", "Voice     ", "Style     ", "Pitch     ", "Back      " };

        public bool isMainMenuOpen;
        public bool isSubMenuOpen_VoiceSettings;
        public bool isSubMenuOpen_Voice;
        public bool isSubMenuOpen_Style;
        public bool isSubMenuOpen_Pitch;
        public bool isSubMenuOpen_Language;

        public bool recevied_OSC;

        private int previousIndex;
        private int currentIndex;

        private int currentSubMenuIndex;
        private int previousSubMenuIndex;

        private int currentSubMenuIndex_2;
        private int previousSubMenuIndex_2;

        private int scrollable_menu_top_index;

        private int timer;

        public Menu_Azure() {
            azure = new Azure_Speech();
            writer = new Writer();
            osc = new OSC(this);
            voice_Manager = new Voice_Manager();
            globals = Globals.Instance;

            recevied_OSC = false;

            isMainMenuOpen = true;
            isSubMenuOpen_VoiceSettings = false;
            isSubMenuOpen_Voice = false;
            isSubMenuOpen_Style = false;
            isSubMenuOpen_Pitch = false;
            isSubMenuOpen_Language = false;
        }

        public void Update() {
            if (!globals.azure_key.Equals("") && !globals.azure_region.Equals("")) {
                azure.Update();
                voice_Manager.Update();
            }
        }

        /// <summary>
        /// Main function for Azure class.
        /// </summary>
        public void LoadMenu() {
            timer = 0;
            currentIndex = 0;
            previousIndex = 0;

            currentSubMenuIndex = 0;
            previousSubMenuIndex = 0;

            isMainMenuOpen = true;
            isSubMenuOpen_VoiceSettings = false;
            isSubMenuOpen_Voice = false;
            isSubMenuOpen_Style = false;
            isSubMenuOpen_Pitch = false;
            isSubMenuOpen_Language = false;

            // Start user input thread and osc listener thread.
            Thread inputThread = new Thread(new ThreadStart(ReadInput));

            RefreshVoices();

            DrawMenu();
            inputThread.Start();
            osc.Listen();

            // Main loop
            do
            {
                if (recevied_OSC) {
                    Start_STTTS();
                    osc.sendMessage(new OscMessage(globals.osc_parameter, false));

                    recevied_OSC = false;
                }
            } while (isMainMenuOpen);

            osc.stopListening();

            //oscThread.Join();
            inputThread.Join();
        }

        /// <summary>
        /// Draws Azure main menu.
        /// </summary>
        private void DrawMenu() {
            Console.Clear();
            writer.writeAt(5, 0, "Speech-To-Text-To-Speech");

            for (int i = 0; i < menuItems.Length; i++) {
                if (i == currentIndex)
                {
                    writer.writeAt(5, i+2, menuItems[i], ConsoleColor.Black, ConsoleColor.White);
                }
                else {
                    writer.writeAt(5, i+2, menuItems[i], ConsoleColor.White, ConsoleColor.Black);
                }
            }
        }

        /// <summary>
        /// Draws list of settings for Azure.
        /// </summary>
        private void DrawSubMenu_VoiceSettings() {
            currentSubMenuIndex = 0;
            previousSubMenuIndex = 0;

            for (int i = 0; i < subMenuVoiceSettings.Length; i++)
            {
                if (i == currentSubMenuIndex)
                {
                    writer.writeAt(25, i + 2, subMenuVoiceSettings[i], ConsoleColor.Black, ConsoleColor.White);
                }
                else
                {
                    writer.writeAt(25, i + 2, subMenuVoiceSettings[i], ConsoleColor.White, ConsoleColor.Black);
                }
            }

            while (isSubMenuOpen_VoiceSettings)
            {
                ReadInput();
            }
        }

        /// <summary>
        /// Draw language select menu.
        /// </summary>
        private void DrawSubMenu_Language() {
            currentSubMenuIndex_2 = 0;
            previousSubMenuIndex_2 = 0;

            scrollable_menu_top_index = 0;

            for (int i = 0; i < 8; i++)
            {
                if (voice_Manager.languages.Count > (i + scrollable_menu_top_index))
                {
                    if (currentSubMenuIndex_2 == i)
                    {
                        writer.writeAt(45, i + 2, GetLanguageString(voice_Manager.languages[i + scrollable_menu_top_index].Key), ConsoleColor.Black, ConsoleColor.White);
                    }
                    else 
                    {
                        writer.writeAt(45, i + 2, GetLanguageString(voice_Manager.languages[i + scrollable_menu_top_index].Key), ConsoleColor.White, ConsoleColor.Black);
                    }
                }
            }

            while (isSubMenuOpen_Language)
            {
                ReadInput();
            }
            voice_Manager.getVoices();
        }

        /// <summary>
        /// Draw voice select menu.
        /// </summary>
        private void DrawSubMenu_Voice()
        {
            currentSubMenuIndex_2 = 0;
            previousSubMenuIndex_2 = 0;

            scrollable_menu_top_index = 0;

            for (int i = 0; i < 8; i++)
            {
                if (voice_Manager.list_of_voices.Count > (i + scrollable_menu_top_index))
                {
                    if (currentSubMenuIndex_2 == i)
                    {
                        if (voice_Manager.list_of_voices[i + scrollable_menu_top_index].name == globals.azure_selected_voice)
                        {
                            writer.writeAt(45, i + 2, "X " + voice_Manager.list_of_voices[i + scrollable_menu_top_index].name, ConsoleColor.Black, ConsoleColor.White);
                        }
                        else
                        {
                            writer.writeAt(45, i + 2, voice_Manager.list_of_voices[i + scrollable_menu_top_index].name, ConsoleColor.Black, ConsoleColor.White);
                        }
                    }
                    else
                    {
                        if (voice_Manager.list_of_voices[i + scrollable_menu_top_index].name == globals.azure_selected_voice)
                        {
                            writer.writeAt(45, i + 2, "X " + voice_Manager.list_of_voices[i + scrollable_menu_top_index].name, ConsoleColor.White, ConsoleColor.Black);
                        }
                        else
                        {
                            writer.writeAt(45, i + 2, voice_Manager.list_of_voices[i + scrollable_menu_top_index].name, ConsoleColor.White, ConsoleColor.Black);
                        }
                    }
                }
            }

            while (isSubMenuOpen_Voice)
            {
                ReadInput();
            }
        }

        /// <summary>
        /// Draw style select menu.
        /// </summary>
        private void DrawSubMenu_Style()
        {
            currentSubMenuIndex_2 = 0;
            previousSubMenuIndex_2 = 0;

            scrollable_menu_top_index = 0;

            for (int i = 0; i < 8; i++)
            {
                if (voice_Manager.selected_voice.style.Length > (i + scrollable_menu_top_index))
                {
                    if (currentSubMenuIndex_2 == i)
                    {
                        if (voice_Manager.selected_voice.style[i + scrollable_menu_top_index] == globals.azure_selected_style)
                        {
                            writer.writeAt(45, i + 2, "X " + voice_Manager.selected_voice.style[i + scrollable_menu_top_index], ConsoleColor.Black, ConsoleColor.White);
                        }
                        else
                        {
                            writer.writeAt(45, i + 2, voice_Manager.selected_voice.style[i + scrollable_menu_top_index], ConsoleColor.Black, ConsoleColor.White);
                        }
                    }
                    else
                    {
                        if (voice_Manager.selected_voice.style[i + scrollable_menu_top_index] == globals.azure_selected_voice)
                        {
                            writer.writeAt(45, i + 2, "X " + voice_Manager.selected_voice.style[i + scrollable_menu_top_index], ConsoleColor.White, ConsoleColor.Black);
                        }
                        else
                        {
                            writer.writeAt(45, i + 2, voice_Manager.selected_voice.style[i + scrollable_menu_top_index], ConsoleColor.White, ConsoleColor.Black);
                        }
                    }
                }
            }

            while (isSubMenuOpen_Style)
            {
                ReadInput();
            }
        }

        public void DrawSubMenu_Pitch() 
        {
            currentSubMenuIndex_2 = 0;
            previousSubMenuIndex_2 = 0;

            scrollable_menu_top_index = 0;

            for(int i = 0; i < voice_Manager.selected_voice.pitch.Length; i++)
            {
                if (currentSubMenuIndex_2 == i)
                {
                    if (voice_Manager.selected_voice.pitch[i].Equals(globals.azure_selected_pitch))
                    {
                        writer.writeAt(45, i + 2, "X " + voice_Manager.selected_voice.pitch[i], ConsoleColor.Black, ConsoleColor.White);
                    }
                    else
                    {
                        writer.writeAt(45, i + 2, voice_Manager.selected_voice.pitch[i], ConsoleColor.Black, ConsoleColor.White);
                    }
                }
                else 
                {
                    if (voice_Manager.selected_voice.pitch[i].Equals(globals.azure_selected_pitch))
                    {
                        writer.writeAt(45, i + 2, "X " + voice_Manager.selected_voice.pitch[i], ConsoleColor.White, ConsoleColor.Black);
                    }
                    else 
                    {
                        writer.writeAt(45, i + 2, voice_Manager.selected_voice.pitch[i], ConsoleColor.White, ConsoleColor.Black);
                    } 
                }
            }

            while (isSubMenuOpen_Pitch)
            {
                ReadInput();
            }
        }

        /// <summary>
        /// Update Azure main menu.
        /// </summary>
        private void UpdateMenu() {
            writer.ClearLine(previousIndex + 2);
            writer.ClearLine(currentIndex + 2);
            writer.writeAt(5, previousIndex + 2, menuItems[previousIndex], ConsoleColor.White, ConsoleColor.Black);
            writer.writeAt(5, currentIndex + 2, menuItems[currentIndex], ConsoleColor.Black, ConsoleColor.White);
        }

        /// <summary>
        /// Update voice setting sub menu.
        /// </summary>
        private void UpdateSubMenuVoiceSettings() {
            writer.ClearLine(previousSubMenuIndex + 2, 25);
            writer.ClearLine(currentSubMenuIndex + 2, 25);
            writer.writeAt(25, previousSubMenuIndex + 2, subMenuVoiceSettings[previousSubMenuIndex], ConsoleColor.White, ConsoleColor.Black);
            writer.writeAt(25, currentSubMenuIndex + 2, subMenuVoiceSettings[currentSubMenuIndex], ConsoleColor.Black, ConsoleColor.White);
        }

        /// <summary>
        /// Update language sub menu.
        /// </summary>
        public void UpdateSubMenuLanguage() {
            for (int i = 0; i < 8; i++)
            {
                writer.ClearLine(i + 2, 45);
                if (voice_Manager.languages.Count > (i + scrollable_menu_top_index))
                {
                    if (currentSubMenuIndex_2 == i)
                    {
                        writer.writeAt(45, i + 2, GetLanguageString(voice_Manager.languages[i + scrollable_menu_top_index].Key), ConsoleColor.Black, ConsoleColor.White);
                    }
                    else
                    {
                        writer.writeAt(45, i + 2, GetLanguageString(voice_Manager.languages[i + scrollable_menu_top_index].Key), ConsoleColor.White, ConsoleColor.Black);
                    }
                }
            }
        }

        /// <summary>
        /// Update voice sub menu.
        /// </summary>
        public void UpdateSubMenuVoices()
        {
            for (int i = 0; i < 8; i++)
            {
                writer.ClearLine(i + 2, 45);
                if (voice_Manager.list_of_voices.Count > (i + scrollable_menu_top_index))
                {
                    if (currentSubMenuIndex_2 == i)
                    {
                        if (voice_Manager.list_of_voices[i + scrollable_menu_top_index].name == globals.azure_selected_voice)
                        {
                            writer.writeAt(45, i + 2, "X " + voice_Manager.list_of_voices[i + scrollable_menu_top_index].name, ConsoleColor.Black, ConsoleColor.White);
                        }
                        else
                        {
                            writer.writeAt(45, i + 2, voice_Manager.list_of_voices[i + scrollable_menu_top_index].name, ConsoleColor.Black, ConsoleColor.White);
                        }
                    }
                    else
                    {
                        if (voice_Manager.list_of_voices[i + scrollable_menu_top_index].name == globals.azure_selected_voice)
                        {
                            writer.writeAt(45, i + 2, "X " + voice_Manager.list_of_voices[i + scrollable_menu_top_index].name, ConsoleColor.White, ConsoleColor.Black);
                        }
                        else
                        {
                            writer.writeAt(45, i + 2, voice_Manager.list_of_voices[i + scrollable_menu_top_index].name, ConsoleColor.White, ConsoleColor.Black);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Update style sub menu.
        /// </summary>
        public void UpdateSubMenuStyle() 
        {
            for (int i = 0; i < 8; i++)
            {
                writer.ClearLine(i + 2, 45);
                if (voice_Manager.selected_voice.style.Length > (i + scrollable_menu_top_index))
                {
                    if (currentSubMenuIndex_2 == i)
                    {
                        if (voice_Manager.selected_voice.style[i + scrollable_menu_top_index] == globals.azure_selected_style)
                        {
                            writer.writeAt(45, i + 2, "X " + voice_Manager.selected_voice.style[i + scrollable_menu_top_index], ConsoleColor.Black, ConsoleColor.White);
                        }
                        else
                        {
                            writer.writeAt(45, i + 2, voice_Manager.selected_voice.style[i + scrollable_menu_top_index], ConsoleColor.Black, ConsoleColor.White);
                        }
                    }
                    else
                    {
                        if (voice_Manager.selected_voice.style[i + scrollable_menu_top_index] == globals.azure_selected_style)
                        {
                            writer.writeAt(45, i + 2, "X " + voice_Manager.selected_voice.style[i + scrollable_menu_top_index], ConsoleColor.White, ConsoleColor.Black);
                        }
                        else
                        {
                            writer.writeAt(45, i + 2, voice_Manager.selected_voice.style[i + scrollable_menu_top_index], ConsoleColor.White, ConsoleColor.Black);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update pitch sub menu.
        /// </summary>
        public void UpdateSubMenuPitch() 
        {
            writer.ClearLine(previousSubMenuIndex_2 + 2, 45);
            writer.ClearLine(currentSubMenuIndex_2 + 2, 45);

            if (voice_Manager.selected_voice.pitch[currentSubMenuIndex_2].Equals(globals.azure_selected_pitch))
            {
                writer.writeAt(45, currentSubMenuIndex_2 + 2, "X " + voice_Manager.selected_voice.pitch[currentSubMenuIndex_2], ConsoleColor.Black, ConsoleColor.White);
            }
            else
            {
                writer.writeAt(45, currentSubMenuIndex_2 + 2, voice_Manager.selected_voice.pitch[currentSubMenuIndex_2], ConsoleColor.Black, ConsoleColor.White);
            }

            if (voice_Manager.selected_voice.pitch[previousSubMenuIndex_2].Equals(globals.azure_selected_pitch))
            {
                writer.writeAt(45, previousSubMenuIndex_2 + 2, "X " + voice_Manager.selected_voice.pitch[previousSubMenuIndex_2], ConsoleColor.White, ConsoleColor.Black);
            }
            else
            {
                writer.writeAt(45, previousSubMenuIndex_2 + 2, voice_Manager.selected_voice.pitch[previousSubMenuIndex_2], ConsoleColor.White, ConsoleColor.Black);
            }
        }

        /// <summary>
        /// Read user input.
        /// </summary>
        private void ReadInput() {
            while (isMainMenuOpen) {
                keyPressed = Console.ReadKey(true);

                switch (keyPressed.Key)
                {
                    case ConsoleKey.Enter:
                        Choice();
                        break;
                    case ConsoleKey.Backspace:
                        if (isSubMenuOpen_VoiceSettings & !isSubMenuOpen_Language & !isSubMenuOpen_Pitch & !isSubMenuOpen_Style & !isSubMenuOpen_Voice) {
                            isSubMenuOpen_VoiceSettings = false;
                            writer.ClearLine(2, 25);
                            writer.ClearLine(3, 25);
                            writer.ClearLine(4, 25);
                            writer.ClearLine(5, 25);
                            writer.ClearLine(6, 25);
                            writer.ClearLine(7, 25);
                        }
                        else if (isSubMenuOpen_Language)
                        {
                            if (globals.azure_selected_locale.Count == 0)
                            {
                                globals.azure_selected_locale.Add("English");
                            }
                            RefreshVoices();

                            isSubMenuOpen_Language = false;
                            writer.ClearLine(2, 45);
                            writer.ClearLine(3, 45);
                            writer.ClearLine(4, 45);
                            writer.ClearLine(5, 45);
                            writer.ClearLine(6, 45);
                            writer.ClearLine(7, 45);
                            writer.ClearLine(8, 45);
                            writer.ClearLine(9, 45);
                        }
                        else if (isSubMenuOpen_Voice)
                        {
                            isSubMenuOpen_Voice = false;
                            writer.ClearLine(2, 45);
                            writer.ClearLine(3, 45);
                            writer.ClearLine(4, 45);
                            writer.ClearLine(5, 45);
                            writer.ClearLine(6, 45);
                            writer.ClearLine(7, 45);
                            writer.ClearLine(8, 45);
                            writer.ClearLine(9, 45);
                        }
                        else if (isSubMenuOpen_Style) 
                        {
                            isSubMenuOpen_Style = false;
                            writer.ClearLine(2, 45);
                            writer.ClearLine(3, 45);
                            writer.ClearLine(4, 45);
                            writer.ClearLine(5, 45);
                            writer.ClearLine(6, 45);
                            writer.ClearLine(7, 45);
                            writer.ClearLine(8, 45);
                            writer.ClearLine(9, 45);
                        }
                        else if (isSubMenuOpen_Pitch)
                        {
                            isSubMenuOpen_Pitch = false;
                            writer.ClearLine(2, 45);
                            writer.ClearLine(3, 45);
                            writer.ClearLine(4, 45);
                            writer.ClearLine(5, 45);
                            writer.ClearLine(6, 45);
                            writer.ClearLine(7, 45);
                            writer.ClearLine(8, 45);
                            writer.ClearLine(9, 45);
                        }
                        else isMainMenuOpen = false;
                        break;
                    case ConsoleKey.UpArrow:
                        // Voice settings UP
                        if (isSubMenuOpen_VoiceSettings & !isSubMenuOpen_Language & !isSubMenuOpen_Pitch & !isSubMenuOpen_Style & !isSubMenuOpen_Voice)
                        {
                            if (currentSubMenuIndex > 0) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex--; UpdateSubMenuVoiceSettings(); }
                        }
                        // Submenu language UP
                        else if (isSubMenuOpen_Language) 
                        {
                            if (currentSubMenuIndex_2 == 0 & scrollable_menu_top_index != 0)
                            {
                                scrollable_menu_top_index--;
                                UpdateSubMenuLanguage();
                            }
                            else if (currentSubMenuIndex_2 > 0) 
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2--;
                                UpdateSubMenuLanguage();
                            }
                        }
                        // Submenu voice UP
                        else if (isSubMenuOpen_Voice)
                        {
                            if (currentSubMenuIndex_2 == 0 & scrollable_menu_top_index != 0)
                            {
                                scrollable_menu_top_index--;
                                UpdateSubMenuVoices();
                            }
                            else if (currentSubMenuIndex_2 > 0)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2--;
                                UpdateSubMenuVoices();
                            }
                        }
                        // Submenu style UP
                        else if (isSubMenuOpen_Style)
                        {
                            if (currentSubMenuIndex_2 == 0 & scrollable_menu_top_index != 0)
                            {
                                scrollable_menu_top_index--;
                                UpdateSubMenuStyle();
                            }
                            else if (currentSubMenuIndex_2 > 0)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2--;
                                UpdateSubMenuStyle();
                            }
                        }
                        // Submenu pitch UP
                        else if (isSubMenuOpen_Pitch)
                        {
                            if (currentSubMenuIndex_2 > 0)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2--;
                                UpdateSubMenuPitch();
                            }
                        }
                        else { if (currentIndex > 0) { previousIndex = currentIndex; currentIndex--; UpdateMenu(); }}
                        break;
                    case ConsoleKey.DownArrow:
                        // Voice settings DOWN
                        if (isSubMenuOpen_VoiceSettings & !isSubMenuOpen_Language & !isSubMenuOpen_Pitch & !isSubMenuOpen_Style & !isSubMenuOpen_Voice)
                        {
                            if (currentSubMenuIndex < subMenuVoiceSettings.Length - 1) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex++; UpdateSubMenuVoiceSettings(); }
                        }
                        // Submenu language DOWN
                        else if (isSubMenuOpen_Language)
                        {
                            if (currentSubMenuIndex_2 == 7 & scrollable_menu_top_index + 7 < voice_Manager.languages.Count - 1)
                            {
                                scrollable_menu_top_index++;
                                UpdateSubMenuLanguage();
                            }
                            else if (currentSubMenuIndex_2 != 7 & currentSubMenuIndex_2 < voice_Manager.languages.Count - 1)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2++;
                                UpdateSubMenuLanguage();
                            }
                        }
                        // Submenu voices DOWN
                        else if (isSubMenuOpen_Voice)
                        {
                            if (currentSubMenuIndex_2 == 7 & scrollable_menu_top_index + 7 < voice_Manager.list_of_voices.Count - 1)
                            {
                                scrollable_menu_top_index++;
                                UpdateSubMenuVoices();
                            }
                            else if (currentSubMenuIndex_2 != 7 & currentSubMenuIndex_2 < voice_Manager.list_of_voices.Count - 1)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2++;
                                UpdateSubMenuVoices();
                            }
                        }
                        // Submenu styles DOWN
                        else if (isSubMenuOpen_Style)
                        {
                            if (currentSubMenuIndex_2 == 7 & scrollable_menu_top_index + 7 < voice_Manager.selected_voice.style.Length - 1)
                            {
                                scrollable_menu_top_index++;
                                UpdateSubMenuStyle();
                            }
                            else if (currentSubMenuIndex_2 != 7 & currentSubMenuIndex_2 < voice_Manager.selected_voice.style.Length - 1)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2++;
                                UpdateSubMenuStyle();
                            }
                        }
                        // Submenu pitch DOWN
                        else if (isSubMenuOpen_Pitch)
                        {
                            if (currentSubMenuIndex_2 < voice_Manager.selected_voice.pitch.Length - 1)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2++;
                                UpdateSubMenuPitch();
                            }
                        }
                        else { if (currentIndex < menuItems.Length - 1) { previousIndex = currentIndex; currentIndex++; UpdateMenu(); }}
                        break;
                    case ConsoleKey.W:
                        // Voice settings ^
                        if (isSubMenuOpen_VoiceSettings & !isSubMenuOpen_Language & !isSubMenuOpen_Pitch & !isSubMenuOpen_Style & !isSubMenuOpen_Voice)
                        {
                            if (currentSubMenuIndex > 0) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex--; UpdateSubMenuVoiceSettings(); }
                        }
                        // Submenu language ^
                        else if (isSubMenuOpen_Language)
                        {
                            if (currentSubMenuIndex_2 == 0 & scrollable_menu_top_index != 0)
                            {
                                scrollable_menu_top_index--;
                                UpdateSubMenuLanguage();
                            }
                            else if (currentSubMenuIndex_2 > 0)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2--;
                                UpdateSubMenuLanguage();
                            }
                        }
                        // Submenu voice UP
                        else if (isSubMenuOpen_Voice)
                        {
                            if (currentSubMenuIndex_2 == 0 & scrollable_menu_top_index != 0)
                            {
                                scrollable_menu_top_index--;
                                UpdateSubMenuVoices();
                            }
                            else if (currentSubMenuIndex_2 > 0)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2--;
                                UpdateSubMenuVoices();
                            }
                        }
                        // Submenu style UP
                        else if (isSubMenuOpen_Style)
                        {
                            if (currentSubMenuIndex_2 == 0 & scrollable_menu_top_index != 0)
                            {
                                scrollable_menu_top_index--;
                                UpdateSubMenuStyle();
                            }
                            else if (currentSubMenuIndex_2 > 0)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2--;
                                UpdateSubMenuStyle();
                            }
                        }
                        // Submenu pitch UP
                        else if (isSubMenuOpen_Pitch)
                        {
                            if (currentSubMenuIndex_2 > 0)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2--;
                                UpdateSubMenuPitch();
                            }
                        }
                        else { if (currentIndex > 0) { previousIndex = currentIndex; currentIndex--; UpdateMenu(); } }
                        break;
                    case ConsoleKey.S:
                        // Voice settings V
                        if (isSubMenuOpen_VoiceSettings & !isSubMenuOpen_Language & !isSubMenuOpen_Pitch & !isSubMenuOpen_Style & !isSubMenuOpen_Voice)
                        {
                            if (currentSubMenuIndex < subMenuVoiceSettings.Length - 1) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex++; UpdateSubMenuVoiceSettings(); ; }
                        }
                        // Submenu language V
                        else if (isSubMenuOpen_Language)
                        {
                            if (currentSubMenuIndex_2 == 7 & scrollable_menu_top_index + 7 < voice_Manager.languages.Count - 1)
                            {
                                scrollable_menu_top_index++;
                                UpdateSubMenuLanguage();
                            }
                            else if (currentSubMenuIndex_2 != 7 & currentSubMenuIndex_2 < voice_Manager.languages.Count - 1)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2++;
                                UpdateSubMenuLanguage();
                            }
                        }
                        // Submenu voices DOWN
                        else if (isSubMenuOpen_Voice)
                        {
                            if (currentSubMenuIndex_2 == 7 & scrollable_menu_top_index + 7 < voice_Manager.list_of_voices.Count - 1)
                            {
                                scrollable_menu_top_index++;
                                UpdateSubMenuVoices();
                            }
                            else if (currentSubMenuIndex_2 != 7 & currentSubMenuIndex_2 < voice_Manager.list_of_voices.Count - 1)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2++;
                                UpdateSubMenuVoices();
                            }
                        }
                        // Submenu styles DOWN
                        else if (isSubMenuOpen_Style)
                        {
                            if (currentSubMenuIndex_2 == 7 & scrollable_menu_top_index + 7 < voice_Manager.selected_voice.style.Length - 1)
                            {
                                scrollable_menu_top_index++;
                                UpdateSubMenuStyle();
                            }
                            else if (currentSubMenuIndex_2 != 7 & currentSubMenuIndex_2 < voice_Manager.selected_voice.style.Length - 1)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2++;
                                UpdateSubMenuStyle();
                            }
                        }
                        // Submenu pitch DOWN
                        else if (isSubMenuOpen_Pitch)
                        {
                            if (currentSubMenuIndex_2 < voice_Manager.selected_voice.pitch.Length - 1)
                            {
                                previousSubMenuIndex_2 = currentSubMenuIndex_2;
                                currentSubMenuIndex_2++;
                                UpdateSubMenuPitch();
                            }
                        }
                        else { if (currentIndex < menuItems.Length - 1) { previousIndex = currentIndex; currentIndex++; UpdateMenu(); } }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Upon pressing enter, determine what action needs to be taken.
        /// </summary>
        private void Choice() {
            if (isSubMenuOpen_VoiceSettings & !isSubMenuOpen_Language & !isSubMenuOpen_Pitch & !isSubMenuOpen_Style & !isSubMenuOpen_Voice)
            {
                switch (currentSubMenuIndex)
                {
                    case 0:
                        isSubMenuOpen_Language = true;
                        DrawSubMenu_Language();
                        break;
                    case 1:
                        if (voice_Manager.list_of_voices.Count != 0)
                        {
                            isSubMenuOpen_Voice = true;
                            DrawSubMenu_Voice();
                        }
                        break;
                    case 2:
                        if (voice_Manager.selected_voice != null)
                        {
                            if (voice_Manager.selected_voice.style.Length > 0 & !voice_Manager.selected_voice.style[0].Equals(""))
                            {
                                isSubMenuOpen_Style = true;
                                DrawSubMenu_Style();
                            }
                        }
                        break;
                    case 3:
                        if (voice_Manager.selected_voice != null)
                        {
                            isSubMenuOpen_Pitch = true;
                            DrawSubMenu_Pitch();
                        }
                        break;
                    case 4:
                        isSubMenuOpen_VoiceSettings = false;
                        writer.ClearLine(2, 25);
                        writer.ClearLine(3, 25);
                        writer.ClearLine(4, 25);
                        writer.ClearLine(5, 25);
                        writer.ClearLine(6, 25);
                        writer.ClearLine(7, 25);
                        break;
                    default:
                        break;
                }
            }
            else if (isSubMenuOpen_Language)
            {
                SelectLanguage();
                UpdateSubMenuLanguage();
            }
            else if (isSubMenuOpen_Voice)
            {
                voice_Manager.selected_voice = voice_Manager.list_of_voices[currentSubMenuIndex_2 + scrollable_menu_top_index];

                globals.azure_selected_voice = voice_Manager.list_of_voices[currentSubMenuIndex_2 + scrollable_menu_top_index].name;
                globals.azure_selected_style = voice_Manager.list_of_voices[0].style[0];
                globals.azure_selected_pitch = voice_Manager.list_of_voices[0].pitch[5];

                UpdateSubMenuVoices();
            }
            else if (isSubMenuOpen_Style)
            {
                globals.azure_selected_style = voice_Manager.selected_voice.style[currentSubMenuIndex_2 + scrollable_menu_top_index];

                UpdateSubMenuStyle();
            }
            else if (isSubMenuOpen_Pitch) 
            {
                globals.azure_selected_pitch = voice_Manager.selected_voice.pitch[currentSubMenuIndex_2];

                UpdateSubMenuPitch();
            }
            else {
                switch (currentIndex)
                {
                    case 0:
                        Start_STTTS();
                        break;
                    case 1:
                        isSubMenuOpen_VoiceSettings = true;
                        DrawSubMenu_VoiceSettings();
                        break;
                    case 2:
                        isMainMenuOpen = false;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Start Speech to Text to Speech.
        /// </summary>
        public void Start_STTTS() 
        {
            if (isMainMenuOpen) {
                if (globals.azure_key.Equals("") | globals.azure_region.Equals(""))
                {
                    writer.writeAt(5, 15, "**Fill Azure Key and Region**");
                }
                else if (globals.azure_selected_voice.Equals("") | voice_Manager.selected_voice == null)
                {
                    writer.writeAt(5, 15, "**Select a voice**");
                }
                else {
                    azure.stt();

                    #region clear line
                    writer.ClearLine(15);
                    writer.ClearLine(16);
                    writer.ClearLine(17);
                    writer.ClearLine(18);
                    writer.ClearLine(19);
                    writer.ClearLine(20);
                    writer.ClearLine(21);
                    #endregion

                    // Wait for Azure to complete speech recognition and synthesis
                    while (!azure.ttsCompleted && !azure.ttsFailed)
                    {
                        LoadingText();
                    }
                    timer = 0;

                    #region clear line
                    writer.ClearLine(15);
                    writer.ClearLine(16);
                    writer.ClearLine(17);
                    writer.ClearLine(18);
                    writer.ClearLine(19);
                    writer.ClearLine(20);
                    writer.ClearLine(21);
                    #endregion

                    if (azure.ttsCompleted) {
                        writer.writeAt(5, 15, "You said: " + azure.result);
                    } else writer.writeAt(5, 15, "Failed to recognise. Make sure Azure settings are matching.");

                    azure.tts();
                }
            }
        }

        /// <summary>
        /// Loading animation
        /// </summary>
        public void LoadingText()
        {
            if (timer == 0) { writer.writeAt(5, 15, "Listening   "); timer++; }
            else if (timer == 1) { writer.writeAt(5, 15, "Listening.  "); timer++; }
            else if (timer == 2) { writer.writeAt(5, 15, "Listening..  "); timer++; }
            else if (timer == 3) { writer.writeAt(5, 15, "Listening..."); timer = 0; }
            System.Threading.Thread.Sleep(500);
        }


        /// <summary>
        /// Returns a modified string.
        /// </summary>
        /// <returns></returns>
        private string GetLanguageString(string str)
        {
            foreach (string temp in globals.azure_selected_locale)
            {
                if (str.Equals(temp))
                {
                    return "X " + str;
                }
            }
            return str;
        }

        /// <summary>
        /// Select or Unselect a language.
        /// </summary>
        private void SelectLanguage() 
        {
            foreach (string language in globals.azure_selected_locale)
            {
                if (language.Equals(voice_Manager.languages[currentSubMenuIndex_2 + scrollable_menu_top_index].Key)) 
                {
                    globals.azure_selected_locale.Remove(language);
                    return;
                }
            }

            globals.azure_selected_locale.Add(voice_Manager.languages[currentSubMenuIndex_2 + scrollable_menu_top_index].Key);

            return;
        }

        private void RefreshVoices() 
        {
            if (globals.azure_region != "" & globals.azure_key != "") 
            {
                bool flag = false;
                // Load voices
                voice_Manager.getVoices();
                System.Threading.Thread.Sleep(2000);

                if (globals.azure_selected_voice != "" | globals.azure_selected_voice != null)
                {
                    foreach (Voice voice in voice_Manager.list_of_voices)
                    {
                        if (globals.azure_selected_voice.Equals(voice.name))
                        {
                            voice_Manager.selected_voice = voice;
                            flag = true;
                        }
                    }
                }
                else if (!flag)
                {
                    if (voice_Manager.list_of_voices.Count > 0)
                    {
                        voice_Manager.selected_voice = voice_Manager.list_of_voices[0];
                        globals.azure_selected_voice = voice_Manager.list_of_voices[0].name;
                        globals.azure_selected_style = voice_Manager.list_of_voices[0].style[0];
                        globals.azure_selected_pitch = voice_Manager.list_of_voices[0].pitch[5];
                    }
                }
            }
        }
    }
}
