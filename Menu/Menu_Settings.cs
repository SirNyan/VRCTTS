using System;
using System.Collections.Generic;
using System.IO;
using NAudio.CoreAudioApi;
using System.Text;
using NAudio.Wave;

namespace VRCTTS
{
    class Menu_Settings
    {
        private Writer writer;
        private StreamWriter sw;
        private StreamReader sr;
        private Globals globals;
        
        private ConsoleKeyInfo keyPressed;

        List<MMDevice> inputDevices;
        List<MMDevice> outputDevices;
        
        private string settingsPath;

        private List<string> settingsText;

        private List<string> namesOfDevices_Input;
        private List<string> namesOfDevices_Output;

        private string[] menuItems = { "Input_Device      ", "Output_Device     ", "OSC               ", "Azure             ","Back              "};

        private int previousIndex;
        private int currentIndex;

        private int previousSubMenuIndex;
        private int currentSubMenuIndex;

        private bool mainMenu;
        private bool isSubMenuOpen_Input;
        private bool isSubMenuOpen_Output;
        private bool isSubMenuOpen_OSC;
        private bool isSubMenuOpen_Azure;


        public Menu_Settings() {
            globals = Globals.Instance;

            inputDevices = new List<MMDevice>();
            outputDevices = new List<MMDevice>();

            namesOfDevices_Input = new List<string>();
            namesOfDevices_Output = new List<string>();

            writer = new Writer();
            settingsText = new List<string>();
            settingsPath = Directory.GetCurrentDirectory() + "/Settings.txt";

            loadAudioDevices();
            LoadSettings();
        }

        /// <summary>
        /// Main function of settings class.
        /// </summary>
        public void LoadMenu() {
            inputDevices.Clear();
            outputDevices.Clear();

            settingsText.Clear();

            namesOfDevices_Input.Clear();
            namesOfDevices_Output.Clear();

            currentIndex = 0;
            previousIndex = 0;

            isSubMenuOpen_Input = false;
            isSubMenuOpen_Output = false;
            isSubMenuOpen_OSC = false;
            mainMenu = true;

            loadAudioDevices();
            LoadSettings();

            DrawMenu();

            while (mainMenu) {
                ReadInput();
            }
        }

        /// <summary>
        /// Draw main menu.
        /// </summary>
        private void DrawMenu() {
            Console.Clear();
            writer.writeAt(5, 0, "--Settings Menu--");

            for (int i = 0; i < menuItems.Length; i++)
            {
                if (i == currentIndex)
                {
                    writer.writeAt(5, i + 2, menuItems[i], ConsoleColor.Black, ConsoleColor.White);
                }
                else
                {
                    writer.writeAt(5, i + 2, menuItems[i], ConsoleColor.White, ConsoleColor.Black);
                }
            }
        }

        /// <summary>
        /// Draw input device selection menu.
        /// </summary>
        private void subMenu_Input()
        {
            currentSubMenuIndex = 0;
            previousSubMenuIndex = 0;


            if (inputDevices.Count > 0)
            {
                for (int i = 0; i < inputDevices.Count; i++)
                {
                    if (currentSubMenuIndex == i)
                    {
                        if (globals.selectedInputDevice == inputDevices[i]) writer.writeAt(25, i + 2, "--> " + namesOfDevices_Input[i], ConsoleColor.Black, ConsoleColor.White);
                        else writer.writeAt(25, i + 2, namesOfDevices_Input[i], ConsoleColor.Black, ConsoleColor.White);
                    }
                    else
                    {
                        if (globals.selectedInputDevice == inputDevices[i]) writer.writeAt(25, i + 2, "--> " + namesOfDevices_Input[i], ConsoleColor.White, ConsoleColor.Black);
                        else writer.writeAt(25, i + 2, namesOfDevices_Input[i], ConsoleColor.White, ConsoleColor.Black);
                    }
                }

                while (isSubMenuOpen_Input)
                {
                    ReadInput();
                }
            }
            else
            {
                isSubMenuOpen_Input = false;
                UpdateMenu();
            }
        }

        /// <summary>
        /// Draw output device selection menu. 
        /// </summary>
        private void subMenu_Output()
        {
            currentSubMenuIndex = 0;
            previousSubMenuIndex = 0;

            if (outputDevices.Count > 0)
            {
                for (int i = 0; i < outputDevices.Count; i++)
                {
                    if (currentSubMenuIndex == i)
                    {
                        if (globals.selectedOutputDevice == outputDevices[i]) writer.writeAt(25, i + 2, "--> " + namesOfDevices_Output[i], ConsoleColor.Black, ConsoleColor.White);
                        else writer.writeAt(25, i + 2, namesOfDevices_Output[i], ConsoleColor.Black, ConsoleColor.White);
                    }
                    else
                    {
                        if (globals.selectedOutputDevice == outputDevices[i]) writer.writeAt(25, i + 2, "--> " + namesOfDevices_Output[i], ConsoleColor.White, ConsoleColor.Black);
                        else writer.writeAt(25, i + 2, namesOfDevices_Output[i], ConsoleColor.White, ConsoleColor.Black);
                    }
                }

                while (isSubMenuOpen_Output)
                {
                    ReadInput();
                }
            }
            else 
            {
                isSubMenuOpen_Output = false;
                UpdateMenu();
            }
        }

        /// <summary>
        /// Draw open sound control menu.
        /// </summary>
        private void subMenu_OSC() {
            currentSubMenuIndex = 0;
            previousSubMenuIndex = 0;

            writer.writeAt(25, 2, "OSC Parameter: " + globals.osc_parameter, ConsoleColor.Black, ConsoleColor.White);
            writer.writeAt(25, 3, "Sender Port: " + globals.osc_port_sender);
            writer.writeAt(25, 4, "Reciever Port: " + globals.osc_port_reciever);
            writer.writeAt(25, 5, "IP: " + globals.osc_IPAddress.ToString());
            writer.writeAt(25, 6, "Back");

            while (isSubMenuOpen_OSC)
            {
                ReadInput();
            }
        }

        /// <summary>
        /// Draw Azure Speech menu.
        /// </summary>
        private void subMenu_Azure()
        {
            currentSubMenuIndex = 0;
            previousSubMenuIndex = 0;

            writer.writeAt(25, 2, "Azure Key: " + globals.azure_key, ConsoleColor.Black, ConsoleColor.White);
            writer.writeAt(25, 3, "Azure Region: " + globals.azure_region);
            writer.writeAt(25, 4, "Back");

            while (isSubMenuOpen_Azure)
            {
                ReadInput();
            }
        }

        /// <summary>
        /// Update settings main menu.
        /// </summary>
        private void UpdateMenu()
        {
            writer.ClearLine(previousIndex + 2);
            writer.ClearLine(currentIndex + 2);


            writer.writeAt(5, previousIndex + 2, menuItems[previousIndex], ConsoleColor.White, ConsoleColor.Black);
            writer.writeAt(5, currentIndex + 2, menuItems[currentIndex], ConsoleColor.Black, ConsoleColor.White);
        }

        /// <summary>
        /// Update Input selection menu.
        /// </summary>
        private void UpdateSubMenu_Input()
        {
            writer.ClearLine(previousSubMenuIndex + 2, 25);
            writer.ClearLine(currentSubMenuIndex + 2, 25);

            if (inputDevices[previousSubMenuIndex] == globals.selectedInputDevice) {
                writer.writeAt(25, previousSubMenuIndex + 2, "--> " + namesOfDevices_Input[previousSubMenuIndex], ConsoleColor.White, ConsoleColor.Black);
            }
            else writer.writeAt(25, previousSubMenuIndex + 2, namesOfDevices_Input[previousSubMenuIndex], ConsoleColor.White, ConsoleColor.Black);

            if (inputDevices[currentSubMenuIndex] == globals.selectedInputDevice) {
                writer.writeAt(25, currentSubMenuIndex + 2, "--> " + namesOfDevices_Input[currentSubMenuIndex], ConsoleColor.Black, ConsoleColor.White);
            }
            else writer.writeAt(25, currentSubMenuIndex + 2, namesOfDevices_Input[currentSubMenuIndex], ConsoleColor.Black, ConsoleColor.White);
        }

        /// <summary>
        /// Update Output selecetion menu.
        /// </summary>
        private void UpdateSubMenu_Output()
        {
            writer.ClearLine(previousSubMenuIndex + 2, 25);
            writer.ClearLine(currentSubMenuIndex + 2, 25);

            if (outputDevices[previousSubMenuIndex] == globals.selectedOutputDevice) {
                writer.writeAt(25, previousSubMenuIndex + 2, "--> " + namesOfDevices_Output[previousSubMenuIndex], ConsoleColor.White, ConsoleColor.Black);
            }
            else writer.writeAt(25, previousSubMenuIndex + 2, namesOfDevices_Output[previousSubMenuIndex], ConsoleColor.White, ConsoleColor.Black);

            if (outputDevices[currentSubMenuIndex] == globals.selectedOutputDevice) {
                writer.writeAt(25, currentSubMenuIndex + 2, "--> " + namesOfDevices_Output[currentSubMenuIndex], ConsoleColor.Black, ConsoleColor.White);
            }
            else writer.writeAt(25, currentSubMenuIndex + 2, namesOfDevices_Output[currentSubMenuIndex], ConsoleColor.Black, ConsoleColor.White);
        }

        /// <summary>
        /// Update open sound control menu.
        /// </summary>
        private void UpdateSubMenu_OSC() 
        {
            writer.ClearLine(previousSubMenuIndex + 2, 25);
            writer.ClearLine(currentSubMenuIndex + 2, 25);

            if (currentSubMenuIndex == 0)
            {
                writer.writeAt(25, 2, "OSC Parameter: " + globals.osc_parameter, ConsoleColor.Black, ConsoleColor.White);
                writer.writeAt(25, 3, "Sender Port: " + globals.osc_port_sender);
                writer.writeAt(25, 4, "Reciever Port: " + globals.osc_port_reciever);
                writer.writeAt(25, 5, "IP: " + globals.osc_IPAddress.ToString()) ;
                writer.writeAt(25, 6, "Back");
            }
            else if (currentSubMenuIndex == 1)
            {
                writer.writeAt(25, 2, "OSC Parameter: " + globals.osc_parameter);
                writer.writeAt(25, 3, "Sender Port: " + globals.osc_port_sender, ConsoleColor.Black, ConsoleColor.White);
                writer.writeAt(25, 4, "Reciever Port: " + globals.osc_port_reciever);
                writer.writeAt(25, 5, "IP: " + globals.osc_IPAddress.ToString());
                writer.writeAt(25, 6, "Back");
            }
            else if (currentSubMenuIndex == 2)
            {
                writer.writeAt(25, 2, "OSC Parameter: " + globals.osc_parameter);
                writer.writeAt(25, 3, "Sender Port: " + globals.osc_port_sender);
                writer.writeAt(25, 4, "Reciever Port: " + globals.osc_port_reciever, ConsoleColor.Black, ConsoleColor.White);
                writer.writeAt(25, 5, "IP: " + globals.osc_IPAddress.ToString());
                writer.writeAt(25, 6, "Back");
            }
            else if (currentSubMenuIndex == 3)
            {
                writer.writeAt(25, 2, "OSC Parameter: " + globals.osc_parameter);
                writer.writeAt(25, 3, "Sender Port: " + globals.osc_port_sender);
                writer.writeAt(25, 4, "Reciever Port: " + globals.osc_port_reciever);
                writer.writeAt(25, 5, "IP: " + globals.osc_IPAddress.ToString(), ConsoleColor.Black, ConsoleColor.White);
                writer.writeAt(25, 6, "Back");
            }
            else {
                writer.writeAt(25, 2, "OSC Parameter: " + globals.osc_parameter);
                writer.writeAt(25, 3, "Sender Port: " + globals.osc_port_sender);
                writer.writeAt(25, 4, "Reciever Port: " + globals.osc_port_reciever);
                writer.writeAt(25, 5, "IP: " + globals.osc_IPAddress.ToString());
                writer.writeAt(25, 6, "Back", ConsoleColor.Black, ConsoleColor.White);
            }
        }

        /// <summary>
        /// Update Azure speech menu.
        /// </summary>
        private void UpdateSubMenu_Azure()
        {
            writer.ClearLine(2, 25);
            writer.ClearLine(3, 25);
            writer.ClearLine(4, 25);

            if (currentSubMenuIndex == 0)
            {
                writer.writeAt(25, 2, "Azure Key: " + globals.azure_key, ConsoleColor.Black, ConsoleColor.White);
                writer.writeAt(25, 3, "Azure Region: " + globals.azure_region);
                writer.writeAt(25, 4, "Back");
            }
            else if (currentSubMenuIndex == 1)
            {
                writer.writeAt(25, 2, "Azure Key: " + globals.azure_key);
                writer.writeAt(25, 3, "Azure Region: " + globals.azure_region, ConsoleColor.Black, ConsoleColor.White);
                writer.writeAt(25, 4, "Back");
            }
            else
            {
                writer.writeAt(25, 2, "Azure Key: " + globals.azure_key);
                writer.writeAt(25, 3, "Azure Region: " + globals.azure_region);
                writer.writeAt(25, 4, "Back", ConsoleColor.Black, ConsoleColor.White);
            }
        }

        /// <summary>
        /// Read user input.
        /// </summary>
        private void ReadInput()
        {
            keyPressed = Console.ReadKey(true);

            switch (keyPressed.Key)
            {
                case ConsoleKey.Enter:
                    Choice();
                    break;
                case ConsoleKey.Backspace:
                    if (isSubMenuOpen_Input)
                    {
                        #region clear line
                        for (int i = 0; i < inputDevices.Count; i++)
                        {
                            writer.ClearLine(i + 2, 25);
                        }
                        #endregion

                        isSubMenuOpen_Input = false;
                        UpdateMenu();
                    }
                    else if (isSubMenuOpen_Output)
                    {

                        #region clear line
                        for (int i = 0; i < outputDevices.Count; i++)
                        {
                            writer.ClearLine(i + 2, 25);
                        }
                        #endregion

                        isSubMenuOpen_Output = false;
                        UpdateMenu();
                    }
                    else if (isSubMenuOpen_OSC) {
                        writer.ClearLine(2, 25);
                        writer.ClearLine(3, 25);
                        writer.ClearLine(4, 25);
                        writer.ClearLine(5, 25);
                        writer.ClearLine(6, 25);

                        isSubMenuOpen_OSC = false;
                        UpdateMenu();
                    }
                    else if (isSubMenuOpen_Azure) {
                        writer.ClearLine(2, 25);
                        writer.ClearLine(3, 25);
                        writer.ClearLine(4, 25);

                        isSubMenuOpen_OSC = false;
                        UpdateMenu();
                    }
                    else {
                        mainMenu = false;
                        SaveSettings();
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (isSubMenuOpen_Input)
                    {
                        if (currentSubMenuIndex > 0) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex--; UpdateSubMenu_Input(); }
                    }
                    else if (isSubMenuOpen_Output)
                    {
                        if (currentSubMenuIndex > 0) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex--; UpdateSubMenu_Output(); }
                    }
                    else if (isSubMenuOpen_OSC)
                    {
                        if (currentSubMenuIndex > 0) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex--; UpdateSubMenu_OSC(); }
                    }
                    else if (isSubMenuOpen_Azure) 
                    {
                        if (currentSubMenuIndex > 0) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex--; UpdateSubMenu_Azure(); }
                    }
                    else
                    {
                        if (currentIndex > 0) { previousIndex = currentIndex; currentIndex--; UpdateMenu(); }
                    }
                    break;
                case ConsoleKey.DownArrow:
                    if (isSubMenuOpen_Input)
                    {
                        if (currentSubMenuIndex < inputDevices.Count - 1) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex++; UpdateSubMenu_Input(); }
                    }
                    else if (isSubMenuOpen_Output) 
                    {
                        if (currentSubMenuIndex < outputDevices.Count - 1) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex++; UpdateSubMenu_Output(); }
                    }
                    else if (isSubMenuOpen_OSC)
                    {
                        if (currentSubMenuIndex < 4) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex++; UpdateSubMenu_OSC(); }
                    }
                    else if (isSubMenuOpen_Azure)
                    {
                        if (currentSubMenuIndex < 2) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex++; UpdateSubMenu_Azure(); }
                    }
                    else
                    {
                        if (currentIndex < 4) { previousIndex = currentIndex; currentIndex++; UpdateMenu(); }
                    }
                    break;
                case ConsoleKey.W:
                    if (isSubMenuOpen_Input)
                    {
                        if (currentSubMenuIndex > 0) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex--; UpdateSubMenu_Input(); }
                    }
                    else if (isSubMenuOpen_Output)
                    {
                        if (currentSubMenuIndex > 0) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex--; UpdateSubMenu_Output(); }
                    }
                    else if (isSubMenuOpen_OSC)
                    {
                        if (currentSubMenuIndex > 0) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex--; UpdateSubMenu_OSC(); }
                    }
                    else if (isSubMenuOpen_Azure)
                    {
                        if (currentSubMenuIndex > 0) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex--; UpdateSubMenu_Azure(); }
                    }
                    else
                    {
                        if (currentIndex > 0) { previousIndex = currentIndex; currentIndex--; UpdateMenu(); }
                    }
                    break;
                case ConsoleKey.S:
                    if (isSubMenuOpen_Input)
                    {
                        if (currentSubMenuIndex < inputDevices.Count-1) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex++; UpdateSubMenu_Input(); }
                    }
                    else if (isSubMenuOpen_Output)
                    {
                        if (currentSubMenuIndex < outputDevices.Count - 1) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex++; UpdateSubMenu_Output(); }
                    }
                    else if (isSubMenuOpen_OSC)
                    {
                        if (currentSubMenuIndex < 4) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex++; UpdateSubMenu_OSC(); }
                    }
                    else if (isSubMenuOpen_Azure)
                    {
                        if (currentSubMenuIndex < 2) { previousSubMenuIndex = currentSubMenuIndex; currentSubMenuIndex++; UpdateSubMenu_Azure(); }
                    }
                    else
                    {
                        if (currentIndex < 4) { previousIndex = currentIndex; currentIndex++; UpdateMenu(); }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Upon pressing enter, determine what action needs to be taken.
        /// </summary>
        private void Choice()
        {
            if (isSubMenuOpen_Input) // Input Devices
            {
                globals.selectedInputDevice = inputDevices[currentSubMenuIndex];

                #region clear line
                for (int i = 0; i < inputDevices.Count; i++)
                {
                    writer.ClearLine(i + 2, 25);
                }
                #endregion

                isSubMenuOpen_Input = false;
                UpdateMenu();
            }
            else if (isSubMenuOpen_Output) // Output Devices
            {
                globals.selectedOutputDevice = outputDevices[currentSubMenuIndex];

                #region clear line
                for (int i = 0; i < outputDevices.Count; i++)
                {
                    writer.ClearLine(i + 2, 25);
                }
                #endregion

                isSubMenuOpen_Output = false;
                UpdateMenu();
            }
            else if (isSubMenuOpen_OSC) {
                if (currentSubMenuIndex == 0) // OSC parameter
                {
                    writer.ClearLine(2, 25);

                    Console.CursorVisible = true;
                    writer.writeAt(25, 2, "OSC Parameter: ", ConsoleColor.Black, ConsoleColor.White);
                    globals.osc_parameter = Console.ReadLine();
                    Console.CursorVisible = false;

                    UpdateSubMenu_OSC();
                }
                else if (currentSubMenuIndex == 1) // Sender port
                {
                    writer.ClearLine(3, 25);

                    Console.CursorVisible = true;
                    writer.writeAt(25, 3, "Sender Port: ", ConsoleColor.Black, ConsoleColor.White);
                    try
                    {
                        globals.osc_port_sender = Int32.Parse(Console.ReadLine());
                    }
                    catch (Exception)
                    {
                        globals.osc_port_sender = 9000;
                    }
                    Console.CursorVisible = false;

                    UpdateSubMenu_OSC();
                }
                else if (currentSubMenuIndex == 2) // Reciever port
                {
                    writer.ClearLine(4, 25);

                    Console.CursorVisible = true;
                    writer.writeAt(25, 4, "Reciever Port: ", ConsoleColor.Black, ConsoleColor.White);
                    try
                    {
                        globals.osc_port_reciever = Int32.Parse(Console.ReadLine());
                    }
                    catch (Exception)
                    {
                        globals.osc_port_reciever = 9001;
                    }
                    Console.CursorVisible = false;

                    UpdateSubMenu_OSC();
                }
                else if (currentSubMenuIndex == 3) // IP address
                {
                    writer.ClearLine(5, 25);

                    Console.CursorVisible = true;
                    writer.writeAt(25, 5, "IP: ", ConsoleColor.Black, ConsoleColor.White);

                    string temp_ip = Console.ReadLine();

                    if (!temp_ip.Equals(""))
                    {
                        globals.osc_IPAddress = temp_ip;
                    }
                    else {
                        globals.osc_IPAddress = "127.0.0.1";
                    }
                    
                    Console.CursorVisible = false;

                    UpdateSubMenu_OSC();
                }
                else { // Back
                    writer.ClearLine(2, 25);
                    writer.ClearLine(3, 25);
                    writer.ClearLine(4, 25);
                    writer.ClearLine(5, 25);
                    writer.ClearLine(6, 25);

                    isSubMenuOpen_OSC = false;
                    UpdateMenu();
                }
            }
            else if (isSubMenuOpen_Azure) {
                if (currentSubMenuIndex == 0) // Azure Key
                {
                    writer.ClearLine(2, 25);

                    Console.CursorVisible = true;
                    writer.writeAt(25, 2, "Azure Key: ", ConsoleColor.Black, ConsoleColor.White);
                    globals.azure_key = Console.ReadLine();
                    Console.CursorVisible = false;

                    UpdateSubMenu_Azure();
                }
                else if (currentSubMenuIndex == 1) // Azure Region
                {
                    writer.ClearLine(3, 25);

                    Console.CursorVisible = true;
                    writer.writeAt(25, 3, "Azure Region: ", ConsoleColor.Black, ConsoleColor.White);
                    globals.azure_region = Console.ReadLine();
                    Console.CursorVisible = false;

                    UpdateSubMenu_Azure();
                }
                else { // Back
                    writer.ClearLine(2, 25);
                    writer.ClearLine(3, 25);
                    writer.ClearLine(4, 25);

                    isSubMenuOpen_Azure = false;
                    UpdateMenu();
                }
            }
            else {
                // Menu settings
                switch (currentIndex)
                {
                    case 0:
                        isSubMenuOpen_Input = true;
                        subMenu_Input();
                        break;
                    case 1:
                        isSubMenuOpen_Output = true;
                        subMenu_Output();
                        break;
                    case 2:
                        isSubMenuOpen_OSC = true;
                        subMenu_OSC();
                        break;
                    case 3:
                        isSubMenuOpen_Azure = true;
                        subMenu_Azure();
                        break;
                    case 4:
                        mainMenu = false;
                        SaveSettings();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Load varaibles from text file.
        /// </summary>
        public void LoadSettings() {
            try
            {
                if (File.Exists(settingsPath)) 
                {
                    sr = new StreamReader(settingsPath);
                    string line;

                    bool foundInputDevice = false;
                    bool foundOutputDevice = false;

                    while ((line = sr.ReadLine()) != null)
                    {
                        settingsText.Add(line);
                    }

                    sr.Close();

                    foreach (string settingsLine in settingsText) {
                        string[] splitLine = settingsLine.Split('=');

                        if (splitLine.Length > 1) {

                            // Input devices.
                            if (splitLine[0].Equals("Input_Device"))
                            {
                                if (inputDevices.Count == 0)
                                {
                                    globals.selectedInputDevice = null;
                                }
                                else
                                {
                                    for (int i = 0; i < inputDevices.Count; i++)
                                    {
                                        if (splitLine[1].Equals(inputDevices[i].FriendlyName))
                                        {
                                            globals.selectedInputDevice = inputDevices[i];
                                            foundInputDevice = true;
                                        }
                                        else if (i == inputDevices.Count - 1 & !foundInputDevice) globals.selectedInputDevice = inputDevices[0];
                                    }
                                }
                            }
                            // Output devices.
                            else if (splitLine[0].Equals("Output_Device"))
                            {
                                if (outputDevices.Count == 0)
                                {
                                    globals.selectedOutputDevice = null;
                                }
                                else
                                {
                                    for (int i = 0; i < outputDevices.Count; i++)
                                    {
                                        if (splitLine[1].Equals(outputDevices[i].FriendlyName))
                                        {
                                            globals.selectedOutputDevice = outputDevices[i];
                                            foundOutputDevice = true;
                                        }
                                        else if (i == outputDevices.Count - 1 & !foundOutputDevice) globals.selectedOutputDevice = outputDevices[0];
                                    }
                                }
                            }
                            //OSC communication parameter.
                            else if (splitLine[0].Equals("OSC_Communication"))
                            {
                                if (splitLine.Length > 0)
                                {
                                    globals.osc_parameter = splitLine[1];
                                }
                                else globals.osc_parameter = "";
                            }
                            // OSC sender port.
                            else if (splitLine[0].Equals("OSC_Port_Sender"))
                            {
                                if (splitLine.Length > 0 & splitLine[1] != "")
                                {
                                    globals.osc_port_sender = Int32.Parse(splitLine[1]);
                                }
                                else globals.osc_port_sender = 9000;
                            }
                            // OSC reciever port.
                            else if (splitLine[0].Equals("OSC_Port_Reciever"))
                            {
                                if (splitLine.Length > 0 & splitLine[1] != "")
                                {
                                    globals.osc_port_reciever = Int32.Parse(splitLine[1]);
                                }
                                else globals.osc_port_reciever = 9001;
                            }
                            // OSC IP.
                            else if (splitLine[0].Equals("OSC_IP"))
                            {
                                if (splitLine.Length > 0)
                                {
                                    globals.osc_IPAddress = splitLine[1];
                                }
                                else globals.osc_IPAddress = "127.0.0.1";
                            }
                            // Azure key.
                            else if (splitLine[0].Equals("Azure_Key"))
                            {
                                if (splitLine.Length > 0)
                                {
                                    globals.azure_key = splitLine[1];
                                }
                                else globals.azure_key = "";
                            }
                            // Azure region.
                            else if (splitLine[0].Equals("Azure_Region"))
                            {
                                if (splitLine.Length > 0)
                                {
                                    globals.azure_region = splitLine[1];
                                }
                                else globals.azure_region = "";
                            }
                            // Azure Selected Voice
                            else if (splitLine[0].Equals("Azure_Selected_Voice")) 
                            {
                                if (splitLine.Length > 0)
                                {
                                    globals.azure_selected_voice = splitLine[1];
                                }
                                else globals.azure_selected_voice = "";
                            }
                            // Azure Selected Pitch
                            else if (splitLine[0].Equals("Azure_Selected_Pitch"))
                            {
                                if (splitLine.Length > 0)
                                {
                                    globals.azure_selected_pitch = splitLine[1];
                                }
                                else globals.azure_selected_pitch = "";
                            }
                            // Azure Selected Style
                            else if (splitLine[0].Equals("Azure_Selected_Style"))
                            {
                                if (splitLine.Length > 0)
                                {
                                    globals.azure_selected_style = splitLine[1];
                                }
                                else globals.azure_selected_style = "";
                            }
                            // Azure Selected Locale
                            else if (splitLine[0].Equals("Azure_Selected_Locale"))
                            {
                                if (splitLine.Length > 0)
                                {
                                    List<string> temp_locale = new List<string>();

                                    foreach (string lang in splitLine[1].Split(','))
                                    {
                                        if (!temp_locale.Equals("")) temp_locale.Add(lang);
                                    }

                                    globals.azure_selected_locale = temp_locale;
                                }
                                else {
                                    List<string> temp_locale = new List<string>();
                                    temp_locale.Add("English");
                                    globals.azure_selected_locale = temp_locale;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n{0}", e.Message);
                throw;
            }
        }

        /// <summary>
        /// Saves settings into a text file.
        /// </summary>
        public void SaveSettings() {
            try
            {
                sw = new StreamWriter(settingsPath);

                sw.WriteLine("[SETTINGS]");
                sw.WriteLine("");
                sw.WriteLine("Input_Device={0}", globals.selectedInputDevice.FriendlyName);
                sw.WriteLine("Output_Device={0}", globals.selectedOutputDevice.FriendlyName);
                sw.WriteLine("");
                sw.WriteLine("[OSC]");
                sw.WriteLine("");
                sw.WriteLine("OSC_Communication={0}", globals.osc_parameter);
                sw.WriteLine("OSC_Port_Sender={0}", globals.osc_port_sender);
                sw.WriteLine("OSC_Port_Reciever={0}", globals.osc_port_reciever);
                sw.WriteLine("OSC_IP={0}", globals.osc_IPAddress);
                sw.WriteLine("");
                sw.WriteLine("[AZURE]");
                sw.WriteLine("");
                sw.WriteLine("Azure_Key={0}", globals.azure_key);
                sw.WriteLine("Azure_Region={0}", globals.azure_region);
                sw.WriteLine("Azure_Selected_Voice={0}", globals.azure_selected_voice);
                sw.WriteLine("Azure_Selected_Pitch={0}", globals.azure_selected_pitch);
                sw.WriteLine("Azure_Selected_Style={0}", globals.azure_selected_style);

                string temp_locale = "";

                if (globals.azure_selected_locale.Count != 0)
                {
                    foreach (string locale in globals.azure_selected_locale)
                    {
                        if (temp_locale.Equals(""))
                        {
                            temp_locale += locale;
                        }
                        else
                        {
                            temp_locale += "," + locale;
                        }
                    }
                }
                
                sw.WriteLine("Azure_Selected_Locale={0}", temp_locale);

                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("\n{0}", e.Message);
                throw;
            }
        }

        /// <summary>
        /// Loads input and output audio devices.
        /// </summary>
        private void loadAudioDevices() {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();

            // Load all audio output devices
            foreach (MMDevice endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                outputDevices.Add(endpoint);
                namesOfDevices_Output.Add(endpoint.FriendlyName);
            }

            // Load all audio input devices
            foreach (MMDevice endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                inputDevices.Add(endpoint);
                namesOfDevices_Input.Add(endpoint.FriendlyName);
            }
        }
    }
}
