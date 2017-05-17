using System.IO;
using System.Windows.Forms;
using DnD.Classes.Player;
using Newtonsoft.Json;
using System;

namespace DnDBot.HeroInformation
{
    /// <summary>
    /// We have the ability to now Serialize and Deserialize (save/load) and with the use of Json properties
    /// we can customize what gets saved to the file and is readable! wow!
    /// </summary>
    public static class SaveLoadInfo
    {
        private static string _fileName;

        public static void Serialize(Hero theDude)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DnDSaves";            
            _fileName = path + "\\" + theDude.Name + ".txt";

            // create any directories that are needed: on the desktop, you should see a folder called DnDSaves with the hero name file in it.
            Directory.CreateDirectory(path);
            
            string json = JsonConvert.SerializeObject(theDude, Formatting.Indented);
            using (StreamWriter steamy = File.AppendText(_fileName))
            {
                steamy.WriteLine(json);
            }
        }

        ///TODO: I dont know if loading is going to do any good for this app's purpose.
    }
}
